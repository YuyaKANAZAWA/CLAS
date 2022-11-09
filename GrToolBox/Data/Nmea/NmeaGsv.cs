using System;
using System.Collections.Generic;
using System.Text;
using static GrToolBox.Data.Nmea.NmeaUtilities;

namespace GrToolBox.Data.Nmea
{
    public class NmeaGsv
    {
        public string rawMsg { get; set; } = "";
        public string xxGSV { get; set; } = "";             // GSV Message ID. Talker ID GN shall not be used.
        public string talkerID { get; set; } = "";
        public int numMsg { get; set; } = -9999;            // Number of messages, total number of GSV messages being output (range: 1-9)
        public int msgNum { get; set; } = -9999;            // Number of this message (range: 1-numMsg)
        public int numSV { get; set; } = -9999;             // Number of known satellites in view regarding both the talker ID and the signalId
        public int[] svid { get; set; } = { -9999, -9999, -9999, -9999 };
                                                            // Satellite ID
        public int[] elv { get; set; } = { -9999, -9999, -9999, -9999 };
                                                            // Elevation (range: 0-90)
        public int[] az { get; set; } = { -9999, -9999, -9999, -9999 };
                                                            // Azimuth (range: 0-359)
        public int[] cn0 { get; set; } = { -9999, -9999, -9999, -9999 };
                                                            // Signal strength (C/N0, range: 0-99), null when not tracking
        public int signalID { get; set; } = -9999;          // NMEA-defined GNSS signal ID, see Signal Identifiers table (only available in NMEA 4.10 and later)
        //-----------------------
        public int[] prnGR { get; set; } = { -9999, -9999, -9999, -9999 };
                                                            // prn番号(G-RitZ)
        public int numSVinThisSentence { get; set; } = 0;
        //-----------------------

        public NmeaGsv() { }

        public void Parse(String inSt)
        {
            rawMsg = inSt;
            string[] token = rawMsg.Split(",");
            // (0) xxGGA
            xxGSV = token[0];
            talkerID = xxGSV[1..3];
            // (1) numMsg
            numMsg = int.Parse(token[1]);
            // (2) msgNum
            msgNum = int.Parse(token[2]);
            // (3) numSV
            numSV = int.Parse(token[3]);
            numSVinThisSentence = numSV - 4 * (msgNum - 1);
            if (numSVinThisSentence > 4)
            {
                numSVinThisSentence = 4;
            }
            int offset = 0;
            int indAst = -1;
            string tmpSvid, tmpElv, tmpAz, tmpCn0;
            bool later_410 = false;                  // True: (NMEA 4.10 and later)
            for (int i = 0; i < numSVinThisSentence; i++)
            {
                offset = 4 * i;
                tmpSvid = token[4 + offset];
                tmpElv = token[5 + offset];
                tmpAz = token[6 + offset];

                if (i < numSVinThisSentence - 1)    //途中のエントリ
                {
                    tmpCn0 = token[7 + offset];
                }
                else
                {   
                    indAst = token[7 + offset].IndexOf("*");
                    if (indAst == -1)
                    {   //最後のエントリに[*]が無ければバージョン410以降．トークンの値がCn0値
                        tmpCn0 = token[7 + offset];
                        later_410 = true;
                    }
                    else
                    {
                        //最後のエントリに[*]があればバージョン410以前．[*]の前がCn0値
                        tmpCn0 = token[7 + offset][0..indAst];
                    }

                }

                if (!string.IsNullOrEmpty(tmpSvid))
                {
                    svid[i] = int.Parse(tmpSvid);
                    prnGR[i] = NmeaSv2PrnGritz(svid[i], talkerID);
                }
                if (!string.IsNullOrEmpty(tmpElv))
                {
                    elv[i] = int.Parse(tmpElv);
                }
                if (!string.IsNullOrEmpty(tmpAz))
                {
                    az[i] = int.Parse(tmpAz);
                }
                if (!string.IsNullOrEmpty(tmpCn0))
                {
                    cn0[i] = int.Parse(tmpCn0);
                }

            }
            if (later_410)
            {
                indAst = token[3 + 4 * numSVinThisSentence + 1].IndexOf("*");
                signalID = int.Parse(token[3 + 4 * numSVinThisSentence + 1][0..indAst]);
            }
        }
    }
}
