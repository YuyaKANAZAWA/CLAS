using System.Diagnostics;
using System.Text;
using static GrToolBox.Data.DataUtilities;

namespace GrToolBox.Data.RTCM.RTCM3
{
    /// <summary>
    /// RTCM3 frame structure information (message length, CRC) and message number
    /// </summary>
    public class FrameInfo_MessageNum
    {
        internal int MessageLen { get; set; }               // Message length [bytes]
        internal int MessageNum { get; set; }               // Message Number, DF002, uint12


        /// <summary>
        /// RTCM3 frame structure information (message length, CRC) and message number
        /// </summary>
        public FrameInfo_MessageNum() { }

        /// <summary>
        /// RTCM3 frame structure information (message length, CRC) and message number
        /// </summary>
        /// <param name="byteData">byte array of sentence data</param>
        public FrameInfo_MessageNum(byte[] byteData)
        {
            int offset = 14;
            MessageLen = (int)BitToUint(byteData, offset, 10);  // 14ビット目から10ビットをuintで読む
            offset += 10;
            //offset += (MessageLen * 8);                         // variable length data message 
            //offset += 24;                                       // CRC 24bits
            MessageNum = (int)BitToUint(byteData, offset, 12);  // Df002, uint12
            int aaa = 0;
        }


        public void DebugPrint()
        {
            //Debug.WriteLine($"RTCM3({MessageLen:000} bytes), MessageNum: {MessageNum}");
            StringBuilder sb = new StringBuilder();
            PrintTo(sb);
            Debug.Write(sb.ToString());
        }

        public void PrintTo(StringBuilder sb)
        {
            sb.AppendLine($"RTCM3({ MessageLen: 000} bytes), MessageNum: { MessageNum}");
        }


    }
}
