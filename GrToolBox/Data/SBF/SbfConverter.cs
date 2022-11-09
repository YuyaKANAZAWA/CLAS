using GrToolBox.Common;
using System.Diagnostics;
using System.Text;
using static GrToolBox.Data.DataUtilities;
using static GrToolBox.Data.SBF.SbfUtilities;

namespace GrToolBox.Data.SBF
{
    public class SbfConverter
    {
        public StringBuilder StringToDisp { get; set; } = new StringBuilder();      // For Debug
        private int BufSize_IndexPart { get; set; } = 18; // バッファサイズを2のべき乗に限る  2^12=4096byte, 2^18=262144byte

        private CircularBuffer<byte> Cb { get; set; }
        private Stopwatch Sw { get; set; } = new Stopwatch();
        private Timer? NoSbfTimer { get; set; }
        private int CountNewSbfs { get; set; } = 0;
        private byte[]? ByteData { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public SbfConverter()
        {
            //int BufSize_IndexPart = 18; // バッファサイズを2のべき乗に限る  2^12=4096byte
            Cb = new(BufSize_IndexPart);
            Sw.Start();
            NoSbfTimer = new Timer(new TimerCallback(ThreadingTimerCallback));
            NoSbfTimer.Change(0, 5000);
        }

        public SbfConverter(byte[] byteData)
        {
            Cb = new(BufSize_IndexPart);
            ByteData = byteData;
        }


        public event EventHandler? NoSbfDataTimerReached;
        private void ThreadingTimerCallback(object? args)
        {
            if (Sw.ElapsedMilliseconds > 4000)
            {
                //Sw.Restart();
                NoSbfDataTimerReached?.Invoke(this, EventArgs.Empty);
            }
        }

        public int AvailableBufferSize()
        {
            return Cb.NumWritable();
        }

        public void AddDataAndAnalysis(byte[] data)
        {
            if(Cb.NumWritable() < data.Length)
            {
                Debug.WriteLine("SbfConverter circullar buffer overflow");
            }
            Cb.Add(data);
            List<byte[]> byteSentences = GetSbfByteSentence(Cb);

            List<IRawData> SbfDatas = GetSbfData(byteSentences);

            if (SbfDatas.Count > 0)
            {
                Sw.Restart();
                NewSbfDataReceivedEventArgs args = new NewSbfDataReceivedEventArgs();
                args.NewSbfDatas = SbfDatas;
                OnNewSbfDataReceived(args);
                CountNewSbfs = 0;
            }
        }


        protected virtual void OnNewSbfDataReceived(NewSbfDataReceivedEventArgs e)
        {
            EventHandler<NewSbfDataReceivedEventArgs>? handler = NewSbfDataReceived;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        public event EventHandler<NewSbfDataReceivedEventArgs>? NewSbfDataReceived;



        public static List<IRawData> GetSbfData(List<byte[]> byteSentences)
        {
            List<IRawData> SbfDatas = new List<IRawData>();
            foreach (var byteData in byteSentences)
            {
                var h = new Header_TimeStamp(byteData);
                h.DebugPrint_Header();
                //Sbf_Debug.printTo(StringToDisp, h);
                //Console.WriteLine(StringToDisp);
                //StringToDisp.Clear();
                
                switch (h.BlockNum)
                {
                    
                    case 4027:  // MeasEpoch
                        MeasEpoch_4027 measEpoch_4027 = new MeasEpoch_4027(byteData, h);
                        SbfDatas.Add(measEpoch_4027);
                        break;
                    case 5891:  // GPSDecodedMessage,GPSNav
                        GPSNav_5891 gPSNav_5891 = new GPSNav_5891(byteData, h);
                        SbfDatas.Add(gPSNav_5891);
                        break;
                    case 4004:
                        GLONav_4004 gLONav_4004 = new GLONav_4004(byteData, h);
                        SbfDatas.Add(gLONav_4004);
                        break;
                    case 4002:
                        GALNav_4002 gALNav_4002 = new GALNav_4002(byteData, h);
                        SbfDatas.Add(gALNav_4002);
                        break;
                    case 4095:  // QZSDecodedMessage,QZSNav
                        QZSNav_4095 qZSNav_4095 = new QZSNav_4095(byteData, h);
                        SbfDatas.Add(qZSNav_4095);
                        break;
                    case 4081:
                        BDSNav_4081 bDSNav_4081 = new BDSNav_4081(byteData, h);
                        SbfDatas.Add(bDSNav_4081);
                        break;
                    case 4069:  // QZSRawL6
                        QZSRawL6_4069 qZSRawL6_4069 = new QZSRawL6_4069(byteData, h);
                        SbfDatas.Add(qZSRawL6_4069);
                        break;
                }
            }
            return SbfDatas;
        }

        /// <summary>
        /// リングバッファに格納されたバイト列からSBFブロックを取り出して返す
        /// </summary>
        /// <param name="cb">データバイト列が格納されたリングバッファ</param>
        /// <returns>SBFブロック毎にリスト化されたバイト配列</returns>
        public static List<byte[]> GetSbfByteSentence(CircularBuffer<byte> cb)
        {
            List<byte[]> byte_sentences = new List<byte[]>();
            byte[] tmpBytes;
            byte[] tmpSentence;
            while (cb.Count() > 8)
            {
                cb.SetMark();
                // SBF判定
                tmpBytes = cb.Read(8);  // 8byte取得(SBF Block Header)
                int offset = 0;
                if (tmpBytes[0] == 0x_24 && tmpBytes[1] == 0x_40)    // $@ 検出
                {
                    offset += 16;
                    uint crc = BitToUint_L(tmpBytes, offset, 16);
                    offset += 16;
                    uint id = BitToUint_L(tmpBytes, offset, 16);
                    offset += 16;
                    int length = (int)BitToUint_L(tmpBytes, offset, 16);
                    offset += 16;
                    if (length > 0 && length % 4 == 0)  // TODO: length=0となる場合がある？　要確認
                    {
                        if (cb.Count() >= length - 8)
                        {
                            // checking CRC
                            cb.BackToMark();
                            tmpSentence = cb.Read(length);
                            ushort calcCrc = CRC_compute16CCITT(tmpSentence[4..]); // "$@"とCRCを飛ばして5バイト目以降(ID以降)でCRCを計算
                            if (calcCrc == crc) // CRC check OK
                            {
                                //センテンスを格納する
                                byte_sentences.Add(tmpSentence);
                                continue;
                            }
                            else
                            {   // NGなら巻き戻して1バイト進める流れなので，かなり無駄
                                Debug.WriteLine("Sbf CRC invalid");
                            }
                        }
                        else
                        {
                            Debug.WriteLine("possible SBF; need more data; Count: " + cb.Count() + ", length: " + length);
                            cb.BackToMark();
                            return byte_sentences;  // cb終端までのデータが，SBFの長さに足りないので戻して，追加のデータ注入を待つ
                        }
                    }   // not SBF (length not multiple of 4)
                }
                cb.BackToMark();
                if (cb.NumWritable() == 0)
                {
                    cb.Clear();
                }
                tmpBytes = cb.Read(1);  // cbを巻き戻して1バイト進める

            }
            return byte_sentences;
        }

    }

    public class NewSbfDataReceivedEventArgs : EventArgs
    {
        public List<IRawData>? NewSbfDatas { get; set; }
    }







}
