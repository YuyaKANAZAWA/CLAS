using GrToolBox.Common;
using GrToolBox.Data.GrNavData;
using GrToolBox.Data.GrObsData;
using GrToolBox.Satellite;
using System.Diagnostics;
using System.Text;
using static GrToolBox.Common.CommonDefinitions;
using static GrToolBox.Data.DataUtilities;
using static GrToolBox.Data.RTCM.RTCM3.Rtcm3Utilities;

namespace GrToolBox.Data.RTCM.RTCM3
{
    /// <summary>
    /// RTCM3 1009-1012
    /// </summary>
    public class GloRtkObs : FrameInfo_MessageNum, IRawData
    {
        private int RefStId { get; set; }                                   // uint12; Reference Station ID
        private int Tow { get; set; }                                       // uint27; GLONASS Epoch Time(tk)
        private bool SyncGnssFlag { get; set; }                             // bit(1); Syncronous GNSS Flag
        private int NumSat { get; set; }                                    // uint5; No. of GLONASS Satellite Signals Processed
        private bool SmoothingIndicator { get; set; }                       // bit(1); GLONASS Divergence-free Smoothing Indicator
        private int SmoothingInterval { get; set; }                         // bit(3); GLONASS Smoothing Interval
        private List<EachSat> EachSats { get; set; } = new List<EachSat>();

        private class EachSat
        {
            protected internal int SatID { get; set; }                      // uint6; GLONASS satellite ID(Satellite Slot Number)
            protected internal bool L1Ind { get; set; }                     // bit(1); GLONASS Code Indicator
            protected internal int FreqChNum { get; set; }                  // uint5; GLONASS Satellite Frequency Channel Number
            protected internal int L1Pr { get; set; }                       // uint25; GLONASS L1 Pseudorange
            protected internal int L1Ph_Pr { get; set; }                    // int20; GLONASS L1 Phaserange - L1 Pseudorange
            protected internal int L1LockTime { get; set; }                 // uint7; GLONASS L1 Lock time Indicator 
            protected internal int L1Pr_Mod_Amb { get; set; }               // uint7; GLONASS Integer L1 Pseudorange Modulus Ambiguity
            protected internal int L1Cnr { get; set; }                      // uint8; GLONASS L1 CNR
            protected internal int L2Ind { get; set; }                      // bit(2); GLONASS L2 Code Indicator
            protected internal int L2_L1_PrDiff { get; set; }               // int14; GLONASS L2 - L1 Pseudorange Difference
            protected internal int L2Ph_L1Pr { get; set; }                  // int20; GLONASS L2 Phaserange - L1 Pseudorange
            protected internal int L2LockTime { get; set; }                 // uint7; GLONASS L2 Lock time Indicatior
            protected internal int L2Cnr { get; set; }                      // uint8; GLONASS L2 CNR
        }

        /// <summary>
        /// RTCM3 1009-1012
        /// </summary>
        public GloRtkObs() { }

        /// <summary>
        /// RTCM3 1009-1012
        /// </summary>
        /// <param name="byteData">byte array of sentence data</param>
        /// <param name="f">instance of FrameInfo_MessageNum</param>
        public GloRtkObs(byte[] byteData, FrameInfo_MessageNum f)
        {
            Decode(byteData, f);
        }

        public int GetID()
        {
            return MessageNum;
        }

        public Satellites GetMeas()
        {
            var sats = new Satellites();

            // TOW[s], RTCMの観測データセンテンスではWnがわからないので，読み込み時はsats.TowにTOW[sec]を格納する
            // 他の情報（ephemeris，手入力，PC時計など）からWnを得て，測位演算の前にTimeを確定させる                                                            
            sats.Tow = (double)Tow / 1000.0;
            foreach(var eachSat in EachSats)
            {
                var satdata = Rtcm_GloObsEach_to_GR(eachSat, MessageNum);
                sats.SatData.Add(satdata);
            }
            return sats;
        }

        public GrNavBase? GetNav()
        {
            return null;   //
        }


        private void Decode(byte[] byteData, FrameInfo_MessageNum f)
        {
            MessageLen = f.MessageLen;
            MessageNum = f.MessageNum;

            int offset = 24;                                            // frame header: 3byte=24bit, skip
            offset += 12;                                               // Message Number, uint12, DF002, skip
            RefStId = (int)BitToUint(byteData, offset, 12);             // uint12, DF003; Reference Station ID
            offset += 12;
            Tow = (int)BitToUint(byteData, offset, 27);                 // uint27, DF034; GLONASS Epoch Time(tk), [ms] in Glonass time frame
            offset += 27;
            SyncGnssFlag = BitToBool(byteData, offset);                 // bit(1), DF005; Syncronous GNSS Flag
            offset++;                                                   // true:同エポックにさらにデータあり（次のフレームを待つ）, false:同エポックにこれ以上データ無し．測位演算開始してよい
            NumSat = (int)BitToUint(byteData, offset, 5);               // uint5, DF035; このメッセージでの衛星数
            offset += 5;
            SmoothingIndicator = BitToBool(byteData, offset);           // bit(1), DF036; Divergence-free Smoothing Indicator, true:ON, false:OFF
            offset++;
            SmoothingInterval = (int)BitToUint(byteData, offset, 3);    // bit(3), DF037; GLONASS Smoothing Interval, 
            offset += 3;

            for (int i = 0; i < NumSat; i++)
            {
                EachSat eachSat = new ();
                EachSats.Add(eachSat);
                eachSat.SatID = (int)BitToUint(byteData, offset, 6);            // uint6, DF038; GLONASS Satellite ID(1--24: slot number, 0: slot number unknown, >32: reserved for sbas),
                                                                                // for GLONASS-M, slot num is provided in navigation message
                offset += 6;
                eachSat.L1Ind = BitToBool(byteData, offset);                    // bit(1), DF039; GLONASS L1 Code Indicator, false:C/A, true:P
                offset++;
                eachSat.FreqChNum = (int)BitToUint(byteData, offset, 5);        // uint5, DF040; GLONASS satellite frequency channel number
                offset += 5;
                eachSat.L1Pr = (int)BitToUint(byteData, offset, 25);            // uint25, DF041; GLONASS L1 Pseudorange [0.02m]
                offset += 25;
                eachSat.L1Ph_Pr = (int)BitToInt(byteData, offset, 20);          // int20, DF042; GLONASS L1 Phaserange - L1 Pseudorange [0.0005m], DF012  -->> Roll overの記述あり．未実装　注意！！
                offset += 20;
                eachSat.L1LockTime = (int)BitToUint(byteData, offset, 7);       // uint7, DF043; GLONASS L1 Lock time Indicator
                offset += 7;

                if (MessageNum == 1010 || MessageNum == 1012)
                {
                    eachSat.L1Pr_Mod_Amb = (int)BitToUint(byteData, offset, 7); // uint7, DF044; GLONASS Integer L1 Pseudorange Modulus Ambiguity
                    offset += 7;                    
                    eachSat.L1Cnr = (int)BitToUint(byteData, offset, 8);        // uint8, DF045; GLONASS L1 CNR [0.25dB-Hz]
                    offset += 8;
                }
                if (MessageNum == 1011 || MessageNum == 1012)
                {                    
                    eachSat.L2Ind = (int)BitToUint(byteData, offset, 2);        // bit(2), DF046; GLONASS L2 Code Indicator, 0:C/A, 1:P, 2 and 3: reserved
                    offset += 2;                    
                    eachSat.L2_L1_PrDiff = BitToInt(byteData, offset, 14);      // int14, DF0047; GLONASS L2 - L1 Pseudorange Difference [0.02m]
                    offset += 14;                    
                    eachSat.L2Ph_L1Pr = BitToInt(byteData, offset, 20);         // int20, DF048; GPS L2 Phaserange - L1 Pseudorange [0.0005m]
                    offset += 20;                    
                    eachSat.L2LockTime = (int)BitToUint(byteData, offset, 7);   // uint7, DF049; GPS L2 Lock time Indicatior
                    offset += 7;
                }
                if (MessageNum == 1012)
                {                    
                    eachSat.L2Cnr = (int)BitToUint(byteData, offset, 8);        // uint8, DF050; GLONASS L2 CNR [0.25dB-Hz]
                    offset += 8;
                }
            }
        }

        private static SatelliteData Rtcm_GloObsEach_to_GR(EachSat s, int messageNum)
        {
            GrPrn prn = new GrPrn(SATID2SNN(s.SatID, SYS.GLO));
            var satData = new SatelliteData(prn);
            
            // L1データ格納
            var raw1 = new RawData();

            double decL1Pr = s.L1Pr * 0.02;                         // DF041; 
            double decL1Pr_Mod_Amb = s.L1Pr_Mod_Amb * 599584.916;   // DF044
            raw1.Valid_Code = (s.L1Pr >= 0) && (s.L1Pr <= 29979246) 
                                && (s.L1Pr_Mod_Amb >= 0) && (s.L1Pr_Mod_Amb <= 127);     // DF041とDF044の範囲チェック

            double decL1Ph_Pr = s.L1Ph_Pr * 0.0005;                 // DF042
            bool Valid_L1Ph_Pr = (s.L1Ph_Pr != -524288);            // DF042; invalid when the value is 0x80000(int20) = only MSB is true = (2^20)/2 * (-1) = -524288
            raw1.Valid_Phase = raw1.Valid_Code
                                && Valid_L1Ph_Pr
                                && (decL1Ph_Pr >= -262.1435) && (decL1Ph_Pr <= 262.1435);   // これは冗長

            raw1.Valid_Dopper = false;

            if (s.L1Ind)
            {
                raw1.ObsCode = "1P";
            }
            else
            {
                raw1.ObsCode = "1C";
            }

            raw1.FreqNr = s.FreqChNum - 7;  // -7..13
            raw1.Frequency = RtcmFreqNum_TO_Freq_L1(raw1.FreqNr);
            raw1.WabeLength = RtcmFreqNum_TO_Length_L1(raw1.FreqNr);

            raw1.Pseudorange = decL1Pr + decL1Pr_Mod_Amb;
            raw1.CarrierPhase = (raw1.Pseudorange + decL1Ph_Pr) / raw1.WabeLength;
            raw1.Doppler = double.NaN;
            raw1.CN0 = (s.L1Cnr == 0) ? double.NaN : s.L1Cnr * 0.25;  // DF015; when 0, CNR is not provided
            satData.Raws.Add(raw1);

            if (messageNum == 1009 || messageNum == 1010)
            {
                return satData;
            }

            // L2データ格納; for 1011 or 1012
            var raw2 = new RawData();
            double decL2_L1_PrDiff = s.L2_L1_PrDiff * 0.02;
            bool Valid_L2_L1_PrDiff = (s.L2_L1_PrDiff >= -163.82) && (s.L2_L1_PrDiff <= 163.82);    // DF047; 0x2000 の判定は，範囲の判定で包含できている
            raw2.Valid_Code = Valid_L2_L1_PrDiff && raw1.Valid_Code;

            double decL2Ph_L1Pr = s.L2Ph_L1Pr * 0.0005;
            bool Valid_L2Ph_L1Pr = (s.L2Ph_L1Pr != -524288);    // DF048; invalid when the value is 0x80000(int20) = only MSB is true = (2^20)/2 * (-1) = -524288
            raw2.Valid_Phase = Valid_L2Ph_L1Pr && raw1.Valid_Code;

            raw2.Valid_Dopper = false;

            switch (s.L2Ind)
            {
                case 0:
                    raw2.ObsCode = "2C";
                    break;
                case 1:
                    raw2.ObsCode = "2P";
                    break;
            }

            raw2.FreqNr = s.FreqChNum;
            raw2.Frequency = RtcmFreqNum_TO_Freq_L2(raw2.FreqNr);
            raw2.WabeLength = RtcmFreqNum_TO_Length_L2(raw2.FreqNr);

            raw2.Pseudorange = raw1.Pseudorange + decL2_L1_PrDiff;
            raw2.CarrierPhase = (raw2.Pseudorange + decL2Ph_L1Pr) / raw2.WabeLength;
            raw2.Doppler = double.NaN;
            raw2.CN0 = (s.L2Cnr == 0) ? double.NaN : s.L2Cnr * 0.25;  // DF015; when 0, CNR is not provided
            satData.Raws.Add(raw1);

            return satData;
        }

        public new void PrintTo(StringBuilder sb)
        {
            sb.Append("messageNum:     " + MessageNum + "\n");
            sb.Append("refStID:        " + RefStId + "\n");
            sb.Append("TOW:            " + (Tow / 1000.0) + "\n");
            sb.Append("syncGnssFlag:   " + SyncGnssFlag + "\n");
            sb.Append("NumSat:         " + NumSat + "\n");
            sb.Append("smoothInd:      " + SmoothingIndicator + "\n");
            sb.Append("smoothInterval: " + SmoothingInterval + "\n");
            for (int i = 0; i < NumSat; i++)
            {
                string ind = "[" + (i + 1) + "] ";
                sb.Append(ind + "GloSatID:      " + EachSats[i].SatID + "\n");
                sb.Append(ind + "L1Indicator:   " + EachSats[i].L1Ind + "\n");
                sb.Append(ind + "Freq_Ch_Num:   " + EachSats[i].FreqChNum + "\n");
                sb.Append(ind + "L1Pseudorange: " + EachSats[i].L1Pr + "\n");
                sb.Append(ind + "L1Ph-Pr:       " + EachSats[i].L1Ph_Pr + "\n");
                sb.Append(ind + "L1LockTime:    " + EachSats[i].L1LockTime + "\n");
                sb.Append(ind + "L1Pr_Mod_Amb:  " + EachSats[i].L1Pr_Mod_Amb + "\n");
                sb.Append(ind + "L1Cnr:         " + EachSats[i].L1Cnr + "\n");
                sb.Append(ind + "L2Indicator:   " + EachSats[i].L2Ind + "\n");
                sb.Append(ind + "L2-L1 Pr Diff: " + EachSats[i].L2_L1_PrDiff + "\n");
                sb.Append(ind + "L2Ph-L1Pr:     " + EachSats[i].L2Ph_L1Pr + "\n");
                sb.Append(ind + "L2LockTime:    " + EachSats[i].L2LockTime + "\n");
                sb.Append(ind + "L2Cnr:         " + EachSats[i].L2Cnr + "\n");
                double fullPseudorange = (EachSats[i].L1Pr * 0.02) + (EachSats[i].L1Pr_Mod_Amb * 599584.916);
                double fullPhaserange = (fullPseudorange + EachSats[i].L1Ph_Pr * 0.0005) / RtcmFreqNum_TO_Length_L1(EachSats[i].FreqChNum);
                sb.Append(string.Format("   Calc L1 Pr:       {0:F3}\n", fullPseudorange));
                sb.Append(string.Format("   Calc L1 Ph:       {0:F3}\n", fullPhaserange));
                if(MessageNum >= 1011)
                {
                    double fullPseudorange2 = fullPseudorange + EachSats[i].L2_L1_PrDiff * 0.02;
                    fullPhaserange = (fullPseudorange + EachSats[i].L2Ph_L1Pr * 0.0005) / RtcmFreqNum_TO_Length_L2(EachSats[i].FreqChNum);
                    sb.Append(string.Format("   Calc L2 Pr:       {0:F3}\n", fullPseudorange2));
                    sb.Append(string.Format("   Calc L2 Ph:       {0:F3}\n", fullPhaserange));
                }

                sb.Append("-------------------------------------------------\n");


            }
        }

        public new void DebugPrint()
        {
            StringBuilder sb = new StringBuilder();
            PrintTo(sb);
            Debug.WriteLine(sb); 
        }
    }
}
