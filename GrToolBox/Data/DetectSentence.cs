using GrToolBox.Common;
using System;
using System.Collections.Generic;
using System.Text;
using static GrToolBox.Data.DataTypeDefinitions;
using static GrToolBox.Data.DataUtilities;
using static GrToolBox.Data.SBF.SbfUtilities;
using static GrToolBox.Settings.SettingsDefinitions;

namespace GrToolBox.Data
{
    public class DetectSentence
    {

        public List<byte[]> Sentences = new List<byte[]>(); // 検出できたセンテンスのbyte配列
        public List<Data_Type> TypeOfSentences = new List<Data_Type>(); // 検出できたセンテンスのデータ種別


        private CircularBuffer<byte> Cb { get; set; }

        private byte[] tmpSentence;


        public DetectSentence(CircularBuffer<byte> cb)
        {
            Cb = cb;
        }


        /// <summary>
        ///   data_Typeで指定されたデータセンテンスを探す．
        ///   目的のセンテンスが見つかれば，そのセンテンスのデータ（バイト配列）をSentencesに，データ種別をTypeOfSentencesに追加する．
        ///   Cbから読み取れるだけ読み取りが完了したとき，または，データセンテンスの先頭を発見したが必要なbyte数がCbにストアされていないため
        ///   読み取り出来ない場合にreturnされる．
        /// </summary>
        /// <param name="data_Type"></param>
        public void GetSentences(Data_Type data_Type)
        {
            Sentences.Clear();
            TypeOfSentences.Clear();
            byte[] tmpBytes;

            while (Cb.Count() > 8)       // RTCM3: データ部無しの最小構成:48bit　->> 6byte,  SBFのヘッダ部8byte
            {
                //byte[] sentence = new byte[1];
                Data_Type typeOfSentence = Data_Type.UnKnown;
                int status = 0;
                switch (data_Type)
                {
                    // RTCM3判定
                    case Data_Type.RTCM3:
                        status = GetRtcm3();
                        typeOfSentence = Data_Type.RTCM3;
                        break;
                    case Data_Type.NMEA:
                        break;
                    case Data_Type.SBF:
                        status = GetSbf();
                        typeOfSentence = Data_Type.SBF;
                        break;
                    case Data_Type.ALL:
                        status = GetRtcm3();
                        if(status == 1 || status == 2) { break;}
                        //sentence = GetSbf();
                        break;
                }

                if (status == 0)
                {
                    tmpBytes = Cb.Read(1);
                    continue;
                }
                else if (status == 1)
                {
                    Sentences.Add(tmpSentence);
                    TypeOfSentences.Add(typeOfSentence);
                }
                else if(status == 2)
                {
                    return;
                }
            }
        }

        /// <summary>
        ///   Cbの先頭(Top)から始まるデータ列がRtcm3か否かを判断する．
        ///   Rtcm3の場合は，そのセンテンスのデータbyte配列をsentenceに格納する．
        ///   センテンスを読み取れた場合，CbのTopはセンテンスのbyte数移動する．
        ///   読み取れなかった場合，CbのTopは移動しない．
        /// </summary>
        /// <returns>
        ///   0: 読み取りできず．CbのTopは，Rtcm3の先頭ではない
        ///   1:　Rtcm3のセンテンスを一つ読み取り完了
        ///   2:　CbのTopはRtcm3の先頭の可能性があるが，Cbに必要byte数が格納されていないため確定判断，読み取りができない
        /// </returns>
        private int GetRtcm3()
        {
            byte[] tmpBytes;
            Cb.SetMark();
            // RTCM3判定
            tmpBytes = Cb.Read(3);     // 3byte取得
            if (tmpBytes[0] == 0b_1101_0011) // Preamble判定
            {
                //Console.WriteLine("RTCM3-フレームヘッダPreamble(0xd3)検出");
                if ((tmpBytes[1] >> 2) == 0b_0000_0000)  // Reserved 6ビット判定
                {
                    //var messageLength = (uint)((tmpBytes[1] << 8) | tmpBytes[2]);     // 汎用関数を作って置き換え
                    uint messageLength = BitToUint(tmpBytes, 14, 10);   // 14ビット目から10ビットをuintで読む
                                                                        //Console.WriteLine("RTCM3-フレームヘッダReserved(6bitゼロ)検出,  Message Length: " + messageLength + "[byte]");
                    if (messageLength < 1024)
                    {
                        if (Cb.Count() >= messageLength + 3)
                        {
                            Cb.BackToMark();
                            tmpSentence = Cb.Read((int)(6 + messageLength));  // preambleからcrcまでをRTCMセンテンスとして格納  CRCチェックを実装すること
                            //Console.WriteLine(BitConverter.ToString(rtcm3));
                            return 1;
                        }
                        else
                        {
                            // possible rtcm; need more data
                            Cb.BackToMark();
                            return 2;
                        }
                    }   // not rtcm
                }   // not rtcm
            }   // not rtcm
            Cb.BackToMark();
            return 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private int GetSbf()
        {
            byte[] tmpBytes;
            Cb.SetMark();
            // SBF判定
            tmpBytes = Cb.Read(8);  // 8byte取得(SBF Block Header)
            int offset = 0;
            if(tmpBytes[0] == 0x_24 && tmpBytes[1] == 0x_40)    // $@ 検出
            {
                offset += 16;
                uint crc = BitToUint_L(tmpBytes, offset, 16);
                offset += 16;
                uint id = BitToUint_L(tmpBytes, offset, 16);
                offset += 16;
                int length = (int)BitToUint_L(tmpBytes, offset, 16);
                offset += 16;
                if(length % 4 == 0)
                {
                    if(Cb.Count() >= length - 8)
                    {
                        // checking CRC
                        Cb.BackToMark();
                        tmpSentence = Cb.Read(length);
                        ushort calcCrc = CRC_compute16CCITT(tmpSentence[4..]); // "$@"とCRCを飛ばして5バイト目以降(ID以降)でCRCを計算
                        if (calcCrc == crc) // CRC check OK
                        {                            
                            return 1;
                        }
                    }
                    else
                    {
                        // possible SBF; need more data
                        Cb.BackToMark();
                        return 2;
                    }
                }   // not SBF (length not multiple of 4)
            }   // not SBF ("$@" is not appeared)
            Cb.BackToMark();
            return 0;
        }

    }
}
