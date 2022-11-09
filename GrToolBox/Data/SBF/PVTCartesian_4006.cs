namespace GrToolBox.Data.SBF
{
    public class PVTCartesian_4006 : Header_TimeStamp
    {
        public byte Mode { get; set; }          // PVT Mode; see SBF Reference
        public byte Error { get; set; }         // PVT error code; see SBF Reference
        public double X { get; set; }           // X coordinate in coordinate frame specified by Datum
        public double Y { get; set; }
        public double Z { get; set; }
        public float Undulation { get; set; }   // Geoid undulation. See the setGeoidUndulation command
        public float Vx { get; set; }           // Velocity in the X direction
        public float Vy { get; set; }
        public float Vz { get; set; }
        public float Cog { get; set; }          // Course over ground
        public double RxClkBias { get; set; }   // Receiver clock bias relative to the GNSS system time reported in the TimeSystem field
        public float RxClkDrift { get; set; }   // Receiver clock drift relative to the GNSS system time (relative frequency error)
        public byte TimeSystem { get; set; }    // Time system of which the offset is provided in this sub-block(0:G, 1:E, 3:R, 4:C)
        public byte Datum { get; set; }         // This field defines in which datum the coordinates are expressed
        public byte NrSV { get; set; }          // Total number of satellites used in the PVT computation
        public byte WACorrInfo { get; set; }    // Bit field providing information about which wide area corrections have been applied
        public ushort ReferenceID { get; set; } // This field indicates the reference ID of the differential information used
        public double MeanCorrAge { get; set; } // This field indicates the reference ID of the differential information used
        public uint SignalInfo { get; set; }    // Bit field indicating the type of GNSS signals having been used in the PVT computations
        public byte AlertFlag { get; set; }     // Bit field indicating integrity related information
        public byte NrBases { get; set; }       // (Rev 1) Number of base stations used in the PVT computation 
        public ushort PPPInfo { get; set; }     // (Rev 1) Bit field containing PPP-related information
        public double Latency { get; set; }     // (Rev 2) Time elapsed between the time of applicability of the position fix and the generation of this SBF block by the receiver
        public double HAccuracy { get; set; }   // (Rev 2) 2DRMS horizontal accuracy
        public double VAccuracy { get; set; }   // (Rev 2) 2-sigma vertical accuracy
        public byte Misc { get; set; }          // (Rev 2) Bit field containing miscellaneous flags

        public PVTCartesian_4006() { }
        public PVTCartesian_4006(byte[] byteData, Header_TimeStamp h)
        {
            Decode(byteData, h);
        }


        private void Decode(byte[] byteData, Header_TimeStamp h)
        {
            int pos = 14;
            byte mode = byteData[pos]; pos++;
            byte error = byteData[pos]; pos++;
            byte[] x = byteData[pos..(pos + 8)]; pos += 8;
            byte[] y = byteData[pos..(pos + 8)]; pos += 8;
            byte[] z = byteData[pos..(pos + 8)]; pos += 8;
            byte[] undulation = byteData[pos..(pos + 4)]; pos += 4;
            byte[] vx = byteData[pos..(pos + 4)]; pos += 4;
            byte[] vy = byteData[pos..(pos + 4)]; pos += 4;
            byte[] vz = byteData[pos..(pos + 4)]; pos += 4;
            byte[] cog = byteData[pos..(pos + 4)]; pos += 4;
            byte[] rxClkBias = byteData[pos..(pos + 8)]; pos += 8;
            byte[] rxClkDrift = byteData[pos..(pos + 4)]; pos += 4;
            byte timeSystem = byteData[pos]; pos++;
            byte datum = byteData[pos]; pos++;
            byte nrSv = byteData[pos]; pos++;
            byte waCorrInfo = byteData[pos]; pos++;
            byte[] referenceID = byteData[pos..(pos + 2)]; pos += 2;
            byte[] meanCorrAge = byteData[pos..(pos + 2)]; pos += 2;
            byte[] signalInfo = byteData[pos..(pos + 4)]; pos += 4;
            byte alertFlag = byteData[pos]; pos++;
            byte nrBases = 0;
            byte[] pppInfo = new byte[2] { 0, 0 };
            if (h.BlockRev == 1)
            {
                nrBases = byteData[pos]; pos++;
                pppInfo = byteData[pos..(pos + 2)]; pos += 2;
            }
            byte[] latency = new byte[2] { 0xff, 0xff };
            byte[] hAccuracy = new byte[2] { 0xff, 0xff }; ;
            byte[] vAccuracy = new byte[2] { 0xff, 0xff }; ;
            byte misc = 0;
            if (h.BlockRev == 2)
            {
                latency = byteData[pos..(pos + 2)]; pos += 2;
                hAccuracy = byteData[pos..(pos + 2)]; pos += 2;
                vAccuracy = byteData[pos..(pos + 2)]; pos += 2;
                misc = byteData[pos]; pos++;
            }

            // Decode header
            Sync1 = h.Sync1;
            Sync2 = h.Sync2;
            BlockNum = h.BlockNum;
            BlockRev = h.BlockRev;
            Length = h.Length;
            Tow = h.Tow;
            WNc = h.WNc;

            // Decode block
            Mode = mode;
            Error = error;
            X = BitConverter.ToDouble(x);
            Y = BitConverter.ToDouble(y);
            Z = BitConverter.ToDouble(z);
            Undulation = BitConverter.ToSingle(undulation);
            Vx = BitConverter.ToSingle(vx);
            Vy = BitConverter.ToSingle(vy);
            Vz = BitConverter.ToSingle(vz);
            Cog = BitConverter.ToSingle(cog);
            RxClkBias = BitConverter.ToDouble(rxClkBias);
            RxClkDrift = BitConverter.ToSingle(rxClkDrift);
            TimeSystem = timeSystem;
            Datum = datum;
            NrSV = nrSv;
            WACorrInfo = waCorrInfo;
            ReferenceID = BitConverter.ToUInt16(referenceID);
            MeanCorrAge = BitConverter.ToUInt16(meanCorrAge) * 0.01;
            SignalInfo = BitConverter.ToUInt32(signalInfo);
            AlertFlag = alertFlag;
            if (BlockNum == 1)
            {
                NrBases = nrBases;
                PPPInfo = BitConverter.ToUInt16(pppInfo);
            }
            if (BlockNum == 2)
            {
                Latency = BitConverter.ToUInt16(latency) * 0.0001;
                HAccuracy = BitConverter.ToUInt16(hAccuracy) * 0.01;
                VAccuracy = BitConverter.ToUInt16(vAccuracy) * 0.01;
                Misc = misc;
            }
        }
    }
}

