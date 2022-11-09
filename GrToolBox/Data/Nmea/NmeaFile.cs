using GrToolBox.Common;
using static GrToolBox.Data.Nmea.NmeaConverter;

namespace GrToolBox.Data.Nmea
{
    public static class NmeaFile
    {

        public static List<NmeaBurstData>? NmeaFileConverter(string path)
        {
            return NmeaFileConverter(path, null);
        }

        public static List<NmeaBurstData>? NmeaFileConverter(string path, IProgress<int>? iProgress)
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
            CircularBuffer<byte> Cb = new CircularBuffer<byte>(18);

            List<string> sentences = new List<string>();

            int len = byteData.Length;
            int pos = 0;

            while (pos < len)
            {
                int numWritable = Cb.NumWritable();
                if (numWritable > 0)
                {
                    int pos_to = ((pos + numWritable) <= len) ? pos + numWritable : len;
                    Cb.Add(byteData[pos..pos_to]);
                    pos = pos_to;
                }
                sentences.AddRange(GetNmeaSentence(Cb));
                if(iProgress!= null)
                {
                    iProgress.Report(pos);
                }
            }
            List<NmeaBurstData> bursts = new List<NmeaBurstData>();
            GetNmeaBurst(sentences, bursts);
            return bursts;

        }
    }
}

