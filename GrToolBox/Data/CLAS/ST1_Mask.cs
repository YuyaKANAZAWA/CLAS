using System.Diagnostics;
using System.Numerics;
using static GrToolBox.Common.CommonDefinitions;
using static GrToolBox.Data.CLAS.ClasDefinitions;
using static GrToolBox.Data.DataUtilities;

namespace GrToolBox.Data.CLAS
{
    public class ST1_Mask : ClasSubTypeHeader_base
    {
        public int NGnss { get; set; } = 0;
        public List<MaskData> MaskData { get; set; } = new List<MaskData>();
        public SubTypeStatus Status { get; set; } = SubTypeStatus.Initialize;
        public int Next { get; set; } = 0;   // BitArray中の位置（次に読み出すビット位置インデックス）
        public List<ClasSSRData> ClasSSRsInThis { get; set; } = new List<ClasSSRData>();

        private GrBitArray BitArray { get; set; } = new GrBitArray();
        private int HaveBit { get; set; } = 0;      // ストアしたビット数

        public ST1_Mask() { }

        private void AddHeaderData(ClasSubTypeHeader_base h)
        {
            if (h == null) return;
            MessageNumber = h.MessageNumber;
            ID = h.ID;
            Time = h.Time;
            UpdateInterval = h.UpdateInterval;
            Multiple = h.Multiple;
            IodSsr = h.IodSsr;
        }

        public void Decode(ClasMessage cm, ClasSubTypeHeader_base? h)
        {
            if(h != null)
            {
                AddHeaderData(h);
            }
            Decode(cm);
        }

        public void Decode(ClasMessage cm)
        {
            int nBitsInCm = 1695 - (cm.Pos - 49);                       // cmのData部分内でのヘッダ開始位置は(cm.Pos-49)
            BitArray.AddByteArray(cm.Bytes, cm.Pos, nBitsInCm);         // ST1はdecode完了しないとサイズ不明．後ろ全部を格納する
            HaveBit += nBitsInCm;                                       // バッファされたビット数
            DecodeStart();
            cm.Pos = Next % 1695;

#if DEBUG
            DebugDisp();
#endif

        } 

        private bool CheckNext(int nBits)
        {
            return ((Next + nBits) <= HaveBit);
        }

        /// <summary>
        /// ST1のデコード
        /// </summary>
        /// <returns>Number of bits read</returns>
        private void DecodeStart()
        {
            byte[] b = BitArray.GetByteArray();
            Next = 45;           // ST1ヘッダ部分は読み込み完了しているので飛ばす

            // No. of GNSS (4bit, 0-15)
            if (!CheckNext(4)) { Status = SubTypeStatus.NeedMoreData; return; }
            NGnss = (int)BitToUint(b, Next, 4);
            Next += 4;
            if(!CheckData<int>(NGnss, 0, 15)) { Status = SubTypeStatus.InvalidData; return; }
            //

            for (int i = 0; i < NGnss; i++)
            {
                // 1衛星システム分のマスクデータ
                if (!CheckNext(61)) { Status = SubTypeStatus.NeedMoreData; return; }
                var m = new MaskData();
                int id = BitToInt(b, Next, 4);
                Next += 4;
                m.GnssID = (SYS)id;
                if (!CheckData<int>(id, 0, 15)) { Status = SubTypeStatus.InvalidData; return; }
                m.SatMask = (ulong)BitToUint(b, Next, 8) << 32;
                Next += 8;
                m.SatMask += (ulong)BitToUint(b, Next, 32);
                Next += 32;
                m.SigMask = (ushort)BitToUint(b, Next, 16);
                Next += 16;
                m.CellMaskFlag = BitToBool(b, Next);
                Next++;

                m.NSat = BitOperations.PopCount(m.SatMask);
                m.NSig = BitOperations.PopCount(m.SigMask);
                int nCell = m.NSat * m.NSig;                    // cell mask のビット数
                m.CellMask = new ushort[m.NSat];

                if (!CheckNext(nCell)) { Status = SubTypeStatus.NeedMoreData; return; }
                if (m.CellMaskFlag)
                {
                    for (int j = 0; j < m.NSat; j++)
                    {
                        m.CellMask[j] = (ushort)BitToUint(b, Next, m.NSig);
                        Next += m.NSig;
                    }
                }
                MaskData.Add(m);
            }
            Status = SubTypeStatus.DecodeDone;
            CreateNewList();
        }

        private void CreateNewList()
        {
            foreach (var m in MaskData)
            {
                List<int> indSigs = new List<int>();
                for (int i = 15; i >= 0; i--)                    // signal maskのインデックスを抽出
                {
                    if ((((ushort)1 << i) & m.SigMask) > 0)
                    {
                        indSigs.Add(15 - i);
                    }
                }

                int count = 0;
                for (int i = 39; i >= 0; i--)
                {
                    if ((((ulong)1 << i) & m.SatMask) > 0)      // satmask(40ビット)のビット真偽を判定していく
                    {
                        var data = new ClasSSRData();
                        data.GnssID = m.GnssID;
                        data.SvN = 40 - i;
                        if (m.CellMaskFlag)                     // cellmaskに対応するsignalのインデックスを抽出
                        {
                            for (int j = m.NSig - 1; j >= 0; j--)
                            {
                                if ((((ushort)1 << j) & m.CellMask[count]) > 0)
                                {
                                    data.IndSigs.Add(indSigs[m.NSig - 1 - j]);
                                }
                            }
                            data.CellMask = m.CellMask[count];
                            count++;
                        }
                        else
                        {
                            foreach (var ind in indSigs)
                            {
                                data.IndSigs.Add(ind);
                            }
                        }
                        ClasSSRsInThis.Add(data);
                    }
                }
            }
        }


        private void DebugDisp()
        {
            switch (Status)
            {
                case SubTypeStatus.NeedMoreData:
                    Debug.WriteLine($"Need more data, HaveBit={HaveBit}");
                    break;
                case SubTypeStatus.InvalidData:
                    Debug.WriteLine($"Invalid data, at {Next} (HaveBit={HaveBit})");
                    break;
                case SubTypeStatus.DecodeDone:
                    Debug.WriteLine($"{MessageNumber:0000} type={ID} sow={Time.TotalSeconds} sui={UpdateInterval} mmi={Multiple} iodSsr={IodSsr} nGnss={NGnss}");
                    foreach (var m in MaskData)
                    {
                        Debug.WriteLine($"  sysid={m.GnssID} satmask={m.SatMask:x} sigmask={m.SigMask:x} cma={Convert.ToInt32(m.CellMaskFlag)} nSat={m.NSat} nSig={m.NSig}");
                    }
                    break;
            }
        }

        public int GetNSat()
        {
            return ClasSSRsInThis.Count;
        }

        public int GetNSat(SYS sys)
        {
            return ClasSSRsInThis.Where(d => d.GnssID == sys).Count();
        }

        public int GetNSig()
        {
            int nsig = 0;
            foreach(var d in ClasSSRsInThis)
            {
                nsig += d.IndSigs.Count;
            }
            return nsig;
        }



        public void DebugDispSSR()
        {
#if DEBUG
            foreach (var d in ClasSSRsInThis)
            {
                Debug.WriteLine($"    sysid={d.GnssID}  svn={d.SvN:d2}  cellmask={d.CellMask:x4} indSigs={string.Join(',', d.IndSigs)} ");
            }
#endif
        }


    }


    public class MaskData
    {
        public SYS GnssID { get; set; } = SYS.UNKNOWN;
        public ulong SatMask { get; set; } = 0;
        public int NSat { get; set; } = 0;
        public ushort SigMask { get; set; } = 0;
        public int NSig { get; set; } = 0;
        public bool CellMaskFlag { get; set; } = false;
        public ushort[] CellMask { get; set; } = new ushort[] {};
    }

}
