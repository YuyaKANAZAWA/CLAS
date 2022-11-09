using System.Diagnostics;
using static GrToolBox.Data.CLAS3.ClasDefinitions;

namespace GrToolBox.Data.CLAS3
{
    public class ST03_Clock
    {
        public ST03Data Data { get; set; } = new ST03Data();
        public SubTypeStatus Status { get; set; } = SubTypeStatus.HeaderDone;
        private ST01_Mask? ST01 { get; set; }
        private int NBitRead { get; set; } = 0;      // 処理したビット数

        internal TraceSwitch TraceSwitch { get; set; } = new TraceSwitch("TSwitch_CLAS_ST03", "Clas Subtype03");

        public ST03_Clock() 
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
            AddHeaderData(h);
            Status = SubTypeStatus.HeaderDone;
            ST01 = st01;
            int nSat = st01.NSat();

            Data.Dcc0.Clear();
            NBitRead = 0;

            for (int i = 0; i < nSat; i++)
            {
                if (bcb.Count() < 15)
                {
                    Status = SubTypeStatus.NeedMoreData;
                    return Status;
                }
                double dcc0 = bcb.GetInt(15) * 0.0016;
                Data.Dcc0.Add(dcc0);
                NBitRead += 15;
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
                    Debug.WriteLine($"Need more data, {NBitRead} bits read, number of Dcc0 read: {Data.Dcc0.Count}");
                    break;
                case SubTypeStatus.InvalidData:
                    Debug.WriteLine($"Invalid data, {NBitRead} bits read");
                    break;
                case SubTypeStatus.DataDone:
                    Debug.WriteLine($"{Data.MessageNumber:0000} type={Data.ID}, Decode done  ({NBitRead} bits read");
                    if(ST01 == null)
                    {
                        Debug.WriteLine("no mask data");
                        break;
                    }
                    int nsat = ST01.NSat();
                    if (Data.Dcc0.Count != nsat)
                    {
                        Debug.WriteLine("Data.Dcc0.Count != nsat");
                    }
                    else
                    {
                        for (int i = 0; i < nsat; i++)
                        {
                            var sat = ST01.Data.Sat[i];
                            var d = Data.Dcc0[i];
                            string s = "";
                            if (Data.Dcc0[i] == -26.2144) s = "(not available)";
                            Debug.WriteLine($"    sysid={sat.GnssID}  svn={sat.SvN:d2}  dcc0 (ST3)={d, 9:0.0000} {s}");
                        }
                    }
                    break;
            }
#endif
        }

        public class ST03Data : ClasHeaderCommonData
        {
            // no extended headers

            // data
            public List<double> Dcc0 { get; set; } = new List<double>();
        }

    }

}
