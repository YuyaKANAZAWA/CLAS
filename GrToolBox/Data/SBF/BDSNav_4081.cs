using GrToolBox.Common;
using GrToolBox.Data.GrNavData;
using GrToolBox.Satellite;
using System.Text;
using static GrToolBox.Common.Constants;
using static GrToolBox.Settings.SettingsDefinitions;

namespace GrToolBox.Data.SBF
{
    public class BDSNav_4081 : Header_TimeStamp, IRawData
    {
        private byte PRN { get; set; }               // ID of the BeiDou satellite of which the ephemeris is given in this block
        private byte Reserved { get; set; }          // 
        private ushort WN { get; set; }              // BeiDou week number as received from the navigation message (from 0 to 8191)
        private byte URA { get; set; }               // User range accuracy index (4-bit value)
        private byte SatH1 { get; set; }             // 1-bit autonomous health
        private byte IODC { get; set; }              // Age of data, clock (5 bits)
        private byte IODE { get; set; }              // Age of data, ephemeris (5 bits)
        private ushort Reserved2 { get; set; }       // 
        private float TGD1 { get; set; }             // B1I equipment group delay differential [s]
        private float TGD2 { get; set; }             // B2I equipment group delay differential (set to the Do-Not-Use (-2e10) value when unknown) [s]
        private uint Toc { get; set; }               // clock data reference time, in BeiDou system time (lagging GPS time by 14 seconds).
        private float A_f2 { get; set; }             // SV clock aging [1 s / s2]
        private float A_f1 { get; set; }             // SV clock drift [1 s / s]
        private float A_f0 { get; set; }             // SV clock bias [1 s]
        private float C_rs { get; set; }             // Amplitude of the sine harmonic correction term to the orbit radius [m]
        private float DEL_N { get; set; }            // Mean motion difference from computed value [semi-circle / s]
        private double M0 { get; set; }              // Mean anomaly at reference time [semi-circle]
        private float C_uc { get; set; }             // Amplitude of the cosine harmonic correction term to the argument of latitude [rad]
        private double E { get; set; }               // Eccentricity
        private float C_us { get; set; }             // Amplitude of the sine harmonic correction term to the argument of latitude [rad]
        private double SQRT_A { get; set; }          // Square root of the semi-major axis [m^{1/2}]
        private uint Toe { get; set; }               // Reference time ephemeris, in BeiDou system time (lagging GPS time by 14 seconds). [s]
        private float C_ic { get; set; }             // Amplitude of the cosine harmonic correction term to the angle of inclination [rad]
        private double OMEGA_0 { get; set; }         // Longitude of ascending node of orbit plane at weekly epoch [semi-circle]
        private float C_is { get; set; }             // Amplitude of the sine harmonic correction term to the angle of inclination [rad]
        private double I_0 { get; set; }             // Inclination angle at reference time [semi-circle]
        private float C_rc { get; set; }             // Amplitude of the cosine harmonic correction term to the orbit radius [m]
        private double OMEGA { get; set; }           // Argument of perigee [semi-circle]
        private float OMEGADOT { get; set; }         // Rate of right ascension [semi-circle / s]
        private float IDOT { get; set; }             // Rate of inclination angle [semi-circle / s]
        private ushort WNt_oc { get; set; }          // BeiDou week number associated with t_oc, modulo 8192. Note that this value relates to the BeiDou system time.
        private ushort WNt_oe { get; set; }          // BeiDou week number associated with t_oe, modulo 8192. Note that this values relates to the BeiDou system time.

        public BDSNav_4081() { }
        public BDSNav_4081(byte[] byteData, Header_TimeStamp h) 
        {
            Decode(byteData, h);
        }

        public int GetID()
        {
            return 4081;
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
            byte prn = byteData[pos]; pos++;
            byte reserved = byteData[pos]; pos++;
            byte[] wn = byteData[pos..(pos + 2)]; pos += 2;
            byte ura = byteData[pos]; pos++;
            byte satH1 = byteData[pos]; pos++;
            byte iodc = byteData[pos]; pos++;
            byte iode = byteData[pos]; pos++;
            byte[] reserved2 = byteData[pos..(pos + 2)]; pos += 2;
            byte[] t_gd1 = byteData[pos..(pos + 4)]; pos += 4;
            byte[] t_gd2 = byteData[pos..(pos + 4)]; pos += 4;
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
            URA = ura;
            SatH1 = satH1;
            IODC = iodc;
            IODE = iode;
            Reserved2 = BitConverter.ToUInt16(reserved2);
            TGD1 = BitConverter.ToSingle(t_gd1);
            TGD2 = BitConverter.ToSingle(t_gd2);
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

        private BdsNavData Sbf2Nav()
        {
            BdsNavData nav = new();

            int toc = (int)Toc + 14;        // GPSにあわせる．BDT startでの閏秒＝14秒, BDT always 14s behind GPST．14秒加算によるWeekの変化処理はTimeGRに任せる
            int wn_toc_C = WNt_oc + 1356;   // BDT start: Jan. 1, 2006 = GPS week 1356
            if (toc >= 604800)
            {
                toc -= 604800;
                wn_toc_C++;
            }
            while (true)                    // tocのWnはロールオーバーあり．週を跨いだ直後のtocは前週の場合がある．sbfヘッダのWNcはGPS，ロールオーバなし．sbf.WNt_ocは8192でロールオーバ
            {
                wn_toc_C += 8192;
                if (wn_toc_C < WNc) continue;
                if (Math.Abs(wn_toc_C - WNc) > 1)
                    wn_toc_C -= 8192;
                break;
            }
            //
            int toe = (int)Toe + 14;        // GPSにあわせる
            int wn_toe_C = WNt_oe + 1356;   // BDT start: Jan. 1, 2006 = GPS week 1356
            if (toe >= 604800)
            {
                toe -= 604800;
                wn_toe_C++;
            }
            while (true)                    // toeのWNロールオーバ処理
            {
                wn_toe_C += 8192;
                if (wn_toe_C < WNc) continue;
                if (Math.Abs(wn_toe_C - WNc) > 1)
                    wn_toe_C -= 8192;
                break;
            }

            var time = new Time.TimeGR(wn_toc_C, toc);
            nav.Prn = new GrPrn(PRN, Data_Type.SBF);
            nav.Toc = toc;                                   // in GPS time frame
            nav.WN_toc = wn_toc_C;                           // in GPS time frame
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
            //nav.BDS.Iode = sbf.IODE;
            nav.Crs = C_rs;
            nav.Deln = DEL_N * PI;           // [sc/s] -->> [rad/s]
            nav.M0 = M0 * PI;                // [sc] -->> [rad]
            //*****************
            nav.Cuc = C_uc;
            nav.Ecc = E;
            nav.Cus = C_us;
            nav.SqrtA = SQRT_A;
            //*****************
            nav.Toe = toe;                   // in GPS time frame
            nav.Cic = C_ic;
            nav.Omega0 = OMEGA_0 * PI;       // [sc] -->> [rad]
            nav.Cis = C_is;
            //*****************
            nav.I0 = I_0 * PI;               // [sc] -->> [rad]
            nav.Crc = C_rc;
            nav.Omega = OMEGA * PI;          // [sc] -->> [rad]
            nav.OmegaDot = OMEGADOT * PI;    // [sc/s] -->> [rad/s]
            //*****************
            nav.Idot = IDOT * PI;            // [sc/s] -->> [rad/s]
            //gr.CodeOnL2 = sbf.CAorPonL2;
            nav.WeekNum = wn_toe_C;          // Week # (to go with TOE) Continuous number, not mod(8192), in GPS time frame
            //gr.L2P_Flag = sbf.L2DataFlag;
            //*****************
            //gr.SvAccuracy = (sbf.URA <= 6) ? System.Math.Pow(2.0, 1.0 + sbf.URA / 2.0) : System.Math.Pow(2.0, sbf.URA - 2.0);
            if (URA <= 6)
            {
                nav.SvAccuracy = Math.Pow(2.0, 1.0 + URA / 2.0);
            }
            else if (URA < 15)
            {
                nav.SvAccuracy = Math.Pow(2.0, URA - 2.0);
            }
            else
            {
                nav.SvAccuracy = double.NaN;
            }
            //nav.BDS.SvHealth = sbf.SatH1;
            nav.SatH1 = SatH1;
            nav.Tgd1 = TGD1;
            nav.Tgd2 = TGD2;
            //nav.BDS.Iodc = sbf.IODC;
            //*****************
            nav.Ttm = (int)Tow + (WNc - wn_toe_C) * 604800;      // WeekNum(toe)から見たタイムスタンプ(sis)

            // AODCの読み替え
            if (IODC < 25)
            {
                //nav.BDS.FitInterval = 1;
                nav.Aodc = IODC;
                nav.Aode = IODE;
            }
            else if (IODC >= 25 && IODC <= 31)
            {
                //nav.BDS.FitInterval = (sbf.IODC - 25 + 2) * 24;
                nav.Aodc = (IODC - 25 + 2) * 24;
                nav.Aode = (IODE - 25 + 2) * 24;
            }

            //NavDataClasses.DebugPrint(gr);
            return nav;

        }
    }
}
