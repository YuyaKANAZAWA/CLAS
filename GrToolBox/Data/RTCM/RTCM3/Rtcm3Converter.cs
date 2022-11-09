using GrToolBox.Common;
using System.Diagnostics;
using System.Text;
using static GrToolBox.Data.DataUtilities;

namespace GrToolBox.Data.RTCM.RTCM3
{
    public class Rtcm3Converter
    {
        public StringBuilder StringToDisp { get; set; } = new StringBuilder();      // For Debug
        private int BufSize_IndexPart { get; set; } = 18; // バッファサイズを2のべき乗に限る  2^12=4096byte, 2^18=262144byte

        private CircularBuffer<byte> Cb { get; set; }
        private Stopwatch Sw { get; set; } = new Stopwatch();
        private Timer? NoRtcmTimer { get; set; }
        private int CountNewSbfs { get; set; } = 0;
        private byte[]? ByteData { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public Rtcm3Converter()
        {
            Cb = new(BufSize_IndexPart);
            Sw.Start();
            NoRtcmTimer = new Timer(new TimerCallback(ThreadingTimerCallback));
            NoRtcmTimer.Change(0, 5000);
        }

        public Rtcm3Converter(byte[] byteData)
        {
            Cb = new(BufSize_IndexPart);
            ByteData = byteData;
        }


        public event EventHandler? NoRtcmDataTimerReached;
        private void ThreadingTimerCallback(object? args)
        {
            if (Sw.ElapsedMilliseconds > 4000)
            {
                //Sw.Restart();
                NoRtcmDataTimerReached?.Invoke(this, EventArgs.Empty);
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
            List<byte[]> byteSentences = GetRtcm3ByteSentence(Cb);

            List<IRawData> RtcmDatas = GetRtcm3Data(byteSentences);

            if (RtcmDatas.Count > 0)
            {
                Sw.Restart();
                NewRtcmDataReceivedEventArgs args = new NewRtcmDataReceivedEventArgs();
                args.NewRtcmDatas = RtcmDatas;
                OnNewRtcmDataReceived(args);
                CountNewSbfs = 0;
            }
        }


        protected virtual void OnNewRtcmDataReceived(NewRtcmDataReceivedEventArgs e)
        {
            EventHandler<NewRtcmDataReceivedEventArgs>? handler = NewRtcmDataReceived;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        public event EventHandler<NewRtcmDataReceivedEventArgs>? NewRtcmDataReceived;



        public static List<IRawData> GetRtcm3Data(List<byte[]> byteSentences)
        {
            List<IRawData> RtcmDatas = new List<IRawData>();
            foreach (var byteData in byteSentences)
            {
                var f = new FrameInfo_MessageNum(byteData);
                //f.DebugPrint();

                switch (f.MessageNum)
                {
                    case 1001:  // 
                    case 1002:  // 
                    case 1003:  // 
                    case 1004:  // 
                        GpsRtkObs gpsRtkObs = new GpsRtkObs(byteData, f);
                        RtcmDatas.Add(gpsRtkObs);
                        //gpsRtkObs.DebugPrint();
                        break;
                    case 1009:  // 
                    case 1010:  // 
                    case 1011:  // 
                    case 1012:  // 
                        GloRtkObs gloRtkObs = new GloRtkObs(byteData, f);
                        RtcmDatas.Add(gloRtkObs);
                        //gpsRtkObs.DebugPrint();
                        break;
                    case 1019:
                        RtcmDatas.Add(new GpsEphemerides(byteData, f));
                        break;
                    case 1020:
                        RtcmDatas.Add(new GloEphemerides(byteData, f));
                        break;
                }
            }
            return RtcmDatas;
        }

        public static List<byte[]> GetRtcm3ByteSentence(CircularBuffer<byte> cb)
        {
            List<byte[]> byte_sentences = new List<byte[]>();
            byte[] tmpBytes;
            byte[] tmpSentence;
            byte[] crcBytes;
            uint crcValue;
            while (cb.Count() > 3)
            {
                cb.SetMark();
                // RTCM3判定
                tmpBytes = cb.Read(3);  // 3byte取得(Preamble+Reserved+MessageLength)
                if (tmpBytes[0] == 0b_1101_0011)    // プリアンブル検出, 0xd3
                {
                    if ((tmpBytes[1] >> 2) == 0b_0000_0000)     // Reserved 6ビット判定
                    {
                        uint messageLength = BitToUint(tmpBytes, 14, 10);   // 14ビット目から10ビットをuintで読む
                        if ((messageLength < 1024) && (cb.Count() >= messageLength + 3))     // 1024は規定，データ+CRC(3byte)に足りているか？
                        {
                            cb.BackToMark();
                            tmpSentence = cb.Read((int)messageLength + 3);  // preambleからCRCの手前まで
                            crcBytes = cb.Read(3);      //  CRC
                            crcValue = BitToUint(crcBytes, 0, 24);
                            uint calc_crc = Crc_Check.Crc24qCalc(tmpSentence);
                            //if (calc_crc != BitConverter.ToUInt32(new byte[] { 0x00, crc[2], crc[1], crc[0] }, 0))
                            if (calc_crc != crcValue)
                            {
                                Debug.WriteLine("RTCM CRC invalid");
                                continue;
                            }
                            byte_sentences.Add(tmpSentence);
                        }
                        else
                        {
                            cb.BackToMark();
                            return byte_sentences;      // プリアンブルを検出したが，データが足りない．
                        }
                    }
                }
                else
                {
                    cb.BackToMark();
                    tmpBytes = cb.Read(1);
                }
            }
            return byte_sentences;
        }
    }

    public class NewRtcmDataReceivedEventArgs : EventArgs
    {
        public List<IRawData>? NewRtcmDatas { get; set; }
    }

}
