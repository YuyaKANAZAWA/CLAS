using System;
using System.Collections.Generic;
using System.Text;
using static GrToolBox.Common.CommonUtilities;

namespace GrToolBox.Nmea
{
    public class NmeaSatelliteData
    {
        //public int Prn { get; set; }

        private int _Prn;

        public string Snn { get; set; }
        public int Prn
        {
            set
            {
                _Prn = value;
                Snn = Prn2Snn(_Prn);
            }
            get { return _Prn; }
        }

        public int SvIdNmea { get; set; } = -9999;

        // sat pos
        // sat vel
        // etc etc

        public double Ele { get; set; }
        public double Azi { get; set; }
        public bool InUse { get; set; } = false;

        public int Status { get; set; } = 0;    // 使用ステータス 0: visible but not used, 001: C1 used, 010: C2 used, 100: C5 used

        public double[] Cn0 { get; set; } = { double.NaN, double.NaN, double.NaN };


    }
}
