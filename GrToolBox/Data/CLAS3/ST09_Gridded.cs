using System.Diagnostics;
using System.Text;
using static GrToolBox.Data.CLAS3.ClasDefinitions;

namespace GrToolBox.Data.CLAS3
{
    public class ST09_Gridded
    {
        public ST09Data Data { get; set; } = new ST09Data();
        public SubTypeStatus Status { get; set; } = SubTypeStatus.HeaderDone;
        private ST01_Mask? ST01 { get; set; }
        private int NBitRead { get; set; } = 0;      // 処理したビット数
       
        internal TraceSwitch TraceSwitch { get; set; } = new TraceSwitch("TSwitch_CLAS_ST9", "Clas Subtype9");

        public ST09_Gridded()
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

            // Extended header 計 (nsat + 20) ビット
            if (bcb.Count() < 20 + nsat)
            {
                Status = SubTypeStatus.NeedMoreData;
                DebugDisp();
                return Status;
            }
            Data.TropType = (int)bcb.GetUint(2);
            Data.StecNbitsFlag = bcb.GetBit();
            Data.NetworkID = (int)bcb.GetUint(5);
            Data.NetSVMask = 0;
            for (int i = 0; i < nsat; i++)
            {
                if (bcb.GetBit())
                {
                    Data.NetSVMask |= (ulong)((ulong)0x_01 << i);
                }
            }
            Data.TropQI_Class = (int)bcb.GetUint(3);
            Data.TropQI_Value = (int)bcb.GetUint(3);
            Data.NGrids = (int)bcb.GetUint(6);
            NBitRead += (20 + nsat);

            Data.Grid.Clear();
            int nStecBits = (Data.StecNbitsFlag) ? 16 : 7;
            for(int i = 0; i < Data.NGrids; i++)
            {
                if (bcb.Count() < 17)
                {
                    Status = SubTypeStatus.NeedMoreData;
                    DebugDisp();
                    return Status;
                }
                var grid = new ST09DataEachGrid();
                grid.THD = bcb.GetInt(9) * 0.004;    // Tropospheric Hydro Static Vertical Delay [m]; -1.024: not available
                grid.TWD = bcb.GetInt(8) * 0.004;    // Tropospheric Wet Vertical Delay [m]; -0.512: not available
                NBitRead += 17;
                for (int j = 0; j < nsat; j++)
                {
                    var sat = new ST09DataEachSat();
                    if (((Data.NetSVMask >> j) & 0x_01) > 0)
                    {
                        if (bcb.Count() < nStecBits)
                        {
                            Status = SubTypeStatus.NeedMoreData;
                            DebugDisp();
                            return Status;
                        }
                        sat.StecRC = bcb.GetInt(nStecBits) * 0.04;   // STEC Residual Correction [TECU]; -2.56 or -1310.72: not available
                        sat.HaveData = true;
                        NBitRead += nStecBits;
                    }
                    grid.Sat.Add(sat);
                }
                Data.Grid.Add(grid);
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
                    Debug.WriteLine($"Need more data, {NBitRead} bits read, number of grids read: {Data.Grid.Count}");
                    break;
                case SubTypeStatus.InvalidData:
                    Debug.WriteLine($"Invalid data, {NBitRead} bits read");
                    break;
                case SubTypeStatus.DataDone:
                    Debug.WriteLine($"{Data.MessageNumber:0000} type={Data.ID}, Decode done  ({NBitRead} bits read)");
                    int nbits = (Data.StecNbitsFlag) ? 16 : 7;
                    Debug.WriteLine($"{Data.MessageNumber:0000} type={Data.ID}, additional header: netSVmask=0x_{Data.NetSVMask:x}, tropType={Data.TropType}, netID={Data.NetworkID}, STEC bits={nbits}, QI(class,value)=({Data.TropQI_Class},{Data.TropQI_Value}), nGrids={Data.NGrids}");
                    if (TraceSwitch.Level == TraceLevel.Verbose) 
                    {
                        if (ST01 == null)
                        {
                            Debug.WriteLine("no mask data");
                            break;
                        }
                        StringBuilder sb = new StringBuilder();
                        int ngrid = Data.NGrids;
                        int nsat = ST01.NSat();
                        for(int i = 0; i < ngrid; i++)
                        {
                            var grid = Data.Grid[i];
                            sb.Append($"    grid({i:00})  Trop  THD={grid.THD,5:0.00}, TWD={grid.TWD,5:0.000} \n");
                            sb.Append($"              STEC  ");
                            for(int j = 0; j < nsat; j++)
                            {
                                var st01sat = ST01.Data.Sat[j];
                                var sat = grid.Sat[j];
                                if (sat.HaveData)
                                {
                                    sb.Append($"({st01sat.GnssID},{st01sat.SvN})={sat.StecRC,5:0.00}, ");
                                }
                                else
                                {
                                    sb.Append($"({st01sat.GnssID},{st01sat.SvN})= no data, ");
                                }
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


    public class ST09Data : ClasHeaderCommonData
    {
        // extended headers
        public int TropType { get; set; }           // Tropospheric Correction Type
        public bool StecNbitsFlag { get; set; }     // STEC Residual Correction Range, true:16bits, false:7bits
        public int NetworkID { get; set; }          // Compact Network ID
        public ulong NetSVMask { get; set; }        // Network SV Mask
        public int TropQI_Class { get; set; }       // Troposhere Quality Indicatior; CLASS
        public int TropQI_Value { get; set; }       // Troposhere Quality Indicatior; VALUE
        public int NGrids { get; set; } = 0;        // Number of Grids
        // data
        public List<ST09DataEachGrid> Grid { get; set; } = new List<ST09DataEachGrid>();
    }

    /// <summary>
    /// Grid毎のTrop,STECデータ．STECは1つのGridで衛星毎(ST01のmaskに応じる)に格納される(net sv maskではない)
    /// </summary>
    public class ST09DataEachGrid
    {
        public double THD { get; set; } = 0.0;                          // Tropospheric Hydro Static Vertical Delay [m]
        public double TWD { get; set; } = 0.0;                          // Tropospheric Wet Vertical Delay [m]
        public List<ST09DataEachSat> Sat { get; set; } = new List<ST09DataEachSat>();  // STEC data; st01のsatellite maskの衛星全部
    }

    public class ST09DataEachSat
    {
        public bool HaveData { get; set; } = false;                     // Network SV Mask bit data for this satellite
        public double StecRC { set; get; } = 0.0;                       // STEC Residual Correction [TECU]
    }
}
