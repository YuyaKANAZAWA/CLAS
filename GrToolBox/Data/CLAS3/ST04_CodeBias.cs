using System.Diagnostics;
using System.Text;
using static GrToolBox.Data.CLAS3.ClasDefinitions;

namespace GrToolBox.Data.CLAS3
{
    public class ST04_CodeBias
    {
        public ST04Data Data { get; set; } = new ST04Data();
        public SubTypeStatus Status { get; set; } = SubTypeStatus.HeaderDone;
        private ST01_Mask? ST01 { get; set; }
        private int NBitRead { get; set; } = 0;      // 処理したビット数
        internal TraceSwitch TraceSwitch { get; set; } = new TraceSwitch("TSwitch_CLAS_ST4", "Clas Subtype4");

        public ST04_CodeBias()
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
            Data.Sat.Clear();   // 退避させるときは，退避してnewする

            int nSat = st01.NSat();
            for(int i = 0; i < nSat; i++)
            {
                int nsig = st01.Data.Sat[i].IndSigs.Count;
                if (bcb.Count() < (11 * nsig))
                {
                    Status = SubTypeStatus.NeedMoreData;
                    DebugDisp();
                    return Status;
                }
                var sat = new ST04DataEachSat();
                for (int j = 0; j < nsig; j++)
                {
                    double codbias = bcb.GetInt(11) * 0.02;
                    sat.Codb.Add(codbias);
                    NBitRead += 11;
                }
                Data.Sat.Add(sat);
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
                            for (int j = 0; j < sat.IndSigs.Count; j++)
                            {
                                sb.Append($"sig_{sat.IndSigs[j],2:00}: {d.Codb[j],5:0.00}");
                                if (d.Codb[j] == -20.48)
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

    public class ST04Data : ClasHeaderCommonData
    {
        // no extended headers

        // data
        public List<ST04DataEachSat> Sat { get; set; } = new List<ST04DataEachSat>();
    }

    public class ST04DataEachSat
    {
        public List<double> Codb { get; set; } = new List<double>();
    }

}


