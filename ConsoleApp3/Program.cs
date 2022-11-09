using ConsoleApp3;
using GrToolBox.Communication;
using GrToolBox.Data.GrNavData;
using GrToolBox.Data.Nmea;
using GrToolBox.Data.SBF;
using GrToolBox.Output;
using GrToolBox.Settings;
using System.IO.Ports;
using System.Text;
using static GrToolBox.Data.Nmea.NmeaConverter;
using static GrToolBox.Output.EpochPosDataStore;
using static GrToolBox.Settings.SettingsDefinitions;

/// <summary>
/// Serialポート，SBF，測位演算サンプルプログラム
/// </summary>


// シリアルポート名を画面表示，選択する
string[] ports = SerialPort.GetPortNames();                         // シリアルポート名（"COM1"，"/dev/ttyS0"等）を取得
Console.WriteLine("The following serial ports has been found:");
int portNum = 0;
foreach (string p in ports)
{
    Console.WriteLine($"{portNum}: {p}");
    portNum++;
}

Console.Write("Enter port number: ");                               // シリアルポートを選択
string? portNumStr = Console.ReadLine();
portNum = int.Parse(portNumStr);
// ここまで

// 測位用設定
GrSettings Stg = new GrSettings();      // 設定用クラス
// 以下，最低限の設定（GPS, L1CAのみ，その他補正等一切無し）
Stg.Communication.SerialPort.PortName = ports[portNum];
Stg.PositioningMode.Processing_Type = Processing_Type.RealTimeProcess;
Stg.PositioningMode.EstType = Estimation_Type.Kalman;
Stg.DataTypesFiles.Data_Type = Data_Type.SBF;
Stg.Observation.GPSCode = new string[] { "C1C", "---", "---" };
Stg.Communication.SerialPort.InitialSendingCommand = "esoc, USB1, GPSNav";


EpochPosData? EpochPosLS = null;
EpochPosData? EpochPosKF = null;
EpochPosData? EpochPosReceiver = null;
EpochPosDataStore EpochPosDataStore = new(2);
EpochPosDataStore.NewEpochPosDataExist += OnExistNewEpochData;


// 測位用のクラス・インスタンス化
Nav Nav = new(Stg);
PointPositioning PP = new(Stg);

// シリアルポート使用準備
SerialDevice SD = new SerialDevice(Stg);

SbfConverter SbfConverter = new SbfConverter();
SbfConverter.NewSbfDataReceived += OnReceiveSbfData;
NmeaConverter NmeaConverter = new NmeaConverter();
NmeaConverter.NewNmeaDataReceived += OnReceiveNmeaData;


SD.AddConverter(SbfConverter);
SD.AddConverter(NmeaConverter);

SD.Open();
Console.ReadLine();
SD.Close();



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

void OnReceiveNmeaData(object? sender, NewNmeaDataReceivedEventArgs e)
{
    var LatestNmeaBurst = e.NewBursts.Last();
    EpochPosReceiver = new EpochPosData(LatestNmeaBurst, PP.Pst);
    EpochPosDataStore.Add(null, null, EpochPosReceiver);
}


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













