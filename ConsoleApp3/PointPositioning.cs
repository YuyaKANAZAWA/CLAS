using GrToolBox.Coordinates;
using GrToolBox.Data.GrNavData;
using GrToolBox.Output;
using GrToolBox.Positioning;
using GrToolBox.Satellite;
using GrToolBox.Settings;
using System.Diagnostics;
using System.Text;
using static GrToolBox.Output.EpochPosDataStore;
using static GrToolBox.Output.ResultSetter;
using static GrToolBox.Satellite.SatelliteUtilities;

namespace ConsoleApp3
{
    public class PointPositioning
    {
        public bool Success { get; private set; }
        public PositionSetter Pst { get; set; }


        private GrSettings? Stg { get; }
        private PointPositioning_LS PPLS { get; set; }
        private PointPositioning_KF PPKF { get; set; }

        private bool FirstSuccess = true;

        public List<PosResultGR> PosResults = new();
        public List<PosResultGR> PosResultsKF = new();


        private bool UseKF { get; set; } = false;


        public PointPositioning(GrSettings stg)
        {
            if (stg == null)
            {
                Stg = new GrSettings();
            }
            else
            {
                Stg = stg;
            }
            Pst = new PositionSetter(Stg);
            PPLS = new PointPositioning_LS(Pst, Stg);   // LSはどんな測位法でも必ず回す
            PPKF = new PointPositioning_KF(Pst, Stg);
            if (Stg.PositioningMode.EstType == SettingsDefinitions.Estimation_Type.Kalman)
            {
                UseKF = true;
            }

            //if (Stg.PositioningMode.PosType == SettingsDefinitions.Positioning_Type.PointPositioning)
            //{
            //    PPLS = new PointPositioning_LS(Pst, Stg);   // LSはどんな測位法でも必ず回す
            //    if (Stg.PositioningMode.EstType == SettingsDefinitions.Estimation_Type.Kalman)
            //    {
            //        UseKF = true;
            //        PPKF = new PointPositioning_KF(Pst, Stg);
            //    }
            //}
        }



        public void Calc(Nav Nav, Satellites Sats)
        {
            SetObsForPVT(Sats, Stg);
            if (Stg.Correction.IonType == SettingsDefinitions.Ion_Type.IonFree)
            {
                SetIFObsForPVT(Sats, Stg);
            }
            Nav.SetSatPos(Sats);                    // set sat pos をNAVからはずす
            Sats.SetEleAzi(PPLS.EstPos, PPLS.TimeSuccess);
            SetObsWeight(Sats, Stg);
            CheckSatellites(Sats, Stg);
            PPLS.CalcPos(Sats);
            if (FirstSuccess && PPLS.Success)  // 初回エポックは仰角等を計算し直して再度測位演算を行う
            {
                FirstSuccess = false;
                if (!Pst.HaveEnuOrg)
                {
                    Pst.SetOrgXYZ(PPLS.EstPos.Xyz);   //  初回位置推定値をENU原点にセットしてENU座標を上書きセット
                    Pst.SetXYZ(PPLS.EstPos, PPLS.EstPos.Xyz);
                }
                Sats.SetEleAzi(PPLS.EstPos, PPLS.TimeSuccess);
                CheckSatellites(Sats, Stg);
                PPLS.CalcPos(Sats);
                Pst.SetOrgXYZ(PPLS.EstPos.Xyz);   //  仰角マスク等を反映した後，ENU原点を再セットしてENU座標を上書き
                Pst.SetXYZ(PPLS.EstPos, PPLS.EstPos.Xyz);

                if (UseKF)
                {
                    PPKF.SetInitialPosClk(PPLS.XYZ, PPLS.b);    // 初回LS推定値をKFの初期値として利用
                }
            }
            var result = SetPointPosResult(PPLS, Sats);
            PosResults.Add(result);

            if (UseKF)
            {
                PPKF.CalcPos(Sats);
                var resultkf = SetPointPosResult(PPKF, Sats);
                PosResultsKF.Add(resultkf);
            }
        }


    }
}
