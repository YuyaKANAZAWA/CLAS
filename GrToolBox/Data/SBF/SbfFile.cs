using GrToolBox.Common;
using static GrToolBox.Data.SBF.SbfConverter;


namespace GrToolBox.Data.SBF
{
    public static class Rtcm3File
    {
        public static List<IRawData>? SbfFileConverter(string path)
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
            return SbfFileConverter(buf);
        }


        public static List<IRawData>? SbfFileConverter(byte[] byteData)
        {
            int len = byteData.Length;
            int pos = 0;
            CircularBuffer<byte> Cb = new CircularBuffer<byte>(len);

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
                byteSentences.AddRange(GetSbfByteSentence(Cb));
            }

            return GetSbfData(byteSentences);
        }
    }
}
