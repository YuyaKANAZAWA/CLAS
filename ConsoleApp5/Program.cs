using ConsoleApp5;
using GrToolBox.Communication;
using GrToolBox.Data.GrNavData;
using GrToolBox.Data.Nmea;
using GrToolBox.Data.SBF;
using GrToolBox.Output;
using GrToolBox.Settings;
using System.Text;
using static GrToolBox.Data.Nmea.NmeaConverter;
using static GrToolBox.Output.EpochPosDataStore;
using static GrToolBox.Settings.SettingsDefinitions;

//string ipAddress = "192.168.3.1";
string ipAddress = "133.19.154.242";
int port = 52585;

// 測位用設定
GrSettings Stg = new GrSettings();
// 以下，最低限の設定（GPS, L1CAのみ，その他補正等なし）
Stg.Communication.TcpClient.IpAddress = ipAddress;
Stg.Communication.TcpClient.Port = port;
Stg.PositioningMode.Processing_Type = Processing_Type.RealTimeProcess;
Stg.PositioningMode.EstType = Estimation_Type.Kalman;
Stg.DataTypesFiles.Data_Type = Data_Type.SBF;
Stg.Observation.GPSCode = new string[] { "C1C", "---", "---" };
Stg.Communication.TcpClient.InitialSendingCommand = "esoc, IPS1, GPSNav";       // Septentrio IPS1 にGPSのエフェメリスを出力要求
//Stg.Communication.TcpClient.InitialSendingCommand = "esoc, USB1, GPSNav";     // Septentrio USB1 にGPSのエフェメリスを出力要求

TCPClient tCPClient = new TCPClient(Stg);

tCPClient.OnConnected += OnConnected;
//tCPClient.OnDisconnected += OnDisconnected;
//tCPClient.OnReceiveData += OnReceiveData;

// 測位用のクラス・インスタンス化
Nav Nav = new(Stg);
PointPositioning PP = new(Stg);
EpochPosData? EpochPosLS = null;
EpochPosData? EpochPosKF = null;
EpochPosData? EpochPosReceiver = null;
EpochPosDataStore EpochPosDataStore = new(2);
EpochPosDataStore.NewEpochPosDataExist += OnExistNewEpochData;

// データ読み出し用コンバータの追加
NmeaConverter NmeaConverter = new NmeaConverter();
NmeaConverter.NewNmeaDataReceived += OnReceiveNmeaData;
SbfConverter SbfConverter = new SbfConverter();
SbfConverter.NewSbfDataReceived += OnReceiveSbfData;
tCPClient.AddConverter(NmeaConverter);
tCPClient.AddConverter(SbfConverter);


try
{
    //接続処理
    // Connect to the remote endpoint.
    tCPClient.Connect();
}
catch (Exception ex)
{
    Console.WriteLine(ex.Message);
}
Console.ReadLine();
tCPClient.Close();




// TCP接続時イベント（TcpClientから飛んでくる）
void OnConnected(EventArgs e)
{
    //接続OK処理
    Console.WriteLine("tClient_OnConnected" + " ThreadID:" + Thread.CurrentThread.ManagedThreadId);
}


// TCPデータ受信イベント（TcpClientから飛んでくる）
//void OnReceiveData(object sender, string e)
//{
//    //Console.WriteLine("OnReceiveData" + " ThreadID:" + Thread.CurrentThread.ManagedThreadId);
//    //別スレッドからくるのでInvokeを使用
//    //Console.WriteLine("ReceiveData:" + e + " ThreadID:" + Thread.CurrentThread.ManagedThreadId);
//    Console.WriteLine(e);
//}

// Sbfデータ受信イベント（SbfConverterから飛んでくる）
void OnReceiveSbfData(object? sender, NewSbfDataReceivedEventArgs e)
{
    if (e.NewSbfDatas == null) return;

    foreach (var d in e.NewSbfDatas)
    {
        var nav = d.GetNav();
        if (nav != null)
        {
            Nav.NavData.Add(nav);   // navデータを格納
        }
    }
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
    EpochPosDataStore.Add(EpochPosLS, EpochPosKF, null);
}


// Nmeaデータ受信イベント（SbfConverterから飛んでくる）
void OnReceiveNmeaData(object? sender, NewNmeaDataReceivedEventArgs e)
{
    var LatestNmeaBurst = e.NewBursts.Last();
    EpochPosReceiver = new EpochPosData(LatestNmeaBurst, PP.Pst);
    EpochPosDataStore.Add(null, null, EpochPosReceiver);
}

// 測位結果が揃ったときのイベント（EpochPosDataStoreから飛んでくる）
void OnExistNewEpochData(object? sender, NewEpochPosDataExistEventArgs e)
{
    var sb = new StringBuilder();
    var Datas = e.NewDatas;
    var EpochPosLS = Datas[0];
    var EpochPosKF = Datas[1];
    var EpochPosReceiver = Datas[2];

    if (EpochPosLS != null)
    {
        sb.Append($"[pos-LS]   {EpochPosLS.Time.ToString("G")}: ");
        if (double.IsNaN(EpochPosLS.Lat))
        {
            sb.Append("No position data\n");
        }
        else
        {
            sb.Append($"Lat: {EpochPosLS.Lat: .0000000}, Lon:  {EpochPosLS.Lon: .0000000}, Ellh:  {EpochPosLS.EllH: .000}\n");
        }
    }
    else
    {
        sb.Append($"[pos-LS] Invalid\n");
    }
    if (EpochPosKF != null)
    {
        sb.Append($"[pos-KF]   {EpochPosKF.Time.ToString("G")}: ");
        if (double.IsNaN(EpochPosKF.Lat))
        {
            sb.Append("No position data\n");
        }
        else
        {
            sb.Append($"Lat: {EpochPosKF.Lat: .0000000}, Lon:  {EpochPosKF.Lon: .0000000}, Ellh:  {EpochPosKF.EllH: .000}\n");
        }
    }
    else
    {
        sb.Append($"[pos-KF]   Invalid\n");
    }
    if (EpochPosReceiver != null)
    {
        sb.Append($"[Receiver] {EpochPosReceiver.Time.ToString("G")}: ");
        if (double.IsNaN(EpochPosReceiver.Lat))
        {
            sb.Append("No position data\n");
        }
        else
        {
            sb.Append($"Lat: {EpochPosReceiver.Lat: .0000000}, Lon:  {EpochPosReceiver.Lon: .0000000}, Ellh:  {EpochPosReceiver.EllH: .000}\n");
        }
    }
    else
    {
        sb.Append($"[EpochPosReceiver] Invalid\n");
    }
    Console.WriteLine(sb.ToString());
    sb.Clear();
}
