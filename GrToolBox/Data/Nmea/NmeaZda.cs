using System;
using System.Collections.Generic;
using System.Text;

namespace GrToolBox.Data.Nmea
{
    public class NmeaZda
    {
        public string rawMsg { get; set; } = "";
        public string xxZDA { get; set; } = "";     // ZDA Message ID (xx = current Talker ID, see NMEA Talker IDs table)
        public string talkerID { get; set; } = "";
        public string rawUtc { get; set; } = "";    // UTC time
        public int day { get; set; } = -9999;       // UTC day (range: 1-31)
        public int month { get; set; } = -9999;     // UTC month (range: 1-12)
        public int year { get; set; } = -9999;      // UTC year
        int ltzh = 0;                               // Local time zone hours (fixed field, always 00)
        int ltzn = 0;                               // Local time zone minutes (fixed field, always 00)

        public string CompleteTime { get; set; } = "";

        public NmeaZda() { }


        public void parseTime(String inSt)
        {
            rawMsg = inSt;
            String[] token = rawMsg.Split(",");
            // (1) UTC time
            if (!string.IsNullOrEmpty(token[1]))
            {
                rawUtc = token[1];
            }
        }

        public bool parse(String inSt)
        {
            rawMsg = inSt;
            string[] token = rawMsg.Split(",");
            for (int i = 0; i < 6; i++)
            {
                if (token[i].Contains("*") || string.IsNullOrEmpty(token[i])) return false;
            }
            // (0) xxGGA
            xxZDA = token[0];
            talkerID = xxZDA[1..3];
            // (1) UTC time
            if (!string.IsNullOrEmpty(token[1]))
            {
                rawUtc = new string(token[1]);
            }
            // (2) day
            if (!string.IsNullOrEmpty(token[2]))
            {
                day = int.Parse(token[2]);
            }
            // (3) month
            if (!string.IsNullOrEmpty(token[3]))
            {
                month = int.Parse(token[3]);
            }
            // (4) year
            if (!string.IsNullOrEmpty(token[4]))
            {
                year = int.Parse(token[4]);
            }
            CompleteTime = $"{year}-{month}-{day}T{rawUtc[0..2]}:{rawUtc[2..4]}:{rawUtc[4..]}";

            return true;
        }

    }
}
