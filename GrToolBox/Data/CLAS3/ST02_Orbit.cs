using System.Diagnostics;
using static GrToolBox.Common.CommonDefinitions;
using static GrToolBox.Data.CLAS3.ClasDefinitions;
using static GrToolBox.Data.DataUtilities;
using static GrToolBox.Data.CLAS3.ClasUtilities;

namespace GrToolBox.Data.CLAS3
{
    public class ST02_Orbit
    {
        public ST02Data Data { get; set; } = new ST02Data();
        public SubTypeStatus Status { get; set; } = SubTypeStatus.HeaderDone;

        private int NBitRead { get; set; } = 0;      // 処理したビット数
        private ST01_Mask? ST01 { get; set; }

        internal TraceSwitch TraceSwitch { get; set; } = new TraceSwitch("TSwitch_CLAS_ST02", "Clas Subtype02");


        public ST02_Orbit() 
        {
            TraceSwitch.Level = TraceLevel.Verbose;
        }

        private void AddHeaderData(ClasHeaderCommonData h)
        {
            Data.MessageNumber = h.MessageNumber;
            Data.ID = h.ID;
            Data.Time = h.Time;
            Data.UpdateInterval = h.UpdateInterval;
            Data.Multiple = h.Multiple;
            Data.IodSsr = h.IodSsr;
        }

        public SubTypeStatus Decode(BitCircularBuffer bcb, ClasHeaderCommonData h, ST01_Mask st01)
        {
            if (bcb == null || h == null || st01 == null)
            {
                throw new ArgumentNullException();
            }
            ST01 = st01;
            AddHeaderData(h);
            Status = SubTypeStatus.HeaderDone;
            int nSat = st01.NSat();
            int nGAL = st01.NSat(SYS.GAL);
            NBitRead = 0;
            Data.Sat.Clear();

            for(int i = 0; i < nSat; i++)
            {
                var s = new ST02DataEachSat();
                int nBitsIODE = (st01.Data.Sat[i].GnssID == SYS.GAL) ? 10 : 8;
                if (bcb.Count() < 41 + nBitsIODE)
                {
                    Status = SubTypeStatus.NeedMoreData;
                    DebugDisp();
                    return Status;
                }
                s.Iode = (int)bcb.GetUint(nBitsIODE);
                s.Dorb[0] = bcb.GetInt(15) * 0.0016;     // Delta Radial
                s.Dorb[1] = bcb.GetInt(13) * 0.0064;     // Delta Along Track
                s.Dorb[2] = bcb.GetInt(13) * 0.0064;     // Delta Cross Track
                NBitRead += (41 + nBitsIODE);
                Data.Sat.Add(s);
            }
            Status = SubTypeStatus.DataDone;
            DebugDisp();
            return Status;
        }



        private void DebugDisp()
        {
#if DEBUG
            switch (Status)
            {
                case SubTypeStatus.NeedMoreData:
                    Debug.WriteLine($"Need more data, {NBitRead} bits read");
                    break;
                case SubTypeStatus.InvalidData:
                    Debug.WriteLine($"Invalid data, {NBitRead} bits read");
                    break;
                case SubTypeStatus.DataDone:
                    Debug.WriteLine($"{Data.MessageNumber:0000} type={Data.ID}, Decode done  ({NBitRead} bits read)");
                    if (ST01 == null)
                    {
                        Debug.WriteLine("no mask data");
                        break;
                    }
                    int nsat = ST01.NSat();
                    if (Data.Sat.Count != nsat)
                    {
                        Debug.WriteLine("Data.Sat.Count != nsat");
                    }
                    else
                    {
                        for (int i = 0; i < nsat; i++)
                        {
                            var sat = ST01.Data.Sat[i];
                            var d = Data.Sat[i];
                            string s = "";
                            if (d.Dorb[0] == -26.2144 || d.Dorb[1] == -26.2144 || d.Dorb[2] == -26.2144) s = "(not available)";
                            Debug.WriteLine($"    sysid={sat.GnssID}  svn={sat.SvN:d2}  orb delta (ST2) Radial={d.Dorb[0],9:0.0000}  Along={d.Dorb[1],9:0.0000}  Cross={d.Dorb[2],9:0.0000} {s}");
                        }
                    }
                    break;
            }
#endif
        }
    }


    public class ST02Data : ClasHeaderCommonData
    {
        // no extended headers

        // data
        public List<ST02DataEachSat> Sat { get; set; } = new List<ST02DataEachSat>();
    }

    public class ST02DataEachSat
    {
        public int Iode { get; set; } = -1;
        public double[] Dorb { get; set; } = new double[3] {0.0, 0.0, 0.0};
    }


}


