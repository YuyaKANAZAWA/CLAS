using GrToolBox.Satellite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static GrToolBox.Common.CommonDefinitions;
using static GrToolBox.Common.Constants;
using static GrToolBox.Coordinates.CoordinateUtilities;
using static GrToolBox.Satellite.SatelliteConstants;
using static System.Math;

namespace GrToolBox.Positioning
{
    public class PositioningUtilities
    {
        /// <summary>
        /// 衛星座標を伝搬時間回転させる
        /// </summary>
        /// <param name="Sat"></param>
        /// <param name="band"></param>
        public static void SatRot(SatelliteData Sat, double[] b)
        {
            int isys = Sat.Prn.ISys;
            double rho = 0.0;
            int ind = 0;
            // C1,C2,C5の順にサーチして，データのあるものを伝搬時間計算に用いる
            // rot後の衛星座標は，擬似距離差10mで衛星座標1mm未満程度の差-->>don't care
            for (int i = 0; i < MAX_OBS_PVT; i++)
            {
                if (!double.IsNaN(Sat.PVT_Code[i]))
                {
                    rho = Sat.PVT_Code[i];
                    ind = i;
                    break;
                }
            }
            double omge = OMGE(Sat.Prn.Sys);
            double tau = rho / CC - b[isys] / CC + Sat.PVT_Dtsv[ind] + Sat.PVT_Tgd[ind];  // GPS,J    <<<---------ION FREEの時は？
            double omegatup = omge * tau;
            double Rz00 = Cos(omegatup);
            double Rz01 = Sin(omegatup);
            double Rz10 = -Rz01;
            double Rz11 = Rz00;
            Sat.PosRot[0] = Rz00 * Sat.Pos[0] + Rz01 * Sat.Pos[1];
            Sat.PosRot[1] = Rz10 * Sat.Pos[0] + Rz11 * Sat.Pos[1];
            Sat.PosRot[2] = Sat.Pos[2];
            if (Sat.Prn.Sys == SYS.GLO)
            {
                double[] rpos = { Sat.PosRot[0], Sat.PosRot[1], Sat.PosRot[2] };
                PZ2wgs(rpos);
                Sat.PosRot[0] = rpos[0];
                Sat.PosRot[1] = rpos[1];
                Sat.PosRot[2] = rpos[2];
            }
        }
    }
}
