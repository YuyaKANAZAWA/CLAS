using GrToolBox.Common;
using System.Diagnostics;
using static GrToolBox.Data.Nmea.NmeaConverter;
using static GrToolBox.Data.Nmea.NmeaUtilities;

namespace GrToolBox.Data.Nmea
{
    public static class NmeaFile2
    {

        public static List<NmeaBurstData> NmeaFileConverter(string path)
        {
            return NmeaFileConverter(path, null);
        }

        public static List<NmeaBurstData> NmeaFileConverter(string path, IProgress<int>? iProgress)
        {
            byte[] buf;
            if (File.Exists(path))
            {
                buf = File.ReadAllBytes(path);
            }
            else
            {
                throw new Exception($"Not found {path}");
            }
            return NmeaFileConverter(buf, iProgress);
        }

        public static List<NmeaBurstData> NmeaFileConverter(byte[] byteData, IProgress<int>? iProgress)
        {
            List<string> sentences = new List<string>();
            int len = byteData.Length;
            int pos = 0;
            int mark = pos;
            int count = 0;
            int countup_lim = len / 10;

            while ((pos + 10) < len)
            {
                mark = pos;
                //if (tmp[0] == 0x24 && tmp[1] == 0x47)        // $Gを探す
                if (byteData[pos] == '$' && byteData[pos + 1] == 'G')        // $Gを探す
                {
                    int searchTo = ((pos + 255) < len) ? (pos + 255) : len;
                    for(int i = pos + 6; i < searchTo - 4; i++)
                    {
                        if (byteData[i] == 0x2a && byteData[i + 3] == 0x0d && byteData[i + 4] == 0x0a)  // 2文字ずつで終端を探す 0x2a:"*", 0x0d:"\r(CR)", 0x0a:"\n(LF)"
                        {
                            // pos ('$') から i+2 ("*XX" -- チェックサム)までを取り出して，チェックサムを確認する
                            if (NmeaCScheck(byteData[pos..(i + 3)]))
                            {
                                var nmea = System.Text.Encoding.ASCII.GetString(byteData[pos..(i + 3)]);
                                sentences.Add(nmea);
                                pos = i + 4;    // whileでpos++されるのでここは「4」
                                break;
                            }
                            else
                            {
                                Debug.WriteLine("Nmea CS check invalid");
                            }
                        }
                    }
                }
                pos++;
                if(iProgress!= null && count++ > countup_lim)
                {
                    count = 0;
                    iProgress.Report(pos);
                }
            }
            if(iProgress != null)   iProgress.Report(pos);
            List<NmeaBurstData> bursts = new List<NmeaBurstData>();
            GetNmeaBurst(sentences, bursts);
            if (iProgress != null)  iProgress.Report(len);

            return bursts;

        }
    }
}

