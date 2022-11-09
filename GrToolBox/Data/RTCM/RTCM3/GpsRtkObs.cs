using GrToolBox.Common;
using GrToolBox.Data.GrNavData;
using GrToolBox.Data.GrObsData;
using GrToolBox.Satellite;
using System.Diagnostics;
using System.Text;
using static GrToolBox.Common.CommonDefinitions;
using static GrToolBox.Data.DataUtilities;
using static GrToolBox.Data.RTCM.RTCM3.Rtcm3Utilities;
using static GrToolBox.Satellite.SatelliteConstants;

namespace GrToolBox.Data.RTCM.RTCM3
{
    /// <summary>
    /// RTCM3 1001-1004
    /// </summary>
    public class GpsRtkObs : FrameInfo_MessageNum, IRawData
    {
        //internal int MessageNum { get; set; }             // Message Number
        private int RefStId { get; set; }                // Reference Station ID
        private int Tow { get; set; }                    // GPS Epoch Time(TOW), GLONASS Epoch Time(tk)
        private bool SyncGnssFlag { get; set; }          // Syncronous GNSS Flag
        private int NumSat { get; set; }                 // No. of GPS/GLONASS Satellite Signals Processed
        private bool SmoothingIndicator { get; set; }    // GPS/GLONASS Divergence-free Smoothing Indicator
        private int SmoothingInterval { get; set; }      // GPS/GLONASS Smoothing Interval
        private List<EachSat> EachSats { get; set; } = new List<EachSat>();

        private class EachSat
        {
            protected internal int SatID { get; set; }                            // GPS Satellite ID, GLONASS(Satellite Slot Number)
            protected internal bool L1Ind { get; set; }                           // GPS/GLONASS L1 Code Indicator
            protected internal int L1Pr { get; set; }                          // GPS/GLONASS L1 Pseudorange, uint24
            protected internal int L1Ph_Pr { get; set; }                       // GPS/GLONASS L1 Phaserange - L1 Pseudorange, int20
            protected internal int L1LockTime { get; set; }                       // GPS/GLONASS L1 Lock time Indicator, uint7
            protected internal int L1Pr_Mod_Amb { get; set; }                  // GPS/GLONASS Integer L1 Pseudorange Modulus Ambiguity, uint8
            protected internal int L1Cnr { get; set; }                          // GPS/GLONASS L1 CNR, uint8
            protected internal int L2Ind { get; set; }                            // GPS/GLONASS L2 Code Indicator, bit2
            protected internal int L2_L1_PrDiff { get; set; }                  // GPS/GLONASS L2 - L1 Pseudorange Difference, int14
            protected internal int L2Ph_L1Pr { get; set; }                     // GPS/GLONASS L2 Phaserange - L1 Pseudorange, int20
            protected internal int L2LockTime { get; set; }                       // GPS/GLONASS L2 Lock time Indicatior, uint7
            protected internal int L2Cnr { get; set; }                          // GPS/GLONASS L2 CNR, uint8
        }

        /// <summary>
        /// RTCM3 1001-1004
        /// </summary>
        public GpsRtkObs() { }

        /// <summary>
        /// RTCM3 1001-1004
        /// </summary>
        /// <param name="byteData">byte array of sentence data</param>
        /// <param name="num">Message number(1001-1004)</param>
        public GpsRtkObs(byte[] byteData, FrameInfo_MessageNum f)
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
            sats.Tow = Tow / 1000.0;
            foreach(var eachSat in EachSats)
            {
                var satdata = Rtcm_GpsObsEach_to_GR(eachSat, MessageNum);
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

            int offset = 24;                                    // frame header: 3byte=24bit, skip
            offset += 12;                                       // Message Number, uint12, DF002, skip
            RefStId = (int)BitToUint(byteData, offset, 12);     // Reference Station ID, uint12, DF003
            offset += 12;
            Tow = (int)BitToUint(byteData, offset, 30);         // GPS Epoch Time(TOW, msec), uint30, DF004
            offset += 30;
            SyncGnssFlag = BitToBool(byteData, offset);         // Syncronous GNSS Flag, bit(1), 1bit, DF005,
            offset++;                                           // true:同エポックにさらにデータあり（次のフレームを待つ）, false:同エポックにこれ以上データ無し．測位演算開始してよい
            NumSat = (int)BitToUint(byteData, offset, 5);       // DF006，このメッセージでの衛星数
            offset += 5;
            SmoothingIndicator = BitToBool(byteData, offset);   // Divergence-free Smoothing Indicator; true:ON, false:OFF
            offset++;                                           // DF007
            SmoothingInterval = (int)BitToUint(byteData, offset, 3);    // GPS Smoothing Interval, 
            offset += 3;                                                // DF008

            for (int i = 0; i < NumSat; i++)
            {
                EachSat eachSat = new ();
                EachSats.Add(eachSat);
                eachSat.SatID = (int)BitToUint(byteData, offset, 6);        // GPS Satellite ID, DF009, PRNに一致
                offset += 6;
                eachSat.L1Ind = BitToBool(byteData, offset);                // GPS L1 Code Indicator, DF010, false:C/A, true:P(Y)
                offset++;
                eachSat.L1Pr = (int)BitToUint(byteData, offset, 24);      // GPS L1 Pseudorange [m], DF011
                offset += 24;
                eachSat.L1Ph_Pr = (int)BitToInt(byteData, offset, 20);  // GPS L1 Phaserange - L1 Pseudorange [m], DF012  -->> Roll overの記述あり．未実装　注意！！
                offset += 20;
                eachSat.L1LockTime = (int)BitToUint(byteData, offset, 7);   // GPS L1 Lock time Indicator, DF013
                offset += 7;

                if (MessageNum == 1002 || MessageNum == 1004)
                {
                    eachSat.L1Pr_Mod_Amb = (int)BitToUint(byteData, offset, 8);     // GPS Integer L1 Pseudorange Modulus Ambiguity, DF014
                    offset += 8;                    
                    eachSat.L1Cnr = (int)BitToUint(byteData, offset, 8);         // GPS L1 CNR, DF015
                    offset += 8;
                }
                if (MessageNum == 1003 || MessageNum == 1004)
                {                    
                    eachSat.L2Ind = (int)BitToUint(byteData, offset, 2);                    // GPS L2 Code Indicator, DF016
                    offset += 2;                    
                    eachSat.L2_L1_PrDiff = BitToInt(byteData, offset, 14);           // GPS L2 - L1 Pseudorange Difference, DF017
                    offset += 14;                    
                    eachSat.L2Ph_L1Pr = BitToInt(byteData, offset, 20);            // GPS L2 Phaserange - L1 Pseudorange, DF018
                    offset += 20;                    
                    eachSat.L2LockTime = (int)BitToUint(byteData, offset, 7);               // GPS L2 Lock time Indicatior, DF019
                    offset += 7;
                }
                if (MessageNum == 1004)
                {                    
                    eachSat.L2Cnr = (int)BitToUint(byteData, offset, 8);         // GPS L2 CNR, DF020
                    offset += 8;
                }
            }
        }

        private static SatelliteData Rtcm_GpsObsEach_to_GR(EachSat s, int messageNum)
        {
            GrPrn prn = new GrPrn(SATID2SNN(s.SatID, SYS.GPS));
            var satData = new SatelliteData(prn);
            
            // L1データ格納
            var raw1 = new RawData();

            double decL1Pr = s.L1Pr * 0.02;                         // DF011
            double decL1Pr_Mod_Amb = s.L1Pr_Mod_Amb * 299792.458;   // DF014
            //bool L1Pr_valid_L2Only = (s.L1Pr & 0x80000) != 0;
            bool L1Pr_valid_L2Only = (s.L1Pr == 524288);            // 80000h - invalid L1 pseudorange; used only in the calculation of L2 measurement
                                                                    // DF011; 0x80000のときinvalidだが，L2 の計算に使用とある．uint24なのでL1Pｒは24ビット読み取ってintにキャスト済みなので
                                                                    // 0x80000 == 524288で判断した．だが524288*0.02 = 10485.76で可能な範囲内に入っている．これで良いのか？
                                                                    // int24で，0ｘ800000なら負の最大値となって意味も分からなくないが...そうなると定義域の範囲が表現できない
            raw1.Valid_Code = !L1Pr_valid_L2Only 
                                && (s.L1Pr >= 0) && (s.L1Pr <= 14989623) 
                                && (s.L1Pr_Mod_Amb >= 0) && (s.L1Pr_Mod_Amb <= 255);     // DF011とDF014の範囲チェック

            double decL1Ph_Pr = s.L1Ph_Pr * 0.0005;                 // DF012
            bool Valid_L1Ph_Pr = (s.L1Ph_Pr != -524288);            // DF012; invalid when the value is 0x80000(int20) = only MSB is true = (2^20)/2 * (-1) = -524288
            raw1.Valid_Phase = raw1.Valid_Code
                                && Valid_L1Ph_Pr
                                && (decL1Ph_Pr >= -262.1435) && (decL1Ph_Pr <= 262.1435);   // これは冗長

            raw1.Valid_Dopper = false;

            if (s.L1Ind)
            {
                raw1.ObsCode = "1W";
            }
            else
            {
                raw1.ObsCode = "1C";
            }

            raw1.Pseudorange = decL1Pr + decL1Pr_Mod_Amb;
            raw1.CarrierPhase = (raw1.Pseudorange + decL1Ph_Pr) / LG1;
            raw1.Doppler = double.NaN;
            raw1.CN0 = (s.L1Cnr == 0)? double.NaN : s.L1Cnr * 0.25;  // DF015; when 0, CNR is not provided
            raw1.FreqNr = 0;                                         // used for SBF
            raw1.Frequency = SatelliteConstants.FG1;
            raw1.WabeLength = SatelliteConstants.LG1;
            satData.Raws.Add(raw1);
            if (messageNum == 1001 || messageNum == 1002)
            {
                return satData;
            }

            // L2データ格納; for 1003 or 1004
            var raw2 = new RawData();
            double decL2_L1_PrDiff = s.L2_L1_PrDiff * 0.02;
            bool Valid_L2_L1_PrDiff = (s.L2_L1_PrDiff >= -163.82) && (s.L2_L1_PrDiff <= 163.82);    // DF017; 0x2000 の判定は，範囲の判定で包含できている
            raw2.Valid_Code = Valid_L2_L1_PrDiff && (raw1.Valid_Code || L1Pr_valid_L2Only);

            double decL2Ph_L1Pr = s.L2Ph_L1Pr * 0.0005;
            bool Valid_L2Ph_L1Pr = (s.L2Ph_L1Pr != -524288);    // DF018; invalid when the value is 0x80000(int20) = only MSB is true = (2^20)/2 * (-1) = -524288
            raw2.Valid_Phase = Valid_L2Ph_L1Pr && (raw1.Valid_Code || L1Pr_valid_L2Only);

            raw2.Valid_Dopper = false;

            switch (s.L2Ind)
            {
                case 0:
                    raw2.ObsCode = "2C";
                    break;
                case 1:
                    raw2.ObsCode = "2W";        // 2W: AS on, 2P AS off, nav data見て判断必要，その他，受信機依存の部分があるのでどうするか
                    break;
                case 2:
                    raw2.ObsCode = "2D";
                    break;
                case 3:
                    raw2.ObsCode = "2Y";
                    break;
            }

            raw2.Pseudorange = raw1.Pseudorange + decL2_L1_PrDiff;
            raw2.CarrierPhase = (raw1.Pseudorange + decL2Ph_L1Pr) / LG2;
            raw2.Doppler = double.NaN;
            raw2.CN0 = (s.L2Cnr == 0) ? double.NaN : s.L2Cnr * 0.25;  // DF015; when 0, CNR is not provided
            raw1.FreqNr = 0;                                         // used for SBF
            raw1.Frequency = SatelliteConstants.FG2;
            raw1.WabeLength = SatelliteConstants.LG2;
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
                sb.Append(ind + "GpsSatID:      " + EachSats[i].SatID + "\n");
                sb.Append(ind + "L1Indicator:   " + EachSats[i].L1Ind + "\n");
                //if (MessageNum >= 1009)
                //{
                //    //sb.Append(ind + "Freq_Ch_Num:   " + EachSats[i].FreqChNum + "\n");
                //}
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
                double fullPseudorange = (EachSats[i].L1Pr * 0.02) + (EachSats[i].L1Pr_Mod_Amb * 299792.458);
                double fullPhaserange = (fullPseudorange + EachSats[i].L1Ph_Pr * 0.0005) / LG1;
                sb.Append(string.Format("   Calc L1 Pr:       {0:F3}\n", fullPseudorange));
                sb.Append(string.Format("   Calc L1 Ph:       {0:F3}\n", fullPhaserange));
                if (MessageNum >= 1003)
                {
                    //fullPseudorange += EachSats[i].L2_L1_PrDiff * 0.002;
                    double fullPseudorange2 = fullPseudorange + EachSats[i].L2_L1_PrDiff * 0.002;
                    fullPhaserange = (fullPseudorange + EachSats[i].L2Ph_L1Pr * 0.0005) / LG1;
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
