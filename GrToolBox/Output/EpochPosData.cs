using GrToolBox.Coordinates;
using GrToolBox.Data.CLAS3;
using GrToolBox.Data.Nmea;
using GrToolBox.Output;
using System.Text;
using static GrToolBox.Common.Constants;
using static GrToolBox.Time.TimeUtilities;

namespace GrToolBox.Output
{
    public class EpochPosData
    {
        public DateTime Time { get; set; }
        public double Lat { get; set; } = double.NaN;
        public double Lon { get; set; } = double.NaN;
        public double Alt { get; set; } = double.NaN;
        public double EllH { get; set; } = double.NaN;
        public double East { get; set; } = double.NaN;
        public double North { get; set; } = double.NaN;
        public double Up { get; set; } = double.NaN;
        public double Hdop { get; set; } = double.NaN;
        public double Vdop { get; set; } = double.NaN;
        public double[] RecCLK { get; set; }

        //public EpochPosData() { }

        public EpochPosData(NmeaBurstData nmea, PositionSetter? ps)
        {
            RecCLK = Enumerable.Repeat<double>(double.NaN, MAX_SYS).ToArray();
            //if (nmea != null && ps != null && ps.HaveEnuOrg)
            //if (nmea != null && ps != null)
            if (nmea != null)
            {
                TimeSpan leap = new TimeSpan(0, 0, GetLeapSecond(nmea.EpochTime));
                Time = nmea.EpochTime + leap;
                Lat = nmea.Lat;
                Lon = nmea.Lon;
                Alt = nmea.Alt;
                EllH = nmea.EllHeight;
                var pos = new Position();
                if(ps != null && ps.HaveEnuOrg)
                {
                    ps.SetLLH(pos, new double[] { nmea.Lat, nmea.Lon, nmea.EllHeight }, "deg");
                    East = pos.Enu[0];
                    North = pos.Enu[1];
                    Up = pos.Enu[2];
                }
            }
        }

        public EpochPosData(PosResultGR gr)
        {
            RecCLK = Enumerable.Repeat<double>(double.NaN, MAX_SYS).ToArray();
            if (gr != null)
            {
                Time = gr.TimeGr.DT;
                Lat = gr.Pos.Llh[0] * RAD2DEG;
                Lon = gr.Pos.Llh[1] * RAD2DEG;
                Alt = gr.Pos.Alt;
                EllH = gr.Pos.Llh[2];
                East = gr.Pos.Enu[0];
                North = gr.Pos.Enu[1];
                Up = gr.Pos.Enu[2];
                Array.Copy(gr.RecCLK, RecCLK, gr.RecCLK.Length);
            }
        }
    }

    /// <summary>
    /// エポック毎の測位結果を蓄積，取りあえず3つのデータソースを取り扱い
    /// </summary>
    public class EpochPosDataStore
    {
        private List<EpochPosData> Data1 { get; set; } = new List<EpochPosData>();
        private List<EpochPosData> Data2 { get; set; } = new List<EpochPosData>();
        private List<EpochPosData> Data3 { get; set; } = new List<EpochPosData>();
        private int NextInd1 { get; set; } = 0;
        private int NextInd2 { get; set; } = 0;
        private int NextInd3 { get; set; } = 0;
        private int Count { get; set; } = 0;
        private int NCalls { get; set; } = 2;

        /// <summary>
        /// エポック毎の測位結果を蓄積
        /// </summary>
        /// <param name="n">呼び出し上限回数．n回データがAddされたら表示できるデータをサーチする．データソース毎にAddするならデータソース数に設定するのが標準</param>
        public EpochPosDataStore(int n)
        {
            NCalls = n;
        }

        public EpochPosDataStore()
        {
            NCalls = 0;
        }

        /// <summary>
        /// エポックデータをAddする．n（コンストラクタで指定）回Addされたら，未処理データ内で最もエポック時刻の古いもので時刻を揃えたデータをイベント（OnNewEpochPosDataExist）で返す．
        /// </summary>
        /// <param name="d1">エポックデータ1</param>
        /// <param name="d2">エポックデータ1</param>
        /// <param name="d3">エポックデータ1</param>
        public void Add(EpochPosData? d1, EpochPosData? d2, EpochPosData? d3) 
        {
            bool notNull = false;
            if(d1 != null)
            {
                Data1.Add(d1);
                notNull = true;
            }
            if(d2 != null)
            {
                Data2.Add(d2);
                notNull = true;
            }
            if (d3 != null)
            {
                Data3.Add(d3);
                notNull = true;
            }
            if(notNull)
                Count++;
            if(Count == NCalls)
            {
                NewEpochPosDataExistEventArgs args = new NewEpochPosDataExistEventArgs();
                args.NewDatas = GetToBeDisped();
                OnNewEpochPosDataExist(args);
                Count = 0;
            }
        }

        /// <summary>
        /// 未処理のデータの内もっとも古い時刻に揃えたデータをサーチしリストにして返す．当該時刻のデータが無いものについてはnullが格納される
        /// </summary>
        /// <returns></returns>
        private List<EpochPosData?> GetToBeDisped()
        {
            List<EpochPosData> cand = new List<EpochPosData>();

            if(NextInd1 < Data1.Count)
            {
                cand.Add(Data1[NextInd1]);
            }
            if(NextInd2 < Data2.Count)
            {
                cand.Add(Data2[NextInd2]);
            }
            if (NextInd3 < Data3.Count)
            {
                cand.Add(Data3[NextInd3]);
            }

            DateTime cand_time;
            EpochPosData? d1 = null;
            EpochPosData? d2 = null;
            EpochPosData? d3 = null;
            List<EpochPosData?> datas = new List<EpochPosData?>();

            if (cand.Count > 0)
            {
                cand_time = cand.Min(x => x.Time);
                if (NextInd1 < Data1.Count)
                {
                    for(int i = Data1.Count - 1; i >= NextInd1; i--)
                    {
                        if (Data1[i] != null && Data1[i].Time == cand_time)
                        {
                            d1 = Data1[i];
                            NextInd1 = i + 1;
                            break;
                        }
                        else
                        {
                            continue;
                        }
                    }
                }
                datas.Add(d1);

                if (NextInd2 < Data2.Count)
                {
                    for (int i = Data2.Count - 1; i >= NextInd2; i--)
                    {
                        if (Data2[i] != null && Data2[i].Time == cand_time)
                        {
                            d2 = Data2[i];
                            NextInd2 = i + 1;
                            break;
                        }
                        else
                        {
                            continue;
                        }
                    }
                }
                datas.Add(d2);

                if (NextInd3 < Data3.Count)
                {
                    for (int i = Data3.Count - 1; i >= NextInd3; i--)
                    {
                        if (Data3[i] != null && Data3[i].Time == cand_time)
                        {
                            d3 = Data3[i];
                            NextInd3 = i + 1;
                            break;
                        }
                        else
                        {
                            continue;
                        }
                    }
                }
                datas.Add(d3);
            }
            return datas;
        }

        public class NewEpochPosDataExistEventArgs : EventArgs
        {
            public List<EpochPosData?>? NewDatas { get; set; }
        }

        public event EventHandler<NewEpochPosDataExistEventArgs>? NewEpochPosDataExist;

        protected virtual void OnNewEpochPosDataExist(NewEpochPosDataExistEventArgs e)
        {
            EventHandler<NewEpochPosDataExistEventArgs>? handler = NewEpochPosDataExist;
            if (handler != null)
            {
                handler(this, e);
            }
        }



        public void Add_Post(EpochPosData? d1, EpochPosData? d2, EpochPosData? d3)
        {
            if (d1 != null) Data1.Add(d1);
            if (d2 != null) Data2.Add(d2);
            if (d3 != null) Data3.Add(d3);
        }

        public void Add_Post(List<EpochPosData>? d1, List<EpochPosData>? d2, List<EpochPosData>? d3)
        {
            if (d1 != null) Data1.AddRange(d1);
            if (d2 != null) Data2.AddRange(d2);
            if (d3 != null) Data3.AddRange(d3);
        }

        public void CsvOut(int num, string path)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Time,Lat,Lon,Alt,EllH,East,North,Up");
            List<EpochPosData> Data;
            if (num == 1)
            {
                Data = Data1;
            }
            else if(num == 2)
            {
                Data = Data2;
            }
            else if(num == 3)
            {
                Data = Data3;
            }

            foreach (EpochPosData e in Data1)
            {
                if (e != null)
                {
                    sb.AppendLine($"{e.Time.ToString("G")},{e.Lat:.000000000},{e.Lon:.000000000},{e.Alt:.000},{e.EllH:.000},{e.East:.000},{e.North:.000},{e.Up:.000}");
                }
            }
            File.WriteAllText(path, sb.ToString());

        }



    }
}
