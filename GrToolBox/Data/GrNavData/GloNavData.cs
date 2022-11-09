using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrToolBox.Data.GrNavData
{
    public class GloNavData : GrNavBase
    {
        public int WN_toc { get; set; }         // week number associated with toc (continuous); GLO,GAL,QZS,BDS: in GPS time frame
        public int Year { get; set; }           // toc year; GLO,GAL,QZS,BDS: in GPS time frame
        public int Month { get; set; }          // toc month; GLO,GAL,QZS,BDS: in GPS time frame
        public int Day { get; set; }            // toc day; GLO,GAL,QZS,BDS: in GPS time frame
        public int Hour { get; set; }           // toc hour; GLO,GAL,QZS,BDS: in GPS time frame
        public int Minute { get; set; }         // toc minute; GLO,GAL,QZS,BDS: in GPS time frame
        public double Second { get; set; }      // toc second; GLO,GAL,QZS,BDS: in GPS time frame
        public double Toc { get; set; }         // Toc(sow); GLO,GAL,QZS,BDS: in GPS time frame
        public int FreqNr { get; set; }         // -7..13
        public double Tau { get; set; }         // \tau_n(tb): time correction to GLONASS time [sec]
        public double Gamma { get; set; }       // \gamma_n(tb):relative deviation of predicted carrier frequency [1Hz/Hz]
        public double X { get; set; }           // x-component of satellite position in PZ-90.02 [km]
        public double Y { get; set; }
        public double Z { get; set; }
        public double Dx { get; set; }          // x-component of satellite velocity in PZ-90.02 [km/sec]
        public double Dy { get; set; }
        public double Dz { get; set; }
        public double Ddx { get; set; }         // x-component of satellite acceleration in PZ-90.02 [km/sec^2]
        public double Ddy { get; set; }
        public double Ddz { get; set; }
        public byte E { get; set; }             // age of data [day]
        public byte SvHealth { get; set; }      // GLO: 1-bit health flag, 0=healthy, 1=unhealthy
        public int MessageFrameTime { get; set; }   // message frame time from Tk (sow in GPS time frame)
    }
}
