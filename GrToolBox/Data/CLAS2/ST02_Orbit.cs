using System.Diagnostics;
using static GrToolBox.Common.CommonDefinitions;
using static GrToolBox.Data.CLAS2.ClasDefinitions;
using static GrToolBox.Data.DataUtilities;
using static GrToolBox.Data.CLAS2.ClasUtilities;

namespace GrToolBox.Data.CLAS2
{
    public class ST02_Orbit : ClasHeaderCommonData
    {
        //public List<double> DeltaClkC0 { get; set; } = new List<double>();
        public SubTypeStatus Status { get; set; } = SubTypeStatus.HeaderDone;
        //public int Next { get; set; } = 0;   // BitArray中の位置（次に読み出すビット位置インデックス）

        //private GrBitArray BitArray { get; set; } = new GrBitArray();
        private int NBitRead { get; set; } = 0;      // ストアしたビット数
        //private ST1_Mask ST1_Mask { get; set; }

        public ST02_Orbit(ClasHeaderCommonData h) 
        {
            if (h != null)
            {
                AddHeaderData(h);
            }
        }

        private void AddHeaderData(ClasHeaderCommonData h)
        {
            MessageNumber = h.MessageNumber;
            ID = h.ID;
            Time = h.Time;
            UpdateInterval = h.UpdateInterval;
            Multiple = h.Multiple;
            IodSsr = h.IodSsr;
        }

        /// <summary>
        /// Decoding Sub Type2 (Orbit Correction Message)
        /// </summary>
        /// <param name="cm">CLAS message (2000bit)</param>
        /// <param name="nSat">全衛星数</param>
        /// <param name="nGAL">GAL衛星数</param>
        /// <param name="h">Header</param>
        //public void Decode(ClasMessage cm, ClasSubTypeHeader_base? h)
        //{
        //    if (h != null && Status == SubTypeStatus.Initialize)
        //    {
        //        AddHeaderData(h);
        //        BitArray.Clear();
        //    }
        //    Decode(cm);

        //}

        public SubTypeStatus Decode(BitCircularBuffer bcb, List<ClasSSRData> ssr)
        {
            int nSat = GetNSat(ssr);
            int nGAL = GetNSat(ssr, SYS.GAL);
            NBitRead = 0;
            foreach (var d in ssr)
            {
                int nBitsIODE = (d.GnssID == SYS.GAL) ? 10 : 8;
                if(bcb.Count() < 41 + nBitsIODE) 
                { 
                    Status = SubTypeStatus.NeedMoreData;
                    DebugDisp(ssr);
                    return Status;
                }
                d.Iode = (int)bcb.GetUint(nBitsIODE);
                d.Dorb[0] = bcb.GetInt(15) * 0.0016;     // Delta Radial
                d.Dorb[1] = bcb.GetInt(13) * 0.0064;     // Delta Along Track
                d.Dorb[2] = bcb.GetInt(13) * 0.0064;     // Delta Cross Track
                NBitRead += (41 + nBitsIODE);
            }
            Status = SubTypeStatus.DataDone;
            DebugDisp(ssr);
            return Status;
        }


        /// <summary>
        /// ST3のデコード
        /// </summary>
        //private void DecodeStart(int nSat)
        //{
        //    byte[] b = BitArray.GetByteArray();
        //    Next = 37;           // ST2ヘッダ部分は読み込み完了しているので飛ばす

        //    foreach(var d in ST1_Mask.ClasSSRsInThis)
        //    {
        //        int nBitsIODE = 8;
        //        bool igal = d.GnssID == SYS.GAL;
        //        if (igal)
        //        {
        //            nBitsIODE = 10;
        //        }
        //        if (!CheckNext(41 + nBitsIODE)) 
        //        { 
        //            Status = SubTypeStatus.NeedMoreData;
        //            DebugDisp();
        //            return;
        //        }
        //        d.Iode = (int)BitToUint(b, Next, nBitsIODE);
        //        Next += nBitsIODE;
        //        d.Dorb[0] = BitToInt(b, Next, 15) * 0.0016;     // Delta Radial
        //        Next += 15;
        //        d.Dorb[1] = BitToInt(b, Next, 13) * 0.0064;     // Delta Along Track
        //        Next += 13;
        //        d.Dorb[2] = BitToInt(b, Next, 13) * 0.0064;     // Delta Cross Track
        //        Next += 13;
        //    }
        //    Status = SubTypeStatus.DecodeDone;
        //    DebugDisp();
        //}

        //private bool CheckNext(int nBits)
        //{
        //    return ((Next + nBits) <= HaveBit);
        //}

        private void DebugDisp(List<ClasSSRData> ssr)
        {
#if DEBUG
            switch (Status)
            {
                case SubTypeStatus.NeedMoreData:
                    Debug.WriteLine($"Need more data, {NBitRead} bits read");
                    break;
                case SubTypeStatus.InvalidData:
                    Debug.WriteLine($"Invalid data, {NBitRead} bits read");
                    break;
                case SubTypeStatus.DataDone:
                    Debug.WriteLine($"{MessageNumber:0000} type={ID}, Decode done  ({NBitRead} bits read)");
                    foreach (var d in ssr)
                    {
                        string s = "";
                        if (d.Dorb[0] == -26.2144 || d.Dorb[1] == -26.2144 || d.Dorb[2] == -26.2144) s = "(not available)";
                        Debug.WriteLine($"    sysid={d.GnssID}  svn={d.SvN:d2}  orb delta (ST2) Radial={d.Dorb[0],9:0.0000}  Along={d.Dorb[1],9:0.0000}  Cross={d.Dorb[2],9:0.0000} {s}");
                    }
                    break;
            }
#endif
        }

//        public void DebugDispSSR()
//        {
//#if DEBUG
//            foreach (var d in ST1_Mask.ClasSSRsInThis)
//            {
//                string s = "";
//                if (d.Dorb[0] == -26.2144 || d.Dorb[1] == -26.2144 || d.Dorb[2] == -26.2144) s = "(not available)";
//                Debug.WriteLine($"    sysid={d.GnssID}  svn={d.SvN:d2}  orb delta (ST2) Radial={d.Dorb[0],9:0.0000}  Along={d.Dorb[1],9:0.0000}  Cross={d.Dorb[2],9:0.0000} {s}");
//            }
//#endif
//        }


    }

}


