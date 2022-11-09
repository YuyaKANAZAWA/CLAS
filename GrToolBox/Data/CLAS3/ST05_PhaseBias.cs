using System.Diagnostics;
using System.Text;
using static GrToolBox.Data.CLAS3.ClasDefinitions;

namespace GrToolBox.Data.CLAS3
{
    public class ST05_PhaseBias
    {
        public ST05Data Data { get; set; } = new ST05Data();
        public SubTypeStatus Status { get; set; } = SubTypeStatus.HeaderDone;
        private ST01_Mask? ST01 { get; set; }
        private int NBitRead { get; set; } = 0;      // 処理したビット数

        internal TraceSwitch TraceSwitch { get; set; } = new TraceSwitch("TSwitch_CLAS_ST5", "Clas Subtype5");

        public ST05_PhaseBias()
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
            NBitRead = 0;

            Data.Sat.Clear();
            int nSat = st01.NSat();

            foreach(var sat in ST01.Data.Sat)
            {
                int nSig = sat.IndSigs.Count;
                var d = new ST05DataEachSat();
                for (int i = 0; i < nSig; i++)
                {
                    if (bcb.Count() < 17)
                    {
                        Status = SubTypeStatus.NeedMoreData;
                        DebugDisp();
                        return Status;
                    }
                    //var pseb = new C_PBias();
                    d.PBias.Add((double)(bcb.GetInt(15) * 16.383));
                    d.Pdi.Add((int)bcb.GetUint(2));
                    NBitRead += 17;
                }
                Data.Sat.Add(d);
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
                    Debug.WriteLine($"Need more data, {NBitRead} bits read, number of sats read: {Data.Sat.Count}");
                    break;
                case SubTypeStatus.InvalidData:
                    Debug.WriteLine($"Invalid data, {NBitRead} bits read");
                    break;
                case SubTypeStatus.DataDone:
                    Debug.WriteLine($"{Data.MessageNumber:0000} type={Data.ID}, Decode done  ({NBitRead} bits read)");
                    if(TraceSwitch.Level == TraceLevel.Verbose) 
                    {
                        if (ST01 == null)
                        {
                            Debug.WriteLine("no mask data");
                            break;
                        }
                        StringBuilder sb = new StringBuilder();
                        int nsat = ST01.NSat();
                        for(int i = 0; i < nsat; i++)
                        {
                            var sat = ST01.Data.Sat[i];
                            var d = Data.Sat[i];
                            sb.Append($"    sysid={sat.GnssID}  svn={sat.SvN:d2} ");
                            for(int j = 0; j < sat.IndSigs.Count; j++)
                            {
                                sb.Append($"sig_{sat.IndSigs[j],2:00}: {d.PBias[j],5:0.000}, {d.Pdi[j]}");
                                if (d.PBias[j] == -16.384)
                                {
                                    sb.Append("(not available)");
                                }
                                sb.Append(", ");
                            }
                            sb.Append("\n");
                        }
                        Debug.Write(sb.ToString());
                    }
                    break;
            }
#endif
        }

    }



    public class ST05Data : ClasHeaderCommonData
    {
        // no extended headers

        // data
        public List<ST05DataEachSat> Sat { get; set; } = new List<ST05DataEachSat>();
    }

    public class ST05DataEachSat
    {
        public List<double> PBias { get; set; } = new List<double>();
        public List<int> Pdi { get; set; } = new List<int>();
    }

}
