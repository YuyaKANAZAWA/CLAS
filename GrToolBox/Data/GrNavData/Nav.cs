using GrToolBox.Satellite;
using GrToolBox.Settings;
using GrToolBox.Time;
using System;
using System.Collections.Generic;
using System.Linq;
using static GrToolBox.Common.CommonDefinitions;
using static GrToolBox.Common.Constants;
using static GrToolBox.Data.GrNavData.NavUtilities;
using static GrToolBox.Satellite.SatelliteConstants;
using static GrToolBox.Settings.SettingsDefinitions;

namespace GrToolBox.Data.GrNavData
{
    public class Nav
    {
        //public List<NavData> NavDatas { get; set; } = new List<NavData>();
        public NavData NavData { get; set; } = new();
        private DataTypesFiles_Settings Setting { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        public Nav(DataTypesFiles_Settings dataTypesFiles)
        {
            Setting = dataTypesFiles;
        }
        public Nav(GrSettings stg) : this(stg.DataTypesFiles) { }

        public void SetSatPos(Satellites Sats)
        {
            switch (Setting.NavType)
            {
                case NavClock_Type.Broadcast:
                    foreach (SatelliteData satData in Sats.SatData)
                    {
                        SetNavData(satData, Sats.Time);
                        CalcSatPos(satData, Sats.Time);
                    }
                    break;
            }
        }

        /// <summary>
        /// selecting appropriate ephemeris for positioning
        /// </summary>
        /// <param name="satData"></param>
        /// <param name="time"></param>
        private void SetNavData(SatelliteData satData, TimeGR time)
        {
            switch (satData.Prn.Sys)
            {
                case SYS.GPS:
                    var canGPS = NavData.GPS.Where(s => s.Prn.N == satData.Prn.N)    // prnが一致するもの
                                            .Where(s => Math.Abs((time.Week - s.WeekNum) * 604800 + (time.Sow - s.Toe)) < s.FitInterval / 2.0 * 3600)  // fit interval内のもの
                                            .LastOrDefault(); // 最新のもの
                    satData.GPS_NavData = canGPS;
                    break;
                case SYS.GAL:
                    var canGAL = NavData.GAL.Where(s => s.Prn.N == satData.Prn.N)    // prnが一致するもの
                                            .Where(s => Math.Abs((time.Week - s.WeekNum) * 604800 + (time.Sow - s.Toe)) < 4.0 / 2.0 * 3600);  // fit interval内のもの
                    var candF = canGAL.Where(s => s.Source == 16).LastOrDefault();  // FNAV用eph
                    var candI = canGAL.Where(s => s.Source == 2).LastOrDefault();   // INAV用eph
                    if(candF != null && candI != null)
                    {
                        if (candF.IODNav < candI.IODNav)    // IODNavが異なる場合，どちらかに合わす
                        {
                            candI = canGAL.Where(s => s.Source == 2).Where(s => s.IODNav == candF.IODNav).LastOrDefault();
                        }
                        else if (candF.IODNav > candI.IODNav)
                        {
                            candF = canGAL.Where(s => s.Source == 16).Where(s => s.IODNav == candI.IODNav).LastOrDefault();
                        }

                    }
                    satData.GAL_FNavData = candF;
                    satData.GAL_INavData = candI;
                    break;
                case SYS.QZS:
                    var canQZS = NavData.GPS.Where(s => s.Prn.N == satData.Prn.N)    // prnが一致するもの qzsデータはgpsに入っている
                                            .Where(s => Math.Abs((time.Week - s.WeekNum) * 604800 + (time.Sow - s.Toe)) < s.FitInterval / 2.0 * 3600)  // fit interval内のもの
                                            .LastOrDefault(); // 最新のもの
                    satData.GPS_NavData = canQZS;
                    //var canQZS = NavData.QZS.Where(s => s.Prn.N == satData.Prn.N)    // prnが一致するもの
                    //                        .Where(s => Math.Abs((time.Week - s.WeekNum) * 604800 + (time.Sow - s.Toe)) < s.FitInterval / 2.0 * 3600)  // fit interval内のもの
                    //                        .LastOrDefault(); // 最新のもの
                    //satData.GPS_NavData = canQZS;
                    break;
                case SYS.BDS:
                    var canBDS = NavData.BDS.Where(s => s.Prn.N == satData.Prn.N)    // prnが一致するもの
                                            .Where(s => Math.Abs((time.Week - s.WeekNum) * 604800 + (time.Sow - s.Toe)) < 3600)  // 通常1時間で更新
                                            .LastOrDefault(); // 最新のもの
                    satData.BDS_NavData = canBDS;
                    //var testcand = NavDatas.Where(s => s.Prn.N == satData.Prn.N);
                    break;
            }
        }

        private void CalcSatPos(SatelliteData satData, TimeGR time)
        {
            switch (satData.Prn.Sys)
            {
                case SYS.GPS:
                case SYS.QZS:
                    if (satData.GPS_NavData == null) break;
                    var gpsNav = GetComNavGEJC(satData.GPS_NavData);
                    double rho;
                    double[] SvcEaTtoe = new double[3];
                    bool haveClk = false;
                    // 衛星時計誤差と離心近点角を求める
                    for (int i = 0; i < MAX_OBS_PVT; i++)
                    {
                        rho = satData.PVT_Code[i];
                        if (double.IsNaN(rho)) continue;
                        if (!haveClk)
                        {
                            haveClk = SatClkGEJC_brd(rho, time, gpsNav, SvcEaTtoe);
                        }
                        satData.PVT_Dtsv[i] = SvcEaTtoe[0];             // 観測量毎に衛星時計誤差を格納
                        satData.PVT_IFDtsv[i] = SvcEaTtoe[0];             // 観測量毎に衛星時計誤差を格納
                        switch (satData.PVT_ObsCode[i][0])          // 観測量毎にTGDを格納
                        {
                            case '1': satData.PVT_Tgd[i] = satData.GPS_NavData.Tgd; 
                                break;
                            case '2': satData.PVT_Tgd[i] = satData.GPS_NavData.Tgd * GAMMAG12; break;
                            case '5': satData.PVT_Tgd[i] = satData.GPS_NavData.Tgd * GAMMAG15; break;   // L5は要修正
                        }
                    }
                    // 測位用の擬似距離エントリがない場合はRawsを順にサーチして利用する（衛星plot用に衛星座標は求めておく）
                    if (!haveClk)
                    {
                        foreach (var raw in satData.Raws)
                        {
                            rho = double.NaN;
                            if (raw.Valid_Code)
                            {
                                rho = raw.Pseudorange;
                                haveClk = SatClkGEJC_brd(rho, time, gpsNav, SvcEaTtoe);
                                //haveClk = true;
                                break;
                            }
                        }
                    }
                    //衛星座標を求める, EA, T_toeはどの擬似距離データを使ってもmm単位で一致する（evernoteまとめ）
                    if (haveClk)
                    {
                        double EA = SvcEaTtoe[1];       // 離心近点角
                        double t_toe = SvcEaTtoe[2];    // toeからの時間
                        SatPosGEJC_brd(gpsNav, EA, t_toe, satData.Pos, satData.Vel);
                    }
                    break;
                    //
                case SYS.GAL:
                    if (satData.GAL_FNavData == null && satData.GAL_INavData == null)   break;
                    var galFNav = GetComNavGEJC(satData.GAL_FNavData);
                    var galINav = GetComNavGEJC(satData.GAL_INavData);
                    SvcEaTtoe = new double[3];
                    haveClk = false;
                    // 衛星時計誤差と離心近点角を求める
                    for (int i = 0; i < MAX_OBS_PVT; i++)
                    {
                        rho = satData.PVT_Code[i];
                        if (double.IsNaN(rho)) continue;

                        if (satData.PVT_ObsCode[i][0] == '1')   // E1: E1のsingleはINAV，(E1,E5a)のdualはFNAV
                        {
                            haveClk = SatClkGEJC_brd(rho, time, galINav, SvcEaTtoe);   // E1--INAV
                            if (haveClk)
                            {
                                satData.PVT_Dtsv[i] = SvcEaTtoe[0];
                                satData.PVT_Tgd[i] = satData.GAL_INavData.BGD_L1E5b;
                            }
                            haveClk = SatClkGEJC_brd(rho, time, galFNav, SvcEaTtoe);   // E1--FNAV
                            if (haveClk)
                            {
                                satData.Dtsv_E1_FNAV = SvcEaTtoe[0];
                            }
                        }
                        if (satData.PVT_ObsCode[i][0] == '5')   // E5a: E5aのsingleはFNAV
                        {
                            haveClk = SatClkGEJC_brd(rho, time, galFNav, SvcEaTtoe);   // E5a--FNAV
                            if (haveClk)
                            {
                                satData.PVT_Dtsv[i] = SvcEaTtoe[0];
                                satData.PVT_Tgd[i] = satData.GAL_FNavData.BGD_L1E5a * GAMMAE15a;
                            }
                        }
                        if (satData.PVT_ObsCode[i][0] == '7')   // E5b: E5bのsingleはINAV，(E1,E5b)のdualはINAV
                        {
                            haveClk = SatClkGEJC_brd(rho, time, galINav, SvcEaTtoe);   // E5b--INAV
                            if (haveClk)
                            {
                                satData.PVT_Dtsv[i] = SvcEaTtoe[0];
                                satData.PVT_Tgd[i] = satData.GAL_INavData.BGD_L1E5b * GAMMAE15b;
                            }
                        }
                    }
                    // 測位用の擬似距離エントリがない場合はRawsを順にサーチして利用する（衛星plot用に衛星座標は求めておく）
                    if (!haveClk)
                    {
                        foreach (var raw in satData.Raws)
                        {
                            rho = double.NaN;
                            if (raw.Valid_Code)
                            {
                                rho = raw.Pseudorange;
                                haveClk = SatClkGEJC_brd(rho, time, galFNav, SvcEaTtoe);
                                if (haveClk) break;
                                haveClk = SatClkGEJC_brd(rho, time, galINav, SvcEaTtoe);
                                if (haveClk) break;
                            }
                        }
                    }
                    if (haveClk)
                    {
                        double EA = SvcEaTtoe[1];       // 離心近点角
                        double t_toe = SvcEaTtoe[2];    // toeからの時間
                        SatPosGEJC_brd(galINav, EA, t_toe, satData.Pos, satData.Vel);
                    }
                    break;
                //
                //case SYS.QZS:
                //    if (satData.QZS_NavData == null) break;
                //    var qzsNav = GetComNavGEJC(satData.QZS_NavData);
                //    SvcEaTtoe = new double[3];
                //    haveClk = false;
                //    // 衛星時計誤差と離心近点角を求める
                //    for (int i = 0; i < MAX_OBS_PVT; i++)
                //    {
                //        rho = satData.PVT_Code[i];
                //        if (double.IsNaN(rho)) continue;
                //        if (!haveClk)
                //        {
                //            haveClk = SatClkGEJC_brd(rho, time, qzsNav, SvcEaTtoe);
                //        }
                //        satData.PVT_Dtsv[i] = SvcEaTtoe[0];             // 観測量毎に衛星時計誤差を格納
                //        satData.PVT_IFDtsv[i] = SvcEaTtoe[0];             // 観測量毎に衛星時計誤差を格納
                //        switch (satData.PVT_ObsCode[i][0])          // 観測量毎にTGDを格納
                //        {
                //            case '1':
                //                satData.PVT_Tgd[i] = satData.QZS_NavData.Tgd;
                //                break;
                //            case '2': satData.PVT_Tgd[i] = satData.QZS_NavData.Tgd * GAMMAG12; break;
                //            case '5': satData.PVT_Tgd[i] = satData.QZS_NavData.Tgd * GAMMAG15; break;   // L5は要修正
                //        }
                //    }
                //    // 測位用の擬似距離エントリがない場合はRawsを順にサーチして利用する（衛星plot用に衛星座標は求めておく）
                //    if (!haveClk)
                //    {
                //        foreach (var raw in satData.Raws)
                //        {
                //            rho = double.NaN;
                //            if (raw.Valid_Code)
                //            {
                //                rho = raw.Pseudorange;
                //                haveClk = SatClkGEJC_brd(rho, time, qzsNav, SvcEaTtoe);
                //                break;
                //            }
                //        }
                //    }
                //    //衛星座標を求める, EA, T_toeはどの擬似距離データを使ってもmm単位で一致する（evernoteまとめ）
                //    if (haveClk)
                //    {
                //        double EA = SvcEaTtoe[1];       // 離心近点角
                //        double t_toe = SvcEaTtoe[2];    // toeからの時間
                //        SatPosGEJC_brd(qzsNav, EA, t_toe, satData.Pos, satData.Vel);
                //    }
                //    break;
                ////

                case SYS.BDS:
                    if (satData.BDS_NavData == null) break;
                    var bdsNav = GetComNavGEJC(satData.BDS_NavData);
                    SvcEaTtoe = new double[3];
                    haveClk = false;
                    // 衛星時計誤差と離心近点角を求める
                    for (int i = 0; i < MAX_OBS_PVT; i++)
                    {
                        rho = satData.PVT_Code[i];
                        if (double.IsNaN(rho)) continue;
                        if (!haveClk)
                        {
                            haveClk = SatClkGEJC_brd(rho, time, bdsNav, SvcEaTtoe);
                            //haveClk = true;
                        }
                        satData.PVT_Dtsv[i] = SvcEaTtoe[0];             // 観測量毎に衛星時計誤差を格納
                        switch (satData.PVT_ObsCode[i][0])          // 観測量毎にTGDを格納
                        {
                            case '1':   // B1;Rinex3.02
                            case '2':   // B1;Rinex3.03
                                satData.PVT_Tgd[i] = satData.BDS_NavData.Tgd1;
                                break;
                            case '7': satData.PVT_Tgd[i] = satData.BDS_NavData.Tgd2; break;
                            case '6': satData.PVT_Tgd[i] = 0.0; break;   // 
                        }
                    }
                    // 測位用の擬似距離エントリがない場合はRawsを順にサーチして利用する（衛星plot用に衛星座標は求めておく）
                    if (!haveClk)
                    {
                        foreach (var raw in satData.Raws)
                        {
                            rho = double.NaN;
                            if (raw.Valid_Code)
                            {
                                rho = raw.Pseudorange;
                                haveClk = SatClkGEJC_brd(rho, time, bdsNav, SvcEaTtoe);
                                //haveClk = true;
                                break;
                            }
                        }
                    }
                    //衛星座標を求める, EA, T_toeはどの擬似距離データを使ってもmm単位で一致する（evernoteまとめ）
                    if (haveClk)
                    {
                        double EA = SvcEaTtoe[1];       // 離心近点角
                        double t_toe = SvcEaTtoe[2];    // toeからの時間
                        SatPosGEJC_brd(bdsNav, EA, t_toe, satData.Pos, satData.Vel);
                    }
                    break;
            }
        }
    }
}
