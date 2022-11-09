using System;
using System.Collections.Generic;
using System.Text;

namespace GrToolBox.Coordinates
{
    public class Position
    {
        public double[] Xyz { get; set; } = { double.NaN, double.NaN, double.NaN };
        public double[] Enu { get; set; } = { double.NaN, double.NaN, double.NaN };
        public double[] Llh { get; set; } = { double.NaN, double.NaN, double.NaN };
        //public double[] LlhDeg { get; set; } = { double.NaN, double.NaN, double.NaN };
        public double GeoidHeight { get; set; } = double.NaN;
        public double Alt { get; set; } = double.NaN;
    }
}
