using static GrToolBox.Common.CommonUtilities;

namespace GrToolBox.Data.Nmea
{
    public class NmeaUtilities
    {

        public static bool NmeaCScheck(byte[] b)
        {
            bool valid = false;
            byte cs = 0;
            byte inCs = 0;

            string inCsString = System.Text.Encoding.ASCII.GetString(b, b.Length - 2, 2);   // センテンス末尾に記録されたCS，16進表記の2文字（*の後ろ）
            //string inCsString = System.Text.Encoding.ASCII.GetString(b, b.Length - 4, 2);   // センテンス末尾に記録されたCS，16進表記の2文字（*の後ろ）
            inCs = Convert.ToByte(inCsString, 16);  // 16進表記の2文字をバイトに変換

            if(b.Length > 10)
            {
                for (int i = 1; i < b.Length - 3; i++)
                {    // cs計算:  $の後ろから*の手前までのXOR
                    cs = (byte)(cs ^ b[i]);
                }

            }
            if (cs == inCs)
            {
                valid = true;
            }
            return valid;
            //return true;
        }

        public static double nmeaLat2Deg(String rawLat, String ns)
        {
            double lat = double.Parse(rawLat[2..]) / 60.0;
            lat += double.Parse(rawLat[0..2]);
            if (ns.StartsWith("S"))
            {
                lat = -lat;
            }
            return lat;
        }

        public static double nmeaLon2Deg(String rawLon, String ew)
        {
            double lon = double.Parse(rawLon[3..]) / 60.0;
            lon += double.Parse(rawLon[0..3]);
            if (ew.StartsWith("W"))
            {
                lon = -lon;
            }
            return lon;
        }
        
        public static int NmeaSv2PrnGritz(int svid, int systemId)
        {
            string talkerID;
            switch (systemId)
            {
                case 1:
                    talkerID = "GP";
                    break;
                case 2:
                    talkerID = "GL";
                    break;
                case 3:
                    talkerID = "GA";
                    break;
                case 4:
                    talkerID = "GB";
                    break;
                case 5:
                    talkerID = "GQ";
                    break;
                default:
                    return -9999;
            }
            return NmeaSv2PrnGritz(svid, talkerID);
        }

        public static int NmeaSv2PrnGritz(int svid, string talkerID)
        {
            int prn = -9999;
            switch (talkerID)
            {
                case "GP":
                    if (svid <= 32)
                    {           // GPS  そのまま
                        prn = svid;
                    }
                    else if (svid <= 151)
                    {    // NMEA(33-64)
                        prn = (svid - 32) + GetPrnOffset(GetISys('S'));
                    }
                    else
                    {                  // NMEA(152-158)
                        prn = (svid - 151) + 332;
                    }
                    break;
                case "GL":                  // Glonass
                    prn = (svid - 64) + GetPrnOffset(GetISys('R'));
                    break;
                case "GA":                  // Galileo
                    if (svid > 300)
                    {
                        prn = (svid - 300) + GetPrnOffset(GetISys('E'));    // NMEA(301-336)ver.2.3-4.0
                    }
                    else
                    {
                        prn = svid + GetPrnOffset(GetISys('E'));          // NMEA(1-36)ver4.10-
                    }
                    break;
                case "GQ":                  // QZSS
                    if (svid > 192)
                    {
                        prn = (svid - 192) + GetPrnOffset(GetISys('J'));    // NMEA(193-202)
                    }
                    else
                    {
                        prn = svid + GetPrnOffset(GetISys('J'));          // NMEA(1-10)
                    }
                    break;
                case "GB":
                case "BD":  // ver 410
                    if (svid > 400)
                    {
                        prn = (svid - 400) + GetPrnOffset(GetISys('C'));
                    }
                    else
                    {
                        prn = svid + GetPrnOffset(GetISys('C'));
                    }
                    break;
            }
            return prn;

        }

        public static int NmeaSigID2L1_2_5(string talker, int signalID)
        {
            int band = -9999;
            if (signalID == -9999) return 0;
            switch (talker)
            {
                case "GP":
                    if (signalID == 1 || signalID == 2 || signalID == 3) band = 0;
                    if (signalID == 4 || signalID == 5 || signalID == 6) band = 1;
                    if (signalID == 7 || signalID == 8) band = 2;
                    break;
                case "GL":
                    if (signalID == 1 || signalID == 2) band = 0;
                    if (signalID == 3 || signalID == 4) band = 1;
                    break;
                case "GA":
                    if (signalID == 6 || signalID == 7) band = 0;
                    if (signalID == 1 || signalID == 2 || signalID ==3 ) band = 2;
                    break;
                case "GQ":
                    if (signalID == 1 || signalID == 2 || signalID == 3) band = 0;
                    if (signalID == 5 || signalID == 6) band = 1;
                    if (signalID == 7 || signalID == 8) band = 2;
                    break;
                case "GB":
                    if (signalID == 1 || signalID == 2 || signalID == 3 || signalID == 4) band = 0;
                    if (signalID == 5 || signalID == 6 || signalID == 7 || signalID == 11 || signalID == 12) band = 1;
                    if (signalID == 8 || signalID == 9 || signalID == 10) band = 2;
                    break;
            }
            return band;
        }
    }

}
