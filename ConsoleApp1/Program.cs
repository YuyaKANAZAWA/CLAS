using GrToolBox.Coordinates;
using GrToolBox.Data.Nmea;
using GrToolBox.Output;

string path = @"D:\Dropbox\FujiYama_AirData\21.12.23_NH985_HND-ITM\sept3570.211";
List<NmeaBurstData>? nmeaBurstDatas = NmeaFile.NmeaFileConverter(path);    // エポック毎の読み取りデータを取得


// 以下は整形と出力

EpochPosDataStore EpochPosDataStore = new EpochPosDataStore();

// ENU原点を第1エポック位置に設定
PositionSetter ps = new PositionSetter();
ps.SetOrgLLH(new double[] { nmeaBurstDatas[0].Lat, nmeaBurstDatas[0].Lon, nmeaBurstDatas[0].EllHeight }, "deg");

int epoch_count = 0;
foreach(var nmea in nmeaBurstDatas)     // エポック毎データを格納
{
    var epoch = new EpochPosData(nmea, ps);
    EpochPosDataStore.Add_Post(epoch, null, null);
    epoch_count++;
}

string path_csv = Path.Combine(Path.GetDirectoryName(path), "Nmea.csv");    // csvに出力
EpochPosDataStore.CsvOut(1, path_csv);

Console.WriteLine($"{epoch_count} エポック分の処理結果を {path_csv} に書き出しました．");
