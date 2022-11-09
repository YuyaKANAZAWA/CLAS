using System.Diagnostics;
using System.Text;
using static GrToolBox.Common.CommonDefinitions;
using static GrToolBox.Data.CLAS3.ClasDefinitions;

namespace GrToolBox.Data.CLAS3
{
    public class ST11_Combined
    {
        public ST11Data Data { get; set; } = new ST11Data();
        public SubTypeStatus Status { get; set; } = SubTypeStatus.HeaderDone;
        private ST01_Mask? ST01 { get; set; }
        private int NBitRead { get; set; } = 0;      // 処理したビット数
       
        internal TraceSwitch TraceSwitch { get; set; } = new TraceSwitch("TSwitch_CLAS_ST11", "Clas Subtype11");

        public ST11_Combined()
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

            // Orbit, Clock, NetworkCorr フラグ 計3ビット
            if (bcb.Count() < 3)
            {
                Status = SubTypeStatus.NeedMoreData;
                DebugDisp();
                return Status;
            }
            Data.OrbitFlag = bcb.GetBit();
            Data.ClockFlag = bcb.GetBit();
            Data.NetCorrFlag = bcb.GetBit();
            NBitRead += 3;

            int nsat = st01.NSat();
            if (Data.NetCorrFlag)
            {
                if (bcb.Count() < 5)
                {
                    Status = SubTypeStatus.NeedMoreData;
                    DebugDisp();
                    return Status;
                }
                Data.NetworkID = (int)bcb.GetUint(5);

                // nsatビット Network SV Mask
                if (bcb.Count() < nsat)
                {
                    Status = SubTypeStatus.NeedMoreData;
                    DebugDisp();
                    return Status;
                }
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

            for(int i = 0; i < nsat; i++)
            {
                var st0sat = ST01.Data.Sat[i];
                var d = new ST11DataEachSat();
                if(((Data.NetSVMask >> i) & 0x_01) > 0)
                {
                    //d.CombCorr.NetSVMaskThis = true;
                    d.HaveData = true;
                    if (Data.OrbitFlag)
                    {
                        int nBitsIODE = (st0sat.GnssID == SYS.GAL) ? 10 : 8;
                        if (bcb.Count() < (nBitsIODE + 41))
                        {
                            Status = SubTypeStatus.NeedMoreData;
                            DebugDisp();
                            return Status;
                        }
                        d.IODE = (int)bcb.GetUint(nBitsIODE);
                        d.Dorb[0] = bcb.GetInt(15) * 0.0016;
                        d.Dorb[1] = bcb.GetInt(13) * 0.0064;
                        d.Dorb[2] = bcb.GetInt(13) * 0.0064;
                        NBitRead += (nBitsIODE + 41);
                    }
                    if (Data.ClockFlag)
                    {
                        if (bcb.Count() < 15)
                        {
                            Status = SubTypeStatus.NeedMoreData;
                            DebugDisp();
                            return Status;
                        }
                        d.Dcc0 = bcb.GetInt(15) * 0.0016;
                        NBitRead += 15;
                    }
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
                    Debug.WriteLine($"{Data.MessageNumber:0000} type={Data.ID}, additional header: netSVmask=0x_{Data.NetSVMask:x}, orbit={Data.OrbitFlag}, clock={Data.ClockFlag}, network corr={Data.NetCorrFlag}, netID={Data.NetworkID}");
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
                            sb.Append($"    sysid={sat.GnssID}  svn={sat.SvN:d2}, ");
                            if (dat.HaveData)
                            {
                                if (Data.OrbitFlag)
                                {
                                    sb.Append($"orb={dat.Dorb[0],5:0.0000}, {dat.Dorb[1],5:0.000}, {dat.Dorb[2],5:0.0000}, ");
                                }
                                else
                                {
                                    sb.Append("orb not recorded, ");
                                }
                                if (Data.ClockFlag)
                                {
                                    sb.Append($"dcc0={dat.Dcc0,5:0.0000}");
                                }
                                else
                                {
                                    sb.Append("clk not recorded, ");
                                }
                                if (Data.OrbitFlag || Data.ClockFlag)
                                {

                                }
                                if (dat.Dorb[0] == -26.2144 || dat.Dorb[1] == -26.2144 || dat.Dorb[2] == -26.2144 || dat.Dcc0 == -26.2144)
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


    public class ST11Data : ClasHeaderCommonData
    {
        // extended headers
        public bool OrbitFlag { get; set; }         // Orbit Existing Flag
        public bool ClockFlag { get; set; }         // Clock Existing Flag
        public bool NetCorrFlag { get; set; }       // Network Correction
        public int NetworkID { get; set; }          // Compact Network ID
        public ulong NetSVMask { get; set; }        // Network SV Mask

        // data
        public List<ST11DataEachSat> Sat { get; set; } = new List<ST11DataEachSat>();
    }

    public class ST11DataEachSat
    {
        public int IODE { get; set; } = 0;
        public double[] Dorb { get; set; } = new double[3] {0.0, 0.0, 0.0};
        public double Dcc0 { get; set; } = 0.0;
        public bool HaveData { get; set; } = false;     // Network SV Mask bit data for this satellite
    }

}
