using System;
using System.Collections.Generic;
using System.Text;
using static GrToolBox.Data.Nmea.NmeaUtilities;

namespace GrToolBox.Data.Nmea
{
    public class NmeaRmc
    {
        public string RawMsg { get; set; } = "";
        public bool Status { get; set; }                // Data validity status
        public DateTime Time { get; set; }              // UTC time
        public double Lat { get; set; } = double.NaN;                // Latitude [deg]
        public double Lon { get; set; } = double.NaN;                 // Longitude [deg]
        public double Speed { get; set; }               // Speed over ground [m/s]
        public double Course { get; set; }              // Course over ground [deg]
        public double MagneticVariation { get; set; }   // Magnetic variation value
        public char MV_EW { get; set; }                 // Magnetic variation E/W indicator


        //public string rawMsg { get; set; } = "";
        //public string xxRMC { get; set; } = "";         // RMC Message ID (xx = current Talker ID, see NMEA Talker IDs table)
        //public string talkerID { get; set; } = "";
        //public string rawUtc { get; set; } = "";        // UTC time
        //public char status { get; set; } = '\0';        // Data validity status
        //public double lat { get; set; } = Double.NaN;   // Latitude [deg]
        //public double lon { get; set; } = Double.NaN;   // Longitude [deg]
        //public double spd { get; set; } = Double.NaN;   // Speed over ground [m/s]
        //public double cog { get; set; } = Double.NaN;   // Course over ground [deg]
        //public string rawDate { get; set; } = "";       // Date in UTC. [ddmmyy]
        //public double mv { get; set; } = Double.NaN;    // Magnetic variation value
        //public char mvEW { get; set; } = '\0';          // Magnetic variation E/W indicator
        //public char posMode { get; set; } = '\0';       // Mode Indicator, only available in NMEA 2.3 and later
        //public char navStatus { get; set; } = '\0';     // Navigational status indicator, only available in NMEA 4.10 and later


        public NmeaRmc(string sentence)
        {
            if (!string.IsNullOrEmpty(sentence))
            {
                RawMsg = sentence;
                Parse();
            }
        }

        //public void parseTime(String inSt)
        //{
        //    rawUtc = "";
        //    rawMsg = inSt;
        //    String[] token = rawMsg.Split(",");
        //    // (1) UTC time
        //    if (!string.IsNullOrEmpty(token[1]))
        //    {
        //        rawUtc = token[1];
        //    }
        //}

        private bool Parse()
        {
            string[] token = RawMsg.Split(",");
            string tmp;
            for (int i = 0; i < 12; i++)
            {
                if (token[i].Contains("*")) return false;
            }
            // (0) xxGGA
            //xxRMC = token[0];
            //talkerID = xxRMC.Substring(1, 3);
            // UTC time
            if (!string.IsNullOrEmpty(token[1]) && !string.IsNullOrEmpty(token[9]))
            {
                // token[1]: hhmmss.ss
                tmp = token[1];
                int hh = int.Parse(tmp[0..2]);
                int mm = int.Parse(tmp[2..4]);
                double dsec = double.Parse(tmp[4..]);
                int sec = (int)dsec;
                int msec = (int)(dsec - (double)sec) * 1000;
                // token[9]: ddmmyy
                tmp = token[9];
                int dd = int.Parse(tmp[0..2]);
                int MM = int.Parse(tmp[2..4]);
                int yyyy = int.Parse("20" + tmp[4..]);
                Time = new DateTime(yyyy, MM, dd, hh, mm, sec, msec, DateTimeKind.Utc);
            }
            // (2) Data validity status
            if (!string.IsNullOrEmpty(token[2]))
            {
                // token[2] is char, A means active
                Status = token[2][0] == 'A';
            }
            // (3),(4) Latitude [deg]に変換
            // token[3]: lat, token[4]: N/S
            if (!string.IsNullOrEmpty(token[3]) && !string.IsNullOrEmpty(token[4]))
            {
                Lat = nmeaLat2Deg(token[3], token[4]);
            }
            // (5),(6) Longitude [deg]に変換
            if (!string.IsNullOrEmpty(token[5]) && !string.IsNullOrEmpty(token[6]))
            {
                // token[5]: lon, token[6]: E/W
                Lon = nmeaLon2Deg(token[5], token[6]);
            }
            // (7) spd, [m/s]に変換
            if (!string.IsNullOrEmpty(token[7]))
            {
                // originally recorded in Knots
                Speed = double.Parse(token[7]) * 1852.0 / 3600.0;
            }
            // (8) course on ground
            if (!string.IsNullOrEmpty(token[8]))
            {
                Course = double.Parse(token[8]);
            }
            // (9) date
            //if (!string.IsNullOrEmpty(token[9]))
            //{
            //    rawDate = new String(token[9]);
            //}
            // (10) mv
            if (!string.IsNullOrEmpty(token[10]))
            {
                MagneticVariation = double.Parse(token[10]);
            }
            // (11) mvEW
            if (!string.IsNullOrEmpty(token[11]))
            {
                MV_EW = token[11][0];
            }
            // (12) posMode
            //if (token.Length > 12)
            //{
            //    if (!string.IsNullOrEmpty(token[12]))
            //    {
            //        posMode = token[12][0];
            //    }
            //}
            //// (13) navStatus
            //if (token.Length > 13)
            //{
            //    if (!string.IsNullOrEmpty(token[13]))
            //    {
            //        navStatus = token[13][0];
            //    }
            //}
            return true;
        }




    }
}
