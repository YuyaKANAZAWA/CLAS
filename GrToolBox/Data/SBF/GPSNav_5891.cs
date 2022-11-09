using GrToolBox.Common;
using GrToolBox.Data.GrNavData;
using GrToolBox.Satellite;
using System.Diagnostics;
using System.Text;
using static GrToolBox.Common.Constants;
using static GrToolBox.Data.SBF.SbfUtilities;
using static GrToolBox.Settings.SettingsDefinitions;

namespace GrToolBox.Data.SBF
{
    public class GPSNav_5891 : Header_TimeStamp, IRawData
    {
        private byte PRN { get; set; }           // ID of the GPS satellite of which the ephemeris is given in this block
        private byte Reserved { get; set; }      // 
        private ushort WN { get; set; }          // Week number (10 bits from subframe 1, word 3)
        private byte CAorPonL2 { get; set; }     // Code(s) on L2 channel (2 bits from subframe 1, word 3)
        private byte URA { get; set; }           // User Range accuracy index (4 bits from subframe 1 word 3)
        private byte Health { get; set; }        // 6-bit health from subframe 1, word 3 (6 bits from subframe 1, word 3)
        private byte L2DataFlag { get; set; }    // Data flag for L2 P-code (1 bit from subframe 1, word 4)
        private ushort IODC { get; set; }        // Issue of data, clock (10 bits from subframe 1)
        private byte IODE2 { get; set; }         // Issue of data, ephemeris (8 bits from subframe 2)
        private byte IODE3 { get; set; }         // Issue of data, ephemeris (8 bits from subframe 3)
        private byte FitIntFlg { get; set; }     // Curve Fit Interval, (1 bit from subframe 2, word 10)
        private byte Reserved2 { get; set; }     // 
        private float Tgd { get; set; }          // Estimated group delay differential [sec]
        private uint Toc { get; set; }           // clock data reference time [sec]
        private float A_f2 { get; set; }         // SV clock aging [1 s / s2]
        private float A_f1 { get; set; }         // SV clock drift [1 s / s]
        private float A_f0 { get; set; }         // SV clock bias [1 s]
        private float C_rs { get; set; }         // Amplitude of the sine harmonic correction term to the orbit radius [m]
        private float DEL_N { get; set; }        // Mean motion difference from computed value [semi-circle / s]
        private double M0 { get; set; }          // Mean anomaly at reference time [semi-circle]
        private float C_uc { get; set; }         // Amplitude of the cosine harmonic correction term to the argument of latitude [rad]
        private double E { get; set; }           // Eccentricity
        private float C_us { get; set; }         // Amplitude of the sine harmonic correction term to the argument of latitude [rad]
        private double SQRT_A { get; set; }      // Square root of the semi-major axis [m^{1/2}]
        private uint Toe { get; set; }           // Reference time ephemeris [s]
        private float C_ic { get; set; }         // Amplitude of the cosine harmonic correction term to the angle of inclination [rad]
        private double OMEGA_0 { get; set; }     // Longitude of ascending node of orbit plane at weekly epoch [semi-circle]
        private float C_is { get; set; }         // Amplitude of the sine harmonic correction term to the angle of inclination [rad]
        private double I_0 { get; set; }         // Inclination angle at reference time [semi-circle]
        private float C_rc { get; set; }         // Amplitude of the cosine harmonic correction term to the orbit radius [m]
        private double OMEGA { get; set; }       // Argument of perigee [semi-circle]
        private float OMEGADOT { get; set; }     // Rate of right ascension [semi-circle / s]
        private float IDOT { get; set; }         // Rate of inclination angle [semi-circle / s]
        private ushort WNt_oc { get; set; }      // WN associated with t_oc, modulo 1024
        private ushort WNt_oe { get; set; }      // WN associated with t_oe, modulo 1024

        public GPSNav_5891() { }

        public GPSNav_5891(byte[] byteData, Header_TimeStamp h)
        {
            Decode(byteData, h);
        }

        public int GetID()
        {
            return 5891;
        }

        public Satellites? GetMeas()
        {
            return null;
        }

        public GrNavBase GetNav()
        {
            return Sbf2Nav();
        }


        private void Decode(byte[] byteData, Header_TimeStamp h)
        {
            int pos = 14;
            byte prn = byteData[pos]; pos++;
            byte reserved = byteData[pos]; pos++;
            byte[] wn = byteData[pos..(pos + 2)]; pos += 2;
            byte caorPonL2 = byteData[pos]; pos++;
            byte ura = byteData[pos]; pos++;
            byte health = byteData[pos]; pos++;
            byte l2DataFlag = byteData[pos]; pos++;
            byte[] iodc = byteData[pos..(pos + 2)]; pos += 2;
            byte iode2 = byteData[pos]; pos++;
            byte iode3 = byteData[pos]; pos++;
            byte fitIntFlg = byteData[pos]; pos++;
            byte reserved2 = byteData[pos]; pos++;
            byte[] tgd = byteData[pos..(pos + 4)]; pos += 4;
            byte[] toc = byteData[pos..(pos + 4)]; pos += 4;
            byte[] a_f2 = byteData[pos..(pos + 4)]; pos += 4;
            byte[] a_f1 = byteData[pos..(pos + 4)]; pos += 4;
            byte[] a_f0 = byteData[pos..(pos + 4)]; pos += 4;
            byte[] c_rs = byteData[pos..(pos + 4)]; pos += 4;
            byte[] del_n = byteData[pos..(pos + 4)]; pos += 4;
            byte[] m0 = byteData[pos..(pos + 8)]; pos += 8;
            byte[] c_uc = byteData[pos..(pos + 4)]; pos += 4;
            byte[] e = byteData[pos..(pos + 8)]; pos += 8;
            byte[] c_us = byteData[pos..(pos + 4)]; pos += 4;
            byte[] sqrt_a = byteData[pos..(pos + 8)]; pos += 8;
            byte[] toe = byteData[pos..(pos + 4)]; pos += 4;
            byte[] c_ic = byteData[pos..(pos + 4)]; pos += 4;
            byte[] omega_0 = byteData[pos..(pos + 8)]; pos += 8;
            byte[] c_is = byteData[pos..(pos + 4)]; pos += 4;
            byte[] i_0 = byteData[pos..(pos + 8)]; pos += 8;
            byte[] c_rc = byteData[pos..(pos + 4)]; pos += 4;
            byte[] omega = byteData[pos..(pos + 8)]; pos += 8;
            byte[] omegadot = byteData[pos..(pos + 4)]; pos += 4;
            byte[] idot = byteData[pos..(pos + 4)]; pos += 4;
            byte[] wnt_oc = byteData[pos..(pos + 2)]; pos += 2;
            byte[] wnt_oe = byteData[pos..(pos + 2)]; pos += 2;

            Sync1 = h.Sync1;
            Sync2 = h.Sync2;
            BlockNum = h.BlockNum;
            BlockRev = h.BlockRev;
            Length = h.Length;
            Tow = h.Tow;
            WNc = h.WNc;
            PRN = prn;
            Reserved = reserved;
            WN = BitConverter.ToUInt16(wn);
            CAorPonL2 = caorPonL2;
            URA = ura;
            Health = health;
            L2DataFlag = l2DataFlag;
            IODC = BitConverter.ToUInt16(iodc);
            IODE2 = iode2;
            IODE3 = iode3;
            FitIntFlg = fitIntFlg;
            Reserved2 = reserved2;
            Tgd = BitConverter.ToSingle(tgd);
            Toc = BitConverter.ToUInt32(toc);
            A_f2 = BitConverter.ToSingle(a_f2);
            A_f1 = BitConverter.ToSingle(a_f1);
            A_f0 = BitConverter.ToSingle(a_f0);
            C_rs = BitConverter.ToSingle(c_rs);
            DEL_N = BitConverter.ToSingle(del_n);
            M0 = BitConverter.ToDouble(m0);
            C_uc = BitConverter.ToSingle(c_uc);
            E = BitConverter.ToDouble(e);
            C_us = BitConverter.ToSingle(c_us);
            SQRT_A = BitConverter.ToDouble(sqrt_a);
            Toe = BitConverter.ToUInt32(toe);
            C_ic = BitConverter.ToSingle(c_ic);
            OMEGA_0 = BitConverter.ToDouble(omega_0);
            C_is = BitConverter.ToSingle(c_is);
            I_0 = BitConverter.ToDouble(i_0);
            C_rc = BitConverter.ToSingle(c_rc);
            OMEGA = BitConverter.ToDouble(omega);
            OMEGADOT = BitConverter.ToSingle(omegadot);
            IDOT = BitConverter.ToSingle(idot);
            WNt_oc = BitConverter.ToUInt16(wnt_oc);
            WNt_oe = BitConverter.ToUInt16(wnt_oe);
        }

        private GpsNavData Sbf2Nav()
        {
            GpsNavData nav = new();

            byte iodc_LSB8 = (byte)(IODC & 0x00ff);             // see ICD-GPS-200, 8 LSB of IODC, IODE in subframes 2 and 3 must be the same 
            if (iodc_LSB8 != IODE2 || iodc_LSB8 != IODE3)
            {
                return null;
            }

            int wn_toc_C = WNt_oc;
            while (true)                        // tocのWnはロールオーバーあり．週を跨いだ直後のtocは前週の場合があるので，sbfヘッダのWNcをそのまま使えない．と，思う
            {
                wn_toc_C += 1024;
                if (wn_toc_C < WNc) continue;
                if (Math.Abs(wn_toc_C - WNc) > 1)
                    wn_toc_C -= 1024;
                break;
            }
            int wn_toe_C = WNt_oe;
            while (true)                        // toeのWNロールオーバ処理
            {
                wn_toe_C += 1024;
                if (wn_toe_C < WNc) continue;
                if (Math.Abs(wn_toe_C - WNc) > 1)
                    wn_toe_C -= 1024;
                break;
            }

            var time = new Time.TimeGR(wn_toc_C, Toc);
            nav.Prn = new GrPrn(PRN, Data_Type.SBF);
            nav.Toc = Toc;
            nav.WN_toc = wn_toc_C;
            nav.Year = time.Year;
            nav.Month = time.Month;
            nav.Day = time.Day;
            nav.Hour = time.Hour;
            nav.Minute = time.Minute;
            nav.Second = time.Second;
            nav.A0 = A_f0;
            nav.A1 = A_f1;
            nav.A2 = A_f2;
            //*****************
            nav.Iode = IODE2;
            nav.Crs = C_rs;
            nav.Deln = DEL_N * PI;   // [sc/s] -->> [rad/s]
            nav.M0 = M0 * PI;        // [sc] -->> [rad]
                                     //*****************
            nav.Cuc = C_uc;
            nav.Ecc = E;
            nav.Cus = C_us;
            nav.SqrtA = SQRT_A;
            //*****************
            nav.Toe = (int)Toe;
            nav.Cic = C_ic;
            nav.Omega0 = OMEGA_0 * PI; // [sc] -->> [rad]
            nav.Cis = C_is;
            //*****************
            nav.I0 = I_0 * PI;       // [sc] -->> [rad]
            nav.Crc = C_rc;
            nav.Omega = OMEGA * PI;  // [sc] -->> [rad]
            nav.OmegaDot = OMEGADOT * PI;    // [sc/s] -->> [rad/s]
                                             //*****************
            nav.Idot = IDOT * PI;    // [sc/s] -->> [rad/s]
            nav.CodeOnL2 = CAorPonL2;
            nav.WeekNum = wn_toe_C;      // GPS Week # (to go with TOE) Continuous number, not mod(1024)
            nav.L2P_Flag = (L2DataFlag == 0x01);
            //*****************
                                            // SV accuracy (meters) look up table: see IS-GPS-200K Section 20.3.3.3.1.3
            nav.SvAccuracy = (URA <= 15) ? DataConstants.GpsSvAccuracy[URA] : 99999.0;
            nav.SvHealth = Health;
            nav.Tgd = Tgd;
            nav.Iodc = (int)IODC;
            //*****************
            nav.Ttm = (int)Tow + (WNc - wn_toe_C) * 604800;      // WeekNum(toe)から見たタイムスタンプ(sis)

            if (FitIntFlg == 0)
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

            //NavDataClasses.DebugPrint(gr);
            return nav;
        }


        public void PrintTo(StringBuilder sb)
        {
            sb.Append("-------------------------------------------------\n");
            sb.Append($"SBF({BlockNum}), {Length:000} bytes\n");
            sb.Append($"{SVID2SNN(PRN),4}");
            sb.AppendFormat($" A0:  {A_f0,19:0.000000000000E+00}");
            sb.AppendFormat($" A1:      {A_f1,19:0.000000000000E+00}");
            sb.AppendFormat($" A2:     {A_f2,19:0.000000000000E+00}");
            sb.AppendFormat("\n");
            sb.AppendFormat($"    Iode:{IODE2,19:0.000000000000E+00}");
            sb.AppendFormat($" Crs:     {C_rs,19:0.000000000000E+00}");
            sb.AppendFormat($" Deln:   {DEL_N,19:0.000000000000E+00}");
            sb.AppendFormat($" M0:     {M0,19:0.000000000000E+00}");
            sb.AppendFormat("\n");
            sb.AppendFormat($"    Cuc: {C_uc,19:0.000000000000E+00}");
            sb.AppendFormat($" Ecc:     {E,19:0.000000000000E+00}");
            sb.AppendFormat($" Cus:    {C_us,19:0.000000000000E+00}");
            sb.AppendFormat($" SqrtA:  {SQRT_A,19:0.000000000000E+00}");
            sb.AppendFormat("\n");
            sb.AppendFormat($"    Toe: {Toe,19:0.000000000000E+00}");
            sb.AppendFormat($" Cic:     {C_ic,19:0.000000000000E+00}");
            sb.AppendFormat($" Omega0: {OMEGA_0,19:0.000000000000E+00}");
            sb.AppendFormat($" Cis:    {C_is,19:0.000000000000E+00}");
            sb.AppendFormat("\n");
            sb.AppendFormat($"    I0:  {I_0,19:0.000000000000E+00}");
            sb.AppendFormat($" Crc:     {C_rc,19:0.000000000000E+00}");
            sb.AppendFormat($" Omega:  {OMEGA,19:0.000000000000E+00}");
            sb.AppendFormat($" Omgdot: {OMEGADOT,19:0.000000000000E+00}");
            sb.AppendFormat("\n");
            sb.AppendFormat($"    Idot:{IDOT,19:0.000000000000E+00}");
            sb.AppendFormat($" CodeOnL2:{CAorPonL2,19:0.000000000000E+00}");
            sb.AppendFormat($" WeekNum:{WN,19:0.000000000000E+00}");
            sb.AppendFormat($" L2PFlag: {L2DataFlag,18:0.000000000000E+00}");
            sb.AppendFormat("\n");
            sb.AppendFormat($"    SvAcc:{URA,18:0.000000000000E+00}");
            sb.AppendFormat($" Health:  {Health,19:0.000000000000E+00}");
            sb.AppendFormat($" Tgd:    {Tgd,19:0.000000000000E+00}");
            sb.AppendFormat($" Iodc:    {IODC,18:0.000000000000E+00}");
            sb.AppendFormat("\n");
            sb.AppendFormat($"    TTM:  Not Specified");
            sb.AppendFormat($"      FitInt:  {FitIntFlg,19:0.000000000000E+00}");
            sb.AppendFormat("\n");
        }

        public void DebugPrint_Content()
        {
            StringBuilder sb = new StringBuilder();
            PrintTo(sb);
            Debug.WriteLine(sb);
        }



    }
}
