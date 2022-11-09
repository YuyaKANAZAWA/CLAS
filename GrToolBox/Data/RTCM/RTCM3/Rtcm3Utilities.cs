using static GrToolBox.Common.CommonDefinitions;
using static GrToolBox.Common.Constants;
using static GrToolBox.Satellite.SatelliteConstants;
using static GrToolBox.Time.TimeUtilities;

namespace GrToolBox.Data.RTCM.RTCM3
{
    public static class Rtcm3Utilities
    {
        /// <summary>
        /// RTCMのGLONASS Satellite Frequency Channel NumberからL1周波数を求める
        /// </summary>
        /// <param name="FreqNum">RTCMのGLONASS Satellite Frequency Channel Number</param>
        /// <returns>L1周波数</returns>
        public static double RtcmFreqNum_TO_Freq_L1(int FreqNum)
        {
            return FR1 + (FreqNum - 7) * FR1SEPARATION;
        }

        /// <summary>
        /// RTCMのGLONASS Satellite Frequency Channel NumberからL1波長を求める
        /// </summary>
        /// <param name="FreqNum">RTCMのGLONASS Satellite Frequency Channel Number</param>
        /// <returns>L1波長</returns>
        public static double RtcmFreqNum_TO_Length_L1(int FreqNum)
        {
            return CC / RtcmFreqNum_TO_Freq_L1(FreqNum);
        }

        public static double RtcmFreqNum_TO_Freq_L2(int FreqNum)
        {
            return FR2 + (FreqNum - 7) * FR2SEPARATION;
        }

        public static double RtcmFreqNum_TO_Length_L2(int FreqNum)
        {
            return CC / RtcmFreqNum_TO_Freq_L2(FreqNum);
        }

        public static string SATID2SNN(int satid, SYS sys)
        {
            switch (sys)
            {
                case SYS.GPS:
                    return "G" + satid.ToString();
                case SYS.GLO:
                    return "R" + satid.ToString();
                default:
                    return "";
            }
        }

        /// <summary>
        /// GLONASSのエポック時刻を推測して返す
        /// </summary>
        /// <param name="approxTime">概略時刻入力</param>
        /// <param name="Nt">RTCM3-1020に格納されている閏年の1月1日を起点とする日数</param>
        /// <returns></returns>
        public static DateTime GetGlonassEpochTime(DateTime approxTime, int Nt)
        {
            int y = approxTime.Year;
            int ly_left = y;            // approxTimeより過去の閏年
            int ly_right = y;           // approxTimeを含んだ未来の閏年

            while (true)    // 年の若い方を，初期値を含まずに探索
            {
                ly_left--;
                if (IsLeapYear(ly_left))
                {
                    break;
                }
            }

            while (true)    // 年の大きい方を，初期値を含んで探索
            {
                if (IsLeapYear(ly_right))
                {
                    break;
                }
                ly_right++;
            }

            int ref_leap_year;  // 日数カウントの起点となる閏年の年号

            // approxTimeが1年前後の精度であることを期待
            // 366<Nt<1096 ならapproxの年号より若い閏年を起点とする
            // 1<=Nt<=366　ならapproxの年号に最も近い閏年を起点とする
            // 1096＜＝Nt<=1461 ならapproxの年号より若い閏年を起点とする
            if (Nt >= 1 && Nt <= 366)
            {
                ref_leap_year = ((y - ly_left) <= (ly_right - y)) ? ly_left : ly_right;
            }
            else
            {
                ref_leap_year = ly_left;
            }


            if (Nt > 366 && Nt < 1096)
            {
                ref_leap_year = ly_left;
            }
            else if(Nt >= 1 && Nt <= 366)
            {
                ref_leap_year = ((y-ly_left) <= (ly_right - y))? ly_left : ly_right;
            }

            return new DateTime(ref_leap_year, 1, 1) + TimeSpan.FromDays(Nt - 1);
        }





    }
}
