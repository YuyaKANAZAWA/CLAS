using GrToolBox.Common;
using static GrToolBox.Data.RTCM.RTCM3.Rtcm3Converter;


namespace GrToolBox.Data.RTCM.RTCM3
{
    public static class Rtcm3File
    {
        public static List<IRawData>? Rtcm3FileConverter(string path)
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
            return Rtcm3FileConverter(buf);
        }


        public static List<IRawData>? Rtcm3FileConverter(byte[] byteData)
        {
            int len = byteData.Length;
            int pos = 0;
            CircularBuffer<byte> Cb = new CircularBuffer<byte>(18);

            List<byte[]> byteSentences = new List<byte[]>();

            while (pos < len)
            {
                int numWritable = Cb.NumWritable();
                if (numWritable > 0)
                {
                    int pos_to = ((pos + numWritable) <= len) ? pos + numWritable : len;
                    Cb.Add(byteData[pos..pos_to]);
                    pos = pos_to;
                }
                byteSentences.AddRange(GetRtcm3ByteSentence(Cb));
            }
            return GetRtcm3Data(byteSentences);
        }
    }
}
