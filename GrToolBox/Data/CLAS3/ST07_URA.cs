using System.Diagnostics;
using System.Text;
using static GrToolBox.Data.CLAS3.ClasDefinitions;

namespace GrToolBox.Data.CLAS3
{
    public class ST07_URA
    {
        public ST07Data Data { get; set; } = new ST07Data();
        public SubTypeStatus Status { get; set; } = SubTypeStatus.HeaderDone;
        private ST01_Mask? ST01 { get; set; }
        private int NBitRead { get; set; } = 0;      // 処理したビット数

        internal TraceSwitch TraceSwitch { get; set; } = new TraceSwitch("TSwitch_CLAS_ST7", "Clas Subtype7");

        public ST07_URA()
        {
            TraceSwitch.Level = TraceLevel.Verbose;
        }

        private void AddHeaderData(ClasHeaderCommonData h)
        {
            Data.MessageNumber = h.MessageNumber;
            Data.ID = h.ID;
            Data.Time = h.Time;
            Data.UpdateInterval = h.UpdateInterval;
            Data.Multiple = h.Multiple;
            Data.IodSsr = h.IodSsr;
        }

        public SubTypeStatus Decode(BitCircularBuffer bcb, ClasHeaderCommonData h, ST01_Mask st01)
        {
            if (bcb == null || h == null || st01 == null)
            {
                throw new ArgumentNullException();
            }
            ST01 = st01;
            AddHeaderData(h);
            Status = SubTypeStatus.HeaderDone;
            Data.Sat.Clear();
            NBitRead = 0;
            int nsat = ST01.NSat();
            for (int i = 0; i < nsat; i++)
            {
                if (bcb.Count() < 6)
                {
                    Status = SubTypeStatus.NeedMoreData;
                    DebugDisp();
                    return Status;
                }
                var ura = new ST07DataEachSat();
                ura.Class = (int)bcb.GetUint(3);
                ura.Value = (int)bcb.GetUint(3);
                NBitRead += 6;
                Data.Sat.Add(ura);
            }
            Status = SubTypeStatus.DataDone;
            DebugDisp();
            return Status;
        }


        private void DebugDisp()
        {
#if DEBUG
            switch (Status)
            {
                case SubTypeStatus.NeedMoreData:
                    Debug.WriteLine($"Need more data, {NBitRead} bits read, number of sats read: {Data.Sat.Count}");
                    break;
                case SubTypeStatus.InvalidData:
                    Debug.WriteLine($"Invalid data, {NBitRead} bits read");
                    break;
                case SubTypeStatus.DataDone:
                    Debug.WriteLine($"{Data.MessageNumber:0000} type={Data.ID}, Decode done  ({NBitRead} bits read)");
                    if(TraceSwitch.Level == TraceLevel.Verbose) 
                    {
                        if (ST01 == null)
                        {
                            Debug.WriteLine("no mask data");
                            break;
                        }
                        StringBuilder sb = new StringBuilder();
                        int nsat = ST01.NSat();
                        for(int i = 0; i < nsat; i++)
                        {
                            var sat = ST01.Data.Sat[i];
                            var dat = Data.Sat[i];
                            sb.Append($"    sysid={sat.GnssID}  svn={sat.SvN:d2}, class:{dat.Class}, value:{dat.Value}");
                            sb.Append("\n");
                        }
                        Debug.Write(sb.ToString());
                    }
                    break;
            }
#endif
        }

    }


    public class ST07Data : ClasHeaderCommonData
    {
        // no extended headers

        // data
        public List<ST07DataEachSat> Sat { get; set; } = new List<ST07DataEachSat>();
    }

    public class ST07DataEachSat
    {
        public int Class { get; set; }
        public int Value { get; set; }

    }

}
