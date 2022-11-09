using System;
using System.Collections.Generic;
using System.Text;
using static System.Math;
using static GrToolBox.Coordinates.CoordinateConstants;
using static GrToolBox.Settings.SettingsDefinitions;

namespace GrToolBox.Coordinates
{
    class CoordinateUtilities
    {
        public static double[] XYZ2llh(double[] xyz, Ell_Type ell_Type)
        {
            double a;       // 長半径
            double f;       // 扁平率
            double e2;      // 離心率の2乗

            if (ell_Type == Ell_Type.ITRF)
            {
                a = SEMI_MAJOR_AXIS_GRS;
                f = FLATTENING_GRS;
                e2 = SQUARED_ECCENTRICITY_GRS;
            }
            else
            {
                a = SEMI_MAJOR_AXIS_WGS;
                f = FLATTENING_WGS;
                e2 = SQUARED_ECCENTRICITY_WGS;
            }

            double[] llh = new double[3];
            double b = a * (1.0 - f);
            double m2 = 1.0 / (1.0 - e2) - 1.0;
            double p = Sqrt(xyz[0] * xyz[0] + xyz[1] * xyz[1]);
            double theta = Atan(xyz[2] * a / (p * b));
            double phi = Atan((xyz[2] + m2 * b * Pow(Sin(theta), 3)) / (p - e2 * a * Pow(Cos(theta), 3)));
            double N = a / Sqrt(1.0 - e2 * Pow(Sin(phi), 2));
            double h = p / Cos(phi) - N;
            llh[0] = phi;
            llh[1] = Atan2(xyz[1], xyz[0]);
            llh[2] = h;
            return llh;

        }

        public static double[] XYZ2llh(double[] xyz)
        {
            return XYZ2llh(xyz, Ell_Type.WGS);
        }


        public static double[] LLH2xyz(double[] llh, Ell_Type ell_Type)
        {
            double a;       // 長半径
            double f;       // 扁平率
            double e2;      // 離心率の2乗

            if (ell_Type == Ell_Type.ITRF)
            {
                a = SEMI_MAJOR_AXIS_GRS;
                f = FLATTENING_GRS;
                e2 = SQUARED_ECCENTRICITY_GRS;
            }
            else
            {
                a = SEMI_MAJOR_AXIS_WGS;
                f = FLATTENING_WGS;
                e2 = SQUARED_ECCENTRICITY_WGS;
            }

            double[] xyz = new double[3];
            double phi = llh[0];
            double lam = llh[1];
            double h = llh[2];
            double sphi = Sin(phi);
            double cphi = Cos(phi);
            double slam = Sin(lam);
            double clam = Cos(lam);
            double N = a / Sqrt(1.0 - e2 * sphi * sphi);
            double Nh = N + h;
            xyz[0] = Nh * cphi * clam;
            xyz[1] = Nh * cphi * slam;
            xyz[2] = (Nh - N * e2) * sphi;
            return xyz;
        }

        public static double[] LLH2xyz(double[] llh)
        {
            return LLH2xyz(llh, Ell_Type.WGS);
        }


        public static double[] XYZ2enu(double[] xyz, double[] orgxyz, double[] orgllh, Ell_Type ell_Type)
        {
            double[] enu = new double[3];
            //double[] orgllh = XYZ2llh(orgxyz, ell_Type);
            double phi = orgllh[0];
            double lam = orgllh[1];
            double cosphi = Cos(phi);
            double coslam = Cos(lam);
            double sinphi = Sin(phi);
            double sinlam = Sin(lam);
            double[] tmp = { xyz[0] - orgxyz[0], xyz[1] - orgxyz[1], xyz[2] - orgxyz[2] };
            enu[0] = -sinlam * tmp[0] + coslam * tmp[1];
            enu[1] = -sinphi * coslam * tmp[0] - sinphi * sinlam * tmp[1] + cosphi * tmp[2];
            enu[2] = cosphi * coslam * tmp[0] + cosphi * sinlam * tmp[1] + sinphi * tmp[2];
            return enu;
        }

        public static double[] XYZ2enu(double[] xyz, double[] orgxyz, double[] orgllh)
        {
            return XYZ2enu(xyz, orgxyz, orgllh, Ell_Type.WGS);
        }






        /// <summary>
        /// ENU座標系からECEF座標系への変換
        /// </summary>
        /// <param name="enu">double[3]: ENU座標 E N U [m]</param>
        /// <param name="orgxyz">double[3]: ENU座標系の原点のECEF座標 X Y Z [m]</param>
        /// <param name="args">
        ///     String, 準拠楕円体の選択，"itrf": GRS80楕円体，その他の文字列または指定無しの場合はWGS-84準拠楕円体が選択される．
        ///     内部でxyz2llhを呼び出しているため，楕円体の指定が必要
        /// </param>
        /// <returns>ECEF座標 X Y Z [m]</returns>
        /// 

        public static double[] ENU2xyz(double[] enu, double[] orgxyz, Ell_Type ell_Type)
        {
            double[] xyz = new double[3];
            double[] orgllh = XYZ2llh(orgxyz, ell_Type);
            double phi = orgllh[0];
            double lam = orgllh[1];
            double cosphi = Cos(phi);
            double coslam = Cos(lam);
            double sinphi = Sin(phi);
            double sinlam = Sin(lam);

            xyz[0] = -sinlam * enu[0] - sinphi * coslam * enu[1] + cosphi * coslam * enu[2] + orgxyz[0];
            xyz[1] = coslam * enu[0] - sinphi * sinlam * enu[1] + cosphi * sinlam * enu[2] + orgxyz[1];
            xyz[2] = cosphi * enu[1] + sinphi * enu[2] + orgxyz[2];

            return xyz;

        }

        public static double[] ENU2xyz(double[] enu, double[] orgxyz)
        {
            return ENU2xyz(enu, orgxyz, Ell_Type.WGS);
        }






        public static void PZ2wgs(double[] rpos)
        {
            double c = 1.0 / (1.0 - 0.008e-6);
            double tx = (rpos[0] + 0.013) / c;
            double ty = (rpos[1] - 0.106) / c;
            double tz = (rpos[2] - 0.022) / c;
            double X = tx - (0.02041e-6) * ty - (0.01716e-6) * tz;
            double Y = (0.02041e-6) * tx + ty - (0.01115e-6) * tz;
            double Z = (0.01716e-6) * tx + (0.01115e-6) * ty + tz;
            rpos[0] = X;
            rpos[1] = Y;
            rpos[2] = Z;
        }


    }
}





