using GrToolBox.Data.CLAS3;
using System.Diagnostics;


string logfile = @"./debug_test.txt";
if (File.Exists(logfile))
{
    File.Delete(logfile);
}
DefaultTraceListener drl;
drl = (DefaultTraceListener)Trace.Listeners["Default"];
drl.LogFileName = @"./debug_test.txt";


string path = @"C:\Users\yuya_\OneDrive - 学校法人立命館\卒業研究\Csharp\CLAS\2022133W.l6";
//string path = @"C:\Users\ykubo\Dropbox\clas\2018261A.l6";
List<ClasMessage> clasMessages =  ClasFile.ClasFileConverter(path);

var ssr = new ClasSSR();

foreach(var cm in clasMessages)
{
    ssr.AddClasMessage(cm);
    
}

int aaa = 0;


