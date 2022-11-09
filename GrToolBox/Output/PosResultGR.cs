using GrToolBox.Common;
using GrToolBox.Coordinates;
using GrToolBox.Satellite;
using GrToolBox.Time;
using System.Collections.Generic;
using System;
using static GrToolBox.Common.Constants;
using static GrToolBox.Common.CommonUtilities;

namespace GrToolBox.Output
{
    [Serializable]
    public class PosResultGR
    {

        public bool Valid { get; set; } = false;
        public string TimeStr { get; set; } = "";
        public TimeGR TimeGr { get; set; }
        // todo Time関連保留
        public Position Pos { get; set; } = null;
        public double[] RecCLK { get; set; } = new double[MAX_SYS]; // sec

        public List<SatelliteData_Base> SatelliteList = new List<SatelliteData_Base>();

        public Dop Dop { get; set; } = new Dop();

        public class NumberOfSatellite
        {
            public int[] Total { get; set; } = new int[MAX_SYS];
            public int[] Used { get; set; } = new int[MAX_SYS];
            public int[] Not_Used { get; set; } = new int[MAX_SYS];
        }


        public NumberOfSatellite CountSat()
        {
            var N = new NumberOfSatellite();
            foreach(SatelliteData_Base s in SatelliteList)
            {
                //int isys = GetISys(s.Prn);
                int isys = s.Prn.ISys;
                N.Total[isys]++;
                if(s.HaveCodeData>0 && s.Status == 0)
                    N.Used[isys]++;
                else
                    N.Not_Used[isys]++;
            }
            return N;
        }

    }
}
