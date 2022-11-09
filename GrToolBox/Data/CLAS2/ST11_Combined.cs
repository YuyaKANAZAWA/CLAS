using System.Diagnostics;
using System.Text;
using static GrToolBox.Common.CommonDefinitions;
using static GrToolBox.Data.CLAS2.ClasDefinitions;
using static GrToolBox.Data.DataUtilities;
using static GrToolBox.Data.CLAS2.ClasUtilities;

namespace GrToolBox.Data.CLAS2
{
    public class ST11_Combined : ClasHeaderCommonData
    {
        public SubTypeStatus Status { get; set; } = SubTypeStatus.HeaderDone;
        private bool OrbitExisting { get; set; } = false;    // Orbit Existing Flag
        private bool ClockExisting { get; set; } = false;    // Clock Existing Flag
        private bool NetworkCorrec { get; set; } = false;   // Network Correction
        private int NetworkID { get; set; }                 // Compact Network ID
        private ulong NetSVMask { get; set; }               // Compact Network SV Mask

        private int NBitRead { get; set; } = 0;      // 処理したビット数

        

        internal TraceSwitch TraceSwitch { get; set; } = new TraceSwitch("TSwitch_CLAS_ST5", "Clas Subtype5");


        public ST11_Combined(ClasHeaderCommonData h)
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
            // Orbit, Clock, NetworkCorr フラグ 計3ビット
            if (bcb.Count() < 3)
            {
                Status = SubTypeStatus.NeedMoreData;
                DebugDisp(ssr);
                return Status;
            }
            OrbitExisting = bcb.GetBit();
            ClockExisting = bcb.GetBit();
            NetworkCorrec = bcb.GetBit();
            NBitRead += 3;

            int nsat = GetNSat(ssr);
            if (NetworkCorrec)
            {
                if (bcb.Count() < 5)
                {
                    Status = SubTypeStatus.NeedMoreData;
                    DebugDisp(ssr);
                    return Status;
                }
                NetworkID = (int)bcb.GetUint(5);

                // nsatビット Network SV Mask
                if (bcb.Count() < nsat)
                {
                    Status = SubTypeStatus.NeedMoreData;
                    DebugDisp(ssr);
                    return Status;
                }
                NetSVMask = 0;
                for(int i = 0; i < nsat; i++)
                {
                    if (bcb.GetBit())
                    {
                        NetSVMask |= (ulong)((ulong)0x_01 << i);
                    }
                }
                NBitRead += nsat;
            }

            for(int i = 0; i < nsat; i++)
            {
                var d = ssr[i];
                d.CombCorr.OrbitExisting = OrbitExisting;
                d.CombCorr.ClockExisting = ClockExisting;
                d.CombCorr.NetworkCorrec = NetworkCorrec;                
                if(((NetSVMask >> i) & 0x_01) > 0)
                {
                    d.CombCorr.NetSVMaskThis = true;
                    if (OrbitExisting)
                    {
                        int nBitsIODE = (d.GnssID == SYS.GAL) ? 10 : 8;
                        if (bcb.Count() < (nBitsIODE + 41))
                        {
                            Status = SubTypeStatus.NeedMoreData;
                            DebugDisp(ssr);
                            return Status;
                        }
                        d.CombCorr.IODE = (int)bcb.GetUint(nBitsIODE);
                        d.CombCorr.Dorb[0] = bcb.GetInt(15) * 0.0016;
                        d.CombCorr.Dorb[1] = bcb.GetInt(13) * 0.0064;
                        d.CombCorr.Dorb[2] = bcb.GetInt(13) * 0.0064;
                        NBitRead += (nBitsIODE + 41);
                    }
                    if (ClockExisting)
                    {
                        if (bcb.Count() < 15)
                        {
                            Status = SubTypeStatus.NeedMoreData;
                            DebugDisp(ssr);
                            return Status;
                        }
                        d.CombCorr.Dcc0 = bcb.GetInt(15) * 0.0016;
                        NBitRead += 15;
                    }
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
                    Debug.WriteLine($"{MessageNumber:0000} type={ID}, additional header: netSVmask=0x_{NetSVMask:x}, orbit={OrbitExisting}, clock={ClockExisting}, network corr={NetworkCorrec}, netID={NetworkID}");
                    if (TraceSwitch.Level == TraceLevel.Verbose) 
                    {
                        StringBuilder sb = new StringBuilder();
                        foreach (var d in ssr)
                        {
                            sb.Append($"    sysid={d.GnssID}  svn={d.SvN:d2} CombCorr (ST11), ");
                            if (d.CombCorr.NetSVMaskThis)
                            {
                                if (OrbitExisting)
                                {
                                    sb.Append($"orb={d.CombCorr.Dorb[0],5:0.0000}, {d.CombCorr.Dorb[1],5:0.000}, {d.CombCorr.Dorb[2],5:0.0000}, ");
                                }
                                else
                                {
                                    sb.Append("orb not recorded, ");
                                }
                                if (ClockExisting)
                                {
                                    sb.Append($"dcc0={d.CombCorr.Dcc0,5:0.0000}");
                                }
                                else
                                {
                                    sb.Append("clk not recorded, ");
                                }
                                if(OrbitExisting || ClockExisting)
                                {

                                }
                                if (d.CombCorr.Dorb[0] == -26.2144 || d.CombCorr.Dorb[1] == -26.2144 || d.CombCorr.Dorb[2] == -26.2144 || d.CombCorr.Dcc0 == -26.2144)
                                {
                                    sb.Append(" (data not available) ");
                                }
                            }
                            else
                            {
                                sb.Append("not recorded");
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


