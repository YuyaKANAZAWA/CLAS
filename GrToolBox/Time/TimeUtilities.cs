using System;
using static GrToolBox.Time.TimeConstants;

namespace GrToolBox.Time
{
    public static class TimeUtilities
    {
        // GPS週番号，週内秒からDateTime
        // weekのロールオーバーは考慮していない（通し番号で与える必要あり）
        public static DateTime WeekSow2DateTime(int week, double sow)
        {
            DateTime dateTime = TimeOfGpsStart;
            dateTime = dateTime.AddDays(week*7);
            dateTime = dateTime.AddSeconds(sow);
            return dateTime;
        }

        // DateTimeからweek,sowに
        public static int[] DateTime2WeekSow(DateTime DT)
        {
            int[] weeksow = new int[2];
            TimeSpan diff = DT.Date - TimeOfGpsStart.Date;
            weeksow[0] = diff.Days / 7;
            weeksow[1] = (int)DT.DayOfWeek * 86400 + DT.Hour * 3600 + DT.Minute * 60 + DT.Second;
            return weeksow;
        }

        // DateTimeからユリウス日に（注意：ユリウス日は日単位の整数で取り扱っている）
        public static int DateTime2Juliday(DateTime DT)
        {
            int Y = DT.Year;
            int M = DT.Month;
            int D = DT.Day;
            int jd;
            
            if (M < 3)
            {
                jd = (int)(30.6001 * (M + 1 + 12 * (1))) + (int)(365.25 * (Y - (1))) + D + 1720982;
            }
            else
            {
                jd = (int)(30.6001 * (M + 1 + 12 * (0))) + (int)(365.25 * (Y - (0))) + D + 1720982;
            }
            return jd;
        }

        // テーブル参照して，GPST開始時からtimeまでの閏秒の合計値を返す
        public static int GetLeapSecond(TimeGR time)
        {
            int leap;
            int nhist = LEAP_HIST.Length;

            for (leap=nhist; leap>0; leap--)
            {
                if (LEAP_HIST[leap-1].Diff(time) < 0) break;
            }
            return leap;
        }

        public static int GetLeapSecond(DateTime time)
        {
            TimeGR timeGR = new TimeGR(time);
            return GetLeapSecond(timeGR);
        }


        /// <summary>
        /// 閏年かどうか判定する
        /// </summary>
        /// <param name="year">西暦年</param>
        /// <returns>true: 閏年，false: 平年</returns>
        public static bool IsLeapYear(int year)
        {
            if(((year % 4 == 0) && (year % 100 != 0)) || (year % 400 == 0))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

    }
}
