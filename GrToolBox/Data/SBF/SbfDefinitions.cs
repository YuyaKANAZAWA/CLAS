namespace GrToolBox.Data.SBF
{
    public class SbfDefinitions
    {
        public enum SbfSignalType : byte
        {
            G_L1CA = 0,     // GPS L1 C/A
            G_L1P = 1,      // GPS L1 P(Y)
            G_L2P = 2,      // GPS L2 P(Y)
            G_L2C = 3,      // GPS L2C
            G_L5 = 4,       // GPS L5 I/Q
            G_L1C = 5,      // GPS L1C
            J_L1CA = 6,     // QZS L1 C/A
            J_L2C = 7,      // QZS L2C
            R_L1CA = 8,     // GLONASS L1 C/A
            R_L1P = 9,      // GLONASS L1 P
            R_L2P = 10,     // GLONASS L2 P
            R_L2CA = 11,    // GLONASS L2 C/A
            R_L3 = 12,      // GLONASS L3
            C_B1C = 13,     // BeiDou B1C
            C_B2a = 14,     // BeiDou B2a
            I_L5 = 15,      // IRNSS L5
            // 16 reserved
            E_E1BC = 17,    // Galileo L1 BC
            // 18 reserved
            E_E6BC = 19,    // Galileo E6 BC
            E_E5a = 20,     // Galileo E5a I/Q
            E_E5b = 21,     // Galileo E5b I/Q
            E_E5 = 22,      // Galileo E5 AltBOC
            MSS = 23,       // MSS L-Band signal
            S_L1CA = 24,    // SBAS L1 C/A
            S_L5 = 25,      // SBAS L5
            J_L5 = 26,
            J_L6 = 27,      // QZSS L6
            C_B1I = 28,     // BeiDou B1I
            C_B2I = 29,     // BeiDou B2b
            C_B3I = 30,     // BeiDou B3
            // 31 reserved
            J_L1C = 32,     // QZS L1C
            J_L1S = 33,     // QZS L1S
            C_B2b = 34      // BeiDou B2b
        }





    }
}
