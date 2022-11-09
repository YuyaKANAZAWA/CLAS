using System;
using System.Collections.Generic;
using System.Text;

namespace GrToolBox.Time
{
    public static class TimeConstants
    {
        // GPS時刻の起点
        public static readonly DateTime TimeOfGpsStart = new DateTime(1980, 1, 6, 0, 0, 0);

        // 閏秒リスト（TimeGR型の配列）
        public static readonly TimeGR[] LEAP_HIST = {
            new TimeGR(1981,7,1,0,0,0),
            new TimeGR(1982,7,1,0,0,0),
            new TimeGR(1983,7,1,0,0,0),
            new TimeGR(1985,7,1,0,0,0),
            new TimeGR(1988,1,1,0,0,0),
            new TimeGR(1990,1,1,0,0,0),
            new TimeGR(1991,1,1,0,0,0),
            new TimeGR(1992,7,1,0,0,0),
            new TimeGR(1993,7,1,0,0,0),
            new TimeGR(1994,7,1,0,0,0),
            new TimeGR(1996,1,1,0,0,0),
            new TimeGR(1997,7,1,0,0,0),
            new TimeGR(1999,1,1,0,0,0),
            new TimeGR(2006,1,1,0,0,0),
            new TimeGR(2009,1,1,0,0,0),
            new TimeGR(2012,7,1,0,0,0),
            new TimeGR(2015,7,1,0,0,0),
            new TimeGR(2017,1,1,0,0,0)
        };


    }






}
