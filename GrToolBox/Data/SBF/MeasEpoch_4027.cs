using GrToolBox.Common;
using GrToolBox.Data.GrNavData;
using GrToolBox.Data.GrObsData;
using GrToolBox.Satellite;
using System.Diagnostics;
using System.Text;
using static GrToolBox.Data.SBF.SbfDefinitions;
using static GrToolBox.Data.SBF.SbfUtilities;
using static GrToolBox.Settings.SettingsDefinitions;

namespace GrToolBox.Data.SBF
{
    public class MeasEpoch_4027 : Header_TimeStamp, IRawData
    {
        private byte N1 { get; set; }
        private byte SB1Length { get; set; }
        private byte SB2Length { get; set; }
        private sbyte CumClkJumps { get; set; }
        private List<MeasEpochChannelType1> Type1 { get; set; } = new List<MeasEpochChannelType1>();

        public MeasEpoch_4027(byte[] byteData, Header_TimeStamp h)
        {
            Decode(byteData, h);
        }

        public int GetID()
        {
            return 4027;
        }

        public Satellites GetMeas()
        {
            var sats = new Satellites();
            sats.Time = new Time.TimeGR(WNc, (double)Tow);

            foreach (var type1 in Type1)
            {
                var satdata = SbfMeasType12_to_GR(type1);
                if (satdata != null)
                {
                    sats.SatData.Add(satdata);
                }
            }
            return sats;
        }
        public GrNavBase? GetNav()
        {
            return null;   
        }

        private void Decode(byte[] byteData, Header_TimeStamp h)
        {
            int pos = 14;
            byte n1 = byteData[pos]; pos++;
            byte sb1Length = byteData[pos]; pos++;
            byte sb2Length = byteData[pos]; pos++;
            byte commonFlags = byteData[pos]; pos++;

            Sync1 = h.Sync1;
            Sync2 = h.Sync2;
            BlockNum = h.BlockNum;
            BlockRev = h.BlockRev;
            Length = h.Length;
            Tow = h.Tow;
            WNc = h.WNc;

            N1 = n1;
            SB1Length = sb1Length;
            SB2Length = sb2Length;
            if (BlockRev == 1)
            {
                CumClkJumps = (sbyte)byteData[pos]; pos++;
            }
            pos++;  // reserved (u1)

            // type1処理
            for (byte i = 0; i < n1; i++)
            {
                var type1 = Decode_MeasEpochChannelType1(byteData, pos, sb1Length, sb2Length);
                Type1.Add(type1);
                pos += sb1Length + type1.N2 * sb2Length;
            }
            //Sbf_Debug.print(meas);
        }


        private static MeasEpochChannelType1 Decode_MeasEpochChannelType1(byte[] byteData, int pos, byte sb1Length, byte sb2length)
        {
            MeasEpochChannelType1 measType1 = new MeasEpochChannelType1();
            byte rxChannel = byteData[pos]; pos++;
            byte type = byteData[pos]; pos++;
            byte svid = byteData[pos]; pos++;
            byte misc = byteData[pos]; pos++;
            byte[] codeLSB = byteData[pos..(pos + 4)]; pos += 4;
            byte[] doppler = byteData[pos..(pos + 4)]; pos += 4;
            byte[] carrierLSB = byteData[pos..(pos + 2)]; pos += 2;
            byte carrierMSB = byteData[pos]; pos++;
            byte cn0 = byteData[pos]; pos++;
            byte[] locktime = byteData[pos..(pos + 2)]; pos += 2;
            byte obsinfo = byteData[pos]; pos++;
            byte n2 = byteData[pos]; pos++;
            if (sb1Length > 20)
            {
                pos += sb1Length - 20;       // padding bytes
            }

            measType1.RxChannel = rxChannel;     // Receiver channel on which this satellite is currently tracked
            measType1.SigIdxLo = (byte)(type & 0b_00011111);    // obsinfoの内容で書き換えあり
            measType1.AntennaID = (byte)((type & 0b_11100000) >> 5);
            measType1.SVID = svid;
            measType1.CodeMSB = (byte)(misc & 0b_00001111);
            measType1.CodeLSB = BitConverter.ToUInt32(codeLSB);
            int tmp = BitConverter.ToInt32(doppler);
            if (tmp != -2147483648)
            {
                measType1.Doppler = tmp * 0.0001;   // [Hz]
            }
            else
            {
                measType1.Doppler = double.NaN;
            }
            measType1.CarrierLSB = BitConverter.ToUInt16(carrierLSB);
            measType1.CarrierMSB = (sbyte)carrierMSB;
            if (measType1.SigIdxLo == 1 || measType1.SigIdxLo == 2)
            {
                measType1.CN0 = cn0 * 0.25;
            }
            else
            {
                measType1.CN0 = cn0 * 0.25 + 10.0;
            }
            measType1.LockTime = BitConverter.ToUInt16(locktime);
            // based on obsinfo
            measType1.Smoothed = (obsinfo & 0b_00000001) > 0;
            measType1.HalfCycleAmb = (obsinfo & 0b_00000100) > 0;
            measType1.FreqNr = (byte)(obsinfo >> 3);
            if (measType1.SigIdxLo == 31)
            {
                measType1.SigIdxLo = (byte)((obsinfo >> 3) + 32);
            }
            measType1.SigType = (SbfSignalType)measType1.SigIdxLo;
            measType1.RinexObsCode = SignalType2RinexCode(measType1.SigType);
            //
            measType1.N2 = n2;

            // frequency & wave length
            measType1.Frequency = SignalNum2Freq(measType1.SigType, measType1.FreqNr);
            measType1.WaveLength = Common.Constants.CC / measType1.Frequency;

            // pseudorange & carrier phase
            measType1.Pseudorange = (measType1.CodeMSB * 4294967296 + measType1.CodeLSB) * 0.001;
            measType1.CarrierPhase = measType1.Pseudorange / measType1.WaveLength + (measType1.CarrierMSB * 65536 + measType1.CarrierLSB) * 0.001;

            // type2処理

            for (byte i = 0; i < n2; i++)
            {
                measType1.Type2.Add(Decode_MeasEpochChannelType2(byteData, pos, sb2length, measType1));
                pos += sb2length;
            }
            return measType1;
        }


        private static MeasEpochChannelType2 Decode_MeasEpochChannelType2(byte[] byteData, int pos, byte sb2Length, MeasEpochChannelType1 measType1)
        {
            int tmpInt;
            MeasEpochChannelType2 measType2 = new MeasEpochChannelType2();
            byte type = byteData[pos]; pos++;
            byte locktime = byteData[pos]; pos++;
            byte cn0 = byteData[pos]; pos++;
            byte offsetsMSB = byteData[pos]; pos++;
            byte carrierMSB = byteData[pos]; pos++;
            byte obsinfo = byteData[pos]; pos++;
            byte[] codeOffsetLSB = byteData[pos..(pos + 2)]; pos += 2;
            byte[] carrierLSB = byteData[pos..(pos + 2)]; pos += 2;
            byte[] dopplerOffsetLSB = byteData[pos..(pos + 2)]; pos += 2;
            if (sb2Length > 12)
            {
                pos += sb2Length - 12;       // padding bytes
            }

            measType2.SigIdxLo = (byte)(type & 0b_00011111);
            measType2.SigType = (SbfSignalType)measType2.SigIdxLo;
            measType2.RinexObsCode = SignalType2RinexCode(measType2.SigType);
            measType2.AntennaID = (byte)(type >> 5);
            measType2.LockTime = locktime;
            measType2.CN0 = measType2.SigIdxLo == 1 || measType2.SigIdxLo == 2 ? cn0 * 0.25 : cn0 * 0.25 + 10.0;
            tmpInt = offsetsMSB & 0b_00000111;
            measType2.CodeOffsetMSB = tmpInt < 4 ? (sbyte)tmpInt : (sbyte)(tmpInt - 8);       // 3bitデータ2の補数処理
            tmpInt = offsetsMSB >> 3;
            measType2.DopplerOffsetMSB = tmpInt < 16 ? (sbyte)tmpInt : (sbyte)(tmpInt - 16);  // 5bitデータ2の補数処理
            measType2.CarrierMSB = (sbyte)carrierMSB;
            // based on obsinfo
            measType2.Smoothed = (obsinfo & 0b_00000001) > 0;
            measType2.HalfCycleAmb = (obsinfo & 0b_00000100) > 0;
            //
            measType2.CodeOffsetLSB = BitConverter.ToUInt16(codeOffsetLSB);
            measType2.CarrierLSB = BitConverter.ToUInt16(carrierLSB);
            measType2.DopplerOffsetLSB = BitConverter.ToUInt16(dopplerOffsetLSB);

            // frequency & wave length
            measType2.Frequency = SignalNum2Freq(measType2.SigType, measType1.FreqNr);
            measType2.WaveLength = Common.Constants.CC / measType2.Frequency;

            // pseudorange & carrier phase
            measType2.Pseudorange = measType1.Pseudorange + (measType2.CodeOffsetMSB * 65536 + measType2.CodeOffsetLSB) * 0.001;
            measType2.CarrierPhase = measType2.Pseudorange / measType2.WaveLength + (measType2.CarrierMSB * 65536 + measType2.CarrierLSB) * 0.001;

            // Doppler
            double alpha = measType2.Frequency / measType1.Frequency;
            measType2.Doppler = measType1.Doppler * alpha + (measType2.DopplerOffsetMSB * 65536 + measType2.DopplerOffsetLSB) * 1e-4;

            return measType2;
        }

        private static SatelliteData SbfMeasType12_to_GR(MeasEpochChannelType1 type1)
        {
            if (type1.SVID == 62 || (type1.SVID >= 107 && type1.SVID <= 119))
            {
                return null;
            }
            GrPrn prn = new GrPrn(type1.SVID, Data_Type.SBF);
            var satData = new SatelliteData(prn);
            var raw1 = new RawData();
            raw1.Valid_Code = type1.CodeMSB != 0 || type1.CodeLSB != 0;
            raw1.Valid_Phase = type1.CarrierMSB != -128 || type1.CarrierLSB != 0;
            raw1.Valid_Dopper = !double.IsNaN(type1.Doppler);
            raw1.ObsCode = type1.RinexObsCode;
            raw1.Pseudorange = type1.Pseudorange;
            raw1.CarrierPhase = type1.CarrierPhase;
            raw1.Doppler = type1.Doppler;
            raw1.CN0 = type1.CN0;
            raw1.FreqNr = type1.FreqNr;
            raw1.Frequency = type1.Frequency;
            raw1.WabeLength = type1.WaveLength;
            satData.Raws.Add(raw1);
            foreach (MeasEpochChannelType2 type2 in type1.Type2)
            {
                var raw2 = new RawData();
                raw2.Valid_Code = type2.CodeOffsetMSB != -4 || type2.CodeOffsetLSB != 0;
                raw2.Valid_Phase = type2.CarrierMSB != -128 || type2.CarrierLSB != 0;
                raw2.Valid_Dopper = type2.DopplerOffsetMSB != -16 || type2.DopplerOffsetLSB != 0;
                raw2.ObsCode = type2.RinexObsCode;
                raw2.Pseudorange = type2.Pseudorange;
                raw2.CarrierPhase = type2.CarrierPhase;
                raw2.Doppler = type2.Doppler;
                raw2.CN0 = type2.CN0;
                raw2.FreqNr = type1.FreqNr;             // 要らない？
                raw2.Frequency = type2.Frequency;
                raw2.WabeLength = type2.WaveLength;
                satData.Raws.Add(raw2);
            }
            return satData;
        }

        public void PrintTo(StringBuilder sb)
        {
            sb.Append("------------------------------------------\n");
            sb.Append($"SBF Block 4027 ({Length} bytes)\n");
            sb.Append($" TOW: {Tow:.000}, Wnc: {WNc}, Nested Type1 Data: {N1}\n");
            foreach (MeasEpochChannelType1 t1 in Type1)
            {
                sb.Append($"  *SVID: {t1.SVID,3} ({SVID2SNN(t1.SVID),4}), RxChannel: {t1.RxChannel,2}, SigIdxLo: {t1.SigIdxLo,2} ({(SbfSignalType)t1.SigIdxLo,6}), Nested Type 2 Data: {t1.N2}\n");
                sb.Append($"     -SigIdxLo: {t1.SigIdxLo,2} ({(SbfSignalType)t1.SigIdxLo,6}), P: {t1.Pseudorange,12:.000}, D: {t1.Doppler,9:.000}, L: {t1.CarrierPhase,12:.000}, C/N0: {t1.CN0,5:.00}\n");
                foreach (MeasEpochChannelType2 t2 in t1.Type2)
                {
                    //sb.Append($"     -SigIdxLo: {t2.SigIdxLo,2} ({(SbfSignalType)t2.SigIdxLo,6}), P: {t2.Pseudorange,12:.000}, D: {t2.Doppler,9:.000}, L: {t2.CarrierPhase,12:.000}, C/N0: {t2.CN0,5:.00}\n");
                }
            }

        }

        public void DebugPrint_Content()
        {
            StringBuilder sb = new StringBuilder();
            PrintTo(sb);
            //var sb = new StringBuilder();
            //sb.Append("------------------------------------------\n");
            //sb.Append($"SBF Block 4027 ({Length} bytes)\n");
            //sb.Append($" TOW: {Tow:.000}, Wnc: {WNc}, Nested Type1 Data: {N1}\n");
            //foreach (MeasEpochChannelType1 t1 in Type1)
            //{
            //    sb.Append($"  *SVID: {t1.SVID,3} ({SVID2SNN(t1.SVID),4}), RxChannel: {t1.RxChannel,2}, SigIdxLo: {t1.SigIdxLo,2} ({(SbfSignalType)t1.SigIdxLo,6}), Nested Type 2 Data: {t1.N2}\n");
            //    sb.Append($"     -SigIdxLo: {t1.SigIdxLo,2} ({(SbfSignalType)t1.SigIdxLo,6}), P: {t1.Pseudorange,12:.000}, D: {t1.Doppler,9:.000}, L: {t1.CarrierPhase,12:.000}, C/N0: {t1.CN0,5:.00}\n");
            //    foreach (MeasEpochChannelType2 t2 in t1.Type2)
            //    {
            //        //sb.Append($"     -SigIdxLo: {t2.SigIdxLo,2} ({(SbfSignalType)t2.SigIdxLo,6}), P: {t2.Pseudorange,12:.000}, D: {t2.Doppler,9:.000}, L: {t2.CarrierPhase,12:.000}, C/N0: {t2.CN0,5:.00}\n");
            //    }
            //}
            Debug.WriteLine(sb.ToString());
        }


    }

    public class MeasEpochChannelType1
    {
        // defined in SBF
        public byte RxChannel { get; set; }     // Receiver channel on which this satellite is currently tracked
        public byte SigIdxLo { get; set; }
        public byte AntennaID { get; set; }
        public byte SVID { get; set; }
        public byte CodeMSB { get; set; }
        public uint CodeLSB { get; set; }
        public double Doppler { get; set; }            // Hz
        public ushort CarrierLSB { get; set; }
        public sbyte CarrierMSB { get; set; }
        public double CN0 { get; set; }
        public ushort LockTime { get; set; }
        public bool Smoothed { get; set; }
        public bool HalfCycleAmb { get; set; }
        public byte FreqNr { get; set; }
        public byte N2 { get; set; }
        public List<MeasEpochChannelType2> Type2 { get; set; } = new List<MeasEpochChannelType2>();     // nested type2

        // defined by user
        public SbfSignalType SigType { get; set; }
        public string RinexObsCode { get; set; }
        public double Pseudorange { get; set; }
        public double CarrierPhase { get; set; }
        public double Frequency { get; set; }
        public double WaveLength { get; set; }
    }

    public class MeasEpochChannelType2
    {
        // defined in SBF
        public byte SigIdxLo { get; set; }
        public byte AntennaID { get; set; }
        public byte LockTime { get; set; }
        public double CN0 { get; set; }
        public sbyte CodeOffsetMSB { get; set; }
        public sbyte DopplerOffsetMSB { get; set; }
        public sbyte CarrierMSB { get; set; }
        public bool Smoothed { get; set; }
        public bool HalfCycleAmb { get; set; }
        public ushort CodeOffsetLSB { get; set; }
        public ushort CarrierLSB { get; set; }
        public ushort DopplerOffsetLSB { get; set; }

        // defined by user
        public SbfSignalType SigType { get; set; }
        public string RinexObsCode { get; set; }
        public double Pseudorange { get; set; }
        public double CarrierPhase { get; set; }
        public double Doppler { get; set; }
        public double Frequency { get; set; }
        public double WaveLength { get; set; }
    }
}
