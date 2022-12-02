using GrToolBox.Data.CLAS3;
using static GrToolBox.Output.OSRData;
using System.Diagnostics;
using System.Text;

string logfile = @"./debug_test.txt";
if (File.Exists(logfile))
{
    File.Delete(logfile);
}
string csvfile = @"./debug_osr.csv";
if (File.Exists(csvfile))
{
    File.Delete(csvfile);
}
DefaultTraceListener drl;
drl = (DefaultTraceListener)Trace.Listeners["Default"];
drl.LogFileName = @"./debug_test.txt";

string path = @"C:\Users\yuya_\OneDrive - 学校法人立命館\卒業研究\Csharp\CLAS\2022133W.l6";
//string path = @"C:\Users\ykubo\Dropbox\clas\2018261A.l6";
List<ClasMessage> clasMessages = ClasFile.ClasFileConverter(path);

ClasSSR? ssr = new ClasSSR();
var sb = new StringBuilder();

sb.AppendLine("msg,tow,sys,prn,pbias1,pbias2,pbias5,cbias1,cbias2,cbias5,trop,iono,antr1,antr2,antr5,relatv,wup1,wup2,wup5,compI1,compI2,compI5,compN,CPC1,CPC2,CPC5,PRC1,PRC2,PRC5,orb,clk,lat,lon,alt");
foreach (var cm in clasMessages)
{
    ssr.AddClasMessage(cm);
    WriteLineOSR(ssr, sb);
}
File.WriteAllText(csvfile, sb.ToString());

int aaa = 0;

