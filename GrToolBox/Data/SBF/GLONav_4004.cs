using GrToolBox.Common;
using GrToolBox.Data.GrNavData;
using GrToolBox.Satellite;
using System.Text;
using static GrToolBox.Settings.SettingsDefinitions;

namespace GrToolBox.Data.SBF
{
    public class GLONav_4004 : Header_TimeStamp, IRawData
    {
        private byte SVID { get; set; }          // ID of the GLONASS satellite for which ephemeris is provided in this block
        private byte FreqNr { get; set; }        // Frequency number of the GLONASS satellite for which ephemeris is provided in this block
        private double X { get; set; }           // x-component of satellite position in PZ-90.02
        private double Y { get; set; }
        private double Z { get; set; }
        private float Dx { get; set; }           // x-component of satellite velocity in PZ-90.02
        private float Dy { get; set; }
        private float Dz { get; set; }
        private float Ddx { get; set; }          // x-component of satellite acceleration in PZ-90.02
        private float Ddy { get; set; }
        private float Ddz { get; set; }
        private float Gamma { get; set; }        // \gamma_n(tb):relative deviation of predicted carrier frequency
        private float Tau { get; set; }          // \tau_n(tb): time correction to GLONASS time
        private float Dtau { get; set; }         // \Delta\tau_n: time difference between L2 and L1 sub-band
        private uint T_oe { get; set; }          // reference time-of-week in GPS time frame
        private ushort WN_toe { get; set; }      // reference week number in GPS time frame (modulo 1024)
        private byte P1 { get; set; }            // time interval between adjacent values of tb
        private byte P2 { get; set; }            // 1-bit odd/even flag of tb
        private byte E { get; set; }             // age of data
        private byte B { get; set; }             // 3-bit health flag, satellite unhealthy if MSB set
        private ushort Tb { get; set; }          // time of day (center of validity interval)
        private byte M { get; set; }             // 2-bit GLONASS-M satellite identifier (01, otherwise 00)
        private byte P { get; set; }             // 2-bit mode of computation of time parameters
        private byte Health { get; set; }        // 1-bit health flag, 0=healthy, 1=unhealthy
        private byte P4 { get; set; }            // 1-bit ’updated’ flag of ephemeris data
        private ushort N_T { get; set; }         // current day number within 4-year interval
        private ushort F_T { get; set; }         // predicted user range accuracy at time tb


        public GLONav_4004() { }
        public GLONav_4004(byte[] byteData, Header_TimeStamp h)
        {
            Decode(byteData, h);
        }

        public int GetID()
        {
            return 4004;
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
            byte freqNr = byteData[pos]; pos++;
            byte[] x = byteData[pos..(pos + 8)]; pos += 8;
            byte[] y = byteData[pos..(pos + 8)]; pos += 8;
            byte[] z = byteData[pos..(pos + 8)]; pos += 8;
            byte[] dx = byteData[pos..(pos + 4)]; pos += 4;
            byte[] dy = byteData[pos..(pos + 4)]; pos += 4;
            byte[] dz = byteData[pos..(pos + 4)]; pos += 4;
            byte[] ddx = byteData[pos..(pos + 4)]; pos += 4;
            byte[] ddy = byteData[pos..(pos + 4)]; pos += 4;
            byte[] ddz = byteData[pos..(pos + 4)]; pos += 4;
            byte[] gamma = byteData[pos..(pos + 4)]; pos += 4;
            byte[] tau = byteData[pos..(pos + 4)]; pos += 4;
            byte[] dtau = byteData[pos..(pos + 4)]; pos += 4;
            byte[] t_oe = byteData[pos..(pos + 4)]; pos += 4;
            byte[] wn_toe = byteData[pos..(pos + 2)]; pos += 2;
            byte p1 = byteData[pos]; pos++;
            byte p2 = byteData[pos]; pos++;
            byte e = byteData[pos]; pos++;
            byte b = byteData[pos]; pos++;
            byte[] tb = byteData[pos..(pos + 2)]; pos += 2;
            byte m = byteData[pos]; pos++;
            byte p = byteData[pos]; pos++;
            byte health = byteData[pos]; pos++;
            byte p4 = byteData[pos]; pos++;
            byte[] n_T = byteData[pos..(pos + 2)]; pos += 2;
            byte[] f_T = byteData[pos..(pos + 2)]; pos += 2;

            Sync1 = h.Sync1;
            Sync2 = h.Sync2;
            BlockNum = h.BlockNum;
            BlockRev = h.BlockRev;
            Length = h.Length;
            Tow = h.Tow;
            WNc = h.WNc;
            SVID = svid;
            FreqNr = freqNr;
            X = BitConverter.ToDouble(x);
            Y = BitConverter.ToDouble(x);
            Z = BitConverter.ToDouble(x);
            Dx = BitConverter.ToSingle(dx);
            Dy = BitConverter.ToSingle(dy);
            Dz = BitConverter.ToSingle(dz);
            Ddx = BitConverter.ToSingle(ddx);
            Ddy = BitConverter.ToSingle(ddy);
            Ddz = BitConverter.ToSingle(ddz);
            Gamma = BitConverter.ToSingle(gamma);
            Tau = BitConverter.ToSingle(tau);
            Dtau = BitConverter.ToSingle(dtau);
            T_oe = BitConverter.ToUInt32(t_oe);
            WN_toe = BitConverter.ToUInt16(wn_toe);
            P1 = p1;
            P2 = p2;
            E = e;
            B = b;
            Tb = BitConverter.ToUInt16(tb);
            M = m;
            P = p;
            Health = health;
            P4 = p4;
            N_T = BitConverter.ToUInt16(n_T);
            F_T = BitConverter.ToUInt16(f_T);

        }

        private GloNavData Sbf2Nav()
        {
            GloNavData nav = new();

            int wn_toe_C = WN_toe;
            while (true)                        // tocのWnはロールオーバーあり．週を跨いだ直後のtocは前週の場合があるので，sbfヘッダのWNcをそのまま使えない．と，思う
            {
                wn_toe_C += 1024;
                if (wn_toe_C < WNc) continue;
                if (Math.Abs(wn_toe_C - WNc) > 1)
                    wn_toe_C -= 1024;
                break;
            }
            var time = new GrToolBox.Time.TimeGR(wn_toe_C, T_oe);

            nav.Prn = new GrPrn(SVID, Data_Type.SBF);
            nav.Toc = T_oe;
            nav.WN_toc = wn_toe_C;
            nav.Year = time.Year;
            nav.Month = time.Month;
            nav.Day = time.Day;
            nav.Hour = time.Hour;
            nav.Minute = time.Minute;
            nav.Second = time.Second;
            nav.FreqNr = (int)FreqNr - 8;
            nav.Tau = Tau;
            nav.Gamma = Gamma;
            nav.X = X;
            nav.Y = Y;
            nav.Z = Z;
            nav.Dx = Dx;
            nav.Dy = Dy;
            nav.Dz = Dz;
            nav.Ddx = Ddx;
            nav.Ddy = Ddy;
            nav.Ddz = Ddz;
            nav.E = E;
            nav.SvHealth = Health;
            return nav;

        }
    }
}
