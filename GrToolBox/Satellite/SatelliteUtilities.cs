using GrToolBox.Coordinates;
using GrToolBox.Data.GrObsData;
using GrToolBox.Time;
using static GrToolBox.Common.CommonDefinitions;
using static GrToolBox.Common.CommonUtilities;
using static GrToolBox.Common.Constants;
using static GrToolBox.Coordinates.CoordinateConstants;
using static GrToolBox.Coordinates.CoordinateUtilities;
using static GrToolBox.Satellite.SatelliteConstants;
using static GrToolBox.Settings.SettingsDefinitions;
using static System.Math;
using GrToolBox.Settings;
using static GrToolBox.Satellite.SatelliteDefinitions;
using GrToolBox.Data.GrNavData;

namespace GrToolBox.Satellite
{
    public static class SatelliteUtilities
    {





        //public static void SatPosR_brd(double[] eph, TimeGR time, SatelliteData s)
        //{
        //    double rho = 0.0;
        //    // SatelliteDataを引数にしたときの処理
        //    // C1, C2，C5を順に試す
        //    for (int i = 0; i < 3; i++)
        //    {
        //        if (!double.IsNaN(s.Code[i]))
        //        {
        //            rho = s.Code[i];
        //            break;
        //        }
        //    }
        //    char sys = s.Syschar;
        //    int isys = s.Isys;

        //    // この下は汎用的
        //    double MuR = MU[isys];
        //    double a = SEMI_MAJOR_AXIS_PZ;
        //    double OMGER = OMGE[isys];
        //    double J02 = 1.0826257e-3;  // Second zonal harmonic of the geopotential

        //    int toc_week = (int)eph[22];    // GPSTでのTOC??
        //    double toc_sow = eph[23];

        //    int year = (int)eph[1];
        //    int month = (int)eph[2];
        //    int day = (int)eph[3];
        //    int hour = (int)eph[4];
        //    int min = (int)eph[5];
        //    double sec = eph[6];
        //    double tau_n = eph[7];
        //    double gammaN = eph[8];
        //    double toe = eph[9];  //  toe??
        //    double[] satXYZ = { eph[10] * 1000.0, eph[14] * 1000.0, eph[18] * 1000.0, 0.0, 0.0, 0.0 };
        //    double[] satXYZv = { eph[11] * 1000.0, eph[15] * 1000.0, eph[19] * 1000.0, 0.0, 0.0, 0.0 };
        //    double[] satXYZa = { eph[12] * 1000.0, eph[16] * 1000.0, eph[20] * 1000.0 };

        //    double tut = time.Sow;
        //    double dtr = 0.0;
        //    double tk = tut - rho / CC - toc_sow - dtr;  // (t-toc) tut,toc_sowはGPST基準だがこれで良いか？dtrで吸収するなら反復が必要だがmではやってないような．．．

        //    double dtp = 0.0;
        //    double dtp_old = 0.0;
        //    double tp = 0.0;
        //    double t_toe = 0.0;
        //    for (int i = 0; i < 10; i++)
        //    {
        //        tp = tut - rho / CC - dtp;
        //        t_toe = time_check(tp - toe);
        //        dtp = -tau_n + gammaN * t_toe;
        //        if (Abs(dtp - dtp_old) < 1e-14) break;
        //        dtp_old = dtp;
        //    }
        //    satXYZ[3] = satXYZv[0];
        //    satXYZ[4] = satXYZv[1];
        //    satXYZ[5] = satXYZv[2];

        //    double[] Xglo = new double[6];
        //    for (int i = 0; i < 6; i++) { Xglo[i] = satXYZ[i]; }

        //    double t_base = 30.0;
        //    if (tk < 0) t_base = -30.0;

        //    double r = 0.0;
        //    double[][] Xa = new double[4][];
        //    for(int i=0; i < 4; i++)
        //    {
        //        Xa[i] = new double[6];
        //    }

        //    double tmp1, tmp2, tmp3, tmp4;
        //    while (true)
        //    {
        //        for (int n = 0; n < 4; n++)
        //        {
        //            r = Sqrt(satXYZ[0]*satXYZ[0] + satXYZ[1]*satXYZ[1] + satXYZ[2]*satXYZ[2]);  // todo 無駄な計算多いので書き換えること
        //            tmp1 = -MuR / (r*r*r);
        //            tmp2 = -1.5*J02*MuR*(a*a)/(r*r*r*r*r);
        //            tmp3 = 1 - 5*satXYZ[2]*satXYZ[2]/(r*r);
        //            tmp4 = OMGER * OMGER;
        //            satXYZv[3] = tmp1*satXYZ[0] + tmp2*satXYZ[0]*tmp3 + tmp4*satXYZ[0] + 2*OMGER*satXYZ[4] + satXYZa[0];
        //            satXYZv[4] = tmp1*satXYZ[1] + tmp2*satXYZ[1]*tmp3 + tmp4*satXYZ[1] - 2*OMGER*satXYZ[3] + satXYZa[1];
        //            satXYZv[5] = tmp1*satXYZ[2] + tmp2*satXYZ[2] * (3 - 5 * satXYZ[2] * satXYZ[2] / (r * r)) + satXYZa[2];
        //            //satXYZv[3] = -MuR / (r * r * r) * satXYZ[0] - 1.5 * J02 * MuR * (a * a) / (r * r * r * r * r) * satXYZ[0] * (1 - 5 * satXYZ[2] * satXYZ[2] / (r * r)) + OMGER * OMGER * satXYZ[0] + 2 * OMGER * satXYZ[4] + satXYZa[0];
        //            //satXYZv[4] = -MuR / (r * r * r) * satXYZ[1] - 1.5 * J02 * MuR * (a * a) / (r * r * r * r * r) * satXYZ[1] * (1 - 5 * satXYZ[2] * satXYZ[2] / (r * r)) + OMGER * OMGER * satXYZ[1] - 2 * OMGER * satXYZ[3] + satXYZa[1];
        //            //satXYZv[5] = -MuR / (r * r * r) * satXYZ[2] - 1.5 * J02 * MuR * (a * a) / (r * r * r * r * r) * satXYZ[2] * (3 - 5 * satXYZ[2] * satXYZ[2] / (r * r)) + satXYZa[2];

        //            for (int i = 0; i < 6; i++) { Xa[n][i] = satXYZv[i]; };

        //            double coef = 0.0;
        //            if (n != 3)
        //            {
        //                if (Abs(tk) < Abs(t_base))
        //                {
        //                    if (n == 2) { coef = tk; } else { coef = tk / 2.0; }
        //                }
        //                else
        //                {
        //                    if (n == 2) { coef = t_base; } else { coef = t_base / 2.0; }
        //                }
        //                for (int i = 0; i < 6; i++) { satXYZ[i] = Xglo[i] + satXYZv[i] * coef; }
        //                for (int i = 0; i < 3; i++) { satXYZv[i] = satXYZ[i + 3]; }
        //            }
        //        }
        //        if (Abs(tk) < Abs(t_base))
        //        {
        //            for (int i = 0; i < 6; i++)
        //            {
        //                satXYZ[i] = Xglo[i] + (Xa[0][i] + 2 * Xa[1][i] + 2 * Xa[2][i] + Xa[3][i]) * tk / 6.0;
        //            }
        //            break;
        //        }
        //        else
        //        {
        //            for (int i = 0; i < 6; i++)
        //            {
        //                Xglo[i] = Xglo[i] + (Xa[0][i] + 2 * Xa[1][i] + 2 * Xa[2][i] + Xa[3][i]) * t_base / 6.0;
        //                satXYZ[i] = Xglo[i];
        //            }
        //            tk -= t_base;
        //            for (int i = 0; i < 3; i++) { satXYZv[i] = satXYZ[i + 3]; }
        //        }
        //    }
        //    s.Pos = new double[3];
        //    s.Pos[0] = satXYZ[0];
        //    s.Pos[1] = satXYZ[1];
        //    s.Pos[2] = satXYZ[2];

        //    s.Vel = new double[3];
        //    s.Vel[0] = satXYZv[0] * tk;
        //    s.Vel[1] = satXYZv[1] * tk;
        //    s.Vel[2] = satXYZv[2] * tk;

        //    s.Dtsv = dtp;       // TODO: dtp, tau_nどっち？tau_nはephemeris, Java版もこの順番で上書きのまま放置
        //    s.Dtsv = tau_n;
        //    s.Tgd = 0.0;
        //}






        //public static double[] GetTgd(SatelliteData satData, Ion_Type ion_Type)
        //{
        //    double[] tgd = new double[3] { 0.0, 0.0, 0.0 };
        //    if (ion_Type == Ion_Type.IonFree)
        //    {
        //        return tgd;
        //    }
        //    //switch (satData.Sys)
        //    switch (satData.Prn.Sys)
        //    {
        //        case SYS.GPS:
        //        case SYS.QZS:
        //            tgd[0] = satData.Tgd;               // L1そのまま
        //            tgd[1] = satData.Tgd * GAMMAG12;    // 
        //            tgd[2] = satData.Tgd * GAMMAG15;    // todo 代用 C5I C5Q C5Xのケア
        //            break;
        //        case SYS.GLO:
        //            break;
        //        case SYS.GAL:
        //            tgd[0] = satData.Tgd;
        //            if (satData.Source == 258)
        //            {
        //                tgd[2] = satData.Tgd * GAMMAE15a;
        //            }
        //            else if ((satData.Source == 513) || (satData.Source == 516) || (satData.Source == 517))
        //            {
        //                tgd[2] = satData.Tgd * GAMMAE15b;
        //            }
        //            break;
        //        case SYS.BDS:
        //        case SYS.IRN:
        //        case SYS.SBS:
        //            break;
        //    }
        //    return tgd;


        //}



        public static void AzElDcs(Position pos, double[] sxyz, double[] azel, double[] dcs)
        {
            double[] senu = XYZ2enu(sxyz, pos.Xyz, pos.Llh, Ell_Type.WGS);
            //double[] senu = XYZ2enu(sxyz, xyz);
            double E = senu[0];
            double N = senu[1];
            double U = senu[2];
            double P = Sqrt(E * E + N * N);
            azel[0] = Atan2(E, N);       // 方位角
            azel[1] = Atan(U / P);       // 仰角

            double rrs0, rrs1, rrs2;
            rrs0 = sxyz[0] - pos.Xyz[0];
            rrs1 = sxyz[1] - pos.Xyz[1];
            rrs2 = sxyz[2] - pos.Xyz[2];
            double rho = Sqrt(rrs0 * rrs0 + rrs1 * rrs1 + rrs2 * rrs2);
            dcs[0] = rrs0 / rho;
            dcs[1] = rrs1 / rho;
            dcs[2] = rrs2 / rho;
        }



        public static void CheckSatellites(Satellites sats, GrSettings stg)
        {
            foreach(SatelliteData s in sats.SatData)
            {
                // 測位用データの準備
                int isys = s.Prn.ISys;  // この衛星のシステムインデックス
                //foreach(RawData r in s.Raws)
                //{
                //    for(int i = 0; i < 3; i++)
                //    {
                //        //if (r.ObsCode_Code == stg.ObsCode.Code[isys][i] && r.Valid_Code)
                //        if (r.ObsCode_Code == stg.Observation.Code[isys][i] && r.Valid_Code)
                //        {
                //            //s.Code[i] = r.Pseudorange;
                //            s.Cn0[i] = r.CN0;
                //            s.HaveCode |= (byte)(0b_001 << i);
                //        }
                //        if (r.ObsCode_Phase == stg.Observation.Phase[isys][i] && r.Valid_Phase)
                //        {
                //            s.Phase[i] = r.CarrierPhase;
                //            s.Cn0[i] = r.CN0;
                //            s.HalfCycleAmb[i] = r.HalfCycleAmb;
                //            s.HavePhase |= (byte)(0b_001 << i);
                //        }
                //        if (r.ObsCode_Doppler == stg.Observation.Doppler[isys][i] && r.Valid_Dopper)
                //        {
                //            s.Doppler[i] = r.Doppler;
                //            s.Cn0[i] = r.CN0;
                //            s.HaveDoppler |= (byte)(0b_001 << i);
                //        }
                //    }
                //}
                // チェック開始
                // 測位用データ無し
                switch (stg.Correction.IonType)
                {
                    case Ion_Type.IonFree:
                        if (s.HaveCode < 0b_011 && s.HavePhase < 0b_011)     // ion free測位時, HaveCodeDataは[L5,L2,L1]のフラグなので(0b_011)より小さければion freeデータは無い
                        {
                            s.Status |= 0b_0001 << (byte)Status_Type.No_Data;
                        }
                        break;
                    default:
                        //if (s.HaveCode == 0 && s.HavePhase == 0)            // ion free時以外は，擬似距離・位相データの有無
                        if (s.HaveCode == 0)            // ion free時以外は，擬似距離・位相データの有無
                        {
                            s.Status |= 0b_0001 << (byte)Status_Type.No_Data;
                        }
                        break;
                }
                // 衛星座標無し
                //if (s.Pos == null)
                if (double.IsNaN(s.Pos[0]))
                {
                    s.Status |= 0b_0001 << (byte)Status_Type.No_Sat_Pos;
                }
                // 除外対象の衛星である
                if (stg.Satellite.Exclude.Get(s.Prn))
                {
                    s.Status |= 0b_0001 << (byte)Status_Type.Excluding;
                }
                // 仰角マスクチェック
                if (stg.Satellite.ApplyElevationMask)
                {
                    if (s.Ele < stg.Satellite.EleMask * DEG2RAD)  // Sat.Eleの初期値はNaN. ==>> Eleが確定するまでは仰角マスクに引っかからない
                    {
                        s.Status |= 0b_0001 << (byte)Status_Type.LowElevation;
                    }
                }
                // CNマスクチェック
                if (stg.Satellite.ApplyCnMask)
                {
                    for(int i = 0; i < 3; i++)
                    {
                        double cnvalue = s.PVT_Cn0[i];
                        if (!double.IsNaN(cnvalue))
                        {
                            if ((int)Ceiling(cnvalue) <= stg.Satellite.CnMask[isys][i])
                            {
                                s.Status |= 0b_0001 << (byte)Status_Type.LowCN;
                                break;
                            }
                        }
                    }
                }

                //foreach (double cnvalue in s.PVT_Cn0)
                //{
                //    if (!double.IsNaN(cnvalue))
                //    {
                //        if ((int)Ceiling(cnvalue) <= stg.Satellite.CnMask)
                //        {
                //            s.Status |= 0b_0001 << (byte)Status_Type.LowCN;
                //            break;
                //        }
                //    }
                //}
                // Healthチェック－－－－－暫定的に外す for debug, SYS毎にケアする必要あり
                //if (s.NavData != null && s.NavData.SvHealth > 0)
                //{
                //    s.Status |= 0b_0001 << (byte)Status_Type.UnHealthy;
                //}

                // 衛星・観測数カウント
                sats.NVisible[isys]++;
                if (s.Status == 0)
                {
                    sats.SysUsed.Set(isys, true);
                    sats.NUsed[isys]++;      // 各システムの測位に利用する衛星数

                    // NObsCode: 各システムの測位に利用するコード擬似距離観測数の総数
                    // Ion freeモードなら，電離層フリー結合の総数
                    int nc = INT2RAISEDBIT[s.HaveCode];
                    int np = INT2RAISEDBIT[s.HavePhase];
                    if (stg.Correction.IonType == Ion_Type.IonFree)
                    {
                        if (nc != 0) { nc--; }
                        if (np != 0) { np--; }
                    }
                    sats.NObsCode[isys] += nc;
                    sats.NObsPhase[isys] += np;
                }
            }
        }

        /// <summary>
        /// 測位用データをセットする
        /// </summary>
        /// <param name="sats"></param>
        /// <param name="stg"></param>
        public static void SetObsForPVT(Satellites sats, GrSettings stg)
        {
            foreach(var s in sats.SatData)
            {
                int isys = s.Prn.ISys;  // この衛星のシステムインデックス
                foreach (RawData raw in s.Raws)
                {
                    for (int i = 0; i < MAX_OBS_PVT; i++)
                    {
                        if (raw.ObsCode_Code == stg.Observation.Code[isys][i])
                        {
                            s.PVT_ObsCode[i] = raw.ObsCode;
                            //if (raw.Valid_Code && raw.Pseudorange < 25000000.0 && s.Prn.Snn != "C11")     // TODO 3万キロはGEO除外のfor debug
                            //if (raw.Valid_Code && raw.Pseudorange < 25000000.0)     // TODO 3万キロはGEO除外のfor debug
                            if (raw.Valid_Code)
                            {
                                //s.Code[i] = raw.Pseudorange;
                                s.PVT_Code[i] = raw.Pseudorange;
                                s.HaveCode |= (byte)(0b_001 << i);
                            }
                            if (raw.Valid_Phase)
                            {
                                //s.Phase[i] = raw.CarrierPhase;
                                s.PVT_Phase[i] = raw.CarrierPhase;
                                s.HalfCycleAmb[i] = raw.HalfCycleAmb;
                                s.HavePhase |= (byte)(0b_001 << i);
                            }
                            if (raw.Valid_Dopper)
                            {
                                //s.Doppler[i] = raw.Doppler;
                                s.PVT_Doppler[i] = raw.Doppler;
                                s.HaveDoppler |= (byte)(0b_001 << i);
                            }
                            //s.Cn0[i] = raw.CN0;
                            s.PVT_Cn0[i] = raw.CN0;
                        }
                    }
                }
                //// Ion Freeの格納
                //if (stg.Correction.IonType == Ion_Type.IonFree)
                //{
                //    double ifcoef12 = 0.0;
                //    double ifcoef15 = 0.0;
                //    // Ion Free計算用の係数を選択
                //    switch (s.Prn.Sys)
                //    {
                //        case SYS.GPS:
                //        case SYS.QZS:
                //            ifcoef12 = IFCOEFG12;
                //            ifcoef15 = IFCOEFG15;
                //            break;
                //        case SYS.GLO:
                //            //ifcoef12 = Header.IFCOEFR12[data.Prn];
                //            break;
                //        case SYS.GAL:
                //            ifcoef12 = IFCOEFE15a;  // C5I, C5Q, C5X: on E5a
                //            ifcoef15 = IFCOEFE15b;  // C7I, C7Q, C7X: on E5b
                //            break;
                //        case SYS.BDS:
                //            ifcoef12 = IFCOEFB12I;  // C2(IQX) or C1(IQX; RINEX3.02) on B1I -- C7(IQX) on B2I
                //            ifcoef15 = IFCOEFB13I;  // C2(IQX) or C1(IQX; RINEX3.02) on B1I -- C6(IQX) on B3I
                //            break;
                //    }

                //    // todo IFのときのTGDをケアすること

                //    // L1-L2の電離層フリー結合計算
                //    // HaveCodeのビットフラグ: [L5 L2 L1]なので，L2，L1が入っている場合はL2,L1の結合を計算
                //    // GALの場合は，C1(E1),C5(E5a),C7(E5b)の順で入っているのが前提．ここは(E1-E5a)になる
                //    // BDSの場合は，C2(B1I),C7(B2I),C6(B3I)の順で入っているのが前提．ここは(B1I-B2I)になる
                //    if ((s.HaveCode & 0b_011) == 0b_011)
                //    {
                //        s.PVT_IFCode[0] = s.PVT_Code[0] + ifcoef12 * (s.PVT_Code[0] - s.PVT_Code[1]);

                //    }
                //    // L1-L5の電離層フリー結合計算（GAL: E1-E5b）(BDS: B1I-B3I)
                //    if ((s.HaveCode & 0b_101) == 0b_101)
                //    {
                //        s.PVT_IFCode[1] = s.PVT_Code[0] + ifcoef15 * (s.PVT_Code[0] - s.PVT_Code[2]);
                //    }
                //}
            }
        }




        public static void SetIFObsForPVT(Satellites sats, GrSettings stg)
        {
            foreach (var s in sats.SatData)
            {
                // Ion Freeの格納
                if (stg.Correction.IonType == Ion_Type.IonFree)
                {
                    //double ifcoef12 = 0.0;
                    //double ifcoef15 = 0.0;
                    // Ion Free計算用の係数を選択
                    switch (s.Prn.Sys)
                    {
                        case SYS.GPS:
                        case SYS.QZS:
                            // IFCOEFG12: C1--C2
                            // IFCOEFG15: C1--C5

                            // L1-L2の電離層フリー結合計算
                            // HaveCodeのビットフラグ: [L5 L2 L1]なので，L2，L1が入っている場合はL2,L1の結合を計算
                            if ((s.HaveCode & 0b_011) == 0b_011)
                            {
                                s.PVT_IFCode[0] = s.PVT_Code[0] + IFCOEFG12 * (s.PVT_Code[0] - s.PVT_Code[1]);
                                s.HaveIFCode |= 0b_01;
                                s.PVT_IFDtsv[0] = s.PVT_Dtsv[0];
                            }
                            // L1-L5の電離層フリー結合計算 GPSのL5は要修正
                            if ((s.HaveCode & 0b_101) == 0b_101)
                            {
                                s.PVT_IFCode[1] = s.PVT_Code[0] + IFCOEFG15 * (s.PVT_Code[0] - s.PVT_Code[2]);
                                s.HaveIFCode |= 0b_10;
                                s.PVT_IFDtsv[1] = s.PVT_Dtsv[0];
                            }
                            break;
                        case SYS.GLO:
                            //ifcoef12 = Header.IFCOEFR12[data.Prn];
                            break;
                        case SYS.GAL:
                            // IFCOEFE15a: E1--E5a (C5I, C5Q, C5X)
                            // IFCOEFE15b: E1--E5b (C7I, C7Q, C7X)

                            // C1(E1),C5(E5a),C7(E5b)の順で入っているのが前提．ここは(E1-E5a)になる.DtsvはFNAV
                            if ((s.HaveCode & 0b_011) == 0b_011)
                            {
                                s.PVT_IFCode[0] = s.PVT_Code[0] + IFCOEFE15a * (s.PVT_Code[0] - s.PVT_Code[1]);
                                s.HaveIFCode |= 0b_01;
                                s.PVT_IFDtsv[0] = s.Dtsv_E1_FNAV;
                            }
                            // E1-E5b, DtsvはINAV
                            if ((s.HaveCode & 0b_101) == 0b_101)
                            {
                                s.PVT_IFCode[1] = s.PVT_Code[0] + IFCOEFE15b * (s.PVT_Code[0] - s.PVT_Code[2]);
                                s.HaveIFCode |= 0b_10;
                                s.PVT_IFDtsv[1] = s.PVT_Dtsv[0];
                            }
                            break;
                        case SYS.BDS:
                            // IFCOEFB12I: C2(IQX) or C1(IQX; RINEX3.02) on B1I -- C7(IQX) on B2I
                            // IFCOEFB13I; C2(IQX) or C1(IQX; RINEX3.02) on B1I -- C6(IQX) on B3I
                            // GAMMAB12I: (FB1I/FB2I)^2
                            // GAMMAB13I: (FB1I/FB3I)^2
                            double rho1, rho2, rho3;

                            // C1 or C2(B1I),C7(B2I),C6(B3I)の順で入っているのが前提．ここは(B1I-B2I)になる.
                            if ((s.HaveCode & 0b_011) == 0b_011)
                            {
                                rho1 = s.PVT_Code[0] - CC * GAMMAB12I * s.BDS_NavData.Tgd1;
                                rho2 = s.PVT_Code[1] - CC * s.BDS_NavData.Tgd2;
                                s.PVT_IFCode[0] = rho1 + IFCOEFB12I * (rho1 - rho2);
                                s.HaveIFCode |= 0b_01;
                                s.PVT_IFDtsv[0] = s.PVT_Dtsv[0];
                            }
                            // E1-E5b, DtsvはINAV
                            if ((s.HaveCode & 0b_101) == 0b_101)
                            {
                                rho1 = s.PVT_Code[0] - CC * GAMMAB13I * s.BDS_NavData.Tgd1;
                                rho3 = s.PVT_Code[2];
                                s.PVT_IFCode[1] = rho1 + IFCOEFB13I * (rho1 - rho3);
                                s.HaveIFCode |= 0b_10;
                                s.PVT_IFDtsv[1] = s.PVT_Dtsv[0];
                            }
                            break;
                    }
                    
                    // L1-L2の電離層フリー結合計算
                    // HaveCodeのビットフラグ: [L5 L2 L1]なので，L2，L1が入っている場合はL2,L1の結合を計算
                    // GALの場合は，C1(E1),C5(E5a),C7(E5b)の順で入っているのが前提．ここは(E1-E5a)になる
                    // BDSの場合は，C2(B1I),C7(B2I),C6(B3I)の順で入っているのが前提．ここは(B1I-B2I)になる．TGDの扱いが必要
                    //if ((s.HaveCode & 0b_011) == 0b_011)
                    //{
                    //    if(s.Prn.Sys != SYS.BDS)
                    //    {
                    //        s.PVT_IFCode[0] = s.PVT_Code[0] + ifcoef12 * (s.PVT_Code[0] - s.PVT_Code[1]);
                    //    }
                    //    else
                    //    {
                    //        s.PVT_IFCode[0] = s.PVT_Code[0] + ifcoef12 * (s.PVT_Code[0] - s.PVT_Code[1]);
                    //    }

                    //}
                    //// L1-L5の電離層フリー結合計算（GAL: E1-E5b）(BDS: B1I-B3I)
                    //if ((s.HaveCode & 0b_101) == 0b_101)
                    //{

                    //    s.PVT_IFCode[1] = s.PVT_Code[0] + ifcoef15 * (s.PVT_Code[0] - s.PVT_Code[2]);
                    //}
                }








            }





        }


        /// <summary>
        /// 観測量に対する重みをセットする
        /// </summary>
        /// <param name="sats"></param>
        /// <param name="stg"></param>
        public static void SetObsWeight(Satellites sats, GrSettings stg)
        {
            if(stg.Correction.WeightingType == Weighting_Type.None)
            {
                return;
            }

            foreach(var s in sats.SatData)
            {
                if (double.IsNaN(s.Ele))
                {
                    continue;
                }
                s.Weight = 1.0 / Math.Sin(s.Ele);
            }

        }




    }
}







