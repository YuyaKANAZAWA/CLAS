using System.Diagnostics;
using System.Text;
using static GrToolBox.Common.CommonDefinitions;
using static GrToolBox.Data.CLAS2.ClasDefinitions;
using static GrToolBox.Data.DataUtilities;
using static GrToolBox.Data.CLAS2.ClasUtilities;

namespace GrToolBox.Data.CLAS2
{
    public class ST06_CodePhaseBias : ClasHeaderCommonData
    {
        public SubTypeStatus Status { get; set; } = SubTypeStatus.HeaderDone;
        private bool CodeBiasExisting { get; set; } = false;        // Code bias Existing Flag
        private bool PhaseBiasExisting { get; set; } = false;       // Phase bias Existing Flag
        private bool NetworkCorrec { get; set; } = false;           // Network bias correction
        private int NetworkID { get; set; }                         // Compact Network ID
        private ulong NetSVMask { get; set; }                       // Compact Network SV Mask

        private int NBitRead { get; set; } = 0;      // 処理したビット数

        

        internal TraceSwitch TraceSwitch { get; set; } = new TraceSwitch("TSwitch_CLAS_ST5", "Clas Subtype5");


        public ST06_CodePhaseBias(ClasHeaderCommonData h)
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
            // Code, Phase, NetworkCorr フラグ 計3ビット
            if (bcb.Count() < 3)
            {
                Status = SubTypeStatus.NeedMoreData;
                DebugDisp(ssr);
                return Status;
            }
            CodeBiasExisting = bcb.GetBit();
            PhaseBiasExisting = bcb.GetBit();
            NetworkCorrec = bcb.GetBit();
            NBitRead += 3;


            int nsat = GetNSat(ssr);

            if (NetworkCorrec)
            {
                // 5bit:networkID, nsat(bit):mask
                if (bcb.Count() < (5 + nsat))
                {
                    Status = SubTypeStatus.NeedMoreData;
                    DebugDisp(ssr);
                    return Status;
                }
                NetworkID = (int)bcb.GetUint(5);
                NBitRead += 5;

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
                var satbias = new SatCPBias();

                satbias.CodeBiasExisting = CodeBiasExisting;
                satbias.PhaseBiasExisting = PhaseBiasExisting;
                satbias.NetworkCorrec = NetworkCorrec;
                satbias.NetworkID = NetworkID;
                if(((NetSVMask >> i) & 0x_01) > 0)
                {
                    satbias.NetSVMaskThis = true;

                    int nsig = d.IndSigs.Count;
                    for(int j = 0; j < nsig; j++)
                    {
                        var bias = new C_PBias();
                        if (CodeBiasExisting)
                        {
                            if (bcb.Count() < 11)
                            {
                                Status = SubTypeStatus.NeedMoreData;
                                DebugDisp(ssr);
                                return Status;
                            }
                            bias.CBias = bcb.GetInt(11) * 0.02;
                            NBitRead += 11;
                        }
                        if (PhaseBiasExisting)
                        {
                            if (bcb.Count() < 17)
                            {
                                Status = SubTypeStatus.NeedMoreData;
                                DebugDisp(ssr);
                                return Status;
                            }
                            bias.PBias = bcb.GetInt(15) * 0.001;
                            bias.Pdi = (int)bcb.GetUint(2);
                            NBitRead += 17;
                        }
                        satbias.CPBIas.Add(bias);
                    }
                }
                d.SatCPBias = satbias;      // 1衛星分読み込み完了した時点で，その衛星のデータを上書きする
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
                    Debug.WriteLine($"{MessageNumber:0000} type={ID}, additional header: netSVmask=0x_{NetSVMask:x}, codebias={CodeBiasExisting}, phasebias={PhaseBiasExisting}, network corr={NetworkCorrec}, netID={NetworkID}");
                    if (TraceSwitch.Level == TraceLevel.Verbose) 
                    {
                        StringBuilder sb = new StringBuilder();
                        foreach (var d in ssr)
                        {
                            int nsig = d.IndSigs.Count;
                            bool notAvailable = false;
                            sb.Append($"    sysid={d.GnssID}  svn={d.SvN:d2} CombCorr (ST11), ");
                            if (d.SatCPBias.NetSVMaskThis)
                            {
                                if (CodeBiasExisting)
                                {
                                    for(int i = 0; i < nsig; i++)
                                    {
                                        var cbias = d.SatCPBias.CPBIas[i].CBias;
                                        if(cbias == -20.48) notAvailable = true;
                                        sb.Append($"cb_{00:i}={cbias, 5:0.0000}, ");
                                    }
                                }
                                else
                                {
                                    sb.Append("code bias not recorded, ");
                                }
                                if (PhaseBiasExisting)
                                {
                                    for (int i = 0; i < nsig; i++)
                                    {
                                        var pbias = d.SatCPBias.CPBIas[i].PBias;
                                        if (pbias == -16.384) notAvailable = true;
                                        sb.Append($"pb_{i,2:00}={pbias,7:0.0000} (pdi={d.SatCPBias.CPBIas[i].Pdi:d2}), ");
                                    }
                                }
                                else
                                {
                                    sb.Append("phase bias not recorded, ");
                                }
                                if (notAvailable)
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


