using System.Diagnostics;
using System.Text;
using static GrToolBox.Data.CLAS3.ClasDefinitions;

namespace GrToolBox.Data.CLAS3
{
    public class ST12_Atomospheric
    {
        public ST12Data Data { get; set; } = new ST12Data();
        public SubTypeStatus Status { get; set; } = SubTypeStatus.HeaderDone;
        private ST01_Mask? ST01 { get; set; }
        private int NBitRead { get; set; } = 0;      // 処理したビット数
       
        internal TraceSwitch TraceSwitch { get; set; } = new TraceSwitch("TSwitch_CLAS_ST12", "Clas Subtype12");

        public ST12_Atomospheric()
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

            NBitRead = 0;
            int nsat = st01.NSat();

            // Extended header 計 15 ビット
            if (bcb.Count() < 15)
            {
                Status = SubTypeStatus.NeedMoreData;
                DebugDisp();
                return Status;
            }
            Data.TropAvailability = (int)bcb.GetUint(2);
            Data.StecAvailability = (int)bcb.GetUint(2);
            Data.NetworkID = (int)bcb.GetUint(5);
            Data.NGrids = (int)bcb.GetUint(6);
            NBitRead += 15;
            Status = SubTypeStatus.HeaderDone;

            // TROP part
            if (Data.TropAvailability != 0)
            {
                DecodeTropPart(bcb);
                if(Status == SubTypeStatus.NeedMoreData)
                {
                    DebugDisp();
                    return Status;
                }
            }

            // STEC part
            if (Data.StecAvailability != 0)
            {
                DecodeStecPart(bcb);
                if (Status == SubTypeStatus.NeedMoreData || Status == SubTypeStatus.InvalidData)
                {
                    DebugDisp();
                    return Status;
                }
            }

            Status = SubTypeStatus.DataDone;
            DebugDisp();
            return Status;
        }


        private void DecodeTropPart(BitCircularBuffer bcb)
        {
            if (bcb.Count() < 8)
            {
                Status = SubTypeStatus.NeedMoreData;
                return;
            }
            Data.TropQI_Class = (int)bcb.GetUint(3);
            Data.TropQI_Value = (int)bcb.GetUint(3);
            Data.TropType = (int)bcb.GetUint(2);
            NBitRead += 8;

            int nbitsTxx = 9;                           // Trop type == 0; T00(9bit)
            if (Data.TropType > 0) nbitsTxx += 14;      // Trop type == 1 or 2
            if (Data.TropType > 1) nbitsTxx += 7;       // Trop type == 2

            if (bcb.Count() < nbitsTxx + 5)
            {
                Status = SubTypeStatus.NeedMoreData;
                return;
            }

            Data.T00 = bcb.GetInt(9) * 0.004;
            if (Data.TropType > 0)
            {
                Data.T01 = bcb.GetInt(7) * 0.002;
                Data.T10 = bcb.GetInt(7) * 0.002;
            }
            if (Data.TropType > 1)
            {
                Data.T11 = bcb.GetInt(7) * 0.001;
            }
            Data.TropResBitsFlag = bcb.GetBit();
            Data.TropResOffset = bcb.GetUint(4) * 0.02;
            NBitRead += (nbitsTxx + 5);

            int nbitTrop = (Data.TropResBitsFlag) ? 8 : 6;
            int nbitGrid = nbitTrop * Data.NGrids;

            if (bcb.Count() < nbitGrid)
            {
                Status = SubTypeStatus.NeedMoreData;
                return;
            }

            Data.TropResGrid = new double[Data.NGrids];
            for(int i = 0; i < Data.NGrids; i++)
            {
                Data.TropResGrid[i] = bcb.GetInt(nbitTrop) * 0.004;
            }
            NBitRead += nbitGrid;
        }


        private void DecodeStecPart(BitCircularBuffer bcb)
        {
            if(ST01 == null)
            {
                Status = SubTypeStatus.InvalidData;
                return;
            }
            int nsat = ST01.NSat();

            if (bcb.Count() < nsat + 6)
            {
                Status = SubTypeStatus.NeedMoreData;
                return;
            }

            Data.NetSVMask = 0;
            for (int i = 0; i < nsat; i++)
            {
                if (bcb.GetBit())
                {
                    Data.NetSVMask |= (ulong)((ulong)0x_01 << i);
                }
            }
            NBitRead += nsat;

            Data.StecSat.Clear();
            for(int i = 0; i < nsat; i++)
            {
                var sat = new ST12DataStecEachSat();
                Data.StecSat.Add(sat);
                if (((Data.NetSVMask >> i) & 0x_01) == 0)
                {
                    continue;
                }
                
                if (bcb.Count() < 8)
                {
                    Status = SubTypeStatus.NeedMoreData;
                    return;
                }

                sat.HaveData = true;
                sat.StecQI_Class = (int)bcb.GetUint(3);
                sat.StecQI_Value = (int)bcb.GetUint(3);
                sat.StecType = (int)bcb.GetUint(2);
                NBitRead += 8;

                int nbitsCxx = 14;                          // Stec type == 0; C00(14bit)
                if (sat.StecType > 0) nbitsCxx += 24;       //      type == 1 or 2 or 3
                if (sat.StecType > 1) nbitsCxx += 10;       //      type == 2 or 3
                if (sat.StecType > 2) nbitsCxx += 16;       //      type == 3

                if (bcb.Count() < nbitsCxx)
                {
                    Status = SubTypeStatus.NeedMoreData;
                    return;
                }
                sat.C00 = bcb.GetInt(14) * 0.05;
                if (sat.StecType > 0)
                {
                    sat.C01 = bcb.GetInt(12) * 0.02;
                    sat.C10 = bcb.GetInt(12) * 0.02;
                }
                if (sat.StecType > 1)
                {
                    sat.C11 = bcb.GetInt(10) * 0.02;
                }
                if (sat.StecType > 2)
                {
                    sat.C02 = bcb.GetInt(8) * 0.005;
                    sat.C20 = bcb.GetInt(8) * 0.005;
                }
                NBitRead += nbitsCxx;

                if (bcb.Count() < 2)
                {
                    Status = SubTypeStatus.NeedMoreData;
                    return;
                }
                sat.StecResSize = (int)bcb.GetUint(2);
                NBitRead += 2;

                int nbitRes = 0;
                double sfRes = 0.0;
                if(sat.StecResSize == 0)
                {
                    nbitRes = 4;
                    sfRes = 0.04;
                }else if(sat.StecResSize == 1)
                {
                    nbitRes = 4;
                    sfRes = 0.12;
                }else if(sat.StecResSize == 2)
                {
                    nbitRes = 5;
                    sfRes = 0.16;
                }else if (sat.StecResSize == 3)
                {
                    nbitRes = 7;
                    sfRes = 0.24;
                }

                int nbitsGrid = nbitRes * Data.NGrids;
                if (bcb.Count() < nbitsGrid)
                {
                    Status = SubTypeStatus.NeedMoreData;
                    return;
                }
                sat.StecResGrid = new double[Data.NGrids];
                for(int j = 0; j < Data.NGrids; j++)
                {
                    sat.StecResGrid[j] = bcb.GetInt(nbitRes) * sfRes;
                }
                NBitRead += nbitsGrid;
            }
        }

        private void DebugDisp()
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
                    Debug.WriteLine($"{Data.MessageNumber:0000} type={Data.ID}, Decode done  ({NBitRead} bits read)");
                    Debug.WriteLine($"{Data.MessageNumber:0000} type={Data.ID}, additional header: tropAvail={Data.TropAvailability}, stecAvail={Data.StecAvailability}, netID={Data.NetworkID}, nGrids={Data.NGrids}");
                    if (TraceSwitch.Level == TraceLevel.Verbose)
                    {
                        StringBuilder sb = new StringBuilder();
                        sb.Append($"    Trop Part: ");
                        if (Data.TropAvailability == 0)
                        {
                            sb.Append(" not available\n");
                        }
                        else
                        {
                            sb.Append(" available\n");
                            sb.Append($"        QI(class,value)=({Data.TropQI_Class},{Data.TropQI_Value}), type={Data.TropType:00}, ");
                            sb.Append($"T00={Data.T00,5:0.000}");
                            if (Data.T00 == -1.024) sb.Append("(N/A)");
                            if(Data.TropType > 0)
                            {
                                sb.Append($", T01={Data.T01,5:0.000}");
                                if (Data.T01 == -0.128) sb.Append("(N/A)");
                                sb.Append($", T10={Data.T10,5:0.000}");
                                if (Data.T10 == -0.128) sb.Append("(N/A)");
                            }
                            if (Data.TropType > 1)
                            {
                                sb.Append($", T11={Data.T11,5:0.000}");
                                if (Data.T11 == -0.064) sb.Append("(N/A)");
                            }
                            sb.Append("\n");
                        }
                        int nbitsTropRes = (Data.TropResBitsFlag) ? 8 : 6;
                        sb.Append($"        TropResSize={nbitsTropRes}bits, TropResOffset={Data.TropResOffset,5:0.000}\n");
                        sb.Append($"        TropRes(#Grid)= ");
                        for (int i = 0; i < Data.NGrids; i++)
                        {
                            sb.Append($"{Data.TropResGrid[i],5:0.000}({i+1}), ");
                        }
                        sb.Append("\n");

                        sb.Append($"    STEC Part: ");
                        if(Data.StecAvailability == 0)
                        {
                            sb.Append(" not available\n");
                        }
                        else
                        {
                            sb.Append($" available, NetSVMask={Data.NetSVMask:x}\n");
                            if (ST01 == null)
                            {
                                sb.Append("no mask data");
                                break;
                            }
                            int nsat = ST01.NSat();
                            for(int i = 0; i < nsat; i++)
                            {
                                var st01sat = ST01.Data.Sat[i];
                                var sat = Data.StecSat[i];
                                if (!sat.HaveData)
                                {
                                    sb.Append($"        ({st01sat.GnssID},{st01sat.SvN:00}): Not recorded (not appear in Net SV mask)\n");
                                    continue;
                                }
                                sb.Append($"        ({st01sat.GnssID},{st01sat.SvN:00}): QI(class,value)=({sat.StecQI_Class},{sat.StecQI_Value}), type={sat.StecType:00}, ");
                                sb.Append($"C00={sat.C00,5:0.000}");
                                if (sat.C00 == -409.6) sb.Append("(N/A)");
                                if (sat.StecType > 0)
                                {
                                    sb.Append($", C01={sat.C01,5:0.000}");
                                    if (sat.C01 == -40.96) sb.Append("(N/A)");
                                    sb.Append($", C10={sat.C10,5:0.000}");
                                    if (sat.C10 == -40.96) sb.Append("(N/A)");
                                }
                                if (sat.StecType > 1)
                                {
                                    sb.Append($", C11={sat.C11,5:0.000}");
                                    if (sat.C11 == -10.24) sb.Append("(N/A)");
                                }
                                if(sat.StecType > 2)
                                {
                                    sb.Append($", C02={sat.C02,5:0.000}");
                                    if (sat.C02 == -0.64) sb.Append("(N/A)");
                                    sb.Append($", C20={sat.C20,5:0.000}");
                                    if (sat.C20 == -0.64) sb.Append("(N/A)");
                                }
                                sb.Append("\n");
                                int nbitRes  = 0;
                                if (sat.StecResSize == 0)
                                {
                                    nbitRes = 4;
                                }
                                else if (sat.StecResSize == 1)
                                {
                                    nbitRes = 4;
                                }
                                else if (sat.StecResSize == 2)
                                {
                                    nbitRes = 5;
                                }
                                else if (sat.StecResSize == 3)
                                {
                                    nbitRes = 7;
                                }

                                sb.Append($"                : ResSize=({sat.StecResSize},{nbitRes}bits), Res=");
                                for(int j = 0; j < Data.NGrids; j++)
                                {
                                    sb.Append($"{sat.StecResGrid[j],5:0.000}({j + 1}), ");
                                }
                                sb.Append("\n");
                            }
                        }
                        Debug.Write(sb.ToString());
                    }
                    break;
            }
#endif
        }

    }


    public class ST12Data : ClasHeaderCommonData
    {
        // extended headers
        public int TropAvailability { get; set; } = 0;  // Tropospheric Correction Availability
        public int StecAvailability { get; set; } = 0;  // STEC Correction Availability
        public int NetworkID { get; set; }              // Compact Network ID
        public int NGrids { get; set; } = 0;        // Number of Grids

        // Trop part common
        public int TropQI_Class { get; set; }       // Troposhere Quality Indicatior; CLASS
        public int TropQI_Value { get; set; }       // Troposhere Quality Indicatior; VALUE
        public int TropType { get; set; }           // Tropospheric Correction Type
        public double T00 { get; set; } = 0.0;      // Tropospheric Polynomial Coefficients T_00
        public double T01 { get; set; } = 0.0;      // T_01
        public double T10 { get; set; } = 0.0;      // T_10
        public double T11 { get; set; } = 0.0;      // T_11
        public bool TropResBitsFlag { get; set; }   // false:6bits, true:8bits
        public double TropResOffset { get; set; }   // Troposphere Residual Offset
        // Trop part grids
        public double[] TropResGrid { get; set; } = Array.Empty<double>();

        // STEC part common
        public ulong NetSVMask { get; set; }        // Network SV Mask
        // STEC Res data for sat
        public List<ST12DataStecEachSat> StecSat { get; set; } = new List<ST12DataStecEachSat>();
    }

    // 衛星毎のSTECデータ
    public class ST12DataStecEachSat
    {
        public bool HaveData { get; set; } = false;     // Network SV Mask bit data for this satellite. if false, all data in this class is not available
        public int StecQI_Class { get; set; } = 0;      // SSR STEC Quality Indicator; bits5-3
        public int StecQI_Value { get; set; } = 0;      // bits2-0
        public int StecType { get; set; }               // Compact SSR STEC Correction Type
        public double C00 { get; set; } = 0.0;      // STEC Polynomial Coefficients C00
        public double C01 { get; set; } = 0.0;      // 
        public double C10 { get; set; } = 0.0;      // 
        public double C11 { get; set; } = 0.0;      // 
        public double C02 { get; set; } = 0.0;      // 
        public double C20 { get; set; } = 0.0;      // 
        public int StecResSize { get; set; }            // 0-3, 0:(4bits,LSB=0.04), 1:(4bits,LSB=0.12), 2:(5bits,LSB=0.16),3:(7bits,LSB=0.24)

        public double[] StecResGrid { get; set; } = Array.Empty<double>();
    }


}
