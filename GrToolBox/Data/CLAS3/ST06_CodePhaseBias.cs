using System.Diagnostics;
using System.Text;
using static GrToolBox.Data.CLAS3.ClasDefinitions;

namespace GrToolBox.Data.CLAS3
{
    public class ST06_CodePhaseBias
    {
        public ST06Data Data { get; set; } = new ST06Data();
        public SubTypeStatus Status { get; set; } = SubTypeStatus.HeaderDone;
        private ST01_Mask? ST01 { get; set; }
        private int NBitRead { get; set; } = 0;      // 処理したビット数

        internal TraceSwitch TraceSwitch { get; set; } = new TraceSwitch("TSwitch_CLAS_ST5", "Clas Subtype5");

        public ST06_CodePhaseBias()
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

            // Code, Phase, NetworkCorr フラグ 計3ビット
            if (bcb.Count() < 3)
            {
                Status = SubTypeStatus.NeedMoreData;
                DebugDisp();
                return Status;
            }
            Data.CodeFlag = bcb.GetBit();
            Data.PhaseFlag = bcb.GetBit();
            Data.NetCorrFlag = bcb.GetBit();
            NBitRead += 3;

            int nsat = st01.NSat();

            if (Data.NetCorrFlag)
            {
                // 5bit:networkID, nsat(bit):mask
                if (bcb.Count() < (5 + nsat))
                {
                    Status = SubTypeStatus.NeedMoreData;
                    DebugDisp();
                    return Status;
                }
                Data.NetworkID = (int)bcb.GetUint(5);
                NBitRead += 5;

                Data.NetSVMask = 0;
                for(int i = 0; i < nsat; i++)
                {
                    if (bcb.GetBit())
                    {
                        Data.NetSVMask |= (ulong)((ulong)0x_01 << i);
                    }
                }
                NBitRead += nsat;
            }

            for (int i = 0; i < nsat; i++)
            {
                int nsig = st01.Data.Sat[i].IndSigs.Count;
                var dat = new ST06DataEachSat();
                if (((Data.NetSVMask >> i) & 0x_01) > 0)
                {
                    dat.HaveData = true;

                    for (int j = 0; j < nsig; j++)
                    {
                        //var bias = new C_PBias();
                        if (Data.CodeFlag)
                        {
                            if (bcb.Count() < 11)
                            {
                                Status = SubTypeStatus.NeedMoreData;
                                DebugDisp();
                                Data.Sat.Add(dat);      // 読み込んだものは途中まででも残す
                                return Status;
                            }
                            dat.CodeB.Add(bcb.GetInt(11) * 0.02);
                            NBitRead += 11;
                        }
                        if (Data.PhaseFlag)
                        {
                            if (bcb.Count() < 17)
                            {
                                Status = SubTypeStatus.NeedMoreData;
                                DebugDisp();
                                Data.Sat.Add(dat);        // 読み込んだものは途中まででも残す
                                return Status;
                            }
                            dat.PhaseB.Add(bcb.GetInt(15) * 0.001);
                            dat.Pdi.Add((int)bcb.GetUint(2));
                            NBitRead += 17;
                        }
                    }
                }
                Data.Sat.Add(dat);  // 1衛星分読み込み完了
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
                    Debug.WriteLine($"{Data.MessageNumber:0000} type={Data.ID}, additional header: netSVmask=0x_{Data.NetSVMask:x}, codebias={Data.CodeFlag}, phasebias={Data.PhaseFlag}, network corr={Data.NetCorrFlag}, netID={Data.NetworkID}");
                    if (TraceSwitch.Level == TraceLevel.Verbose) 
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
                            var dat = Data.Sat[i];
                            int nsig = sat.IndSigs.Count;
                            bool notAvailable = false;
                            sb.Append($"    sysid={sat.GnssID}  svn={sat.SvN:d2}, ");
                            if (dat.HaveData)
                            {
                                if (Data.CodeFlag)
                                {
                                    for (int j = 0; j < nsig; j++)
                                    {
                                        var cbias = dat.CodeB[j];
                                        if (cbias == -20.48) notAvailable = true;
                                        sb.Append($"cb_{sat.IndSigs[j],2:00}={cbias,5:0.0000}, ");
                                    }
                                }
                                else
                                {
                                    sb.Append("code bias not recorded, ");
                                }
                                if (Data.PhaseFlag)
                                {
                                    for (int j = 0; j < nsig; j++)
                                    {
                                        var pbias = dat.PhaseB[j];
                                        if (pbias == -16.384) notAvailable = true;
                                        sb.Append($"pb_{sat.IndSigs[j],2:00}={pbias,7:0.0000} (pdi={dat.Pdi[j]:d2}), ");
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


    public class ST06Data : ClasHeaderCommonData
    {
        // extended headers
        public bool CodeFlag { get; set; }         // Code Bias Existing Flag
        public bool PhaseFlag { get; set; }         // Clock Existing Flag
        public bool NetCorrFlag { get; set; }       // Network Bias Correction
        public int NetworkID { get; set; }          // Compact Network ID
        public ulong NetSVMask { get; set; }        // Network SV Mask

        // data
        public List<ST06DataEachSat> Sat { get; set; } = new List<ST06DataEachSat>();   // st01のsatellite maskの衛星全部
    }

    public class ST06DataEachSat
    {
        public List<double> CodeB { get; set; } = new List<double>();
        public List<double> PhaseB { get; set; } = new List<double>();
        public List<int> Pdi { get; set; } = new List<int>();
        public bool HaveData { get; set; } = false;     // Network SV Mask bit data for this satellite
    }

}
