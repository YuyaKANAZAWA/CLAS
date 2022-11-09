using GrToolBox.Common;
using System.Diagnostics;
using static GrToolBox.Data.DataTypeDefinitions;
using static GrToolBox.Data.Nmea.NmeaUtilities;

namespace GrToolBox.Data.Nmea
{
    public class NmeaConverter
    {
        private CircularBuffer<byte> Cb { get; set; }
        private List<NmeaBurstData> bursts { get; set; } = new List<NmeaBurstData>();
        private int CountNewEpoch { get; set; } = 0;
        private Stopwatch Sw { get; set; } = new Stopwatch();
        private Timer NoNmeaTimer { get; set; }

        public NmeaConverter()
        {
            int BufSize_IndexPart = 18; // バッファサイズを2のべき乗に限る  2^12=4096byte
            Cb = new(BufSize_IndexPart);
            Sw.Start();
            NoNmeaTimer = new Timer(new TimerCallback(ThreadingTimerCallback));
            NoNmeaTimer.Change(0, 2000);
        }


        public event EventHandler? NoNmeaDataTimerReached;
        private void ThreadingTimerCallback(object? args)
        {
            if(Sw.ElapsedMilliseconds > 3000)
            {
                //Sw.Restart();
                NoNmeaDataTimerReached?.Invoke(this, EventArgs.Empty);
            }
        }


        public int AvailableBufferSize()
        {
            return Cb.NumWritable();
        }

        public void AddDataAndAnalysis(byte[] data)
        {
            Cb.Add(data);
            List<string> sentences = GetNmeaSentence(Cb);
            CountNewEpoch = GetNmeaBurst(sentences, bursts);
            if (CountNewEpoch > 0)
            {
                Sw.Restart();
                List<NmeaBurstData> newBursts = bursts.GetRange(bursts.Count - CountNewEpoch - 1, CountNewEpoch);
                NewNmeaDataReceivedEventArgs args = new NewNmeaDataReceivedEventArgs();
                //args.NewBursts = bursts;
                args.NewBursts = newBursts;
                OnNewNmeaDataReceived(args);
                CountNewEpoch = 0;
            }
        }


        protected virtual void OnNewNmeaDataReceived(NewNmeaDataReceivedEventArgs e)
        {
            EventHandler<NewNmeaDataReceivedEventArgs>? handler = NewNmeaDataReceived;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        public event EventHandler<NewNmeaDataReceivedEventArgs>? NewNmeaDataReceived;

        public class NewNmeaDataReceivedEventArgs : EventArgs
        {
            public List<NmeaBurstData> NewBursts { get; set; } = new List<NmeaBurstData>();
        }



        public static int GetNmeaBurst(List<string> sentences, List<NmeaBurstData> bursts)
        {
            int countNewEpoch = 0;
            //List<NmeaBurstData> bursts = new List<NmeaBurstData>();
            // エポック進行を検出したらListにNmeaBurstDataを新たに加えてデータを格納する．AddXxxToBurstの返り値がTrueならListの末尾から一つ手前の要素が格納完了した最新のエポックになる
            // エポック進行したらイベント発生させ，格納完了したBurstをリストにして通知する
            foreach (string sentence in sentences)
            {
                if (sentence.Contains("RMC"))
                {
                    var rmc = new NmeaRmc(sentence);
                    if (AddRmcToBurst(rmc, bursts))
                    {
                        //Console.WriteLine("New Epoch"); 
                        countNewEpoch++;
                    };
                }
                else if (sentence.Contains("GGA"))
                {
                    var gga = new NmeaGga(sentence);
                    if (AddGgaToBurst(gga, bursts))
                    {
                        //Console.WriteLine("New Epoch");
                        countNewEpoch++;
                    };
                }
            }
            return countNewEpoch;
        }


        public static List<string> GetNmeaSentence(CircularBuffer<byte> cb)
        {
            List<string> sentences = new List<string>();
            byte[] tmp;
            byte[] readbuf;
            byte[] byte_sentence;
            bool gotSentence = false;
            string nmea = "";
            while (cb.Count() > 10)
            {
                cb.SetMark();
                tmp = cb.Read(2);
                if (tmp[0] == 0x24 && tmp[1] == 0x47)        // $Gを探す
                //tmp = cb.Read(6);
                //if ((tmp[0] == 0x24 && tmp[1] == 0x47) 
                //    && (tmp[3] == 'G' && tmp[4] == 'G' && tmp[5] == 'A')
                //    || (tmp[3] == 'R' && tmp[4] == 'M' && tmp[5] == 'C'))        // $Gを探す
                {
                    int len = cb.Count();
                    len = (len < 256) ? len : 256;           // nmea 1センテンス196バイト以下を想定している．必要に応じて調整する必要あり
                    readbuf = cb.Read(len);

                    int i = 0;
                    int nByteFromMark = -1;
                    while (i < len - 1)
                    {
                        if (i > 4)
                        {
                            if (readbuf[i - 3] == 0x2a && readbuf[i] == 0x0d && readbuf[i + 1] == 0x0a)  // 2文字ずつで終端を探す 0x2a:"*", 0x0d:"\r(CR)", 0x0a:"\n(LF)"
                            {
                                nByteFromMark = 2 + i;
                                //nByteFromMark = 6 + i;
                                cb.BackToMark();
                                byte_sentence = cb.Read(nByteFromMark);
                                if (NmeaCScheck(byte_sentence))
                                {
                                    nmea = System.Text.Encoding.ASCII.GetString(byte_sentence);
                                    //Debug.WriteLine(nmea);
                                    sentences.Add(nmea);
                                    cb.Read(2);                       // センテンス末尾の改行(\r\n)を読み飛ばし
                                    gotSentence = true;
                                    break;
                                }
                                else
                                {
                                    Debug.WriteLine("Nmea CS check invalid");
                                }
                            }
                        }
                        i++;
                    }
                    if (!gotSentence && len < 256)   // $Gは見つけたが，バッファ内データが少なくて終端が無いときは次のデータを待つ
                    {
                        cb.BackToMark();
                        Debug.WriteLine("Back to nmea");
                        break;
                    }
                    else
                    {
                        gotSentence = false;
                    }
                }
                else
                {
                    cb.BackToMark();
                    tmp = cb.Read(1);
                }
            }
            return sentences;
        }


        public static bool AddRmcToBurst(Nmea.NmeaRmc rmc, List<NmeaBurstData> bursts)
        {
            if (rmc == null) throw new ArgumentNullException();
            if (bursts.Count == 0) bursts.Add(new NmeaBurstData() { Type = Nmea_Type.Burst });
            NmeaBurstData data = bursts.Last();
            bool newEpoch = false;
            if (data.Rmc != null) { newEpoch = true; } // RMC記録あり --> 新エポック

            // RMC記録無し
            //if (data.EpochTime.Year != 1 && data.EpochTime != rmc.Time) { newEpoch = true; }
            if (data.IsTimeValid && data.EpochTime != rmc.Time) { newEpoch = true; }
            if ((data.EpochTimeSpan.Hours == rmc.Time.Hour)
                && (data.EpochTimeSpan.Minutes == rmc.Time.Minute)
                && (data.EpochTimeSpan.Seconds == rmc.Time.Second)
                && (data.EpochTimeSpan.Milliseconds == rmc.Time.Millisecond)) { newEpoch = true; }

            if (newEpoch)
            {
                data = new NmeaBurstData() { Type = Nmea_Type.Burst };
                bursts.Add(data);
            }

            data.Rmc = rmc;
            return newEpoch;
        }


        public static bool AddGgaToBurst(Nmea.NmeaGga gga, List<NmeaBurstData> bursts)
        {
            if (gga == null) throw new ArgumentNullException();
            if (bursts.Count == 0) bursts.Add(new NmeaBurstData() { Type = Nmea_Type.Burst });
            NmeaBurstData data = bursts.Last();
            bool newEpoch = false;
            if (data.Gga != null) { newEpoch = true; } // GGA記録あり --> 新エポック

            // 
            if (data.EpochTimeSpan != gga.Time) { newEpoch = true; }

            if (newEpoch)
            {
                data = new NmeaBurstData() { Type = Nmea_Type.Burst };
                bursts.Add(data);
            }
            data.Gga = gga;
            return newEpoch;
        }


    }

    //public class NewNmeaDataReceivedEventArgs : EventArgs
    //{
    //    public List<NmeaBurstData> NewBursts { get; set; } = new List<NmeaBurstData>();
    //}

}
