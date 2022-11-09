using System.Diagnostics;
using System.Text;
using static GrToolBox.Common.CommonDefinitions;
using static GrToolBox.Data.CLAS.ClasDefinitions;
using static GrToolBox.Data.DataUtilities;

namespace GrToolBox.Data.CLAS
{
    public class ST4_CodeBias : ClasSubTypeHeader_base
    {
        public SubTypeStatus Status { get; set; } = SubTypeStatus.Initialize;
        public int Next { get; set; } = 0;   // BitArray中の位置（次に読み出すビット位置インデックス）

        private GrBitArray BitArray { get; set; } = new GrBitArray();
        private int HaveBit { get; set; } = 0;      // ストアしたビット数
        private ST1_Mask ST1_Mask { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="datas">SubType1(Mask info) object</param>
        public ST4_CodeBias(ST1_Mask sT1_Mask)
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
            int nSig = ST1_Mask.GetNSig();
            int nBitsST3 = 37 + (11 * nSig);                            // ST4: 37ビット(ヘッダ)+11ビット*信号数
            int nBitsRequired = nBitsST3 - HaveBit;                     // 今回読み出し必要なビット数
            int nBitsInCm = 1695 - (cm.Pos - 49);                       // cmのData部分内での利用可能ビット数 (cmのヘッダ：49ビット)
                                                                        // 足りないときは全部ストア
            int nBitsToStore = (nBitsRequired <= nBitsInCm) ? nBitsRequired : nBitsInCm;

            BitArray.AddByteArray(cm.Bytes, cm.Pos, nBitsToStore);
            HaveBit += nBitsToStore;
            cm.Pos += nBitsToStore;

            DecodeStart();
        }


        /// <summary>
        /// ST3のデコード
        /// </summary>
        private void DecodeStart()
        {
            byte[] b = BitArray.GetByteArray();
            Next = 37;           // ST2ヘッダ部分は読み込み完了しているので飛ばす

            foreach (var d in ST1_Mask.ClasSSRsInThis)
            {
                int nsig = d.IndSigs.Count;
                if (!CheckNext(11 * nsig))
                {
                    Status = SubTypeStatus.NeedMoreData;
                    DebugDisp();
                    return;
                }
                d.Codb.Clear();
                for(int i = 0; i < nsig; i++)
                {
                    double codbias = BitToInt(b, Next, 11) * 0.02;
                    d.Codb.Add(codbias);
                    Next += 11;
                }
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
            StringBuilder sb = new StringBuilder();
            foreach (var d in ST1_Mask.ClasSSRsInThis)
            {
                sb.Append($"    sysid={d.GnssID}  svn={d.SvN:d2}  code bias (ST4) ");
                for(int i = 0; i < d.IndSigs.Count; i++)
                {
                    sb.Append($"sig_{d.IndSigs[i], 2:00}: {d.Codb[i], 5:0.00}");
                    if(d.Codb[i] == -20.48)
                    {
                        sb.Append("(not available)");
                    }
                    sb.Append(", ");
                }
                sb.Append("\n");
            }
            Debug.Write(sb.ToString());
#endif
        }
    }

}


