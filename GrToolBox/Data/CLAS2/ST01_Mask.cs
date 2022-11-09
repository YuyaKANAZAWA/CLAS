using System.Diagnostics;
using System.Numerics;
using static GrToolBox.Common.CommonDefinitions;
using static GrToolBox.Data.CLAS2.ClasDefinitions;
using static GrToolBox.Data.DataUtilities;

namespace GrToolBox.Data.CLAS2
{
    public class ST01_Mask : ClasHeaderCommonData
    {
        public int NGnss { get; set; } = 0;


        public List<MaskData> MaskData { get; set; } = new List<MaskData>();
        public SubTypeStatus Status { get; set; } = SubTypeStatus.HeaderDone;
        //public int Next { get; set; } = 0;   // BitArray中の位置（次に読み出すビット位置インデックス）
        //private List<ClasSSRData> ClasSSRsInThis { get; set; } = new List<ClasSSRData>();

        //private GrBitArray BitArray { get; set; } = new GrBitArray();
        private int NBitRead { get; set; } = 0;      // 処理したビット数

        public ST01_Mask(ClasHeaderCommonData h)
        {
            if (h != null)
            {
                AddHeaderData(h);
            }
        }

        private void AddHeaderData(ClasHeaderCommonData h)
        {
            if (h == null) return;
            MessageNumber = h.MessageNumber;
            ID = h.ID;
            Time = h.Time;
            UpdateInterval = h.UpdateInterval;
            Multiple = h.Multiple;
            IodSsr = h.IodSsr;            
        }

        public SubTypeStatus Decode(BitCircularBuffer bcb, List<ClasSSRData> ssr)
        {
            // No. of GNSS (4bit, 0-15)
            if (bcb.Count() < 4)
            {
                Status = SubTypeStatus.NeedMoreData;
                return Status;
            }
            NGnss = (int)bcb.GetUint(4);
            if (!CheckData<int>(NGnss, 0, 15))
            {
                Status = SubTypeStatus.InvalidData;
                return Status;
            }
            NBitRead += 4;

            for (int i = 0; i < NGnss; i++)
            {
                // 1衛星システム分のマスクデータ
                if (bcb.Count() < 61)
                {
                    Status = SubTypeStatus.NeedMoreData;
                    return Status;
                }
                var m = new MaskData();
                int id = (int)bcb.GetUint(4);
                m.GnssID = (SYS)id;
                if (!CheckData<int>(id, 0, 15))
                {
                    Status = SubTypeStatus.InvalidData;
                    return Status;
                }
                NBitRead += 4;
                m.SatMask = (ulong)bcb.GetUint(8) << 32;
                m.SatMask += (ulong)bcb.GetUint(32);
                NBitRead += 40;
                m.SigMask = (ushort)bcb.GetUint(16);
                NBitRead += 16;
                m.CellMaskFlag = bcb.GetBit();
                NBitRead += 1;
                m.NSat = BitOperations.PopCount(m.SatMask);
                m.NSig = BitOperations.PopCount(m.SigMask);
                int nCell = m.NSat * m.NSig;                    // cell mask のビット数
                m.CellMask = new ushort[m.NSat];
                if (bcb.Count() < nCell)
                {
                    Status = SubTypeStatus.NeedMoreData;
                    return Status;
                }
                if (m.CellMaskFlag)
                {
                    for (int j = 0; j < m.NSat; j++)
                    {
                        m.CellMask[j] = (ushort)bcb.GetUint(m.NSig);
                        NBitRead += m.NSig;
                    }
                }
                MaskData.Add(m);
            }
            Status = SubTypeStatus.DataDone;
            CreateNewList(ssr);
            DebugDisp(ssr);
            return Status;
       }


        private void CreateNewList(List<ClasSSRData> ssr)
        {
            ssr.Clear();
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
                        var d = new ClasSSRData();
                        d.GnssID = m.GnssID;
                        d.SvN = 40 - i;
                        if (m.CellMaskFlag)                     // cellmaskに対応するsignalのインデックスを抽出
                        {
                            for (int j = m.NSig - 1; j >= 0; j--)
                            {
                                if ((((ushort)1 << j) & m.CellMask[count]) > 0)
                                {
                                    d.IndSigs.Add(indSigs[m.NSig - 1 - j]);
                                }
                            }
                            d.CellMask = m.CellMask[count];
                            count++;
                        }
                        else
                        {
                            foreach (var ind in indSigs)
                            {
                                d.IndSigs.Add(ind);
                            }
                        }
                        ssr.Add(d);
                    }
                }
            }
        }







        ///// <summary>
        ///// ST1のデコード
        ///// </summary>
        ///// <returns>Number of bits read</returns>
        //private void DecodeStart()
        //{
        //    byte[] b = BitArray.GetByteArray();
        //    Next = 45;           // ST1ヘッダ部分は読み込み完了しているので飛ばす

        //    // No. of GNSS (4bit, 0-15)
        //    if (!CheckNext(4)) { Status = SubTypeStatus.NeedMoreData; return; }
        //    NGnss = (int)BitToUint(b, Next, 4);
        //    Next += 4;
        //    if(!CheckData<int>(NGnss, 0, 15)) { Status = SubTypeStatus.InvalidData; return; }
        //    //

        //    for (int i = 0; i < NGnss; i++)
        //    {
        //        // 1衛星システム分のマスクデータ
        //        if (!CheckNext(61)) { Status = SubTypeStatus.NeedMoreData; return; }
        //        var m = new MaskData();
        //        int id = BitToInt(b, Next, 4);
        //        Next += 4;
        //        m.GnssID = (SYS)id;
        //        if (!CheckData<int>(id, 0, 15)) { Status = SubTypeStatus.InvalidData; return; }
        //        m.SatMask = (ulong)BitToUint(b, Next, 8) << 32;
        //        Next += 8;
        //        m.SatMask += (ulong)BitToUint(b, Next, 32);
        //        Next += 32;
        //        m.SigMask = (ushort)BitToUint(b, Next, 16);
        //        Next += 16;
        //        m.CellMaskFlag = BitToBool(b, Next);
        //        Next++;

        //        m.NSat = BitOperations.PopCount(m.SatMask);
        //        m.NSig = BitOperations.PopCount(m.SigMask);
        //        int nCell = m.NSat * m.NSig;                    // cell mask のビット数
        //        m.CellMask = new ushort[m.NSat];

        //        if (!CheckNext(nCell)) { Status = SubTypeStatus.NeedMoreData; return; }
        //        if (m.CellMaskFlag)
        //        {
        //            for (int j = 0; j < m.NSat; j++)
        //            {
        //                m.CellMask[j] = (ushort)BitToUint(b, Next, m.NSig);
        //                Next += m.NSig;
        //            }
        //        }
        //        MaskData.Add(m);
        //    }
        //    Status = SubTypeStatus.DecodeDone;
        //    CreateNewList();
        //}



        private void DebugDisp(List<ClasSSRData> ssr)
        {
            switch (Status)
            {
                case SubTypeStatus.NeedMoreData:
                    Debug.WriteLine($"Need more data, {NBitRead} bits read");
                    break;
                case SubTypeStatus.InvalidData:
                    Debug.WriteLine($"Invalid data, {NBitRead} bits read");
                    break;
                case SubTypeStatus.DataDone:
                    Debug.WriteLine($"{MessageNumber:0000} type={ID} sow={Time.TotalSeconds} sui={UpdateInterval} mmi={Multiple} iodSsr={IodSsr} nGnss={NGnss}");
                    foreach (var m in MaskData)
                    {
                        Debug.WriteLine($"  sysid={m.GnssID} satmask={m.SatMask:x} sigmask={m.SigMask:x} cma={Convert.ToInt32(m.CellMaskFlag)} nSat={m.NSat} nSig={m.NSig}");
                    }
                    foreach (var d in ssr)
                    {
                        Debug.WriteLine($"    sysid={d.GnssID}  svn={d.SvN:d2}  cellmask={d.CellMask:x4} indSigs={string.Join(',', d.IndSigs)} ");
                    }
                    break;
            }
        }

        //public int GetNSat()
        //{
        //    return ClasSSRsInThis.Count;
        //}

        //public int GetNSat(SYS sys)
        //{
        //    return ClasSSRsInThis.Where(d => d.GnssID == sys).Count();
        //}

        //public int GetNSig()
        //{
        //    int nsig = 0;
        //    foreach(var d in ClasSSRsInThis)
        //    {
        //        nsig += d.IndSigs.Count;
        //    }
        //    return nsig;
        //}



//        public void DebugDispSSR()
//        {
//#if DEBUG
//            foreach (var d in ClasSSRsInThis)
//            {
//                Debug.WriteLine($"    sysid={d.GnssID}  svn={d.SvN:d2}  cellmask={d.CellMask:x4} indSigs={string.Join(',', d.IndSigs)} ");
//            }
//#endif
//        }


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
