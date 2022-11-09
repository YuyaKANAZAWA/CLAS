using GrToolBox.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static GrToolBox.Data.CLAS3.ClasConverter;

namespace GrToolBox.Data.CLAS3
{
    public class ClasFile
    {
        //public static List<IRawData>? Rtcm3FileConverter(string path)
        
        public static List<ClasMessage> ClasFileConverter(string path)
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
            //ClasFileConverter(buf);
            return ClasFileConverter(buf);
        }


        //public static List<IRawData>? Rtcm3FileConverter(byte[] byteData)
        public static List<ClasMessage> ClasFileConverter(byte[] byteData)
        {
            int len = byteData.Length;
            int pos = 0;
            CircularBuffer<byte> Cb = new CircularBuffer<byte>(18);

            List<ClasMessage> clasMessages = new List<ClasMessage>();

            while (pos < len)
            {
                int numWritable = Cb.NumWritable();
                if (numWritable > 0)
                {
                    int pos_to = ((pos + numWritable) <= len) ? pos + numWritable : len;
                    Cb.Add(byteData[pos..pos_to]);
                    pos = pos_to;
                }
                //GetClasMessages(Cb);
                clasMessages.AddRange(GetClasMessages(Cb));
            }
            //return GetRtcm3Data(byteSentences);
            return clasMessages;
        }

        




    }
}
