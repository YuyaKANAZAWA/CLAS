using GrToolBox.Common;
using static GrToolBox.Data.DataUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using static GrToolBox.Data.CLAS.ClasDefinitions;

namespace GrToolBox.Data.CLAS
{
    public class ClasConverter
    {

        public static List<ClasMessage> GetClasMessages(CircularBuffer<byte> cb)
        {
            List<ClasMessage> messages = new List<ClasMessage>();
            while (cb.Count() >= 250)        // 250bytes; 2000bit for one clas message
            {
                cb.SetMark();
                byte[] tmpBytes;
                tmpBytes = cb.Read(4);      // 4bytes; Preamble
                if (BitToUint(tmpBytes, 0, 32) == 0x_1acffc1d)
                {
                    cb.BackToMark();
                    tmpBytes = cb.Read(250);
                    ClasMessage message = new ClasMessage(tmpBytes);
                    messages.Add(message);
                }
                else
                {
                    tmpBytes = cb.Read(1);
                }
            }
            return messages;
        }



        //public static void RenewClasSsr(SubTypes stp, List<ClasSSRData> ssr, ClasMessage cm)
        public static void RenewClasSsr(ClasSSR ssr, ClasMessage cm)
        {
            //マルチ中かどうかの判断


            while(cm.Pos < 1744)        // 1744: RSCの手前まで
            {
                var head = new ClasSubTypeHeader_base();      // subtype header, common part
                if (!head.Search(cm))
                {
                    Debug.WriteLine("RenewClasSsr: invalid header");
                    return;
                }

                switch (head.ID)
                {
                    case SubType.ST1_Mask:
                        //stp.ST1_Mask.AddHeaderData(head);
                        //stp.ST1_Mask.Decode(cm, ssr);
                        //ssr.ST1_Mask.AddHeaderData(head);
                        //ssr.ST1_Mask.Decode(cm, ssr);
#if DEBUG
                        //_ = stp.GetSSR();
#endif
                        break;
                }
            }



        }



    }

}

