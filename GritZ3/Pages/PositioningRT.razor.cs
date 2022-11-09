using AntDesign;
using GritZ3.Classes;
using GritZ3.Positioning;
using GrToolBox.Communication;
using GrToolBox.Data.GrNavData;
using GrToolBox.Data.Nmea;
using GrToolBox.Data.SBF;
using GrToolBox.Output;
using GrToolBox.Settings;
using System.Diagnostics;
using static GrToolBox.Output.EpochPosDataStore;
using static GrToolBox.Settings.SettingsDefinitions;
using static GrToolBox.Data.Nmea.NmeaConverter;
using GritZ3.Components.Cards;

namespace GritZ3.Pages
{
    public partial class PositioningRT
    {
        public DrawerInfo DataSourceInfo { get; set; } = new DrawerInfo() { ID = 1, Name = "DataSource", Visible = false };
        public DrawerInfo PositioningMethodsInfo { get; set; } = new DrawerInfo() { ID = 2, Name = "PositioningMethods", Visible = false };
        public DrawerInfo SignalsInfo { get; set; } = new DrawerInfo() { ID = 3, Name = "Signals", Visible = false };
        public DrawerInfo SatelliteInfo { get; set; } = new DrawerInfo() { ID = 4, Name = "Satellite", Visible = false };
        public DrawerInfo IonTropInfo { get; set; } = new DrawerInfo() { ID = 5, Name = "IonTrop", Visible = false };
        public DrawerInfo TemplateInfo { get; set; } = new DrawerInfo() { ID = 6, Name = "Template", Visible = false };
        public GrSettings Stg { get; set; } = new GrSettings();
        ParamsPosRTControlCard ParamsControlCard = new(2);
        private void Open_DrawerDataSource()
        {
            DataSourceInfo.Visible = true;
        }
        private void Open_DrawerPositioningMethods()
        {
            PositioningMethodsInfo.Visible = true;
        }
        private void Open_DrawerSignals()
        {
            SignalsInfo.Visible = true;
        }
        private void Open_DrawerSatellite()
        {
            SatelliteInfo.Visible = true;
        }
        private void Open_DrawerIonTrop()
        {
            IonTropInfo.Visible = true;
        }
        private void Open_DrawerTemplate()
        {
            TemplateInfo.Visible = true;
        }
        private string ActiveKey { get; set; } = "1";       // 表示タブ

        public PositioningRT()
        {

        }
        protected override async Task OnInitializedAsync()
        {

        }
        protected async Task ReloadSerialList()
        {

        }
        private Nav Nav { get; set; }
        private PointPositioning PP { get; set; }
        private SerialDevice SD { get; set; }
        private TCPClient? TCPClient { get; set; }
        private SbfConverter? SbfConverter { get; set; }
        private List<PVTCartesian_4006> ResultSbf { get; set; } = new List<PVTCartesian_4006>();
        private NmeaConverter? NmeaConverter { get; set; }
        private NmeaBurstData LatestNmeaBurst { get; set; } = new();


        private void OnClickStartStopButton(string str)
        {
            // for debug取りあえずベタ打ち
            //Stg.Satellite.EleMask = 15;
            //Stg.Correction.TropType = Trop_Type.MOPS;
            Stg.Correction.WeightingType = Weighting_Type.Inv_Sin;

            EpochPosDataStore.NewEpochPosDataExist += OnExistNewEpochData;

            var type = Stg.Communication.Connection;
            if (type == Connection_Type.Serial)
            {
                if (str == "Start")
                {
                    StartPositioning_Serial();
                }
                else if (str == "Stop")
                {
                    StopPositioning_Serial();
                }
            }
            else if(type == Connection_Type.TCP)
            {
                if (str == "Start")
                {
                    StartPositioning_Tcp();
                }
                else if (str == "Stop")
                {
                    StopPositioning_Tcp();
                }
            }
        }

        private void StartPositioning_Serial()
        {
            if (string.IsNullOrEmpty(Stg.Communication.SerialPort.PortName))
            {
                if (ParamsControlCard.AlertDatas.Count > 2)
                {
                    ParamsControlCard.AlertDatas = new Stack<AlertData>(ParamsControlCard.AlertDatas.Reverse().Skip(1));
                }
                ParamsControlCard.AlertDatas.Push(new AlertData { TimeData = DateTime.Now, Type = AlertType.Error, Text = "Serial port name is not specified." });
                return;
            }
            //if(SbfConverter != null)
            //{
            //    SbfConverter.NewSbfDataReceived -= CalcAndDispSbfData;
            //    SbfConverter.NoSbfDataTimerReached -= NoSbfDataReceived;
            //    SbfConverter = null;
            //}
            Nav = new(Stg);
            PP = new(Stg);
            SD = new(Stg);
            SbfConverter = new SbfConverter();
            SbfConverter.NewSbfDataReceived += CalcAndDispSbfData;
            SbfConverter.NoSbfDataTimerReached += NoSbfDataReceived;
            SD.AddConverter(SbfConverter);

            NmeaConverter = new NmeaConverter();
            NmeaConverter.NewNmeaDataReceived += OnReceiveNmeaData;
            NmeaConverter.NoNmeaDataTimerReached += NoNmeaDataReceived;
            SD.AddConverter(NmeaConverter);

            if (ParamsControlCard.AlertDatas.Count > 2)
            {
                ParamsControlCard.AlertDatas = new Stack<AlertData>(ParamsControlCard.AlertDatas.Reverse().Skip(1));
            }
            if (SD.Open())
            {
                ParamsControlCard.StartButtonDisabled = true;
                ParamsControlCard.StopButtonDisabled = false;
                ParamsControlCard.AlertDatas.Push(new AlertData { TimeData = DateTime.Now, Type = AlertType.Success, Text = Stg.Communication.SerialPort.PortName + " is opened" });
            }
            else
            {
                ParamsControlCard.AlertDatas.Push(new AlertData { TimeData = DateTime.Now, Type = AlertType.Error, Text = Stg.Communication.SerialPort.PortName + " can not be opened" });
            }

        }

        private void StopPositioning_Serial()
        {
            if (SD.Close())
            {
                ParamsControlCard.StartButtonDisabled = false;
                ParamsControlCard.StopButtonDisabled = true;
                if (ParamsControlCard.AlertDatas.Count > 2)
                {
                    ParamsControlCard.AlertDatas = new Stack<AlertData>(ParamsControlCard.AlertDatas.Reverse().Skip(1));
                }
                ParamsControlCard.AlertDatas.Push(new AlertData { TimeData = DateTime.Now, Type = AlertType.Warning, Text = Stg.Communication.SerialPort.PortName + " is closed" });

                ParamsControlCard.BadgeData[0].Status = "error";
                ParamsControlCard.BadgeData[0].Description = "1: No connection";
                ParamsControlCard.BadgeData[1].Status = "error";
                ParamsControlCard.BadgeData[1].Description = "1: No connection";
            }

            if (SbfConverter != null)
            {
                SbfConverter.NewSbfDataReceived -= CalcAndDispSbfData;
                SbfConverter.NoSbfDataTimerReached -= NoSbfDataReceived;
                SbfConverter = null;
            }

            if (NmeaConverter != null)
            {
                NmeaConverter.NewNmeaDataReceived -= OnReceiveNmeaData;
                NmeaConverter.NoNmeaDataTimerReached -= NoNmeaDataReceived;
                NmeaConverter = null;
            }

        }



        private void StartPositioning_Tcp()
        {
            Nav = new(Stg);
            PP = new(Stg);

            TCPClient = new TCPClient(Stg);
            TCPClient.OnConnected += OnTcpConnected;
            TCPClient.OnDisconnected += OnTcpDisconnected;
            NmeaConverter = new NmeaConverter();
            NmeaConverter.NewNmeaDataReceived += OnReceiveNmeaData;
            NmeaConverter.NoNmeaDataTimerReached += NoNmeaDataReceived;
            SbfConverter = new SbfConverter();
            SbfConverter.NewSbfDataReceived += CalcAndDispSbfData;
            SbfConverter.NoSbfDataTimerReached += NoSbfDataReceived;

            TCPClient.AddConverter(NmeaConverter);
            TCPClient.AddConverter(SbfConverter);

            if (ParamsControlCard.AlertDatas.Count > 2)
            {
                ParamsControlCard.AlertDatas = new Stack<AlertData>(ParamsControlCard.AlertDatas.Reverse().Skip(1));
            }

            try
            {
                //接続処理
                TCPClient.Connect();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                ParamsControlCard.AlertDatas.Push(new AlertData { TimeData = DateTime.Now, Type = AlertType.Error, Text = "TCP/IP not connected" });
            }
        }


        private void StopPositioning_Tcp()
        {
            if (TCPClient != null)
            {
                TCPClient.Close();
            }
        }

        /** TCP接続OKイベント **/
        private void OnTcpConnected(EventArgs e)
        {
            if (ParamsControlCard.AlertDatas.Count > 2)
            {
                ParamsControlCard.AlertDatas = new Stack<AlertData>(ParamsControlCard.AlertDatas.Reverse().Skip(1));
            }
            //接続OK処理
            Debug.WriteLine("TCPClient_OnConnected" + " ThreadID:" + Thread.CurrentThread.ManagedThreadId);
            ParamsControlCard.StartButtonDisabled = true;
            ParamsControlCard.StopButtonDisabled = false;
            ParamsControlCard.AlertDatas.Push(new AlertData { TimeData = DateTime.Now, Type = AlertType.Success, Text = $"Connected ({Stg.Communication.TcpClient.IpAddress}:{Stg.Communication.TcpClient.Port})" });
        }

        private void OnTcpDisconnected(object? sender, EventArgs e)
        {
            ParamsControlCard.StartButtonDisabled = false;
            ParamsControlCard.StopButtonDisabled = true;
            if (ParamsControlCard.AlertDatas.Count > 2)
            {
                ParamsControlCard.AlertDatas = new Stack<AlertData>(ParamsControlCard.AlertDatas.Reverse().Skip(1));
            }
            ParamsControlCard.AlertDatas.Push(new AlertData { TimeData = DateTime.Now, Type = AlertType.Warning, Text = $"Disconnected ({Stg.Communication.TcpClient.IpAddress}:{Stg.Communication.TcpClient.Port})" });
            ParamsControlCard.BadgeData[0].Status = "error";
            ParamsControlCard.BadgeData[0].Description = "1: No connection";
            ParamsControlCard.BadgeData[1].Status = "error";
            ParamsControlCard.BadgeData[1].Description = "1: No connection";

            if (NmeaConverter != null)
            {
                NmeaConverter.NewNmeaDataReceived -= OnReceiveNmeaData;
                NmeaConverter.NoNmeaDataTimerReached -= NoNmeaDataReceived;
                NmeaConverter = null;
            }
            if (SbfConverter != null)
            {
                SbfConverter.NewSbfDataReceived -= CalcAndDispSbfData;
                SbfConverter.NoSbfDataTimerReached -= NoSbfDataReceived;
                SbfConverter = null;
            }

            TCPClient = null;

        }





        private string[] PlotNames { get; set; } = new string[3] { "Gr-LS", "Gr-KF", "Receiver" };
        private string[] PlotNamesCLK { get; set; } = new string[7] { "GPS", "GLO", "GAL", "QZS", "BDS", "IRN", "SBS" };
        private EpochPosData? EpochPosLS { get; set; }
        private EpochPosData? EpochPosKF { get; set; }
        private EpochPosData EpochPosReceiver { get; set; }
        private EpochPosData? EpochPosLS_forCard { get; set; }
        private EpochPosData? EpochPosKF_forCard { get; set; }
        private EpochPosData EpochPosReceiver_forCard { get; set; }
        private EpochPosDataStore EpochPosDataStore { get; set; } = new EpochPosDataStore(2);

        private int MeasCount { get; set; } = 0;

        private async void CalcAndDispSbfData(object? sender, NewSbfDataReceivedEventArgs e)      // SbfConverterから飛んでくる
        {
            if (e.NewSbfDatas == null)
            {
                return;
            }
            else
            {
                if(ParamsControlCard.BadgeData[0].Status != "processing")
                {
                    ParamsControlCard.BadgeData[0].Status = "processing";
                    ParamsControlCard.BadgeData[0].Description = "1: SBF received";
                }

            }
            await Task.Run(() =>
            {
                foreach (var d in e.NewSbfDatas)
                {
                    var nav = d.GetNav();
                    if (nav != null)
                    {
                        Nav.NavData.Add(nav);
                    }
                }
            });

            await Task.Run(() =>
            {
                foreach (var meas in e.NewSbfDatas)  // エポック観測データ毎に測位演算
                {
                    var Sats = meas.GetMeas();
                    if (Sats == null) break;

                    PP.Calc(Nav, Sats);
                    if (PP.PosResults.Count > 0)
                    {
                        EpochPosLS = new EpochPosData(PP.PosResults.Last());
                    }
                    else
                    {
                        EpochPosLS = null;
                    }
                    if (PP.PosResultsKF.Count > 0)
                    {
                        EpochPosKF = new EpochPosData(PP.PosResultsKF.Last());
                    }
                    else
                    {
                        EpochPosKF = null;
                    }
                }
            });
            await Task.Run( ()=> EpochPosDataStore.Add(EpochPosLS, EpochPosKF, null) );
                //EpochPosDataStore.Add(EpochPosLS, EpochPosKF, null);            
        }


        private async void OnReceiveNmeaData(object? sender, NewNmeaDataReceivedEventArgs e)  // NmeaConverterから飛んでくる
        {
            LatestNmeaBurst = e.NewBursts.Last();
            //if (PP.Pst == null || !PP.Pst.HaveEnuOrg) return;
            EpochPosReceiver = new EpochPosData(LatestNmeaBurst, PP.Pst);
            if(ParamsControlCard.BadgeData[1].Status != "processing")
            {
                ParamsControlCard.BadgeData[1].Status = "processing";
                ParamsControlCard.BadgeData[1].Description = "1: Nmea received";
            }

            //Debug.WriteLine(e.NewBursts.Count + " new nmea burst received: " + LatestNmeaBurst.EpochTime.ToString("T") + "H: " + LatestNmeaBurst.Alt);

            //await InvokeAsync(() =>
            //{
            //    // 状態変更を Blazor に通知
            //    StateHasChanged();
            //});

            await InvokeAsync(()=> EpochPosDataStore.Add(null, null, EpochPosReceiver) );
            //EpochPosDataStore.Add(null, null, EpochPosReceiver);

        }


        private void NoNmeaDataReceived(object? sender, EventArgs e)
        {
            ParamsControlCard.BadgeData[1].Status = "warning";
            ParamsControlCard.BadgeData[1].Description = "1: No Nmea received";
            InvokeAsync(() =>
            {
                StateHasChanged();
            });
        }


        /// <summary>
        /// 長時間SBFデータ受信が無いとき
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NoSbfDataReceived(object? sender, EventArgs e)
        {
            ParamsControlCard.BadgeData[0].Status = "warning";
            ParamsControlCard.BadgeData[0].Description = "1: No SBF received";
            InvokeAsync(() =>
            {
                StateHasChanged();
            });
        }

        private void ApplySettingChange(int num)        // ドロワー閉じたときに呼ばれる
        {
            StateHasChanged();
        }



        private List<EpochPosData?>? testData { get; set; } = null;


        private List<EpochPosData?>? Data1 { get; set; } = null;    // data for plot
        private List<EpochPosData?>? Data2 { get; set; } = null;
        private List<EpochPosData?>? Data3 { get; set; } = null;

        private async void OnExistNewEpochData(object? sender, NewEpochPosDataExistEventArgs e)
        {            
            await Task.Run( ()=>
            {
                testData = e.NewDatas;
                EpochPosLS_forCard = testData[0];
                EpochPosKF_forCard = testData[1];
                EpochPosReceiver_forCard = testData[2];

                Data1 = new List<EpochPosData>() { EpochPosLS_forCard };
                Data2 = new List<EpochPosData>() { EpochPosKF_forCard };
                Data3 = new List<EpochPosData>() { EpochPosReceiver_forCard };

            });
            await InvokeAsync(() => StateHasChanged());
        }


    }


}
