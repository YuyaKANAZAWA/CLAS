using GrToolBox.Data.GrNavData;
using GrToolBox.Satellite;
using GrToolBox.Time;
using System.Diagnostics;
using System.Text;
using static GrToolBox.Common.Constants;
using static GrToolBox.Data.DataUtilities;
using static GrToolBox.Time.TimeUtilities;
using static GrToolBox.Data.RTCM.RTCM3.Rtcm3Utilities;
using static GrToolBox.Common.CommonDefinitions;

namespace GrToolBox.Data.RTCM.RTCM3
{
    /// <summary>
    /// RTCM3 1020
    /// </summary>
    public class GloEphemerides : FrameInfo_MessageNum, IRawData
    {
        //public int MessageNum { get; set; }             // Message Number
        private int SatID { get; set; }                 // uint6, DF038; GLONASS satellite ID(Satellite Slot Number)
        private int FreqChNum { get; set; }             // uint5, DF040; GLONASS Satellite Frequency Channel Number
        private bool AlmHealth { get; set; }            // bit(1), Df104; GLONASS almanac health
        private bool AlmHealthAvailInd { get; set; }    // bit(1), DF105; GLONASS almanac health availability indicator
        private byte P1 { get; set; }                   // bit(2), DF106; GLONASS P1 word
        private ushort Tk { get; set; }                 // bit(12), DF107; Time referenced to the beggning of GLONASS subframe within the current day
        private bool MsbBnWord { get; set; }            // bit(1), DF108; ephemeris health flag
        private bool P2 { get; set; }                   // bit(1), DF109; 
        private int Tb { get; set; }                    // uint7, DF110; Time to which GLONASS navigation data are referenced
        private int X_vel { get; set; }                 // intS24, DF111; ECEF-X velocity in PZ-90
        private int X_cod { get; set; }                 // intS27, DF112; ECEF-X coordinate in PZ-90
        private int X_acc { get; set; }                 // intS5, DF113; ECEF-X acceleration in PZ-90
        private int Y_vel { get; set; }                 // intS24, DF114; ECEF-Y velocity in PZ-90
        private int Y_cod { get; set; }                 // intS27, DF115; ECEF-Y coordinate in PZ-90
        private int Y_acc { get; set; }                 // intS5, DF116; ECEF-Y acceleration in PZ-90
        private int Z_vel { get; set; }                 // intS24, DF117; ECEF-Z velocity in PZ-90
        private int Z_cod { get; set; }                 // intS27, DF118; ECEF-Z coordinate in PZ-90
        private int Z_acc { get; set; }                 // intS5, DF119; ECEF-Z acceleration in PZ-90
        private bool P3 { get; set; }                   // bit(1), DF120; 
        private int Gamma { get; set; }                 // intS11, DF121; \gamma_n(t_b) : relative deviation of predicted carrier frequency [1Hz/Hz]
        private byte M_P { get; set; }                  // bit(2), DF122; 
        private bool M_ln3 { get; set; }                // bit(1), DF123; GLONASS-M ln word extracted from third string of the subframe
        private int Tau { get; set; }                   // intS22, DF124; \tau_n(t_b): correction to the satellite time relative to GLONASS system time
        private int M_DelTau { get; set; }              // intS5, DF125; time difference between navigation RF signal transmitted in L2 sub-band and navigation RF signal transmitted in L1 suub-band
        private byte E { get; set; }                    // uint5, DF126; age of GLONASS navigation data
        private bool M_P4 { get; set; }                 // bit(1), DF127; 
        private int M_Ft { get; set; }                  // uint4, DF128; GLONASS-M predicted satellite user range accuracy at time t_b
        private int M_Nt { get; set; }                  // uint11, DF129; GLONASS calender number of day within four-year interval starting from the January 1 in a leap year
        private byte M_M { get; set; }                  // bit(2), DF130; Type of GLONASS satellite (01: M or 00: not M)
        private bool AAD { get; set; }                  // bit(1), DF131; Availability of Additional Data
                                                        // rest of parameters (DF132-136) contain data or not (true: additional parameters are in the message / false: arbitrary values)
        private int Na { get; set; }                    // uint11, DF132; GLONASS calender number of day within the four-year period to which \tau_c referenced
        private int TauC { get; set; }                  // intS32, DF133; difference between GLONASS system tie and UTC(SU)
        private int M_N4 { get; set; }                  // uint5, DF134; GLONASS four year interval number starting from 1996
        private int M_TauGps { get; set; }              // intS22, DF135; correction to GPS system time relative to GLONASS system time
        private bool M_ln5 { get; set; }                // bit(1), DF126; GLONASS-M ln word extracted from fifth string of the subframe

        private DateTime StartTime { get; set; }


        /// <summary>
        /// RTCM3 1020 GLONASS Ephemerides
        /// </summary>
        public GloEphemerides() { }

        /// <summary>
        /// RTCM3 1020 GLONASS Ephemerides
        /// </summary>
        /// <param name="byteData">byte array of sentence data</param>
        /// <param name="f">instance of FrameInfo_MessageNum</param>
        public GloEphemerides(byte[] byteData, FrameInfo_MessageNum f)
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
            return Rtcm1020_to_GR();
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
            FreqChNum = (int)BitToUint(byteData, offset, 5);        offset += 5;
            AlmHealth = BitToBool(byteData, offset);                offset++;
            AlmHealthAvailInd = BitToBool(byteData, offset);        offset++;
            P1 = (byte)BitToUint(byteData, offset, 2);              offset += 2;
            Tk = (ushort)BitToUint(byteData, offset, 12);           offset += 12;
            MsbBnWord = BitToBool(byteData, offset);                offset++;
            P2 = BitToBool(byteData, offset);                       offset++;
            Tb = (int)BitToUint(byteData, offset, 7);               offset += 7;
            X_vel = BitToIntS(byteData, offset, 24);                offset += 24;
            X_cod = BitToIntS(byteData, offset, 27);                offset += 27;
            X_acc = BitToIntS(byteData, offset, 5);                 offset += 5;
            Y_vel = BitToIntS(byteData, offset, 24);                offset += 24;
            Y_cod = BitToIntS(byteData, offset, 27);                offset += 27;
            Y_acc = BitToIntS(byteData, offset, 5);                 offset += 5;
            Z_vel = BitToIntS(byteData, offset, 24);                offset += 24;
            Z_cod = BitToIntS(byteData, offset, 27);                offset += 27;
            Z_acc = BitToIntS(byteData, offset, 5);                 offset += 5;
            P3 = BitToBool(byteData, offset);                       offset++;
            Gamma = BitToIntS(byteData, offset, 11);                offset += 11;
            M_P = (byte)BitToUint(byteData, offset, 2);             offset += 2;
            M_ln3 = BitToBool(byteData, offset);                    offset++;
            Tau = BitToIntS(byteData, offset, 22);                  offset += 22;
            M_DelTau = BitToIntS(byteData, offset, 5);              offset += 5;
            E = (byte)BitToUint(byteData, offset, 5);               offset += 5;
            M_P4 = BitToBool(byteData, offset);                     offset++;
            M_Ft = (int)BitToUint(byteData, offset, 4);             offset += 4;
            M_Nt = (int)BitToUint(byteData, offset, 11);            offset += 11;
            M_M = (byte)BitToUint(byteData, offset, 2);             offset += 2;
            AAD = BitToBool(byteData, offset);                      offset++;
            Na = (int)BitToUint(byteData, offset, 11);              offset += 11;
            TauC = BitToIntS(byteData, offset, 32);                 offset += 32;
            M_N4 = (int)BitToUint(byteData, offset, 5);             offset += 5;
            M_TauGps = BitToIntS(byteData, offset, 22);             offset += 22;
            M_ln5 = BitToBool(byteData,offset);                     //offset++;
        }

        private GloNavData Rtcm1020_to_GR()
        {
            var nav = new GloNavData();
            
            nav.Prn = new Common.GrPrn(SATID2SNN(SatID, SYS.GLO));

            if (StartTime == DateTime.MinValue)
            {
                StartTime = DateTime.Now;
            }

            DateTime Tb_datetime = Rtcm3Utilities.GetGlonassEpochTime(StartTime, M_Nt);
            TimeSpan tocMinutes = TimeSpan.FromMinutes(Tb * 15);
            Tb_datetime += tocMinutes;    // glonass time frame
            Tb_datetime -= TimeSpan.FromHours(3);   // UTC
            Tb_datetime += TimeSpan.FromSeconds(GetLeapSecond(Tb_datetime));    // GPS time frame

            var weeksow = DateTime2WeekSow(Tb_datetime);

            nav.WN_toc = weeksow[0];
            nav.Toc = weeksow[1];                           // Toc(sow); GLO,GAL,QZS,BDS: in GPS time frame
            nav.Year = Tb_datetime.Year;                    // toc year; GLO,GAL,QZS,BDS: in GPS time frame
            nav.Month = Tb_datetime.Month;                  //     month
            nav.Day = Tb_datetime.Day;                      //     day
            nav.Hour = Tb_datetime.Hour;                    //     hour
            nav.Minute = Tb_datetime.Minute;                //     minute
            nav.Second = Tb_datetime.Second;                //     second
            nav.FreqNr = FreqChNum - 7;                     // -7..13
            nav.Tau = Tau * POW2_M30;                       // SF=2^{-30}, DF124; [sec]
            nav.Gamma = Gamma * POW2_M40;                   // SF=2^{-40}, DF121; [1Hz/Hz]
            nav.X = X_cod * POW2_M11;                       // SF=2^{-11}, DF112; [km]
            nav.Y = Y_cod * POW2_M11;                       // SF=2^{-11}, DF115; [km]
            nav.Z = Z_cod * POW2_M11;                       // SF=2^{-11}, DF118; [km]
            nav.Dx = X_vel * POW2_M20;                      // SF=2^{-20}, DF111; [km/s]
            nav.Dy = Y_vel * POW2_M20;                      // SF=2^{-20}, DF114; [km/s]
            nav.Dz = Z_vel * POW2_M20;                      // SF=2^{-20}, DF117; [km/s]
            nav.Ddx = X_acc * POW2_M30;                     // SF=2^{-30}, DF113; [km/s^2]
            nav.Ddy = Y_acc * POW2_M30;                     // SF=2^{-30}, DF116; [km/s^2]
            nav.Ddz = Z_acc * POW2_M30;                     // SF=2^{-30}, DF119; [km/s^2]
            nav.E = E;                                      // SF=1; DF126, [day]
            nav.SvHealth = (!M_ln3 && AlmHealthAvailInd && AlmHealth)? (byte)0 : (byte)1;   // see glonass icd

            var mtf_h = (Tk >> 7);
            //var mtf_m = 

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
            var nav = Rtcm1020_to_GR();
            sb.Append("-------------------------------------------------\n");
            sb.Append("messageNum:     " + MessageNum + "\n");
            sb.Append(nav.Prn?.Snn);
            sb.Append(" ");
            sb.Append(nav.Year);
            sb.Append(" ");
            sb.Append($"  {nav.Month:00}");
            sb.Append(" ");
            sb.Append($"  {nav.Day:00}");
            sb.Append(" ");
            sb.Append($"  {nav.Hour:00}");
            sb.Append(" ");
            sb.Append($"  {nav.Minute:00}");
            sb.Append(" ");
            sb.Append($"  {nav.Second:00}");
            sb.Append(" ");
            sb.AppendFormat($"Tau:  {nav.Tau,19:0.000000000000E+00}");
            sb.Append(" ");
            sb.AppendFormat($"Gam:  {nav.Gamma,19:0.000000000000E+00}");
            sb.Append(" ");
            sb.AppendFormat($"MFT:  {nav.Gamma,19:0.000000000000E+00}");
            sb.Append("\n");
            //
            sb.Append("    ");
            sb.AppendFormat($"  X:  {nav.X,19:0.000000000000E+00}");
            sb.Append(" ");
            sb.AppendFormat($"X_v:  {nav.Dx,19:0.000000000000E+00}");
            sb.Append(" ");
            sb.AppendFormat($"X_a:  {nav.Ddx,19:0.000000000000E+00}");
            sb.Append(" ");
            sb.AppendFormat($"Hea:  {nav.SvHealth,19:0.000000000000E+00}");
            sb.Append("\n");
            //
            sb.Append("    ");
            sb.AppendFormat($"  Y:  {nav.Y,19:0.000000000000E+00}");
            sb.Append(" ");
            sb.AppendFormat($"Y_v:  {nav.Dy,19:0.000000000000E+00}");
            sb.Append(" ");
            sb.AppendFormat($"Y_a:  {nav.Ddy,19:0.000000000000E+00}");
            sb.Append(" ");
            sb.AppendFormat($"Frq:  {nav.FreqNr,19:0.000000000000E+00}");
            sb.Append("\n");
            //
            sb.Append("    ");
            sb.AppendFormat($"  Z:  {nav.Z, 19:0.000000000000E+00}");
            sb.Append(" ");
            sb.AppendFormat($"Z_v:  {nav.Dz, 19:0.000000000000E+00}");
            sb.Append(" ");
            sb.AppendFormat($"Z_a:  {nav.Ddz, 19:0.000000000000E+00}");
            sb.Append(" ");
            sb.AppendFormat($"Age:  {nav.E, 19:0.000000000000E+00}");
            sb.Append("\n");
        }
    }
}
