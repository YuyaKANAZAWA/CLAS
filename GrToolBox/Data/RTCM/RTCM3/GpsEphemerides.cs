using GrToolBox.Data.GrNavData;
using GrToolBox.Satellite;
using GrToolBox.Time;
using System.Diagnostics;
using System.Text;
using static GrToolBox.Common.Constants;
using static GrToolBox.Data.DataUtilities;

namespace GrToolBox.Data.RTCM.RTCM3
{
    /// <summary>
    /// RTCM3 1019
    /// </summary>
    public class GpsEphemerides : FrameInfo_MessageNum, IRawData
    {
        //public int MessageNum { get; set; }             // Message Number
        public int SatID { get; set; }          // uint6, DF009, GPS Satellite ID
        public int WeekNum { get; set; }        // uint10, DF076, GPS Week Number
        public int SvAccuracy { get; set; }     // uint4, DF077, SV accuracy
        public int CodeOnL2 { get; set; }       // bit(2), DF078, GPS Code on L2
        public int Idot { get; set; }           // int14, DF079, IDOT
        public byte Iode { get; set; }          // uint8, DF071, IODE
        public int Toc { get; set; }            // uint16, DF081, TOC
        public int A2 { get; set; }             // int8, DF082, SV clock drift rate
        public int A1 { get; set; }             // int16, DF083, SV clock drift
        public int A0 { get; set; }             // int22, DF084, SV clock bias
        public int Iodc { get; set; }           // uint10, DF085, IODC Issue of Data, Clock 
        public int Crs { get; set; }            // int16, DF086, Crs
        public int Deln { get; set; }           // int16, DF087, Delta n 
        public int M0 { get; set; }             // int32, DF088,  M0 
        public int Cuc { get; set; }            // int16, DF089, Cuc
        public uint Ecc { get; set; }           // uint32, DF090; e Eccentricity
        public int Cus { get; set; }            // int16, DF091, Cus
        public uint SqrtA { get; set; }         // uint32, DF092, sqrt(A)
        public int Toe { get; set; }            // uint16, DF093, Toe Time of Ephemeris
        public int Cic { get; set; }            // int16, DF094, Cic
        public int Omega0 { get; set; }         // int32, DF095, OMEGA0
        public int Cis { get; set; }            // int16, DF096, Cis
        public int I0 { get; set; }             // int32, DF097, i0
        public int Crc { get; set; }            // int16, DF098, Crc
        public int Omega { get; set; }          // int32, DF099, Omega
        public int OmegaDot { get; set; }       // int24, DF100, OMEGA DOT
        public int Tgd { get; set; }            // int8, DF101, TGD
        public byte SvHealth { get; set; }      // uint6, DF102, SV health
        public bool L2P_Flag { get; set; }      // bit(1), Df103, L2 P data flag
        public bool FitIntFlg { get; set; }     // bit(1), DF137, Fit Interval

        private DateTime StartTime { get; set; }

        /// <summary>
        /// RTCM3 1019 GPS Ephemerides
        /// </summary>
        public GpsEphemerides() { }

        /// <summary>
        /// RTCM3 1019 GPS Ephemerides
        /// </summary>
        /// <param name="byteData">byte array of sentence data</param>
        /// <param name="f">instance of FrameInfo_MessageNum</param>
        public GpsEphemerides(byte[] byteData, FrameInfo_MessageNum f)
        {
            Decode(byteData, f);
        }

        public int GetID()
        {
            return MessageNum;
        }

        public Satellites? GetMeas()
        {
            return null;
        }

        public GrNavBase? GetNav()
        {
            return Rtcm1019_to_GR();
        }

        public void SetApproxStartDateTime(DateTime t)
        {
            StartTime = t;
        }


        private void Decode(byte[] byteData, FrameInfo_MessageNum f)
        {
            MessageLen = f.MessageLen;
            MessageNum = f.MessageNum;

            int offset = 24;                                    // frame header: 3byte=24bit, skip
            offset += 12;                                       // Message Number, uint12, DF002, skip

            SatID = (int)BitToUint(byteData, offset, 6);            offset += 6;
            WeekNum = (int)BitToUint(byteData, offset, 10);         offset += 10;
            SvAccuracy = (int)BitToUint(byteData, offset, 4);       offset += 4;
            CodeOnL2 = (int)BitToUint(byteData, offset, 2);         offset += 2;
            Idot = BitToInt(byteData, offset, 14);                  offset += 14;
            Iode = (byte)BitToUint(byteData, offset, 8);            offset += 8;
            Toc = (int)BitToUint(byteData, offset, 16);             offset += 16;
            A2 = BitToInt(byteData, offset, 8);                     offset += 8;
            A1 = BitToInt(byteData, offset, 16);                    offset += 16;
            A0 = BitToInt(byteData, offset, 22);                    offset += 22;
            Iodc = (int)BitToUint(byteData, offset, 10);            offset += 10;
            Crs = BitToInt(byteData, offset, 16);                   offset += 16;
            Deln = BitToInt(byteData, offset, 16);                  offset += 16;
            M0 = BitToInt(byteData, offset, 32);                    offset += 32;
            Cuc = BitToInt(byteData, offset, 16);                   offset += 16;
            Ecc = BitToUint(byteData, offset, 32);                  offset += 32;
            Cus = BitToInt(byteData, offset, 16);                   offset += 16;
            SqrtA = BitToUint(byteData, offset, 32);                offset += 32;
            Toe = (int)BitToUint(byteData, offset, 16);             offset += 16;
            Cic = BitToInt(byteData, offset, 16);                   offset += 16;
            Omega0 = BitToInt(byteData, offset, 32);                offset += 32;
            Cis = BitToInt(byteData, offset, 16);                   offset += 16;
            I0 = BitToInt(byteData, offset, 32);                    offset += 32;
            Crc = BitToInt(byteData, offset, 16);                   offset += 16;
            Omega = BitToInt(byteData, offset, 32);                 offset += 32;
            OmegaDot = BitToInt(byteData, offset, 24);              offset += 24;
            Tgd = BitToInt(byteData, offset, 8);                    offset += 8;
            SvHealth = (byte)BitToUint(byteData, offset, 6);        offset += 6;
            L2P_Flag = BitToBool(byteData, offset);                 offset += 1;
            FitIntFlg = BitToBool(byteData, offset);         offset += 1;
        }

        private GpsNavData Rtcm1019_to_GR()
        {
            var nav = new GpsNavData();

            nav.Prn = new Common.GrPrn(SatID);

            if(StartTime == DateTime.MinValue)
            {
                StartTime = DateTime.Now;
            }

            TimeSpan oneYear = TimeSpan.FromDays(365);
            TimeGR tb = new(StartTime - oneYear);
            TimeGR te = new(StartTime + oneYear);

            int nRollOver_b = tb.Week / 1024;
            int nRollOver_e = te.Week / 1024;

            if(nRollOver_b == nRollOver_e)                      // 前後1年でロールオーバー無し
            {
                nav.WN_toc = WeekNum + (nRollOver_b * 1024);    // week number associated with toc (continuous); GLO,GAL,QZS,BDS: in GPS time frame
            }
            else
            {
                nav.WN_toc = (WeekNum < 160)? WeekNum + (nRollOver_e * 1024) : WeekNum + (nRollOver_b * 1024);
            }

            nav.Toc = Toc * 16;                         // SF=2^4,  Toc(sow); GLO,GAL,QZS,BDS: in GPS time frame
            TimeGR toc_gr = new(nav.WN_toc, nav.Toc);
            nav.Year = toc_gr.Year;                     // toc year; GLO,GAL,QZS,BDS: in GPS time frame
            nav.Month = toc_gr.Month;                   //     month
            nav.Day = toc_gr.Day;                       //     day
            nav.Hour = toc_gr.Hour;                     //     hour
            nav.Minute = toc_gr.Minute;                 //     minute
            nav.Second = toc_gr.Second;                 //     second
            nav.A0 = A0 * POW2_M31;                     // SF=2^{-31}; SV clock bias (seconds), GAL,QZS,BDS
            nav.A1 = A1 * POW2_M43;                     // SF=2^{-43}; SV clock drift (sec/sec), GAL,QZS,BDS
            nav.A2 = A2 * POW2_M55;                     // SF=2^{-55}; SV clock drift rate (sec/sec2), GAL,QZS,BDS
            //
            nav.Iode = Iode;                            // IODE Issue of Data, Ephemeris
            nav.Crs = Crs * POW2_M5;                    // SF=2^{-5}; Crs (meters), GAL,QZS,BDS
            nav.Deln = Deln * POW2_M43 * PI;            // SF=2^{-43}; PI (SC/sec)-->>(rad/sec); Delta n (rad/sec), GAL,QZS,BDS
            nav.M0 = M0 * POW2_M31 * PI;                // SF=2^{-31}; PI (SC)-->>(rad); M0 (rad), GAL,QZS,BDS
            //
            nav.Cuc = Cuc * POW2_M29;                   // SF=2^{-31}; Cuc (rad), GAL,QZS,BDS
            nav.Ecc = Ecc * POW2_M33;                   // SF=2^{-33}; e Eccentricity, GAL,QZS,BDS
            nav.Cus = Cus * POW2_M29;                   // SF=2^{-29}; Cus (radians), GAL,QZS,BDS
            nav.SqrtA = SqrtA * POW2_M19;               // SF=2^{-19}; sqrt(A) (sqrt(m)), GAL,QZS,BDS
            //
            nav.Toe = Toe * 16;                         // SF=2^{4}; Toe Time of Ephemeris (sec of GPS week), GAL,QZS,BDS
            nav.Cic = Cic * POW2_M29;                   // SF=2^{-29}; Cic (radians), GAL,QZS,BDS
            nav.Omega0 = Omega0 * POW2_M31 * PI;        // SF=2^{-31}; PI (SC)-->>(rad); OMEGA0 (radians), GAL,QZS,BDS
            nav.Cis = Cis * POW2_M29;                   // SF=2^{-29}; Cis (radians), GAL,QZS,BDS
            nav.I0 = I0 * POW2_M31 * PI;                // SF=2^{-31}; PI (SC)-->>(rad); i0 (radians), GAL,QZS,BDS
            nav.Crc = Crc * POW2_M5;                    // SF=2^{-5}; Crc (meters), GAL,QZS,BDS
            nav.Omega = Omega * POW2_M31 * PI;          // SF=2^{-31}; PI (SC)-->>(rad); Omega (radians), GAL,QZS,BDS
            nav.OmegaDot = OmegaDot * POW2_M43 * PI;    // SF=2^{-43}; PI (SC/sec)-->>(rad/sec); OMEGA DOT (rad/sec), GAL,QZS,BDS
            //
            nav.Idot = Idot * POW2_M43 * PI;            // F=2^{-43}; PI (SC/sec)-->>(rad/sec); IDOT (rad/sec), GAL,QZS,BDS
            nav.CodeOnL2 = (byte)CodeOnL2;              // Codes on L2 channel
            nav.WeekNum = nav.WN_toc;                   // GPS Week # (to go with TOE) Continuous number, not mod(1024), GAL,QZS,BDS:in GPS time frame
            nav.L2P_Flag = L2P_Flag;
                                                        // SV accuracy (meters) look up table: see IS-GPS-200K Section 20.3.3.3.1.3
            nav.SvAccuracy = (SvAccuracy <= 15) ? DataConstants.GpsSvAccuracy[SvAccuracy] : 99999.0;
            nav.SvHealth = SvHealth;                    // GPS: SV health (bits 17-22 w 3 sf 1); GLO: 1-bit health flag, 0=healthy, 1=unhealthy
            //
            nav.Tgd = Tgd * POW2_M31;                   // SF=2^{-31}; TGD (seconds)
            nav.Iodc = Iodc;                            // IODC Issue of Data, Clock, BDS:AODC(5bit data)
            nav.Ttm = 0;                                // WeekNum(toe)から見たタイムスタンプ(sis) -->> 1004のTOWが必要．保留
                                                        //nav.Ttm = (int)Tow + (WNc - wn_toe_C) * 604800;
            if (!FitIntFlg)
            {
                nav.FitInterval = 4;
            }
            else if (nav.Iodc >= 240 && nav.Iodc <= 247) nav.FitInterval = 8;
            else if (nav.Iodc >= 248 && nav.Iodc <= 255 || nav.Iodc == 496) nav.FitInterval = 14;
            else if (nav.Iodc >= 497 && nav.Iodc <= 503 || nav.Iodc >= 1021 && nav.Iodc <= 1023) nav.FitInterval = 26;
            else if (nav.Iodc >= 504 && nav.Iodc <= 510) nav.FitInterval = 50;
            else if (nav.Iodc >= 752 && nav.Iodc <= 756 || nav.Iodc == 511) nav.FitInterval = 74;
            else if (nav.Iodc == 757) nav.FitInterval = 98;
            else nav.FitInterval = 6;

            return nav;
        }


        public new void DebugPrint()
        {
            StringBuilder sb = new StringBuilder();
            PrintTo(sb);
            Debug.WriteLine(sb);
        }

        public new void PrintTo(StringBuilder sb)
        {
            var nav = Rtcm1019_to_GR();
            sb.Append("-------------------------------------------------\n");
            sb.Append("messageNum:     " + MessageNum + "\n");
            sb.Append(nav.Prn?.Snn);
            sb.AppendFormat($" A0:  {nav.A0,19:0.000000000000E+00}");
            sb.AppendFormat($" A1:      {nav.A1,19:0.000000000000E+00}");
            sb.AppendFormat($" A2:     {nav.A2,19:0.000000000000E+00}");
            sb.AppendFormat("\n");
            sb.AppendFormat($"    Iode:{nav.Iode,19:0.000000000000E+00}");
            sb.AppendFormat($" Crs:     {nav.Crs,19:0.000000000000E+00}");
            sb.AppendFormat($" Deln:   {nav.Deln,19:0.000000000000E+00}");
            sb.AppendFormat($" M0:     {nav.M0,19:0.000000000000E+00}");
            sb.AppendFormat("\n");
            sb.AppendFormat($"    Cuc: {nav.Cuc,19:0.000000000000E+00}");
            sb.AppendFormat($" Ecc:     {nav.Ecc,19:0.000000000000E+00}");
            sb.AppendFormat($" Cus:    {nav.Cus,19:0.000000000000E+00}");
            sb.AppendFormat($" SqrtA:  {nav.SqrtA,19:0.000000000000E+00}");
            sb.AppendFormat("\n");
            sb.AppendFormat($"    Toe: {nav.Toe,19:0.000000000000E+00}");
            sb.AppendFormat($" Cic:     {nav.Cic,19:0.000000000000E+00}");
            sb.AppendFormat($" Omega0: {nav.Omega0,19:0.000000000000E+00}");
            sb.AppendFormat($" Cis:    {nav.Cis,19:0.000000000000E+00}");
            sb.AppendFormat("\n");
            sb.AppendFormat($"    I0:  {nav.I0,19:0.000000000000E+00}");
            sb.AppendFormat($" Crc:     {nav.Crc,19:0.000000000000E+00}");
            sb.AppendFormat($" Omega:  {nav.Omega,19:0.000000000000E+00}");
            sb.AppendFormat($" Omgdot: {nav.OmegaDot,19:0.000000000000E+00}");
            sb.AppendFormat("\n");
            sb.AppendFormat($"    Idot:{nav.Idot,19:0.000000000000E+00}");
            sb.AppendFormat($" CodeOnL2:{nav.CodeOnL2,19:0.000000000000E+00}");
            sb.AppendFormat($" WeekNum:{nav.WeekNum,19:0.000000000000E+00}");
            sb.AppendFormat($" L2PFlag: {nav.L2P_Flag,18:0.000000000000E+00}");
            sb.AppendFormat("\n");
            sb.AppendFormat($"    SvAcc:{nav.SvAccuracy,18:0.000000000000E+00}");
            sb.AppendFormat($" Health:  {nav.SvHealth,19:0.000000000000E+00}");
            sb.AppendFormat($" Tgd:    {nav.Tgd,19:0.000000000000E+00}");
            sb.AppendFormat($" Iodc:    {nav.Iodc,18:0.000000000000E+00}");
            sb.AppendFormat("\n");
            sb.AppendFormat($"    TTM:  Not Specified");
            sb.AppendFormat($"      FitInt:  {nav.FitInterval,19:0.000000000000E+00}");
            sb.AppendFormat("\n");

        }
    }
}
