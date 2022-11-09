using System.Diagnostics;
using static GrToolBox.Common.CommonDefinitions;
using static GrToolBox.Data.CLAS.ClasDefinitions;
using static GrToolBox.Data.DataUtilities;

namespace GrToolBox.Data.CLAS
{
    public class ST2_Orbit : ClasSubTypeHeader_base
    {
        //public List<double> DeltaClkC0 { get; set; } = new List<double>();
        public SubTypeStatus Status { get; set; } = SubTypeStatus.Initialize;
        public int Next { get; set; } = 0;   // BitArray中の位置（次に読み出すビット位置インデックス）

        private GrBitArray BitArray { get; set; } = new GrBitArray();
        private int HaveBit { get; set; } = 0;      // ストアしたビット数
        private ST1_Mask ST1_Mask { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="datas">SubType1(Mask info) object</param>
        public ST2_Orbit(ST1_Mask sT1_Mask) 
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

        /// <summary>
        /// Decoding Sub Type2 (Orbit Correction Message)
        /// </summary>
        /// <param name="cm">CLAS message (2000bit)</param>
        /// <param name="nSat">全衛星数</param>
        /// <param name="nGAL">GAL衛星数</param>
        /// <param name="h">Header</param>
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
            int nGAL = ST1_Mask.GetNSat(SYS.GAL);
            int nBitsST3 = 37 + (51 * nGAL) + 49 * (nSat - nGAL);       // ST2: 37ビット(ヘッダ)+51ビット(GAL)+49ビット(その他)
            int nBitsRequired = nBitsST3 - HaveBit;                     // 今回読み出し必要なビット数
            int nBitsInCm = 1695 - (cm.Pos - 49);                       // cmのData部分内での利用可能ビット数 (cmのヘッダ：49ビット)
                                                                        // 足りないときは全部ストア
            int nBitsToStore = (nBitsRequired <= nBitsInCm) ? nBitsRequired : nBitsInCm;

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
            Next = 37;           // ST2ヘッダ部分は読み込み完了しているので飛ばす

            foreach(var d in ST1_Mask.ClasSSRsInThis)
            {
                int nBitsIODE = 8;
                bool igal = d.GnssID == SYS.GAL;
                if (igal)
                {
                    nBitsIODE = 10;
                }
                if (!CheckNext(41 + nBitsIODE)) 
                { 
                    Status = SubTypeStatus.NeedMoreData;
                    DebugDisp();
                    return;
                }
                d.Iode = (int)BitToUint(b, Next, nBitsIODE);
                Next += nBitsIODE;
                d.Dorb[0] = BitToInt(b, Next, 15) * 0.0016;     // Delta Radial
                Next += 15;
                d.Dorb[1] = BitToInt(b, Next, 13) * 0.0064;     // Delta Along Track
                Next += 13;
                d.Dorb[2] = BitToInt(b, Next, 13) * 0.0064;     // Delta Cross Track
                Next += 13;
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
                if (d.Dorb[0] == -26.2144 || d.Dorb[1] == -26.2144 || d.Dorb[2] == -26.2144) s = "(not available)";
                Debug.WriteLine($"    sysid={d.GnssID}  svn={d.SvN:d2}  orb delta (ST2) Radial={d.Dorb[0],9:0.0000}  Along={d.Dorb[1],9:0.0000}  Cross={d.Dorb[2],9:0.0000} {s}");
            }
#endif
        }


    }

}


