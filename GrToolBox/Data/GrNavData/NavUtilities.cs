using GrToolBox.Time;
using static GrToolBox.Common.CommonDefinitions;
using static GrToolBox.Common.Constants;
using static GrToolBox.Satellite.SatelliteConstants;
using static System.Math;

namespace GrToolBox.Data.GrNavData
{
    public static class NavUtilities
    {
        public static double time_check(double t)
        {
            if (t > 302400.0)
            {
                t = t - 604800.0;
            }
            else if (t < -302400)
            {
                t = t + 604800;
            }
            return t;
        }

        public static bool SatClkGEJC_brd(double rho, TimeGR time, ComNavGEJC nav, double[] SvcEaTtoe)
        {
            if (double.IsNaN(rho) || time == null || nav == null || SvcEaTtoe == null) return false;

            double tut = time.Sow;
            double dtp = 0.0;
            double dtp_old;
            double tp;
            double t_toe = 0.0, t_toc;
            double M;
            double E = 0.0, E_old;
            double delt;

            for (int k = 0; k < 10; k++)
            {
                dtp_old = dtp;
                tp = tut - (rho / CC) - dtp_old;
                t_toe = time_check(tp - nav.Toe);             // 軌道元期からの時間tp-(t_oe)を計算
                M = nav.M0 + nav.N * t_toe;                       // 平均近点角Mを求める
                E = M;
                for (int j = 0; j < 10; j++)
                {                                         // ケプラー方程式を解く
                    E_old = E;
                    E = E - (M - E + nav.Ecc * Sin(E)) / (nav.Ecc * Cos(E) - 1);
                    if (Abs(E - E_old) < 1E-14) break;
                }
                // 衛星時計誤差の計算（ここから）
                t_toc = time_check(tp - nav.Toc);             // 時計元期からの時刻t-(t_oc)を計算
                delt = FF(nav.Sys) * nav.Ecc * nav.SqrtA * Sin(E);     // 相対論補正値
                dtp = nav.A0 + nav.A1 * (t_toc) + nav.A2 * (t_toc) * (t_toc) + delt;
                // 衛星時計誤差の計算（ここまで）
                if (Abs(dtp - dtp_old) < 1E-14) break;
                //dtp_old = dtp;
            }
            // 衛星時計誤差を格納
            SvcEaTtoe[0] = dtp;     // 衛星時計誤差
            SvcEaTtoe[1] = E;       // 離心近点角（ケプラー方程式の解）
            SvcEaTtoe[2] = t_toe;   // t-toe
            return true;


        }

        public static void SatPosGEJC_brd(ComNavGEJC nav, double E, double t_toe, double[] SatPos, double[] SatVel)
        {
            double nu;
            double Phit;
            double dPhi;
            double dr;
            double dI;
            double Phi;
            double r;
            double I, xI, yI;
            double Omegal;

            nu = Atan2(Sqrt(1 - nav.Ecc * nav.Ecc) * Sin(E), Cos(E) - nav.Ecc);   // 真近点角
            Phit = nav.Omega + nu;                                  // 補正前の緯度引数
            dPhi = nav.Cus * Sin(2 * Phit) + nav.Cuc * Cos(2 * Phit);   // 緯度引数補正値
            dr = nav.Crs * Sin(2 * Phit) + nav.Crc * Cos(2 * Phit);     // 軌道半径補正値
            dI = nav.Cis * Sin(2 * Phit) + nav.Cic * Cos(2 * Phit);     // 軌道傾斜角補正値
            Phi = Phit + dPhi;                                  // 緯度引数
            r = nav.A * (1 - nav.Ecc* Cos(E)) + dr;                      // 軌道半径を計算
            I = nav.I0 + nav.Idot * t_toe + dI;                         // 軌道傾斜角を計算

            // 軌面内での直交座標を計算
            xI = r * Cos(Phi);
            yI = r * Sin(Phi);

            // 昇交点経度を求める
            if (nav.Sys != SYS.BDS)
            {
                Omegal = nav.Omega0 + (nav.OmegaDot - OMGE(nav.Sys)) * t_toe - OMGE(nav.Sys) * nav.Toe;
            }
            else
            {
                Omegal = nav.Omega0 + (nav.OmegaDot - OMGE(nav.Sys)) * t_toe - OMGE(nav.Sys) * (nav.Toe - 14);
            }

            // ECEF系での座標を格納
            //SatPos = new double[3];
            SatPos[0] = xI * Cos(Omegal) - yI * Cos(I) * Sin(Omegal);
            SatPos[1] = xI * Sin(Omegal) + yI * Cos(I) * Cos(Omegal);
            SatPos[2] = yI * Sin(I);

            // 衛星速度の計算（ここから）-------------------------------
            double Mdot = nav.N;
            double Edot = Mdot / (1 - nav.Ecc * Cos(E));
            double nudot = Edot * Sin(E) * (1 + nav.Ecc * Cos(nu)) / ((1 - nav.Ecc* Cos(E)) * Sin(nu));
            double dPhidot = (2 * nudot) * (nav.Cus * Cos(2 * Phit) - nav.Cuc * Sin(2 * Phit));
            double drdot = (2 * nudot) * (nav.Crs * Cos(2 * Phit) - nav.Crc * Sin(2 * Phit));
            double dIdot = (2 * nudot) * (nav.Cis * Cos(2 * Phit) - nav.Cic * Sin(2 * Phit));
            double Phidot = nudot + dPhidot;
            double rdot = nav.A * Edot * nav.Ecc * Sin(E) + drdot;
            double Omegaldot = nav.OmegaDot - OMGE(nav.Sys);
            nav.Idot += dIdot;
            double xIdot = rdot * Cos(Phi) - yI * Phidot;
            double yIdot = rdot * Sin(Phi) + xI * Phidot;
            double tmpx = xIdot - yI * Cos(I) * Omegaldot;
            double tmpy = xI * Omegaldot + yIdot * Cos(I) - yI * Sin(I) * nav.Idot;
            // 速度格納
            //SatVel = new double[3];
            SatVel[0] = tmpx * Cos(Omegal) - tmpy * Sin(Omegal);
            SatVel[1] = tmpx * Sin(Omegal) + tmpy * Cos(Omegal);
            SatVel[2] = yIdot * Sin(I) + yI * Cos(I) * nav.Idot;
            //（ここまで）---------------------------------------------
        }


        public static ComNavGEJC GetComNavGEJC(GpsNavData nav)
        {
            if (nav == null) return null;
            ComNavGEJC com = new();
            com.Sys = SYS.GPS;
            com.A0 = nav.A0;
            com.A1 = nav.A1;
            com.A2 = nav.A2;
            com.Deln = nav.Deln;
            com.M0 = nav.M0;
            com.Ecc = nav.Ecc;
            com.SqrtA = nav.SqrtA;
            com.Toe = nav.Toe;
            com.Toc = nav.Toc;
            com.Crs = nav.Crs;
            com.Cuc = nav.Cuc;
            com.Cus = nav.Cus;
            com.Cic = nav.Cic;
            com.Omega0 = nav.Omega0;
            com.Cis = nav.Cis;
            com.I0 = nav.I0;
            com.Crc = nav.Crc;
            com.Omega = nav.Omega;
            com.OmegaDot = nav.OmegaDot;
            com.Idot = nav.Idot;
            //-------------
            com.A = com.SqrtA * com.SqrtA;
            com.N0 = Sqrt(MU(com.Sys) / (com.A * com.A * com.A));
            com.N = com.N0 + com.Deln;
            return com;
        }

        public static ComNavGEJC GetComNavGEJC(GalNavData nav)
        {
            if (nav == null) return null;
            ComNavGEJC com = new();
            com.Sys = SYS.GAL;
            com.A0 = nav.A0;
            com.A1 = nav.A1;
            com.A2 = nav.A2;
            com.Deln = nav.Deln;
            com.M0 = nav.M0;
            com.Ecc = nav.Ecc;
            com.SqrtA = nav.SqrtA;
            com.Toe = nav.Toe;
            com.Toc = nav.Toc;
            com.Crs = nav.Crs;
            com.Cuc = nav.Cuc;
            com.Cus = nav.Cus;
            com.Cic = nav.Cic;
            com.Omega0 = nav.Omega0;
            com.Cis = nav.Cis;
            com.I0 = nav.I0;
            com.Crc = nav.Crc;
            com.Omega = nav.Omega;
            com.OmegaDot = nav.OmegaDot;
            com.Idot = nav.Idot;
            //-------------
            com.A = com.SqrtA * com.SqrtA;
            com.N0 = Sqrt(MU(com.Sys) / (com.A * com.A * com.A));
            com.N = com.N0 + com.Deln;
            return com;
        }

        public static ComNavGEJC GetComNavGEJC(QzsNavData nav)
        {
            if (nav == null) return null;
            ComNavGEJC com = new();
            com.Sys = SYS.QZS;
            com.A0 = nav.A0;
            com.A1 = nav.A1;
            com.A2 = nav.A2;
            com.Deln = nav.Deln;
            com.M0 = nav.M0;
            com.Ecc = nav.Ecc;
            com.SqrtA = nav.SqrtA;
            com.Toe = nav.Toe;
            com.Toc = nav.Toc;
            com.Crs = nav.Crs;
            com.Cuc = nav.Cuc;
            com.Cus = nav.Cus;
            com.Cic = nav.Cic;
            com.Omega0 = nav.Omega0;
            com.Cis = nav.Cis;
            com.I0 = nav.I0;
            com.Crc = nav.Crc;
            com.Omega = nav.Omega;
            com.OmegaDot = nav.OmegaDot;
            com.Idot = nav.Idot;
            //-------------
            com.A = com.SqrtA * com.SqrtA;
            com.N0 = Sqrt(MU(com.Sys) / (com.A * com.A * com.A));
            com.N = com.N0 + com.Deln;
            return com;
        }

        public static ComNavGEJC GetComNavGEJC(BdsNavData nav)
        {
            if (nav == null) return null;
            ComNavGEJC com = new();
            com.Sys = SYS.BDS;
            com.A0 = nav.A0;
            com.A1 = nav.A1;
            com.A2 = nav.A2;
            com.Deln = nav.Deln;
            com.M0 = nav.M0;
            com.Ecc = nav.Ecc;
            com.SqrtA = nav.SqrtA;
            com.Toe = nav.Toe;
            com.Toc = nav.Toc;
            com.Crs = nav.Crs;
            com.Cuc = nav.Cuc;
            com.Cus = nav.Cus;
            com.Cic = nav.Cic;
            com.Omega0 = nav.Omega0;
            com.Cis = nav.Cis;
            com.I0 = nav.I0;
            com.Crc = nav.Crc;
            com.Omega = nav.Omega;
            com.OmegaDot = nav.OmegaDot;
            com.Idot = nav.Idot;
            //-------------
            com.A = com.SqrtA * com.SqrtA;
            com.N0 = Sqrt(MU(com.Sys) / (com.A * com.A * com.A));
            com.N = com.N0 + com.Deln;
            return com;
        }
    }
}
