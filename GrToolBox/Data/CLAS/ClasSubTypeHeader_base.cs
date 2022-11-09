using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static GrToolBox.Data.CLAS.ClasDefinitions;
using static GrToolBox.Data.DataUtilities;
using System.Diagnostics;

namespace GrToolBox.Data.CLAS
{
    public class ClasSubTypeHeader_base
    {
        public SubTypeStatus Status { get; set; } = SubTypeStatus.Initialize;
        private int Next { get; set; } = 0;   // BitArray中の位置（次に読み出すビット位置インデックス）

        private GrBitArray BitArray { get; set; } = new GrBitArray();
        private int HaveBit { get; set; } = 0;      // ストアしたビット数

        private int NBitsInHeaderBase { get; set; } = 25;         // time以外で25ビット

        private int NBitsInPreviousCM { get; set; } = 0;

        public int MessageNumber { get; set; } = 0;
        public SubType ID { get; set; } = SubType.Unknown;
        public TimeSpan Time { get; set; } = TimeSpan.MinValue;
        public int UpdateInterval { get; set; } = -1;
        public bool Multiple { get; set; } = false;
        public int IodSsr { get; set; }

        public ClasSubTypeHeader_base() { }

        public void Search_Decode(ClasMessage cm)
        {
            byte[] b = cm.Bytes;
            int startPos = cm.Pos;
            int nBitsInCm = 1695 - (cm.Pos - 49);                       // cmのData部分内での利用可能ビット数 (cmのヘッダ：49ビット)
            //HaveBit += nBitsInCm;

            if(Status == SubTypeStatus.Initialize)
            {
                b = cm.Bytes;
                startPos = cm.Pos;
                HaveBit = nBitsInCm;
            }
            else if(Status == SubTypeStatus.NeedMoreData)
            {
                Status = SubTypeStatus.Continueing;
                BitArray.AddByteArray(cm.Bytes, cm.Pos, nBitsInCm);
                b = BitArray.GetByteArray();
                startPos = 0;
                HaveBit = NBitsInPreviousCM + nBitsInCm;
            }




            while (startPos < b.Length * 8 - 256 - 49)
            {
                Next = startPos;

                if (!CheckNext(12))
                {
                    Status = SubTypeStatus.NeedMoreData;
                    break;
                }
                MessageNumber = (int)BitToUint(b, Next, 12);
                Next += 12;
                if (MessageNumber != 4073)
                {
                    startPos++;
                    continue;
                }

                if (!CheckNext(4))
                {
                    Status = SubTypeStatus.NeedMoreData;
                    break;
                }
                int id = (int)BitToUint(b, Next, 4);
                ID = Enum.IsDefined(typeof(SubType), id) ? (SubType)id : SubType.Unknown;
                Next += 4;
                if(ID == SubType.Unknown)
                {
                    startPos++;
                    continue;
                }

                int nBitsTime = (ID == SubType.ST1_Mask) ? 20 : 12;
                NBitsInHeaderBase += nBitsTime;     // header baseの全ビット数確定
                if (!CheckNext(nBitsTime + 9))      // header_baseの残りビット数確定-存在チェック
                {
                    Status = SubTypeStatus.NeedMoreData;
                    break;
                }
                Time = new TimeSpan(0, 0, (int)BitToUint(b, Next, nBitsTime));
                Next += nBitsTime;
                if(!CheckData<int>((int)Time.TotalSeconds, 0, 3599))
                {
                    startPos++;
                    continue;
                }

                UpdateInterval = (int)BitToUint(b, Next, 4);
                Next += 4;
                if (!CheckData<int>(UpdateInterval, 0, 15))
                {
                    startPos++;
                    continue;
                }

                Multiple = BitToBool(b, Next);
                Next++;

                IodSsr = (int)BitToUint(b, Next, 4);
                Next += 4;
                if (!CheckData<int>(UpdateInterval, 0, 15))
                {
                    startPos++;
                    continue;
                }


                if(Status == SubTypeStatus.NeedMoreData)
                {
                    NBitsInPreviousCM = Next - startPos;
                    BitArray.AddByteArray(b, startPos, NBitsInPreviousCM);                    
                }
                else
                {
                    Status = SubTypeStatus.DecodeDone;
                    cm.Pos = 49 + NBitsInHeaderBase - NBitsInPreviousCM;
#if DEBUG
                    DebugDisp(true, cm);
#endif

                }

            }













        }

        private bool CheckNext(int nBits)
        {
            return ((Next + nBits) <= HaveBit);
        }



        public bool Search(ClasMessage cm)
        {
            byte[] b = cm.Bytes;
            int startPos = cm.Pos;
            bool found = false;

            while(startPos < b.Length * 8 - 256 - 49)
            {
                int pos = startPos;
                MessageNumber = (int)BitToUint(b, pos, 12);
                pos += 12;
                int id = (int)BitToUint(b, pos, 4);
                ID = Enum.IsDefined(typeof(SubType), id) ? (SubType)id : SubType.Unknown;
                pos += 4;
                if (ID == SubType.ST1_Mask)
                {
                    Time = new TimeSpan(0, 0, (int)BitToUint(b, pos, 20));
                    pos += 20;
                }
                else
                {
                    Time = new TimeSpan(0, 0, (int)BitToUint(b, pos, 12));
                    pos += 12;
                }
                UpdateInterval = (int)BitToUint(b, pos, 4);
                pos += 4;
                Multiple = BitToBool(b, pos);
                pos++;
                IodSsr = (int)BitToUint(b, pos, 4);
                pos += 4;

                if (MessageNumber == 4073 && ID != SubType.Unknown)
                {
                    cm.Pos = startPos;      //--->>>> headerがヒットしたビットをposにする
                    found = true;
                    break;
                }
                else
                {
                    startPos++;
                }
            }
#if DEBUG
            DebugDisp(found, cm);
#endif
            return found;
        }


        private void DebugDisp(bool found, ClasMessage cm)
        {
            if (found)
            {
                Debug.WriteLine($">Subtype header found: start at {cm.Pos} [bit] of this CLAS message");
                Debug.WriteLine($"{MessageNumber:0000} type={ID} sow={Time.TotalSeconds} sui={UpdateInterval} mmi={Multiple} iodSsr={IodSsr}");
            }
            else
            {
                Debug.WriteLine("ClasSubTypeHeader_base: subtype header not found");
            }
        }

    }

}
