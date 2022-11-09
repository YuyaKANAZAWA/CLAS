using System.Diagnostics;
using System.Text;
using static GrToolBox.Common.CommonDefinitions;
using static GrToolBox.Data.CLAS2.ClasDefinitions;
using static GrToolBox.Data.DataUtilities;
using static GrToolBox.Data.CLAS2.ClasUtilities;

namespace GrToolBox.Data.CLAS2
{
    public class ST04_CodeBias : ClasHeaderCommonData
    {
        public SubTypeStatus Status { get; set; } = SubTypeStatus.HeaderDone;
        //public int Next { get; set; } = 0;   // BitArray中の位置（次に読み出すビット位置インデックス）

        //private GrBitArray BitArray { get; set; } = new GrBitArray();
        private int NBitRead { get; set; } = 0;      // 処理したビット数

        internal TraceSwitch TraceSwitch { get; set; } = new TraceSwitch("TSwitch_CLAS_ST4", "Clas Subtype4");


        public ST04_CodeBias(ClasHeaderCommonData h)
        {
            TraceSwitch.Level = TraceLevel.Verbose;
            if (h != null)
            {
                AddHeaderData(h);
            }
        }

        private void AddHeaderData(ClasHeaderCommonData h)
        {
            MessageNumber = h.MessageNumber;
            ID = h.ID;
            Time = h.Time;
            UpdateInterval = h.UpdateInterval;
            Multiple = h.Multiple;
            IodSsr = h.IodSsr;
        }


        public SubTypeStatus Decode(BitCircularBuffer bcb, List<ClasSSRData> ssr)
        {
            NBitRead = 0;
            foreach (var d in ssr)
            {
                int nsig = d.IndSigs.Count;
                if(bcb.Count() < (11 * nsig))
                {
                    Status = SubTypeStatus.NeedMoreData;
                    DebugDisp(ssr);
                    return Status;
                }
                d.Codb.Clear();
                for (int i = 0; i < nsig; i++)
                {
                    double codbias = bcb.GetInt(11) * 0.02;
                    d.Codb.Add(codbias);
                    NBitRead += 11;
                }
            }
            Status = SubTypeStatus.DataDone;
            DebugDisp(ssr);
            return Status;
        }


        private void DebugDisp(List<ClasSSRData> ssr)
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
                    Debug.WriteLine($"{MessageNumber:0000} type={ID}, Decode done  ({NBitRead} bits read)");
                    if(TraceSwitch.Level == TraceLevel.Verbose) 
                    {
                        StringBuilder sb = new StringBuilder();
                        foreach (var d in ssr)
                        {
                            sb.Append($"    sysid={d.GnssID}  svn={d.SvN:d2}  code bias (ST4) ");
                            for (int i = 0; i < d.IndSigs.Count; i++)
                            {
                                sb.Append($"sig_{d.IndSigs[i],2:00}: {d.Codb[i],5:0.00}");
                                if (d.Codb[i] == -20.48)
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

}


