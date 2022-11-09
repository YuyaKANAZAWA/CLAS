using GrToolBox.Satellite;
using static GrToolBox.Common.Constants;
using static GrToolBox.Data.SBF.SbfDefinitions;

namespace GrToolBox.Data.SBF
{
    public static class SbfUtilities
    {
        public static ushort CRC_compute16CCITT(byte[] byteData)
        {
            ushort crc = 0;

            for (int i = 0; i < byteData.Length; i++)
            {
                crc = (ushort)(crc << 8 ^ SbfConstants.CRC_SBF_LookUp[crc >> 8 ^ byteData[i]]);
            }
            return crc;
        }



        public static string SVID2SNN(byte svid)
        {
            if (svid <= 37)
            {
                return "G" + svid.ToString();
            }
            else if (38 <= svid && svid <= 61)
            {
                return "R" + (svid - 37).ToString();
            }
            else if (svid == 62)
            {
                return "NA";
            }
            else if (63 <= svid && svid <= 68)
            {
                return "R" + (svid - 38).ToString();
            }
            else if (71 <= svid && svid <= 106)
            {
                return "E" + (svid - 70).ToString();
            }
            else if (107 <= svid && svid <= 119)
            {
                return "NA";
            }
            else if (120 <= svid && svid <= 140)
            {
                return "S" + (svid - 100).ToString();
            }
            else if (141 <= svid && svid <= 180)
            {
                return "C" + (svid - 140).ToString();
            }
            else if (181 <= svid && svid <= 187)
            {
                return "J" + (svid - 180).ToString();
            }
            else if (191 <= svid && svid <= 197)
            {
                return "I" + (svid - 190).ToString();
            }
            else if (198 <= svid && svid <= 215)
            {
                return "S" + (svid - 157).ToString();
            }
            else if (216 <= svid && svid <= 222)
            {
                return "I" + (svid - 208).ToString();
            }
            else if (223 <= svid && svid <= 245)
            {
                return "C" + (svid - 182).ToString();
            }
            else
            {
                return "NA";
            }
        }


        public static double SignalNum2Freq(SbfSignalType type, byte freqNr)
        {
            switch (type)
            {
                case SbfSignalType.G_L1CA:
                case SbfSignalType.G_L1P:
                case SbfSignalType.G_L1C:
                case SbfSignalType.J_L1CA:
                case SbfSignalType.C_B1C:
                case SbfSignalType.E_E1BC:
                case SbfSignalType.S_L1CA:
                case SbfSignalType.J_L1C:
                case SbfSignalType.J_L1S:
                    return SatelliteConstants.FG1;  // Hz
                case SbfSignalType.G_L2P:
                case SbfSignalType.G_L2C:
                case SbfSignalType.J_L2C:
                    return SatelliteConstants.FG2;  // Hz
                case SbfSignalType.G_L5:
                case SbfSignalType.C_B2a:
                case SbfSignalType.I_L5:
                case SbfSignalType.E_E5a:
                case SbfSignalType.S_L5:
                case SbfSignalType.J_L5:
                    return SatelliteConstants.FG5;  // Hz
                case SbfSignalType.R_L1CA:
                case SbfSignalType.R_L1P:
                    return SatelliteConstants.FR1 + (freqNr - 8) * SatelliteConstants.FR1SEPARATION;    // Hz
                case SbfSignalType.R_L2P:
                case SbfSignalType.R_L2CA:
                    return SatelliteConstants.FR2 + (freqNr - 8) * SatelliteConstants.FR2SEPARATION;    // Hz
                case SbfSignalType.R_L3:
                    return SatelliteConstants.FR3;  // Hz
                case SbfSignalType.E_E6BC:
                case SbfSignalType.J_L6:
                    return SatelliteConstants.FE6;  // Hz
                case SbfSignalType.E_E5b:
                case SbfSignalType.C_B2I:
                case SbfSignalType.C_B2b:
                    return SatelliteConstants.FE5b; // Hz
                case SbfSignalType.E_E5:
                    return SatelliteConstants.FE5;  //Hz
                case SbfSignalType.MSS:
                    return double.NaN;
                case SbfSignalType.C_B1I:
                    return SatelliteConstants.FB1I; // Hz
                case SbfSignalType.C_B3I:
                    return SatelliteConstants.FB3I; // Hz
                default:
                    return double.NaN;
            }
        }


        public static double SignalNum2WaveLength(SbfSignalType type, byte freqNr)
        {
            return Common.Constants.CC / SignalNum2Freq(type, freqNr);
        }


        public static string SignalType2RinexCode(SbfSignalType type)
        {
            switch (type)
            {
                case SbfSignalType.G_L1CA:  return "1C";
                case SbfSignalType.G_L1P:   return "1W";
                case SbfSignalType.G_L2P:   return "2W";
                case SbfSignalType.G_L2C:   return "2L";
                case SbfSignalType.G_L5:    return "5Q";
                case SbfSignalType.G_L1C:   return "1L";
                case SbfSignalType.J_L1CA:  return "1C";
                case SbfSignalType.J_L2C:   return "2L";
                case SbfSignalType.R_L1CA:  return "1C";
                case SbfSignalType.R_L1P:   return "1P";
                case SbfSignalType.R_L2P:   return "2P";
                case SbfSignalType.R_L2CA:  return "2C";
                case SbfSignalType.R_L3:    return "3Q";
                case SbfSignalType.C_B1C:   return "1P";
                case SbfSignalType.C_B2a:   return "5P";
                case SbfSignalType.I_L5:    return "5A";
                case SbfSignalType.E_E1BC:  return "1C";
                case SbfSignalType.E_E6BC:  return "6C";
                case SbfSignalType.E_E5a:   return "5Q";
                case SbfSignalType.E_E5b:   return "7Q";
                case SbfSignalType.E_E5:    return "8Q";
                case SbfSignalType.MSS:     return "NA";
                case SbfSignalType.S_L1CA:  return "1C";
                case SbfSignalType.S_L5:    return "5I";
                case SbfSignalType.J_L5:    return "5Q";
                case SbfSignalType.J_L6:    return "";
                case SbfSignalType.C_B1I:   return "2I";
                case SbfSignalType.C_B2I:   return "7I";
                case SbfSignalType.C_B3I:   return "6I";
                case SbfSignalType.J_L1C:   return "1L";
                case SbfSignalType.J_L1S:   return "1Z";
                case SbfSignalType.C_B2b:   return "7D";
                default:                    return "";
            }
        }


        public static string SbfBlockName(ushort num)
        {
            switch (num)
            {
                case 4027:  return "MeasEpoch";
                case 4000:  return "MeasExtra";
                case 5891:  return "GPSNav";
                case 4004:  return "GLONav";
                case 4002:  return "GALNav";
                case 4081:  return "BDSNav";
                case 4095:  return "QZSNav";
                case 4006:  return "PVTCartesian";
                case 4007:  return "PVTGeodetic";
                default:    return "Unknown";
            }
        }


        public static string[][][] GetSbfSignalList()
        {
            string[][][] codeList2 = new string[MAX_SYS][][];
            for (int i = 0; i < MAX_SYS; i++)
            {
                codeList2[i] = new string[3][];
            }
            // GPS
            codeList2[0][0] = new string[] { "---", "C1C", "C1W", "C1L" };
            codeList2[0][1] = new string[] { "---", "C2W", "C2L" };
            codeList2[0][2] = new string[] { "---", "C5Q" };
            // GLO
            codeList2[1][0] = new string[] { "---", "C1C", "C1P" };
            codeList2[1][1] = new string[] { "---", "C2C", "C2P" };
            codeList2[1][2] = new string[] { "---", "C3Q" };
            // GAL
            codeList2[2][0] = new string[] { "---", "C1C" };
            codeList2[2][1] = new string[] { "---" };
            codeList2[2][2] = new string[] { "---", "C5Q", "C7Q", "C8Q" };
            // QZS
            codeList2[3][0] = new string[] { "---", "C1C", "C1W", "C1L", "C1Z" };
            codeList2[3][1] = new string[] { "---", "C2L" };
            codeList2[3][2] = new string[] { "---", "C5Q" };
            // BDS
            codeList2[4][0] = new string[] { "---", "C1P", "C2I" };
            codeList2[4][1] = new string[] { "---", "C5P" };
            codeList2[4][2] = new string[] { "---", "C7I", "C7D" };
            // IRN
            codeList2[5][0] = new string[] { "---" };
            codeList2[5][1] = new string[] { "---" };
            codeList2[5][2] = new string[] { "---", "C5A" };
            // SBS
            codeList2[6][0] = new string[] { "---", "C1C" };
            codeList2[6][1] = new string[] { "---" };
            codeList2[6][2] = new string[] { "---", "C5I" };

            return codeList2;
        }










    }
}
