using System;
using System.Collections.Generic;
using System.Text;
using static GrToolBox.Data.Nmea.NmeaUtilities;

namespace GrToolBox.Data.Nmea
{
    public class NmeaGsa
    {
        public string rawMsg { get; set; } = "";
        public string xxGSV { get; set; } = "";             // GSA Message ID.  (xx = current Talker ID, see NMEA Talker IDs table)
        public string talkerID { get; set; } = "";
        public char opMode { get; set; } = '\0';            // Operation mode: M = Manually set to operate in 2D or 3D mode, 
                                                            //                 A = Automatically switching between 2D or 3D mode
        public int navMode { get; set; } = -9999;           // Navigation mode, see position fix flags description
        public int[] svid { get; set; } = new int[12];      // Satellite number, if not exists: -9999
        public double PDOP { get; set; } = double.NaN;      // Position dilution of precision
        public double HDOP { get; set; } = double.NaN;      // Horizontal dilution of precision
        public double VDOP { get; set; } = double.NaN;      // Vertical dilution of precision
        public int systemId { get; set; } = -9999;          // NMEA-defined GNSS system ID, see Signal Identifiers table(only available in NMEA 4.10 and later)
        //-----------------------
        public int[] PrnGR { get; set; } = new int[12];

        //コンストラクタ
        public NmeaGsa(String inSt)
        {
            for(int i=0; i<12; i++)
            {
                svid[i] = -9999;
                PrnGR[i] = -9999;
            }
            rawMsg = inSt;
            Parse();
        }

        //private methods
        private void Parse()
        {
            bool later_410 = false;                  // True: (NMEA 4.10 and later)
            string[] token = rawMsg.Split(",");
            if (token.Length == 19)
            {
                later_410 = true;
            }
            // (0) xxGGA
            xxGSV = token[0];
            talkerID = xxGSV[1..3];
            // (1) opMode
            opMode = token[1][0];
            // (2) navMode
            if (!string.IsNullOrEmpty(token[2])){
                navMode = int.Parse(token[2]);
            }
            // (3-14) svid
            for(int i=3; i<15; i++)
            {
                if (!string.IsNullOrEmpty(token[i]))
                {
                    svid[i-3] = int.Parse(token[i]);
                }
            }
            // (15) PDOP
            if (!string.IsNullOrEmpty(token[15]))
            {
                PDOP = double.Parse(token[15]);
            }
            // (16) HDOP
            if (!string.IsNullOrEmpty(token[16]))
            {
                HDOP = double.Parse(token[16]);
            }
            // (17) VDOP
            if (!later_410)
            {
                int indAst = token[17].IndexOf("*");
                if (indAst > 0)
                {
                    VDOP = double.Parse(token[17][0..indAst]);
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(token[17]))
                {
                    VDOP = double.Parse(token[17]);
                }
                // (18) systemID 4.10以降のみ
                int indAst = token[18].IndexOf("*");
                systemId = int.Parse(token[18][0..indAst]);
            }

            for(int i=0; i<12; i++)
            {
                if (svid[i] != -9999)
                {
                    if (later_410)
                    {
                        PrnGR[i] = NmeaSv2PrnGritz(svid[i], systemId);
                    }
                    else
                    {
                        PrnGR[i] = NmeaSv2PrnGritz(svid[i], talkerID);
                    }
                }
            }
        }
    }
}
