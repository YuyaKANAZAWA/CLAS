using GrToolBox.Coordinates;
using GrToolBox.GrMath;
using GrToolBox.Satellite;
using GrToolBox.Settings;
using GrToolBox.Time;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using static GrToolBox.Common.CommonDefinitions;
using static GrToolBox.Common.CommonUtilities;
using static GrToolBox.Common.Constants;
using static GrToolBox.Coordinates.CoordinateUtilities;
using static GrToolBox.GrMath.MathUtilities;
using static GrToolBox.Satellite.SatelliteConstants;
using static GrToolBox.Satellite.SatelliteUtilities;
using static System.Math;

namespace GrToolBox.Positioning
{
    public class PointPositioning_LS
    {
        Correction_Setting Stg { get; set; }
        public bool Success { get; private set; }
        public TimeGR TimeSuccess { get; private set; } // 測位成功時点（最近）の時刻

        public double[] XYZ { get; set; } = new double[] { 0.0, 0.0, 0.0 };
        public double[] b;

        public Position EstPos { get; private set; }

        private PositionSetter Pst { get; set; }



        /*-----------------
          constructor 
        -----------------*/
        public PointPositioning_LS(PositionSetter pst, Correction_Setting stg)
        {
            //this.Data = data;
            //this.Time = time;
            this.Stg = stg;
            this.Pst = pst;
        }

        public PointPositioning_LS(PositionSetter pst, Settings.GrSettings stg) : this(pst, stg.Correction) { }


        /*-----------------
          public methods 
        -----------------*/

        // 通常のLS単独測位
        public void CalcPos(Satellites Sats)
        {
            if (double.IsNaN(XYZ[0]))
            {
                XYZ[0] = 0.0; XYZ[1] = 0.0; XYZ[2] = 0.0;
            }
            var sysuse = Sats.SysUsed;               // 使用システム GREJCIS 7ビットのフラグ
            var nSys = Sats.NUsed.Count(n => n > 0);   // 使用システム数

            EstPos = new Position();
            Success = false;

            if (Sats.NObsCode.Sum() < 3 + nSys)
            {
                return;
            }

            // 受信機時計誤差（全システムを用意）
            b = new double[MAX_SYS];
            //b = Enumerable.Repeat<double>(double.NaN, MAX_SYS).ToArray();


            // 測位演算loop
            for (int loop = 0; loop < 10; loop++)
            {
                List<double[]> _H = new List<double[]>();
                List<double> _Y = new List<double>();
                BitArray useCols = new BitArray(3 + MAX_SYS);
                useCols.Set(0, true);       // 受信機座標部分(0--2)にフラグセット
                useCols.Set(1, true);
                useCols.Set(2, true);
                for (int i = 0; i < MAX_SYS; i++)
                {
                    useCols.Set(3 + i, sysuse.Get(i));  // 受信機時計誤差部分にフラグセット
                }

                double tr;
                foreach (SatelliteData Sat in Sats.SatData)
                {
                    int isys = Sat.Prn.ISys;
                    if (Sat.Status > 0)
                    {
                        continue;
                    }

                    //衛星回転
                    PositioningUtilities.SatRot(Sat, b);

                    // 幾何学距離の計算値 r(\hat{x})
                    tr = Sqrt((XYZ[0] - Sat.PosRot[0]) * (XYZ[0] - Sat.PosRot[0])
                            + (XYZ[1] - Sat.PosRot[1]) * (XYZ[1] - Sat.PosRot[1])
                            + (XYZ[2] - Sat.PosRot[2]) * (XYZ[2] - Sat.PosRot[2]));

                    // 方向余弦の計算，観測行列の行を作成
                    double[] _Hrow = new double[3 + MAX_SYS];
                    _Hrow[0] = Sat.Weight * (XYZ[0] - Sat.PosRot[0]) / tr;
                    _Hrow[1] = Sat.Weight * (XYZ[1] - Sat.PosRot[1]) / tr;
                    _Hrow[2] = Sat.Weight * (XYZ[2] - Sat.PosRot[2]) / tr;
                    _Hrow[3 + isys] = Sat.Weight * 1.0;

                    if (Stg.IonType == SettingsDefinitions.Ion_Type.IonFree)
                    {
                        for (int i = 0; i < 2; i++)
                        {
                            if (!double.IsNaN(Sat.PVT_IFCode[i]))
                            {
                                double h = tr + b[isys] - CC * Sat.PVT_IFDtsv[i] + Sat.Trop.SlantDelay;   // 補正はここでまとめて入れる
                                //double h = tr + b[isys] - CC * Sat.PVT_IFDtsv[i] + Sat.Trop.SlantDelay;   // 補正はここでまとめて入れる
                                double y = Sat.Weight * (Sat.PVT_IFCode[i] - h);
                                _H.Add(_Hrow);
                                _Y.Add(y);
                            }
                        }
                    }
                    else
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            if (!double.IsNaN(Sat.PVT_Code[i]))
                            {
                                double h = tr + b[isys] - CC * (Sat.PVT_Dtsv[i] - Sat.PVT_Tgd[i]) + Sat.Trop.SlantDelay;   // 補正はここでまとめて入れる
                                double y = Sat.Weight * (Sat.PVT_Code[i] - h);
                                _H.Add(_Hrow);
                                _Y.Add(y);
                            }
                        }
                    }
                }
                // 要らない列を削除
                double[][] H = DelCols(_H.ToArray(), useCols);
                Matrix HH = new Matrix(H);
                Matrix YY = new Matrix(_Y.ToArray(), _Y.Count);
                Matrix sol = HH.Solve(YY);              // HH*X=Yを解く　TODO:重み付きにする

                double dx = sol.Get(0, 0);
                double dy = sol.Get(1, 0);
                double dz = sol.Get(2, 0);

                // 位置推定値を更新
                XYZ[0] += dx;
                XYZ[1] += dy;
                XYZ[2] += dz;

                // 受信機時計誤差推定値を更新
                int j = 0;
                for (int i = 0; i < MAX_SYS; i++)
                {
                    if (sysuse.Get(i))
                    {
                        b[i] += sol.Get(3 + j, 0);
                        j++;
                    }
                }

                if (Sqrt(dx * dx + dy * dy + dz * dz) < 0.01)
                {
                    Success = true;
                    TimeSuccess = new TimeGR(Sats.Time.Week, Sats.Time.Sow);  // 測位成功時の時刻をTimeSuccessにコピー
                    Pst.SetXYZ(EstPos, XYZ);
                    // Matrix res = YY.Minus(HH.Times(sol));

                    break;
                }
            }
        }

        // LS電離層フリー単独測位
        private void CalcPosIonFree(List<SatelliteData> Data, TimeGR Time)
        {

        }

        /*-----------------
          private methods 
        -----------------*/

        ///// <summary>
        ///// 衛星座標を伝搬時間回転させる
        ///// </summary>
        ///// <param name="Sat"></param>
        ///// <param name="band"></param>
        //private void SatRot(SatelliteData Sat, double[] b)
        //{
        //    //int isys = GetISys(Sat.Sys);
        //    int isys = Sat.Prn.ISys;
        //    double rho = 0.0;
        //    int ind = 0;
        //    // C1,C2,C5の順にサーチして，データのあるものを伝搬時間計算に用いる
        //    // rot後の衛星座標は，擬似距離差10mで衛星座標1mm未満程度の差-->>don't care
        //    for(int i = 0; i < MAX_OBS_PVT; i++)
        //    {
        //        if (!double.IsNaN(Sat.PVT_Code[i]))
        //        {
        //            rho = Sat.PVT_Code[i];
        //            ind = i;
        //            break;
        //        }
        //    }

        //    //double omge = OMGE[isys];
        //    double omge = OMGE(Sat.Prn.Sys);
        //    //double tgd = 0.0;
        //    //double tau = rho / CC - b[isys] / CC + Sat.Dtsv[ind] + tgd;  // GPS,J
        //    double tau = rho / CC - b[isys] / CC + Sat.PVT_Dtsv[ind] + Sat.PVT_Tgd[ind];  // GPS,J    <<<---------ION FREEの時は？
        //    double omegatup = omge * tau;
        //    double Rz00 = Cos(omegatup);
        //    double Rz01 = Sin(omegatup);
        //    double Rz10 = -Rz01;
        //    double Rz11 = Rz00;
        //    Sat.PosRot[0] = Rz00 * Sat.Pos[0] + Rz01 * Sat.Pos[1];
        //    Sat.PosRot[1] = Rz10 * Sat.Pos[0] + Rz11 * Sat.Pos[1];
        //    Sat.PosRot[2] = Sat.Pos[2];
        //    if (Sat.Prn.Sys == SYS.GLO)
        //    {
        //        double[] rpos = { Sat.PosRot[0], Sat.PosRot[1], Sat.PosRot[2] };
        //        PZ2wgs(rpos);
        //        Sat.PosRot[0] = rpos[0];
        //        Sat.PosRot[1] = rpos[1];
        //        Sat.PosRot[2] = rpos[2];
        //    }
        //}
    }
}
