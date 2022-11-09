using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static GrToolBox.Common.CommonDefinitions;
using static GrToolBox.Data.SBF.Rtcm3File;

namespace GrToolBox.Data.CLAS3
{
    public static class ClasUtilities
    {

        public static int GetSvN(SYS s, int indSat)
        {
            if (s == SYS.QZS)
            {
                return indSat + 193;
            }
            else if (s == SYS.GPS || s == SYS.GLO || s == SYS.GAL || s == SYS.BDS)
            {
                return indSat + 1;
            }
            else
            {
                return -1;
            }
        }

        public static string GetSigName(SYS s, int indSig)
        {
            if(s == SYS.UNKNOWN || s == SYS.IRN || indSig > 15) { return ""; }
            return(SSRSignalTable[(int)s, indSig]);
        }

        public static readonly string[,] SSRSignalTable = new string[,] { 
            { "L1 C/A",              "L1 P",         "L1 Z-tracking", "L1C(D)",        "L1C(P)",       "L1C(D+P)",   "L2C(M)",                "L2C(L)",        "L2C(M+L)", "L2 P",     "L2 Z-tracking", "L5 I",   "L5 Q",     "L5 I+Q",   "",         "" },
            { "G1 C/A",              "G1 P",         "G2 C/A",        "G2P",           "G1a(D)",       "G1a(P)",     "G1a(D+P)",              "G2a(D)",        "G2a(P)",   "G2a(D+P)", "G3I",           "G3 Q",   "G3 I+Q",   "",         "",         "" },
            { "E1 B1/NAV OS/CS/SoL", "E1 C no data", "E1 B+C",        "E5aI F/NAV OS", "E5aQ no data", "E5a(I+Q)",   "E5b I I/NAV OS/CS/Sol", "E5b Q no data", "E5b I+Q",  "E5 I",     "E5Q",           "E5 I+Q", "Service1", "Service2", "Service3", "" },
            { "B1 I",                "B1 Q",         "B1 I+Q",        "B3 I",          "B3 Q",         "B3 I+Q",     "B2 I",                  "B2 Q",          "B2 I+Q",   "",         "",              "",       "",         "",         "",         "" },
            { "L1 C/A",              "L1C(D)",       "L1C(P)",        "L1C(D+P)",      "L2C(M)",       "L2C(L)",     "L2C(M+L)",              "L5 I",          "L5 Q",     "L5 I+Q",   "",              "",       "",         "",         "",         "" },
            { "L1 C/A",              "L5 I",         "L5 Q",          "L5 I+Q",        "",             "",           "",                      "",              "",         "",         "",              "",       "",         "",         "",         "" }
        };
        

        public static void ClasMessage2L6file(List<ClasMessage> clasMessages)
        {
            // clasMesssageのbyte[]をl6ファイルとしてバイナリファイルをつくる
            string logfile = @"./debug_test.l6";
            if (File.Exists(logfile))
            {
                File.Delete(logfile);
            }
            using (FileStream fs = new FileStream(logfile, FileMode.Create))
            using (BinaryWriter bw = new BinaryWriter(fs))
            {
                for (int i = 0; i < clasMessages.Count; i++)
                {
                    bw.Write(clasMessages[i].Bytes);
                }
            }
            return;

        }



    }
}
