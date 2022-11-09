using System;
using System.Collections.Generic;
using System.Text;

namespace GrToolBox.Coordinates
{
    public static class CoordinateConstants
    {
        public static readonly double SEMI_MAJOR_AXIS_WGS = 6378137.0;      //WGS84準拠楕円体長半径
        public static readonly double SEMI_MAJOR_AXIS_GRS = 6378137.0;      //GRS80楕円体長半径(WGSと同じ)
        public static readonly double SEMI_MAJOR_AXIS_PZ = 6378136.0;       //PZ-90.02
        public static readonly double FLATTENING_WGS = 1 / 298.257223563;   //WGS84楕円体扁平率
        public static readonly double FLATTENING_GRS = 1 / 298.257222101;   //GRS80楕円体扁平率
        public static readonly double SQUARED_ECCENTRICITY_WGS = FLATTENING_WGS * (2 - FLATTENING_WGS);    //WGS84楕円体離心率の2乗
        public static readonly double SQUARED_ECCENTRICITY_GRS = FLATTENING_GRS * (2 - FLATTENING_GRS);    //GRS80楕円体離心率の2乗
    }
}
