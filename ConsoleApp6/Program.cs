using GrToolBox.Settings;
using static GrToolBox.Data.SBF.Rtcm3File;
using static GrToolBox.Settings.SettingsDefinitions;
using GrToolBox.Data.GrNavData;
using GrToolBox.Output;
using ConsoleApp6;
using static GrToolBox.Data.Nmea.NmeaFile;

using GrToolBox.Data.CLAS3;
using static GrToolBox.Data.CLAS3.ClasUtilities;

//string path_sbf = @"D:\Dropbox\FujiYama_AirData\21.12.23_NH985_HND-ITM\sept3570.21_";
//string path_nmea = @"D:\Dropbox\FujiYama_AirData\21.12.23_NH985_HND-ITM\sept3570.211";
//string path_sbf = @"C:\Users\yuya_\OneDrive - 学校法人立命館\卒業研究\Csharp\log_0930_02\log_0930_02_0000.sbf";       // Fullpath
//string path_nmea = @"C:\Users\yuya_\OneDrive - 学校法人立命館\卒業研究\Csharp\log_0930_02\log_0930_02_0000.nmea";

string path_sbf = @"C:\Users\yuya_\OneDrive - 学校法人立命館\卒業研究\20221013_琵琶湖南湖周遊\log_1013_next\log_1013_next_0000.sbf";       // Fullpath
string path_nmea = @"C:\Users\yuya_\OneDrive - 学校法人立命館\卒業研究\20221013_琵琶湖南湖周遊\log_1013_next\log_1013_next_0000.nmea";





var SbfDatas = SbfFileConverter(path_sbf);
var NmeaBurstDatas = NmeaFileConverter(path_nmea);

Console.WriteLine(SbfDatas.Count + " SBF sentences read");
Console.WriteLine(NmeaBurstDatas.Count + " Nmea burst read");


// 測位用設定
GrSettings Stg = new GrSettings();
// 以下，最低限の設定（GPS, L1CAのみ，その他補正等なし）
Stg.PositioningMode.Processing_Type = Processing_Type.PostProcess;
Stg.PositioningMode.EstType = Estimation_Type.Kalman;
Stg.DataTypesFiles.Data_Type = Data_Type.SBF;
Stg.Observation.GPSCode = new string[] { "C1C", "---", "---" };

// 測位用のクラス・インスタンス化
Nav Nav = new(Stg);
PointPositioning PP = new(Stg);
EpochPosData? EpochPosLS = null;
EpochPosData? EpochPosKF = null;
EpochPosData? EpochPosReceiver = null;
EpochPosDataStore EpochPosDataStore = new();

// CLAS用
List<ClasMessage> clasMessages = new List<ClasMessage>();

if (SbfDatas == null)
{
    Console.WriteLine("No SBF data");
    return;
}

foreach (var d in SbfDatas)
{
    if (d.GetID() == 4069)
    {
        var clas = d.GetNav();
        if (clas != null)
        {
            clasMessages.Add((ClasMessage)clas);
        }
    }
}

ClasMessage2L6file(clasMessages);

//var ssr = new ClasSSR();
//foreach (var cm in clasMessages)
//{
//    ssr.AddClasMessage(cm);
//}


foreach (var d in SbfDatas)
{
    var nav = d.GetNav();
    if (nav != null)
    {
        Nav.NavData.Add(nav);   // navデータを格納
    }
}

int epoch_count = 0;
foreach (var meas in SbfDatas)  // エポック観測データ毎に測位演算
{
    var Sats = meas.GetMeas();
    if (Sats == null) continue;

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
    EpochPosDataStore.Add_Post(EpochPosLS, EpochPosKF, null);
    epoch_count++;
}

foreach(var burst in NmeaBurstDatas)
{
    var EpochPosNmea = new EpochPosData(burst, PP.Pst);
    EpochPosDataStore.Add_Post(null, null, EpochPosNmea);
}

string path_csv_LS = Path.Combine(Path.GetDirectoryName(path_sbf), "ResultLS.csv");

EpochPosDataStore.CsvOut(3, path_csv_LS);   // EpochPosDataStoreの1番目にLSの結果が入っている，引数を2にすればKF結果，3にすればNMEAがCSV出力される

Console.WriteLine($"{epoch_count} エポック分の処理結果を {path_csv_LS} に書き出しました．");

