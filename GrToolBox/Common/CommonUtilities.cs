using static GrToolBox.Common.CommonDefinitions;
using static GrToolBox.Common.Constants;

namespace GrToolBox.Common
{
    public static class CommonUtilities
    {

        /// <summary>
        ///     SYSを受け取り，インデックスを返す
        /// </summary>
        /// <param name="s">CommonDefinitions enum SYS</param>
        /// <returns>インデックス</returns>
        public static int GetISys(SYS s)
        {
            switch (s)
            {
                case SYS.GPS: return 0;
                case SYS.GLO: return 1;
                case SYS.GAL: return 2;
                case SYS.QZS: return 3;
                case SYS.BDS: return 4;
                case SYS.IRN: return 5;
                case SYS.SBS: return 6;                   
            }
            return -1;
        }

        public static int GetISys(int prn)
        {
            return GetISys(GetSys(prn));
        }

        public static int GetISys(char c)
        {
            switch (c)
            {
                case 'G': return 0;
                case 'R': return 1;
                case 'E': return 2;
                case 'J': return 3;
                case 'C': return 4;
                case 'I': return 5;
                case 'S': return 6;
            }
            //return GetISys(GetSys(c));
            return -1;
        }


        public static SYS GetSys(int prn)
        {
            int n = prn / 50;
            switch (n)
            {
                case 0: return SYS.GPS;
                case 1: return SYS.GLO;
                case 2: return SYS.GAL;
                case 3: return SYS.QZS;
                case 4: 
                case 5: return SYS.BDS;
                case 6: return SYS.IRN;
                case 7: return SYS.SBS;
            }
            return SYS.UNKNOWN;
        }

        public static SYS GetSys(char c)
        {
            switch (c)
            {
                case 'G': return SYS.GPS;
                case 'R': return SYS.GLO;
                case 'E': return SYS.GAL;
                case 'J': return SYS.QZS;
                case 'C': return SYS.BDS;
                case 'I': return SYS.IRN;
                case 'S': return SYS.SBS;
            }
            return SYS.UNKNOWN;
        }

        public static SYS GetSys(string s)
        {
            switch (s)
            {
                case "GPS": return SYS.GPS;
                case "GLO": return SYS.GLO;
                case "GAL": return SYS.GAL;
                case "QZS": return SYS.QZS;
                case "BDS": return SYS.BDS;
                case "IRN": return SYS.IRN;
                case "SBS": return SYS.SBS;
            }
            return SYS.UNKNOWN;
        }

        public static char GetSysChar(SYS s)
        {
            switch (s)
            {
                case SYS.GPS: return 'G';
                case SYS.GLO: return 'R';
                case SYS.GAL: return 'E';
                case SYS.QZS: return 'J';
                case SYS.BDS: return 'C';
                case SYS.IRN: return 'I';
                case SYS.SBS: return 'S';
            }
            return '\0';
        }



        public static int GetPrnOffset(SYS s)
        {
            switch (s)
            {
                case SYS.GPS: return 0;
                case SYS.GLO: return 50;
                case SYS.GAL: return 100;
                case SYS.QZS: return 150;
                case SYS.BDS: return 200;
                case SYS.IRN: return 300;
                case SYS.SBS: return 350;
            }
            return -1;
        }

        public static int GetPrnOffset(int prn)
        {
            return GetPrnOffset(GetSys(prn));
        }


        public static string Prn2Snn(int prn)   // Prn(Gritz)を受け取り，"G01"形式の文字列を返す
        {
            return $"{GetSys(prn)}{prn - GetPrnOffset(prn):00}";
        }


        public static int Snn2Prn(string Snn)
        {
            SYS sys = GetSys(Snn[0]);
            int prn = int.Parse(Snn[1..]) + GetPrnOffset(sys);
            return prn;
        }
    }


}

