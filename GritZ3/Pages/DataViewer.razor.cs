using AntDesign;
using GritZ3.Classes;
using GrToolBox.Communication;
using GrToolBox.Coordinates;
using GrToolBox.Data.Nmea;
using GrToolBox.Output;
using GrToolBox.Settings;
using System.Diagnostics;
using static GrToolBox.Settings.SettingsDefinitions;
using static GrToolBox.Data.Nmea.NmeaConverter;
using GritZ3.Components.Cards.GeneralDispCard;
using GritZ3.Components.Cards;
using System.Text;

namespace GritZ3.Pages
{
    public partial class DataViewer
    {
        public DrawerInfo DataSource1 { get; set; } = new DrawerInfo() { ID = 1, Name = "DataSource1", Visible = false };
        public DrawerInfo DataSource2 { get; set; } = new DrawerInfo() { ID = 2, Name = "DataSource2", Visible = false };
        public DrawerInfo DataSource3 { get; set; } = new DrawerInfo() { ID = 3, Name = "DataSource1", Visible = false };

        private string ActiveKey { get; set; } = "1";
        private SerialDevice? SD { get; set; }
        private TCPClient? TCPClient { get; set; }
        private NmeaConverter? NmeaConverter { get; set; } = new NmeaConverter();

        private ComStatus comStatus { get; set; }

        private enum ComStatus
        {
            NotConnected,
            Connected,
            Disconnected,
            ConnectionFailed
        }

        private NmeaBurstData? LatestNmeaBurst { get; set; }
        //private DataForPlot DataForPlot { get; set; } = new DataForPlot();
        private PositionSetter? PS { get; set; } = null;

        private GrSettings Stg1 { get; set; } = new GrSettings();
        private GrSettings Stg2 { get; set; } = new GrSettings();
        private GrSettings Stg3 { get; set; } = new GrSettings();

        private ParamsPosRTControlCard ParamsControlCard { get; set; } = new(1);

        private EpochPosData? EpochPosReceiver1 { get; set; }
        private List<EpochPosData>? Data1 { get; set; }

        private string[] PlotNames { get; set; } = new string[3] { "Data source 1", "Data source 2", "Data source 3" };

        private bool E_NRendered { get; set; } = false;

        private int EpochCount { get; set; } = 0;
        private DispData Data1ForCard { get; set; } = new();
        private bool RecordingFlag { get; set; } = false;
        private int RecordingStartCount { get; set; } = 0;

        public DataViewer()
        {
            //DispData.TagInfo taginfo = new DispData.TagInfo() { Title = "NMEA", Color = "blue-inverse" };
            //Data1ForCard.TagInfos.Add(taginfo);
            Data1ForCard.Title = "Not specified";
            Data1ForCard.Contents.Add(new DispData.Content() { Title = "", Value = "" });
            Data1ForCard.Contents.Add(new DispData.Content() { Title = "", Value = "" });
            //Data1ForCard.Contents.Add(new DispData.Content() { Title = "IP address: ", Value = "N/A" });
            //Data1ForCard.Contents.Add(new DispData.Content() { Title = "Port: ", Value = "N/A" });
        }




        /// <summary>
        /// 長時間NMEAデータ受信が無いとき
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NoNmeaData(object? sender, EventArgs e)
        {
            LatestNmeaBurst = null;
            InvokeAsync(() =>
            {
                // 状態変更を Blazor に通知
                StateHasChanged();
            });

        }

        private void Open_DrawerDataSource1()
        {
            DataSource1.Visible = true;
        }
        private void Open_DrawerDataSource2()
        {
            DataSource2.Visible = true;
        }
        private void Open_DrawerDataSource3()
        {
            DataSource3.Visible = true;
        }

        private void ApplySettingChange(int num)        // ドロワー閉じたときに呼ばれる
        {
            if(Stg1.Communication.Connection == Connection_Type.Serial)
            {
                Data1ForCard.Contents.Clear();
                Data1ForCard.Title = "Serial Port";
                Data1ForCard.Contents.Add(new DispData.Content() { Title = "Port name", Value = Stg1.Communication.SerialPort.PortName });
                Data1ForCard.Contents.Add(new DispData.Content() { Title = "Baud rate", Value = Stg1.Communication.SerialPort.BaudRate.ToString() });
            }
            else if(Stg1.Communication.Connection == Connection_Type.TCP)
            {
                Data1ForCard.Contents.Clear();
                Data1ForCard.Title = "TCP/IP";
                Data1ForCard.Contents.Add(new DispData.Content() { Title = "IP address", Value = Stg1.Communication.TcpClient.IpAddress });
                Data1ForCard.Contents.Add(new DispData.Content() { Title = "Port", Value = Stg1.Communication.TcpClient.Port.ToString() });
            }
            StateHasChanged();
        }

        private void OnClickStartStopButton(string str)
        {
            var type = Stg1.Communication.Connection;
            if(type == Connection_Type.Serial)
            {
                if (str == "Start") 
                    StartView_Serial();
                else if (str == "Stop") 
                    StopView_Serial();
            }
            else if(type == Connection_Type.TCP)
            {
                if (str == "Start")
                    StartView_Tcp();
                else if (str == "Stop")
                    StopView_Tcp();

            }
        }
        

        private async void OnClickRecordingSwitch(string str)
        {
            if(str == "StartRec")
            {
                await File.WriteAllTextAsync("./wwwroot/record.txt", "");
                await InvokeAsync(() =>
                {
                    RecordingFlag = true;
                    if (Data1 != null)
                    {
                        RecordingStartCount = Data1.Count;
                    }
                });
            }
            else if(str == "StopRec")
            {
                await InvokeAsync(() => RecordingFlag = false);
            }
        }

        private async void OnClickFileButton(string str)
        {
            await InvokeAsync(() => 
            { 
                ParamsControlCard.DLFileInfos.Converting = true; 
                ParamsControlCard.DLFileInfos.HaveData = false;
                StateHasChanged();
            });

            string dirPath = "./wwwroot/";
            string nmeaFileName = "record.txt";
            string outFileName = string.Empty;
            string saveAsName = string.Empty;
            string description = string.Empty;
            int nEpochs = 0;
            List<EpochPosData>? epochPos = null;

            var burstDatas = await Task.Run(() => NmeaFile2.NmeaFileConverter(dirPath + nmeaFileName));
            if (burstDatas != null)
            {
                epochPos = await Task.Run(() => OutputUtilities.NmeaBurst2EpochPos(burstDatas));
                nEpochs = epochPos.Count;
            }

            if (str == "KML")
            {
                outFileName = "kml.txt";
                saveAsName = "record.kml";
                if(epochPos != null)
                {
                    var kml = new KmlOut();
                    kml.FilePath = dirPath + outFileName;
                    await kml.WriteAsync(epochPos);
                }
            }
            else if(str == "GPX")
            {

            }
            else if(str == "CSV")
            {
                outFileName = "csv.txt";
                saveAsName = "record.csv";
                if (epochPos != null)
                {
                    await OutputUtilities.OutputCsvAsync(epochPos, dirPath + outFileName);
                }
            }
            if (nEpochs != 0)
            {
                long size = new FileInfo(dirPath + outFileName).Length;
                double sizeKB = size / 1024.0;
                await InvokeAsync(async () =>
                {
                    ParamsControlCard.DLFileInfos.Converting = false;
                    ParamsControlCard.DLFileInfos.HaveData = true;
                    ParamsControlCard.DLFileInfos.FileName = outFileName;
                    ParamsControlCard.DLFileInfos.SaveAsName = saveAsName;
                    ParamsControlCard.DLFileInfos.Description = $"CSV ({sizeKB:0.0} KB)";
                    StateHasChanged();
                    await InvokeAsync(() => NoticeWithIcon(NotificationType.Success, $"Success!!", $"Download file has been prepared"));
                });
            }
            else
            {
                ParamsControlCard.DLFileInfos.Converting = false;
                ParamsControlCard.DLFileInfos.HaveData = false;
                StateHasChanged();
                await InvokeAsync(() => NoticeWithIcon(NotificationType.Error, $"Failed to output csv", $"There were no data to be output."));
            }

        }


        private void StartView_Serial()
        {
            EpochCount = 0;
            if (string.IsNullOrEmpty(Stg1.Communication.SerialPort.PortName))
            {
                //if (ParamsControlCard.AlertDatas.Count > 2)
                //{
                //    ParamsControlCard.AlertDatas = new Stack<AlertData>(ParamsControlCard.AlertDatas.Reverse().Skip(1));
                //}
                //ParamsControlCard.AlertDatas.Push(new AlertData { TimeData = DateTime.Now, Type = AlertType.Error, Text = "Serial port name is not specified." });
                InvokeAsync(() => NoticeWithIcon(NotificationType.Error, "Serial port is not specified", $"{DateTime.Now}"));
                return;
            }
            SD = new(Stg1);
            NmeaConverter = new NmeaConverter();
            NmeaConverter.NewNmeaDataReceived += OnReceiveNmeaData;
            NmeaConverter.NoNmeaDataTimerReached += NoNmeaDataReceived1;
            SD.AddConverter(NmeaConverter);

            //if (ParamsControlCard.AlertDatas.Count > 2)
            //{
            //    ParamsControlCard.AlertDatas = new Stack<AlertData>(ParamsControlCard.AlertDatas.Reverse().Skip(1));
            //}
            if (SD.Open())
            {
                //ParamsControlCard.StartButtonDisabled = true;
                //ParamsControlCard.StopButtonDisabled = false;
                //ParamsControlCard.AlertDatas.Push(new AlertData { TimeData = DateTime.Now, Type = AlertType.Success, Text = Stg1.Communication.SerialPort.PortName + " is opened" });
                InvokeAsync(() => NoticeWithIcon(NotificationType.Success, $"Connected to {Stg1.Communication.SerialPort.PortName}", $"{DateTime.Now}"));
            }
            else
            {
                //ParamsControlCard.AlertDatas.Push(new AlertData { TimeData = DateTime.Now, Type = AlertType.Error, Text = Stg1.Communication.SerialPort.PortName + " can not be opened" });
                InvokeAsync(() => NoticeWithIcon(NotificationType.Error, $"{Stg1.Communication.SerialPort.PortName} can not be opened", $"{DateTime.Now}"));
            }
        }


        private void StopView_Serial()
        {
            if (SD != null && SD.Close())
            {
                ParamsControlCard.StartButtonDisabled = false;
                ParamsControlCard.StopButtonDisabled = true;
                //if (ParamsControlCard.AlertDatas.Count > 2)
                //{
                //    ParamsControlCard.AlertDatas = new Stack<AlertData>(ParamsControlCard.AlertDatas.Reverse().Skip(1));
                //}
                ParamsControlCard.AlertDatas.Push(new AlertData { TimeData = DateTime.Now, Type = AlertType.Warning, Text = Stg1.Communication.SerialPort.PortName + " is closed" });
                InvokeAsync(() => NoticeWithIcon(NotificationType.Warning, $"{Stg1.Communication.SerialPort.PortName} is closed", $"{DateTime.Now}"));
                ParamsControlCard.BadgeData[0].Status = "error";
                ParamsControlCard.BadgeData[0].Description = "1: No connection";
                SD = null;
            }
            if (NmeaConverter != null)
            {
                NmeaConverter.NewNmeaDataReceived -= OnReceiveNmeaData;
                NmeaConverter.NoNmeaDataTimerReached -= NoNmeaDataReceived1;
                NmeaConverter = null;
            }
        }


        private void StartView_Tcp()
        {
            TCPClient = new TCPClient(Stg1);
            TCPClient.OnConnected += OnConnected;
            TCPClient.OnDisconnected += OnDisconnected;
            NmeaConverter NmeaConverter = new NmeaConverter();
            TCPClient.AddConverter(NmeaConverter);
            NmeaConverter.NewNmeaDataReceived += OnReceiveNmeaData;

            //if (ParamsControlCard.AlertDatas.Count > 2)
            //{
            //    ParamsControlCard.AlertDatas = new Stack<AlertData>(ParamsControlCard.AlertDatas.Reverse().Skip(1));
            //}

            try
            {
                //接続処理
                TCPClient.Connect();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                ParamsControlCard.AlertDatas.Push(new AlertData { TimeData = DateTime.Now, Type = AlertType.Error, Text = "TCP/IP not connected" });
                InvokeAsync(() => NoticeWithIcon(NotificationType.Error, $"Could not establish TCP/IP connection", $"{DateTime.Now}"));
            }
        }

        private void StopView_Tcp()
        {
            if (TCPClient != null)
            {
                TCPClient.Close();
            }
        }


        /** 接続OKイベント **/
        private void OnConnected(EventArgs e)
        {
            //if (ParamsControlCard.AlertDatas.Count > 2)
            //{
            //    ParamsControlCard.AlertDatas = new Stack<AlertData>(ParamsControlCard.AlertDatas.Reverse().Skip(1));
            //}
            //接続OK処理
            Debug.WriteLine("tClient_OnConnected" + " ThreadID:" + Thread.CurrentThread.ManagedThreadId);
            ParamsControlCard.StartButtonDisabled = true;
            ParamsControlCard.StopButtonDisabled = false;
            //ParamsControlCard.AlertDatas.Push(new AlertData { TimeData = DateTime.Now, Type = AlertType.Success, Text = $"Connected ({Stg1.Communication.TcpClient.IpAddress}:{Stg1.Communication.TcpClient.Port})" });
            InvokeAsync(()=> NoticeWithIcon(NotificationType.Success, "Connected", $"Connected to Data Source1: {Stg1.Communication.TcpClient.IpAddress}:{Stg1.Communication.TcpClient.Port}"))  ;
        }

        private void OnDisconnected(object? sender, EventArgs e)
        {
            ParamsControlCard.StartButtonDisabled = false;
            ParamsControlCard.StopButtonDisabled = true;
            //if (ParamsControlCard.AlertDatas.Count > 2)
            //{
            //    ParamsControlCard.AlertDatas = new Stack<AlertData>(ParamsControlCard.AlertDatas.Reverse().Skip(1));
            //}
            //ParamsControlCard.AlertDatas.Push(new AlertData { TimeData = DateTime.Now, Type = AlertType.Warning, Text = $"Disconnected ({Stg1.Communication.TcpClient.IpAddress}:{Stg1.Communication.TcpClient.Port})" });
            InvokeAsync(() => NoticeWithIcon(NotificationType.Warning, $"Disconnected from {Stg1.Communication.TcpClient.IpAddress}:{Stg1.Communication.TcpClient.Port}", $"{DateTime.Now}"));
            ParamsControlCard.BadgeData[0].Status = "error";
            ParamsControlCard.BadgeData[0].Description = "1: No connection";
            TCPClient = null;

            if (NmeaConverter != null)
            {
                NmeaConverter.NewNmeaDataReceived -= OnReceiveNmeaData;
                NmeaConverter.NoNmeaDataTimerReached -= NoNmeaDataReceived1;
                NmeaConverter = null;
            }
        }


        // NMEAデータ受信処理
        private async void OnReceiveNmeaData(object? sender, NewNmeaDataReceivedEventArgs e)
        {
            if(ParamsControlCard.BadgeData[0].Status != "processing")
            {
                ParamsControlCard.BadgeData[0].Status = "processing";
                ParamsControlCard.BadgeData[0].Description = "1: Nmea received";
            }

            LatestNmeaBurst = e.NewBursts.Last();

            if (PS == null)
            {
                PS = new PositionSetter();
                PS.SetOrgLLH(new double[] { LatestNmeaBurst.Lat, LatestNmeaBurst.Lon, LatestNmeaBurst.Alt }, "deg");
            }
            EpochPosReceiver1 = new EpochPosData(LatestNmeaBurst, PS);
            Data1 = new List<EpochPosData>() { new EpochPosData(LatestNmeaBurst, PS) };
            await InvokeAsync(() =>
            {
                // 状態変更を Blazor に通知
                StateHasChanged();
            });

            if(LatestNmeaBurst != null && RecordingFlag)
            {
                var sb = new StringBuilder();
                if(LatestNmeaBurst.Rmc != null)
                {
                    sb.Append(LatestNmeaBurst.Rmc.RawMsg);
                    sb.Append("\r\n");
                }
                if(LatestNmeaBurst.Gga != null)
                {
                    sb.Append(LatestNmeaBurst.Gga.RawMsg);
                    sb.Append("\r\n");
                }
                await File.AppendAllTextAsync("./wwwroot/record.txt", sb.ToString());
                long size = new FileInfo("./wwwroot/record.txt").Length;
                double sizeKB = size / 1024.0;
                ParamsControlCard.RecordingFileInfo = $"record.nmea({sizeKB:0.0}[KB])";
            }

        }

        private void NoNmeaDataReceived1(object? sender, EventArgs e)
        {
            ParamsControlCard.BadgeData[0].Status = "warning";
            ParamsControlCard.BadgeData[0].Description = "1: No Nmea received";
            InvokeAsync(() =>
            {
                StateHasChanged();
            });

        }

        private async Task NoticeWithIcon(NotificationType type, string mes, string des)
        {
            await _notice.Open(new NotificationConfig()
            {
                Message = mes,
                Description = des,
                NotificationType = type
            });
        }

        //private async Task SuccessNotice()
        //{
        //    await NoticeWithIcon(NotificationType.Success);
        //}

        //private async Task IssueingNotice()
        //{
        //    await NoticeWithIcon(NotificationType.Success);
        //}

    }

}
