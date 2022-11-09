using System.Diagnostics;
using System.Text;
using static GrToolBox.Data.CLAS3.ClasDefinitions;

namespace GrToolBox.Data.CLAS3
{
    public class ST08_STEC
    {
        public ST08Data Data { get; set; } = new ST08Data();
        public SubTypeStatus Status { get; set; } = SubTypeStatus.HeaderDone;
        private ST01_Mask? ST01 { get; set; }
        private int NBitRead { get; set; } = 0;      // 処理したビット数
       
        internal TraceSwitch TraceSwitch { get; set; } = new TraceSwitch("TSwitch_CLAS_ST08", "Clas Subtype08");

        public ST08_STEC()
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
            int nsat = st01.NSat();

            // Extended header 計 (nsat + 7) ビット
            if (bcb.Count() < 7 + nsat)
            {
                Status = SubTypeStatus.NeedMoreData;
                DebugDisp();
                return Status;
            }
            Data.StecType = (int)bcb.GetUint(2);
            Data.NetworkID = (int)bcb.GetUint(5);
            Data.NetSVMask = 0;
            for (int i = 0; i < nsat; i++)
            {
                if (bcb.GetBit())
                {
                    Data.NetSVMask |= (ulong)((ulong)0x_01 << i);
                }
            }
            NBitRead += (7 + nsat);

            int[] nStecBits = new int[4] {20, 44, 54, 70};
            int nstecbits = nStecBits[Data.StecType];           // number of stec data bits for 1 sat

            Data.Sat.Clear();
            for (int i =0; i < nsat; i++)
            {
                var sat = new ST08DataEachSat();
                if (((Data.NetSVMask >> i) & 0x_01) > 0)
                {
                    if (bcb.Count() < nstecbits)
                    {
                        Status = SubTypeStatus.NeedMoreData;
                        DebugDisp();
                        return Status;
                    }
                    sat.HaveData = true;
                    sat.StecQI_Class = (int)bcb.GetUint(3);
                    sat.StecQI_Value = (int)bcb.GetUint(3);
                    sat.C00 = bcb.GetInt(14) * 0.05;
                    if(Data.StecType > 0)
                    {
                        sat.C01 = bcb.GetInt(12) * 0.02;
                        sat.C10 = bcb.GetInt(12) * 0.02;
                    }
                    if(Data.StecType > 1)
                    {
                        sat.C11 = bcb.GetInt(10) * 0.02;
                    }
                    if(Data.StecType > 2)
                    {
                        sat.C02 = bcb.GetInt(8) * 0.005;
                        sat.C20 = bcb.GetInt(8) * 0.005;
                    }
                    NBitRead += nstecbits;
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
                    Debug.WriteLine($"Need more data, {NBitRead} bits read, number of grids read: {Data.Sat.Count}");
                    break;
                case SubTypeStatus.InvalidData:
                    Debug.WriteLine($"Invalid data, {NBitRead} bits read");
                    break;
                case SubTypeStatus.DataDone:
                    Debug.WriteLine($"{Data.MessageNumber:0000} type={Data.ID}, Decode done  ({NBitRead} bits read)");
                    Debug.WriteLine($"{Data.MessageNumber:0000} type={Data.ID}, additional header: netSVmask=0x_{Data.NetSVMask:x}, stecType={Data.StecType}, netID={Data.NetworkID}");
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
                            if (dat.HaveData)
                            {
                                sb.Append($"    sysid={sat.GnssID}  svn={sat.SvN:d2}, QI_class={dat.StecQI_Class}, QI_value={dat.StecQI_Value}, C00={dat.C00,5:0.00}, ");
                                if (Data.StecType > 0)
                                {
                                    sb.Append($"C01={dat.C01,5:0.00}, C10={dat.C10,5:0.00}, ");
                                }
                                if (Data.StecType > 1)
                                {
                                    sb.Append($"C11={dat.C11,5:0.00}, ");
                                }
                                if (Data.StecType > 2)
                                {
                                    sb.Append($"C02={dat.C02,5:0.000}, C20={dat.C20,5:0.000}, ");
                                }
                            }
                            else
                            {
                                sb.Append($"    sysid={sat.GnssID}  svn={sat.SvN:d2}, not recorded");
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


    public class ST08Data : ClasHeaderCommonData
    {
        // extended headers
        public int StecType { get; set; }           // Compact SSR STEC Correction Type
        public int NetworkID { get; set; }          // Compact Network ID
        public ulong NetSVMask { get; set; }        // Network SV Mask
        // data
        public List<ST08DataEachSat> Sat { get; set; } = new List<ST08DataEachSat>();
    }

    /// <summary>
    /// 
    /// </summary>
    public class ST08DataEachSat
    {
        public bool HaveData { get; set; } = false;
        public int StecQI_Class { get; set; } = 0;  // SSR STEC Quality Indicator; bits5-3
        public int StecQI_Value { get; set; } = 0;  // bits2-0
        public double C00 { get; set; } = 0.0;      // STEC Polynomial Coefficients C00
        public double C01 { get; set; } = 0.0;      // 
        public double C10 { get; set; } = 0.0;      // 
        public double C11 { get; set; } = 0.0;      // 
        public double C02 { get; set; } = 0.0;      // 
        public double C20 { get; set; } = 0.0;      // 
    }

}
