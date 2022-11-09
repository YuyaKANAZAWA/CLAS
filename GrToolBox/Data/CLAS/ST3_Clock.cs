using System.Diagnostics;
using static GrToolBox.Data.CLAS.ClasDefinitions;
using static GrToolBox.Data.DataUtilities;

namespace GrToolBox.Data.CLAS
{
    public class ST3_Clock : ClasSubTypeHeader_base
    {
        public SubTypeStatus Status { get; set; } = SubTypeStatus.Initialize;
        public int Next { get; set; } = 0;   // BitArray中の位置（次に読み出すビット位置インデックス）

        private GrBitArray BitArray { get; set; } = new GrBitArray();
        private int HaveBit { get; set; } = 0;      // ストアしたビット数
        private ST1_Mask ST1_Mask { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="sT1_Mask">SubType1(Mask info) object</param>
        public ST3_Clock(ST1_Mask sT1_Mask)
        {
            ST1_Mask = sT1_Mask;
        }


        private void AddHeaderData(ClasSubTypeHeader_base h)
        {
            MessageNumber = h.MessageNumber;
            ID = h.ID;
            Time = h.Time;
            UpdateInterval = h.UpdateInterval;
            Multiple = h.Multiple;
            IodSsr = h.IodSsr;
        }

        public void Decode(ClasMessage cm, ClasSubTypeHeader_base? h)
        {
            if (h != null && Status == SubTypeStatus.Initialize)
            {               
                AddHeaderData(h);
                BitArray.Clear();
            }
            Decode(cm);

        }

        private void Decode(ClasMessage cm)
        {
            int nSat = ST1_Mask.GetNSat();
            int nBitsST3 = 37 + 15 * nSat;
            int nBitsRequired = nBitsST3 - HaveBit;                     // ST3: 37+15*Nsat [bits]
            int nBitsInCm = 1695 - (cm.Pos - 49);                       // cmのData部分内での利用可能ビット数 (cmのヘッダ：49ビット)
                                                                        // 足りないときは全部ストア
            int nBitsToStore = (nBitsRequired <= nBitsInCm)? nBitsRequired : nBitsInCm;

            BitArray.AddByteArray(cm.Bytes, cm.Pos, nBitsToStore);
            HaveBit += nBitsToStore;
            cm.Pos += nBitsToStore;

            DecodeStart(nSat);
        }

        /// <summary>
        /// ST3のデコード
        /// </summary>
        private void DecodeStart(int nSat)
        {
            byte[] b = BitArray.GetByteArray();
            Next = 37;           // ST3ヘッダ部分は読み込み完了しているので飛ばす

            for(int i = 0; i < nSat; i++)
            {
                if (!CheckNext(15)) 
                { 
                    Status = SubTypeStatus.NeedMoreData;
                    DebugDisp();
                    return; 
                }
                double dcc0 = BitToInt(b, Next, 15) * 0.0016;
                Next += 15;
                ST1_Mask.ClasSSRsInThis[i].Dcc0 = dcc0;
            }
            Status = SubTypeStatus.DecodeDone;
            DebugDisp();
        }

        private bool CheckNext(int nBits)
        {
            return ((Next + nBits) <= HaveBit);
        }

        private void DebugDisp()
        {
#if DEBUG
            switch (Status)
            {
                case SubTypeStatus.NeedMoreData:
                    Debug.WriteLine($"Need more data, HaveBit={HaveBit}");
                    break;
                case SubTypeStatus.InvalidData:
                    Debug.WriteLine($"Invalid data, at {Next} (HaveBit={HaveBit})");
                    break;
                case SubTypeStatus.DecodeDone:
                    Debug.WriteLine($"{MessageNumber:0000} type={ID}, Decode done  (HaveBit={HaveBit})");
                    break;
            }
#endif
        }

        public void DebugDispSSR()
        {
#if DEBUG
            foreach (var d in ST1_Mask.ClasSSRsInThis)
            {
                string s = "";
                if (d.Dcc0 == -26.2144) s = "(not available)";
                Debug.WriteLine($"    sysid={d.GnssID}  svn={d.SvN:d2}  dcc0 (ST3)={d.Dcc0,9:0.0000} {s}");
            }
#endif
        }

    }
}


