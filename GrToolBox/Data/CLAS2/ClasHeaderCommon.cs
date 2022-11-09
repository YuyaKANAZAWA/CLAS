using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static GrToolBox.Data.CLAS2.ClasDefinitions;
using static GrToolBox.Data.DataUtilities;

namespace GrToolBox.Data.CLAS2
{
    public class ClasHeaderCommon
    {
        //public int MessageNumber { get; set; } = 0;
        //public SubType ID { get; set; } = SubType.Unknown;
        //public TimeSpan Time { get; set; } = TimeSpan.MinValue;
        //public int UpdateInterval { get; set; } = -1;
        //public bool Multiple { get; set; } = false;
        //public int IodSsr { get; set; }

        public ClasHeaderCommonData Data { get; set; } = new ClasHeaderCommonData();

        public SubTypeStatus Status { get; set; }

        private int NBitsHeader { get; set; } = 25;         // time以外で25ビット


        public SubTypeStatus Search_Decode(BitCircularBuffer bcb)
        {
            Status = SubTypeStatus.SearchHeader;
            //while (bcb.Count() > 0)
            while (Status == SubTypeStatus.SearchHeader)
            {
                bcb.SetMark();

                // Message number: 4073
                if(bcb.Count() < 12)
                {
                    Status = SubTypeStatus.NeedMoreData;
                    break;
                }
                Data.MessageNumber = (int)bcb.GetUint(12);
                if (Data.MessageNumber != 4073)
                {
                    bcb.BackToMark();
                    bcb.ProceedBit(1);
                    continue;
                }

                // Subtype ID
                if (bcb.Count() < 4)
                {
                    Status = SubTypeStatus.NeedMoreData;
                    bcb.BackToMark();
                    break;
                }
                int id = (int)bcb.GetUint(4);
                Data.ID = Enum.IsDefined(typeof(SubType), id) ? (SubType)id : SubType.Unknown;
                if (Data.ID == SubType.Unknown)
                {
                    bcb.BackToMark();
                    bcb.ProceedBit(1);
                    continue;
                }

                // Subtype ID が分かると，Headerのbit数が分かる
                int nBitsTime = (Data.ID == SubType.ST1_Mask) ? 20 : 12;
                NBitsHeader += nBitsTime;               // header baseの全ビット数確定
                if (bcb.Count() < (nBitsTime + 9))      // header_baseの残りビット数確定-存在チェック
                {
                    Status = SubTypeStatus.NeedMoreData;
                    bcb.BackToMark();
                    break;
                }

                // time
                Data.Time = new TimeSpan(0, 0, (int)bcb.GetUint(nBitsTime));
                int maxSec = (Data.ID == SubType.ST1_Mask) ? 604799 : 3599;
                if (!CheckData<int>((int)Data.Time.TotalSeconds, 0, maxSec))
                {
                    bcb.BackToMark();
                    bcb.ProceedBit(1);
                    continue;
                }

                // update interval
                Data.UpdateInterval = (int)bcb.GetUint(4);
                if (!CheckData<int>(Data.UpdateInterval, 0, 15))
                {
                    bcb.BackToMark();
                    bcb.ProceedBit(1);
                    continue;
                }

                Data.Multiple = bcb.GetBit();
                Data.IodSsr = (int)bcb.GetUint(4);
                if (!CheckData<int>(Data.UpdateInterval, 0, 15))
                {
                    bcb.BackToMark();
                    bcb.ProceedBit(1);
                    continue;
                }

                Status = SubTypeStatus.HeaderDone;
                break;
            }
            DebugDisp();
            return Status;
        }


        private void DebugDisp()
        {
#if DEBUG
            if(Status == SubTypeStatus.HeaderDone)
            {
                Debug.WriteLine($">Subtype header found: ");
                Debug.WriteLine($"{Data.MessageNumber:0000} type={Data.ID} sow={Data.Time.TotalSeconds} sui={Data.UpdateInterval} mmi={Data.Multiple} iodSsr={Data.IodSsr}");
            }else if(Status == SubTypeStatus.SearchHeader)
            {
                Debug.WriteLine("Subtype header not found, waiting new data");
            }else if(Status == SubTypeStatus.NeedMoreData)
            {
                Debug.WriteLine("Need more data for header, waiting new data");
            }
#endif
        }

    }


    public class ClasHeaderCommonData
    {
        public int MessageNumber { get; set; } = 0;
        public SubType ID { get; set; } = SubType.Unknown;
        public TimeSpan Time { get; set; } = TimeSpan.MinValue;
        public int UpdateInterval { get; set; } = -1;
        public bool Multiple { get; set; } = false;
        public int IodSsr { get; set; }

    }




}
