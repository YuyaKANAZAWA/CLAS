using static GrToolBox.Common.CommonDefinitions;
using static GrToolBox.Common.Constants;

namespace GrToolBox.Satellite
{
    public static class SatelliteConstants
    {
        // GPS

        /// <summary>
        /// GPS L1 Frequency; 1575.42[MHz]
        /// </summary>
        public static readonly double FG1 = 1575420000.0;           // GPS L1周波数[Hz]
        public static readonly double LG1 = CC / FG1;               // GPS L1波長[m]
        public static readonly double FG2 = 1227600000.0;           // GPS L2周波数[Hz]
        public static readonly double LG2 = CC / FG2;               // GPS L2波長[m]
        public static readonly double FG5 = 1176450000.0;           // GPS L5周波数[Hz]
        public static readonly double LG5 = CC / FG5;               // GPS L5波長[m]
        public static readonly double GAMMAG12 = 1.646944444;       // (f1/f2)^2
        public static readonly double GAMMAG15 = 1.793270321;       // (f1/f5)^2
        public static readonly double IFCOEFG12 = 1.5457277802;     // -1/(1-(f1/f2)^2):  IonFree(1,2) = P1 + IFCOEFG12*(P1-P2)
        public static readonly double IFCOEFG15 = 1.2606043275;     // -1/(1-(f1/f5)^2):  IonFree(1,5) = P1 + IFCOEFG15*(P1-P5)
        // GLO
        public static readonly double FR1 = 1602000000.0;           // GLO G1
        public static readonly double FR2 = 1246000000.0;           // GLO G2
        public static readonly double FR3 = 1202025000.0;           // GLO G3
        public static readonly double FR1SEPARATION = 562500.0;     // GLO G1 FDMA separation [Hz]
        public static readonly double FR2SEPARATION = 437500.0;     // GLO G2 FDMA separation [Hz]
        // GAL
        public static readonly double FE1 = 1575420000.0;           // GAL E1
        public static readonly double FE6 = 1278750000.0;           // GAL E6
        public static readonly double FE5 = 1191795000.0;           // GAL E5
        public static readonly double FE5a = 1176450000.0;          // GAL E5a
        public static readonly double FE5b = 1207140000.0;          // GAL E5b
        public static readonly double LE1 = CC / FE1;               //
        public static readonly double LE6 = CC / FE6;               //
        public static readonly double LE5 = CC / FE5;               //
        public static readonly double LE5a = CC / FE5a;             //
        public static readonly double LE5b = CC / FE5b;             //
        public static readonly double GAMMAE15a = 1.79327032136;    // (f1/f5a)^2  1.793270321361059
        public static readonly double GAMMAE15b = 1.70324619362;    // (f1/f5b)^2  1.703246193622522
        public static readonly double IFCOEFE15a = 1.2606043275;    // -1/(1-(f1/f5a)^2):  IonFree(1,2) = P1 + IFCOEFG12*(P1-P5a)
        public static readonly double IFCOEFE15b = 1.4219771242;    // -1/(1-(f1/f5b)^2):  IonFree(1,5) = P1 + IFCOEFG15*(P1-P5b)
        // BDS
        public static readonly double FB1I = 1561098000.0;          // BDS B1I (2I)
        public static readonly double FB2I = 1207140000.0;          // BDS B2I (7I)
        public static readonly double FB3I = 1268520000.0;          // BDS B1I (6I)
        public static readonly double LB1I = CC / FB1I;             // wave length B1I
        public static readonly double LB2I = CC / FB2I;             // wave length B2I
        public static readonly double LB3I = CC / FB3I;             // wave length B3I
        public static readonly double GAMMAB12I = 1.67241884516;    // (FB1I/FB2I)^2  1.672418845159437
        public static readonly double GAMMAB13I = 1.51448751301;    // (FB1I/FB3I)^2  1.514487513007284
        public static readonly double IFCOEFB12I = 1.487168313617;  // -1/(1-(FB1I/FB2I)^2):  IonFree(1,2) = B1I + IFCOEFB12I*(B1I-B2I)
        public static readonly double IFCOEFB13I = 1.943681770146;  // -1/(1-(FB1I/FB3I)^2):  IonFree(1,3) = B1I + IFCOEFB13I*(B1I-B3I)


        public static double OMGE(SYS sys)   // 地球自転角速度 [rad/s]
        {
            switch (sys)
            {
                case SYS.GPS: return 7.2921151467e-5;
                case SYS.GLO: return 7.292115e-5;
                case SYS.GAL: return 7.2921151467e-5;
                case SYS.QZS: return 7.2921151467e-5;
                case SYS.BDS: return 7.292115e-5;
                case SYS.IRN: return 0.0;
                case SYS.SBS: return 0.0;
                default: return 0.0;
            }
        }

        public static double MU(SYS sys)   // 地心重力定数 [m^3s^{-2}]
        {
            switch (sys)
            {
                case SYS.GPS: return 3.986005e14;
                case SYS.GLO: return 3.9860044e14;
                case SYS.GAL: return 3.986004418e14;
                case SYS.QZS: return 3.986005e14;
                case SYS.BDS: return 3.986004418e14;
                case SYS.IRN: return 0.0;
                case SYS.SBS: return 0.0;
                default: return 0.0;
            }
        }

        public static double FF(SYS sys)    // 相対論補正係数
        {
            switch (sys)
            {
                case SYS.GPS: return -4.442807633e-10;
                case SYS.GLO: return 0.0;
                case SYS.GAL: return -4.442807309e-10;
                case SYS.QZS: return -4.442807633e-10;
                case SYS.BDS: return -4.442807309e-10;
                case SYS.IRN: return 0.0;
                case SYS.SBS: return 0.0;
                default: return 0.0;
            }

        }


    }
}
