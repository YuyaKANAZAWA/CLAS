using System;
using static GrToolBox.Time.TimeUtilities;
using static System.Math;

namespace GrToolBox.Time
{
    /// <summary>
    /// 以下のいずれかで時刻を格納する
    /// TimeGR time = new TimeGR(int week, double sow)
    /// TimeGR time = new TimeGR(int year, int month, int day, int hour, int minute, double second)
    /// TimeGR time = new TimeGR(String ymdhms, String delimitter)
    /// 
    /// 年月日時分秒の取得（秒のみdouble型）
    /// time.Year, time.Month, time.Day, time.Hour, time.Minute, time.Second
    /// 
    /// GPS週番号，週内秒の取得（Sowはdouble型）
    /// Week: time.Week: 1980年1月6日(日曜日)から始まる週を0とする通算週番号．ロールオーバーは無視
    /// Sow: time.Sow
    /// 
    /// 年内通算日の取得（1月1日が"1"）
    /// time.DayOfYear
    /// 
    /// 週内通算日の取得（日曜が"0"）
    /// time.DayOfWeek
    /// 
    /// 内部処理用DateTime型DTに対してDateTimeのプロパティが使える（一例）
    /// DT.Secondは秒の整数部分までしか保持していないので注意（millisecondは使用していない）
    /// 年：DT.Year, 月: DT.Month, 日: DT.Day, 時: DT.Hour, 分: DT.Minute, 秒: DT.Second, ミリ秒: DT.Millisecond
    /// 年内通算日: DT.DayOfYear  -- 1月1日が"1"
    /// 週内通算日: DT.DayOfWeek  -- enum型なので，日曜:0，月曜1...が欲しければキャスト(int)する
    /// 日時の比較: "<, <=, >, >=, ==, !="等が使える．大きい方が新しい日付
    /// </summary>

    public class TimeGR
    {
        public DateTime DT { get; private set; }
        public int Week { get; private set; }
        public double Sow { get; private set; }
        public int Year { get { return DT.Year; } }
        public int Month { get { return DT.Month; } }
        public int Day { get { return DT.Day; } }
        public int Hour { get { return DT.Hour; } }
        public int Minute { get { return DT.Minute; } }
        public double Second { get; private set; }
        public int DayOfYear { get { return DT.DayOfYear; } }
        public int DayOfWeek { get { return (int)DT.DayOfWeek; } }
        public double SecondOfDay { get { return Hour * 3600 + Minute * 60 + Second; } }

        /*----------------
            コンストラクタ
        -----------------*/

        // week, sowで設定
        public TimeGR(int week, double sow)
        {
            // sowが1週分を超えているときの対応
            if ((sow >= 604800) || (sow < 0))
            {
                int w = (int)(Floor(sow / 604800.0));
                Week = week + w;
                Sow = sow - 604800 * w;
            }
            else
            {
                Week = week;
                Sow = sow;
            }
            DT = WeekSow2DateTime(Week, Sow);
            Second = DT.Second + (Sow - (int)Sow);
        }

        // 年月日時分秒で設定　秒のみdouble
        public TimeGR(int year, int month, int day, int hour, int minute, double second)
        {
            Second = second;
            DT = new DateTime(year, month, day, hour, minute, (int)second);
            int[] weeksow = DateTime2WeekSow(DT);
            Week = weeksow[0];
            Sow = weeksow[1] + (second - (int)second);
        }


        public TimeGR(String ymdhms, String delimitter)
        {
            //string[] ymdhms_ = ymdhms.Split(delimitter, 6);
            string[] ymdhms_ = ymdhms.Split(delimitter, StringSplitOptions.RemoveEmptyEntries);
            int year = int.Parse(ymdhms_[0]);
            int month = int.Parse(ymdhms_[1]);
            int day = int.Parse(ymdhms_[2]);
            int hour = int.Parse(ymdhms_[3]);
            int minute = int.Parse(ymdhms_[4]);
            double second = double.Parse(ymdhms_[5]);
            double int_second = Floor(second);

            Second = second;
            DT = new DateTime(year, month, day, hour, minute, (int)int_second);
            int[] weeksow = DateTime2WeekSow(DT);
            Week = weeksow[0];
            Sow = weeksow[1] + (second - (int)second);
        }

        // DateTime型で作成 (DateTime型はmsの桁までしか扱えないので注意)
        public TimeGR(DateTime dt)
        {
            Second = (double)dt.Second + dt.Millisecond;
            DT = new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second);
            int[] weeksow = DateTime2WeekSow(DT);
            Week = weeksow[0];
            Sow = weeksow[1] + (Second - (int)Second);
        }

        /*----------------
            public methods
        -----------------*/

        // ユリウス日を返す（日数の整数部分のみ）
        public int GetJuliday()
        {
            return DateTime2Juliday(DT);
        }

        // 修正ユリウス日を返す
        public double GetMJD()
        {
            return (DateTime2Juliday(DT) - 2400000.5);
        }

        public string GetStringISO_DATE_TIME()
        {
            return $"{DT.Year}-{DT.Month:00}-{DT.Day:00}T{DT.Hour:00}:{DT.Minute:00}:{ (DT.Second + DT.Millisecond/1000.0):00.000}";
        }

        public double Diff(TimeGR anotherTime)
        {
            double diff = (this.DT - anotherTime.DT).TotalSeconds + ((this.Second - (int)this.Second) - (anotherTime.Second - (int)anotherTime.Second));
            return diff;
        }

        public void Copy(TimeGR time)
        {
            this.DT = time.DT;
            this.Week = time.Week;
            this.Sow = time.Sow;
            this.Second = time.Second;
        }

        public bool EqualsTo(TimeGR anotherTime)
        {
           return (Abs(Diff(anotherTime)) < 0.001 );
        }

    }
}
