using System;
using System.Collections.Generic;
using System.Text;

namespace GrToolBox.Common
{
    public static class Constants
    {
        public static readonly double CC = 299792458.0;
        public static readonly double PI = 3.1415926535898;
        public static readonly double DEG2RAD = PI / 180.0;
        public static readonly double RAD2DEG = 180.0 / PI;

        public static readonly int MAX_SAT_G = 37;
        public static readonly int MAX_SAT_R = 30;
        public static readonly int MAX_SAT_E = 36;
        public static readonly int MAX_SAT_J = 7;
        public static readonly int MAX_SAT_C = 63;
        public static readonly int MAX_SAT_I = 14;
        public static readonly int MAX_SAT_S = 39;
        public static readonly int MAX_SAT = 400;
        public static readonly int MAX_SYS = 7;
        public static readonly int MAX_OBS_PVT = 3;

        //public static readonly char[] SYS = { 'G', 'R', 'E', 'J', 'C', 'I', 'S' };
        //public static readonly int[] PRNOFFSET = { 0, 50, 100, 150, 200, 250, 300 };

        public static readonly int MAX_OBS_TYPE = 25;

        public static readonly int[] INT2RAISEDBIT = { 0, 1, 1, 2, 1, 2, 2, 3 };    // 0-7の整数に対するTrue Bit数

        public static readonly double POW2_M5  = 0.03125;                           // 2^{-5}
        public static readonly double POW2_M11 = 4.882812500000000E-04;             // 2^{-11}
        public static readonly double POW2_M19 = 1.9073486328125E-06;               // 2^{-19}
        public static readonly double POW2_M20 = 9.536743164062500E-07;             // 2^{-20}
        public static readonly double POW2_M29 = 1.862645149230957E-09;             // 2^{-29}
        public static readonly double POW2_M30 = 9.313225746154785E-10;             // 2^{-30}
        public static readonly double POW2_M31 = 4.656612873077393E-10;             // 2^{-31}
        public static readonly double POW2_M33 = 1.1641532182693481E-10;            // 2^{-33}
        public static readonly double POW2_M40 = 9.094947017729282E-13;             // 2^{-40}
        public static readonly double POW2_M43 = 1.1368683772161603E-13;            // 2^{-43}
        public static readonly double POW2_M55 = 2.7755575615628914E-17;            // 2^{-55}
    }
}
