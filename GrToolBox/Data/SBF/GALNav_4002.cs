using GrToolBox.Common;
using GrToolBox.Data.GrNavData;
using GrToolBox.Satellite;
using System.Text;
using static GrToolBox.Common.Constants;
using static GrToolBox.Settings.SettingsDefinitions;

namespace GrToolBox.Data.SBF
{
    public class GALNav_4002 : Header_TimeStamp, IRawData
    {
        private byte SVID { get; set; }              // SVID of the Galileo satellite
        private byte Source { get; set; }            // this field indicates how to interpret the clock correction parameters
        private double SQRT_A { get; set; }          // Square root of the semi-major axis [m^{1/2}]
        private double M0 { get; set; }              // Mean anomaly at reference time [semi-circle]
        private double E { get; set; }               // Eccentricity
        private double I_0 { get; set; }             // Inclination angle at reference time [semi-circle]
        private double OMEGA { get; set; }           // Argument of perigee [semi-circle]
        private double OMEGA_0 { get; set; }         // Longitude of ascending node of orbit plane at weekly epoch [semi-circle]
        private float OMEGADOT { get; set; }         // Rate of right ascension [semi-circle / s]
        private float IDOT { get; set; }             // Rate of inclination angle [semi-circle / s]
        private float DEL_N { get; set; }            // Mean motion difference from computed value [semi-circle / s]
        private float C_uc { get; set; }             // Amplitude of the cosine harmonic correction term to the argument of latitude [rad]
        private float C_us { get; set; }             // Amplitude of the sine harmonic correction term to the argument of latitude [rad]
        private float C_rc { get; set; }             // Amplitude of the cosine harmonic correction term to the orbit radius [m]
        private float C_rs { get; set; }             // Amplitude of the sine harmonic correction term to the orbit radius [m]
        private float C_ic { get; set; }             // Amplitude of the cosine harmonic correction term to the angle of inclination [rad]
        private float C_is { get; set; }             // Amplitude of the sine harmonic correction term to the angle of inclination [rad]
        private uint Toe { get; set; }               // Reference time ephemeris [s]
        private uint Toc { get; set; }               // clock data reference time [sec]
        private float A_f2 { get; set; }             // SV clock aging [1 s / s2]
        private float A_f1 { get; set; }             // SV clock drift [1 s / s]
        private double A_f0 { get; set; }            // SV clock bias [1 s]
        private ushort WNt_oe { get; set; }          // WN associated with t_oe, in GPS time frame, modulo 4096
        private ushort WNt_oc { get; set; }          // WN associated with t_oc, in GPS time frame, modulo 4096
        private ushort IODNav { get; set; }          // Issue of data, navigation (10 bits)
        private ushort Health_OSSOL { get; set; }    // Bit field indicating the last received Health Status (HS) and Data Validity Status(DVS) of the E5a, E5b and L1-B signals
        private byte Health_PRS { get; set; }        // Reserved
        private byte SISA_L1E5a { get; set; }        // Signal-In-Space Accuracy Index (L1, E5a)
        private byte SISA_L1E5b { get; set; }        // Signal-In-Space Accuracy Index (L1, E5b)
        private byte SISA_L1AE6A { get; set; }       // Reserved
        private float BGD_L1E5a { get; set; }        // Last received broadcast group delay (L1, E5a) [sec]
        private float BGD_L1E5b { get; set; }        // Last received broadcast group delay (L1, E5b) [sec]
        private float BGD_L1AE6A { get; set; }       // Reserved
        private byte CNAVenc { get; set; }           // 2-bit C/NAV encryption status

        public GALNav_4002() { }
        public GALNav_4002(byte[] byteData, Header_TimeStamp h)
        {
            Decode(byteData, h);
        }

        public int GetID()
        {
            return 4002;
        }

        public Satellites? GetMeas()
        {
            return null;
        }

        public GrNavBase GetNav()
        {
            return Sbf2Nav();
        }

        public void PrintTo(StringBuilder sb) { }

        private void Decode(byte[] byteData, Header_TimeStamp h)
        {
            int pos = 14;
            byte svid = byteData[pos]; pos++;
            byte source = byteData[pos]; pos++;
            byte[] sqrt_a = byteData[pos..(pos + 8)]; pos += 8;
            byte[] m0 = byteData[pos..(pos + 8)]; pos += 8;
            byte[] e = byteData[pos..(pos + 8)]; pos += 8;
            byte[] i_0 = byteData[pos..(pos + 8)]; pos += 8;
            byte[] omega = byteData[pos..(pos + 8)]; pos += 8;
            byte[] omega_0 = byteData[pos..(pos + 8)]; pos += 8;
            byte[] omegadot = byteData[pos..(pos + 4)]; pos += 4;
            byte[] idot = byteData[pos..(pos + 4)]; pos += 4;
            byte[] del_n = byteData[pos..(pos + 4)]; pos += 4;
            byte[] c_uc = byteData[pos..(pos + 4)]; pos += 4;
            byte[] c_us = byteData[pos..(pos + 4)]; pos += 4;
            byte[] c_rc = byteData[pos..(pos + 4)]; pos += 4;
            byte[] c_rs = byteData[pos..(pos + 4)]; pos += 4;
            byte[] c_ic = byteData[pos..(pos + 4)]; pos += 4;
            byte[] c_is = byteData[pos..(pos + 4)]; pos += 4;
            byte[] toe = byteData[pos..(pos + 4)]; pos += 4;
            byte[] toc = byteData[pos..(pos + 4)]; pos += 4;
            byte[] a_f2 = byteData[pos..(pos + 4)]; pos += 4;
            byte[] a_f1 = byteData[pos..(pos + 4)]; pos += 4;
            byte[] a_f0 = byteData[pos..(pos + 8)]; pos += 8;
            byte[] wnt_oe = byteData[pos..(pos + 2)]; pos += 2;
            byte[] wnt_oc = byteData[pos..(pos + 2)]; pos += 2;
            byte[] iodnav = byteData[pos..(pos + 2)]; pos += 2;
            byte[] health_ossol = byteData[pos..(pos + 2)]; pos += 2;
            byte health_prs = byteData[pos]; pos++;
            byte sisa_L1E5a = byteData[pos]; pos++;
            byte sisa_L1E5b = byteData[pos]; pos++;
            byte sisa_L1AE6A = byteData[pos]; pos++;
            byte[] bgd_L1E5a = byteData[pos..(pos + 4)]; pos += 4;
            byte[] bgd_L1E5b = byteData[pos..(pos + 4)]; pos += 4;
            byte[] bgd_L1AE6A = byteData[pos..(pos + 4)]; pos += 4;
            byte cnavenc = byteData[pos]; pos++;

            Sync1 = h.Sync1;
            Sync2 = h.Sync2;
            BlockNum = h.BlockNum;
            BlockRev = h.BlockRev;
            Length = h.Length;
            Tow = h.Tow;
            WNc = h.WNc;
            SVID = svid;
            Source = source;
            SQRT_A = BitConverter.ToDouble(sqrt_a);
            M0 = BitConverter.ToDouble(m0);
            E = BitConverter.ToDouble(e);
            I_0 = BitConverter.ToDouble(i_0);
            OMEGA = BitConverter.ToDouble(omega);
            OMEGA_0 = BitConverter.ToDouble(omega_0);
            OMEGADOT = BitConverter.ToSingle(omegadot);
            IDOT = BitConverter.ToSingle(idot);
            DEL_N = BitConverter.ToSingle(del_n);
            C_uc = BitConverter.ToSingle(c_uc);
            C_us = BitConverter.ToSingle(c_us);
            C_rc = BitConverter.ToSingle(c_rc);
            C_rs = BitConverter.ToSingle(c_rs);
            C_ic = BitConverter.ToSingle(c_ic);
            C_is = BitConverter.ToSingle(c_is);
            Toe = BitConverter.ToUInt32(toe);
            Toc = BitConverter.ToUInt32(toc);
            A_f2 = BitConverter.ToSingle(a_f2);
            A_f1 = BitConverter.ToSingle(a_f1);
            A_f0 = BitConverter.ToDouble(a_f0);
            WNt_oe = BitConverter.ToUInt16(wnt_oe);
            WNt_oc = BitConverter.ToUInt16(wnt_oc);
            IODNav = BitConverter.ToUInt16(iodnav);
            Health_OSSOL = BitConverter.ToUInt16(health_ossol);
            Health_PRS = health_prs;
            SISA_L1E5a = sisa_L1E5a;
            SISA_L1E5b = sisa_L1E5b;
            SISA_L1AE6A = sisa_L1AE6A;
            BGD_L1E5a = BitConverter.ToSingle(bgd_L1E5a);
            BGD_L1E5b = BitConverter.ToSingle(bgd_L1E5b);
            BGD_L1AE6A = BitConverter.ToSingle(bgd_L1AE6A);
            CNAVenc = cnavenc;
        }

        private GalNavData Sbf2Nav()
        {
            GalNavData nav = new();

            int wn_toc_C = WNt_oc;
            while (true)                        // tocのWnはロールオーバーあり．週を跨いだ直後のtocは前週の場合があるので，sbfヘッダのWNcをそのまま使えない．と，思う
            {
                wn_toc_C += 4096;
                if (wn_toc_C < WNc) continue;
                if (Math.Abs(wn_toc_C - WNc) > 1)
                    wn_toc_C -= 4096;
                break;
            }
            int wn_toe_C = WNt_oe;
            while (true)                        // toeのWNロールオーバ処理
            {
                wn_toe_C += 4096;
                if (wn_toe_C < WNc) continue;
                if (Math.Abs(wn_toe_C - WNc) > 1)
                    wn_toe_C -= 4096;
                break;
            }

            var time = new GrToolBox.Time.TimeGR(wn_toc_C, Toc);
            nav.Prn = new GrPrn(SVID, Data_Type.SBF);
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
            nav.IODNav = IODNav;
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
            nav.Source = Source;
            nav.WeekNum = wn_toe_C;

            nav.SISA_L1E5a = SISAIndex_To_SISAValue(SISA_L1E5a);
            nav.SISA_L1E5b = SISAIndex_To_SISAValue(SISA_L1E5b);

            nav.Health_OSSOL = Health_OSSOL;

            nav.BGD_L1E5a = BGD_L1E5a;
            nav.BGD_L1E5b = BGD_L1E5b;

            //*****************
            nav.Ttm = (int)Tow + (WNc - wn_toe_C) * 604800;      // WeekNum(toe)から見たタイムスタンプ(sis) 

            return nav;
        }

        private static double SISAIndex_To_SISAValue(byte b)
        {
            if (b == 255)
            {
                return double.NaN;  // do-not-usedの場合はNaNを返す
            }
            else
            {
                if (b >= 0 && b <= 49)
                {
                    return (double)b * 0.01;     // 0 cm to 49 cm with 1 cm resolution
                }
                else if (b >= 50 && b <= 74)
                {
                    return (double)b * 0.02;    // 50 cm to 0.98 m with 2 cm resolution
                }
                else if (b >= 75 && b <= 99)
                {
                    return (double)b * 0.04;    // 1 m to 1.96m with 4 cm resolution
                }
                else if (b >= 100 && b <= 125)
                {
                    return (double)b * 0.16;    // 2 m to 6 m with 16 cm resolution
                }
                else if (b >= 125 && b <= 254)
                {
                    return double.NaN;          // Spare
                }
                else
                {
                    return double.NaN;          // No Accuracy Prediction Available (NAPA)
                }
            }
        }

    }
}
