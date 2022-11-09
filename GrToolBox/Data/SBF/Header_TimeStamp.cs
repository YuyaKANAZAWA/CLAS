using System.Diagnostics;
using static GrToolBox.Data.DataUtilities;
using static GrToolBox.Data.SBF.SbfUtilities;

namespace GrToolBox.Data.SBF
{
    /// <summary>
    /// Base class for SBF data
    /// </summary>
    public class Header_TimeStamp
    {
        public char Sync1 { get; set; }
        public char Sync2 { get; set; }
        public ushort BlockNum { get; set; }
        public ushort BlockRev { get; set; }
        public ushort Length { get; set; }
        public double Tow { get; set; }       // TOW [sec]
        public ushort WNc { get; set; }

        public Header_TimeStamp() { }

        public Header_TimeStamp(byte[] byteData)
        {
            int pos = 0;
            byte sync1 = byteData[pos]; pos++;
            byte sync2 = byteData[pos]; pos++;
            byte[] crc = byteData[pos..(pos + 2)]; pos += 2;
            byte[] id = byteData[pos..(pos + 2)]; pos += 2;
            byte[] length = byteData[pos..(pos + 2)]; pos += 2;
            byte[] tow = byteData[pos..(pos + 4)]; pos += 4;
            byte[] wnc = byteData[pos..(pos + 2)]; pos += 2;

            Sync1 = (char)byteData[pos];
            Sync2 = (char)byteData[pos];
            BlockNum = (ushort)BitToUint_L(id, 0, 13);
            BlockRev = (ushort)BitToUint_L(id, 13, 3);
            Length = BitConverter.ToUInt16(length);
            Tow = BitConverter.ToUInt32(tow) * 0.001;
            WNc = BitConverter.ToUInt16(wnc);
        }


        public void DebugPrint_Header()
        {
            string name = SbfBlockName(BlockNum);
            if (name.Length < 6)
            {
                for (int i = 0; i < name.Length - 6; i++) { name += " "; }
            }
            else if (name.Length > 6)
            {
                name = name[0..6];
            }
            //Debug.WriteLine($"  **SBF({BlockNum},{name}), {Length:000} bytes, Tow:{Tow:.0}\n");
        }


    }

}
