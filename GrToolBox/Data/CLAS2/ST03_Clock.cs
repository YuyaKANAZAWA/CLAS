using System.Diagnostics;
using static GrToolBox.Data.CLAS2.ClasDefinitions;
using static GrToolBox.Data.DataUtilities;
using static GrToolBox.Data.CLAS2.ClasUtilities;

namespace GrToolBox.Data.CLAS2
{
    public class ST03_Clock : ClasHeaderCommonData
    {
        public SubTypeStatus Status { get; set; } = SubTypeStatus.HeaderDone;
        //public int Next { get; set; } = 0;   // BitArray中の位置（次に読み出すビット位置インデックス）

        //private GrBitArray BitArray { get; set; } = new GrBitArray();
        private int NBitRead { get; set; } = 0;      // 処理したビット数

        public ST03_Clock(ClasHeaderCommonData h)
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

        public SubTypeStatus Decode(BitCircularBuffer bcb, List<ClasSSRData> ssr)
        {
            NBitRead = 0;
            int nSat = GetNSat(ssr);
            for (int i = 0; i < nSat; i++)
            {
                if (bcb.Count() < 15)
                {
                    Status = SubTypeStatus.NeedMoreData;
                    return Status;
                }
                double dcc0 = bcb.GetInt(15) * 0.0016;
                ssr[i].Dcc0 = dcc0;
                NBitRead += 15;
            }
            Status = SubTypeStatus.DataDone;
            DebugDisp(ssr);
            return Status;
        }

        //private void Decode(ClasMessage cm)
        //{
        //    int nSat = ST1_Mask.GetNSat();
        //    int nBitsST3 = 37 + 15 * nSat;
        //    int nBitsRequired = nBitsST3 - HaveBit;                     // ST3: 37+15*Nsat [bits]
        //    int nBitsInCm = 1695 - (cm.Pos - 49);                       // cmのData部分内での利用可能ビット数 (cmのヘッダ：49ビット)
        //                                                                // 足りないときは全部ストア
        //    int nBitsToStore = (nBitsRequired <= nBitsInCm)? nBitsRequired : nBitsInCm;

        //    BitArray.AddByteArray(cm.Bytes, cm.Pos, nBitsToStore);
        //    HaveBit += nBitsToStore;
        //    cm.Pos += nBitsToStore;

        //    DecodeStart(nSat);
        //}

        /// <summary>
        /// ST3のデコード
        /// </summary>
        //private void DecodeStart(int nSat)
        //{
        //    byte[] b = BitArray.GetByteArray();
        //    Next = 37;           // ST3ヘッダ部分は読み込み完了しているので飛ばす

        //    for(int i = 0; i < nSat; i++)
        //    {
        //        if (!CheckNext(15)) 
        //        { 
        //            Status = SubTypeStatus.NeedMoreData;
        //            DebugDisp();
        //            return; 
        //        }
        //        double dcc0 = BitToInt(b, Next, 15) * 0.0016;
        //        Next += 15;
        //        ST1_Mask.ClasSSRsInThis[i].Dcc0 = dcc0;
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
                    Debug.WriteLine($"{MessageNumber:0000} type={ID}, Decode done  ({NBitRead} bits read");
                    foreach (var d in ssr)
                    {
                        string s = "";
                        if (d.Dcc0 == -26.2144) s = "(not available)";
                        Debug.WriteLine($"    sysid={d.GnssID}  svn={d.SvN:d2}  dcc0 (ST3)={d.Dcc0,9:0.0000} {s}");
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
//                if (d.Dcc0 == -26.2144) s = "(not available)";
//                Debug.WriteLine($"    sysid={d.GnssID}  svn={d.SvN:d2}  dcc0 (ST3)={d.Dcc0,9:0.0000} {s}");
//            }
//#endif
//        }

    }
}


