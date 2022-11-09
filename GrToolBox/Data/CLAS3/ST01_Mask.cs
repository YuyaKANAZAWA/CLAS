using System.Diagnostics;
using System.Numerics;
using static GrToolBox.Common.CommonDefinitions;
using static GrToolBox.Data.CLAS3.ClasDefinitions;
using static GrToolBox.Data.CLAS3.ClasUtilities;
using static GrToolBox.Data.DataUtilities;

namespace GrToolBox.Data.CLAS3
{
    public class ST01_Mask : ClasHeaderCommonData
    {
        public ST01Data Data { get; set; } = new ST01Data();
        public SubTypeStatus Status { get; set; } = SubTypeStatus.HeaderDone;
        private List<MaskData> MaskData { get; set; } = new List<MaskData>();
        private int NBitRead { get; set; } = 0;      // 処理したビット数

        public ST01_Mask()
        {
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

        public SubTypeStatus Decode(BitCircularBuffer bcb, ClasHeaderCommonData h)
        {
            MaskData.Clear();
            Data = new ST01Data();

            AddHeaderData(h);
            Status = SubTypeStatus.HeaderDone;
            // No. of GNSS (4bit, 0-15)
            if (bcb.Count() < 4)
            {
                Status = SubTypeStatus.NeedMoreData;
                return Status;
            }
            int NGnss = (int)bcb.GetUint(4);
            if (!CheckData<int>(NGnss, 0, 15))
            {
                Status = SubTypeStatus.InvalidData;
                return Status;
            }
            Data.NGnss = NGnss;
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
            ExtractData();
            DebugDisp();
            return Status;
        }

        public int NSat()
        {
            return Data.Sat.Count;
        }

        public int NSat(SYS sys)
        {
            return Data.Sat.Where(d => d.GnssID == sys).Count();
        }



        private void ExtractData()
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
                        var d = new ST01DataEachSat();
                        d.GnssID = m.GnssID;
                        d.IndSat = 39 - i;
                        if (m.CellMaskFlag)                     // cellmaskに対応するsignalのインデックスを抽出
                        {
                            for (int j = m.NSig - 1; j >= 0; j--)
                            {
                                if ((((ushort)1 << j) & m.CellMask[count]) > 0)
                                {
                                    //d.IndSigs.Add(indSigs[m.NSig - 1 - j]);
                                    d.AddIndSigs(indSigs[m.NSig - 1 - j]);
                                }
                            }
                            d.CellMask = m.CellMask[count];
                            count++;
                        }
                        else
                        {
                            foreach (var ind in indSigs)
                            {
                                //d.IndSigs.Add(ind);
                                d.AddIndSigs(ind);
                            }
                        }
                        Data.Sat.Add(d);
                    }
                }
            }
        }
   

        private void DebugDisp()
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
                    Debug.WriteLine($"{MessageNumber:0000} type={ID} sow={Time.TotalSeconds} sui={UpdateInterval} mmi={Multiple} iodSsr={IodSsr} nGnss={Data.NGnss}");
                    foreach (var m in MaskData)
                    {
                        Debug.WriteLine($"  sysid={m.GnssID} satmask={m.SatMask:x} sigmask={m.SigMask:x} cma={Convert.ToInt32(m.CellMaskFlag)} nSat={m.NSat} nSig={m.NSig}");
                    }
                    foreach (var d in Data.Sat)
                    {
                        Debug.WriteLine($"    sysid={d.GnssID}  svn={d.SvN:d2}  cellmask={d.CellMask:x4} indSigs={string.Join(',', d.IndSigs)} ");
                    }
                    break;
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


    public class ST01Data : ClasHeaderCommonData
    {
        // extended header
        public int NGnss { get; set; } = 0;

        public List<ST01DataEachSat> Sat { get; set; } = new List<ST01DataEachSat>();
    }

    public class ST01DataEachSat
    {
        public SYS GnssID { get; set; } = SYS.UNKNOWN;
        public int IndSat {
            get { return indSat; }
            set
            {
                indSat = value;
                SvN = GetSvN(GnssID, value);
            }
        }
        private int indSat { get; set; } = -1; 
        public int SvN { get; set; } = -1;
        public List<int> IndSigs { get; } = new List<int>();
        public List<string> SigNames { get; } = new List<string>();
        public ushort CellMask { get; set; } = 0;

        public void AddIndSigs(int ind)
        {
            IndSigs.Add(ind);
            // SigNames.Add(GetSigName(GnssID, ind));
        }
    }


}
