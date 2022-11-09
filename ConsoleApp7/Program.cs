using GrToolBox.Data;
using GrToolBox.Data.RTCM.RTCM3;
using System.Text;

string path = @"D:\Dropbox\GRitz_SampleData\RTCM3_Test1\RLOGIN.bin";
//string path = @"D:\Dropbox\GRitz_SampleData\RTCM3_Test1\putty.log";
//string path = @"C:\Users\ykubo\Dropbox\GRitz_SampleData\RTCM3_Test1\RLOGIN.bin";




List<IRawData>? rtcmDatas = Rtcm3File.Rtcm3FileConverter(path);    // エポック毎の読み取りデータを取得

StringBuilder sb = new StringBuilder();
foreach(var data in rtcmDatas)
{
    data.PrintTo(sb);
}

Console.WriteLine(sb.ToString());

string papth_write = Path.Combine(Path.GetDirectoryName(path), "Result.txt");
File.WriteAllText(papth_write, sb.ToString());
