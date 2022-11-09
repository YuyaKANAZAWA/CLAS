using System.Diagnostics;
using System.Text;
using static GrToolBox.Data.CLAS3.ClasDefinitions;

namespace GrToolBox.Data.CLAS3
{
    public class ST10_ServiceInfo
    {
        public ST10Data Data { get; set; } = new ST10Data();
        public SubTypeStatus Status { get; set; } = SubTypeStatus.HeaderDone;
        private ST01_Mask? ST01 { get; set; }
        private int NBitRead { get; set; } = 0;      // 処理したビット数
        private byte AuxFrameFlags { get; set; } = 0b_0000_0000;

        internal TraceSwitch TraceSwitch { get; set; } = new TraceSwitch("TSwitch_CLAS_ST7", "Clas Subtype7");

        public ST10_ServiceInfo()
        {
            TraceSwitch.Level = TraceLevel.Verbose;
        }

        private void AddHeaderData(ClasHeaderCommonData h)
        {
            Data.MessageNumber = h.MessageNumber;
            Data.ID = h.ID;
            //Data.Time = h.Time;                       // not in ST10
            //Data.UpdateInterval = h.UpdateInterval;   // not in ST10
            Data.Multiple = h.Multiple;
            //Data.IodSsr = h.IodSsr;                   // not in ST10
        }

        public SubTypeStatus Decode(BitCircularBuffer bcb, ClasHeaderCommonData h, ST01_Mask st01)
        {
            if (bcb == null || h == null || st01 == null)
            {
                throw new ArgumentNullException();
            }
            ST01 = st01;
            AddHeaderData(h);

            if (bcb.Count() < 5)
            {
                Status = SubTypeStatus.NeedMoreData;
                DebugDisp();
                return Status;
            }
            Data.InfoMesCounter = (int)bcb.GetUint(3);
            Data.DataSize = (int)bcb.GetUint(2);
            Status = SubTypeStatus.HeaderDone;
            NBitRead = 0;

            if (Data.InfoMesCounter == 0)
            {
                Data.AuxFrameDatas = new byte[8][];     // リセット
                Data.Packets.Clear();                   // Service Information Packetsを残すならクリアしない
                AuxFrameFlags = 0;
            }

            int nbitsAuxFrame = 40 * (Data.DataSize + 1);
            if (bcb.Count() < nbitsAuxFrame)
            {
                Status = SubTypeStatus.NeedMoreData;
                DebugDisp();
                return Status;
            }

            Data.AuxFrameDatas[Data.InfoMesCounter] = bcb.GetByteArray(nbitsAuxFrame);
            AuxFrameFlags |= (byte)(0b_0000_0001 << Data.InfoMesCounter);
            NBitRead += nbitsAuxFrame;

            if (!Data.Multiple)     // last frame of Aux Frame Data, この時点のInfoCounter値が，そのPacketのAuxFrame数
            {
                // 全部揃っているか確認して，必要な処理を実装する．データ内容はTBD(is-qzss-l6-004)
                byte shouldHave = 0;
                for(int i = 0; i < Data.InfoMesCounter + 1; i++)
                {
                    shouldHave |= (byte)(0b_0000_0001 << i);
                }
                if (AuxFrameFlags == shouldHave)
                {
                    // AuxFrames全部揃ってる
                    var packet = new ST10ServiceInfoPacket();
                    var circ = new BitCircularBuffer(8);
                    int nbits = 0;
                    foreach(var b in Data.AuxFrameDatas)
                    {
                        if(b != null)
                        {
                            circ.AddByteArray(b);
                            nbits += 8 * b.Length;
                        }
                    }
                    packet.ServiceInfoType = (int)circ.GetUint(4);
                    packet.IodServiceInfo = (int)circ.GetUint(3);
                    packet.PacketData.AddRange(circ.GetByteArray(nbits - 7));
                    Data.Packets.Add(packet);
                }
                else
                {
                    // 揃っていない
                }
            }
            else
            {

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
                    Debug.WriteLine($"Need more data, {NBitRead} bits read");
                    break;
                case SubTypeStatus.InvalidData:
                    Debug.WriteLine($"Invalid data, {NBitRead} bits read");
                    break;
                case SubTypeStatus.DataDone:
                    Debug.WriteLine($"{Data.MessageNumber:0000} type={Data.ID}, Decode done  ({NBitRead} bits read)");
                    if(TraceSwitch.Level == TraceLevel.Verbose) 
                    {
                        StringBuilder sb = new StringBuilder();
                        sb.Append($"    ServiceInfoType={Data.Packets[0].ServiceInfoType}, IODServiceInfo={Data.Packets[0].IodServiceInfo}\n");
                        int count = 0;
                        sb.Append("    ");
                        foreach(var d in Data.Packets[0].PacketData)
                        {
                            sb.Append($"{d:x} ");
                            count++;
                            if(count == 16)
                            {
                                sb.Append("\n    ");
                            }
                        }
                        Debug.Write(sb.ToString());
                    }
                    break;
            }
#endif
        }

    }


    public class ST10Data : ClasHeaderCommonData
    {
        // extended headers
        public int InfoMesCounter { get; set; } = 0;
        public int DataSize { get; set; } = 0;

        // data
        public byte[][] AuxFrameDatas { get; set; } = new byte[8][];
        public List<ST10ServiceInfoPacket> Packets { get; set; } = new List<ST10ServiceInfoPacket>();
    }

    public class ST10ServiceInfoPacket
    {
        public int ServiceInfoType { get; set; }
        public int IodServiceInfo { get; set; } 
        public List<byte> PacketData { get; set; } = new List<byte>();
    }

}
