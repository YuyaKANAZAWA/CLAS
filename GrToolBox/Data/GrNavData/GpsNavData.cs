namespace GrToolBox.Data.GrNavData
{
    public class GpsNavData : GrNavBase
    {
        //public GrPrn Prn { get; set; }
        public double Toc { get; set; }         // Toc(sow); GLO,GAL,QZS,BDS: in GPS time frame
        public int WN_toc { get; set; }         // week number associated with toc (continuous); GLO,GAL,QZS,BDS: in GPS time frame
        public int Year { get; set; }           // toc year; GLO,GAL,QZS,BDS: in GPS time frame
        public int Month { get; set; }          // toc month; GLO,GAL,QZS,BDS: in GPS time frame
        public int Day { get; set; }            // toc day; GLO,GAL,QZS,BDS: in GPS time frame
        public int Hour { get; set; }           // toc hour; GLO,GAL,QZS,BDS: in GPS time frame
        public int Minute { get; set; }         // toc minute; GLO,GAL,QZS,BDS: in GPS time frame
        public double Second { get; set; }      // toc second; GLO,GAL,QZS,BDS: in GPS time frame
        public double A0 { get; set; }          // SV clock bias (seconds), GAL,QZS,BDS
        public double A1 { get; set; }          // SV clock drift (sec/sec), GAL,QZS,BDS
        public double A2 { get; set; }          // SV clock drift rate (sec/sec2), GAL,QZS,BDS
                                                //*****************
        public byte Iode { get; set; }        // IODE Issue of Data, Ephemeris
        public double Crs { get; set; }         // Crs (meters), GAL,QZS,BDS
        public double Deln { get; set; }        // Delta n (radians/sec), GAL,QZS,BDS
        public double M0 { get; set; }          // M0 (radians), GAL,QZS,BDS
                                                //*****************
        public double Cuc { get; set; }         // Cuc (radians), GAL,QZS,BDS
        public double Ecc { get; set; }         // e Eccentricity, GAL,QZS,BDS
        public double Cus { get; set; }         // Cus (radians), GAL,QZS,BDS
        public double SqrtA { get; set; }       // sqrt(A) (sqrt(m)), GAL,QZS,BDS
                                                //*****************
        public int Toe { get; set; }            // Toe Time of Ephemeris (sec of GPS week), GAL,QZS,BDS
        public double Cic { get; set; }         // Cic (radians), GAL,QZS,BDS
        public double Omega0 { get; set; }      // OMEGA0 (radians), GAL,QZS,BDS
        public double Cis { get; set; }         // Cis (radians), GAL,QZS,BDS
                                                //*****************
        public double I0 { get; set; }          // i0 (radians), GAL,QZS,BDS
        public double Crc { get; set; }         // Crc (meters), GAL,QZS,BDS
        public double Omega { get; set; }       // Omega (radians), GAL,QZS,BDS
        public double OmegaDot { get; set; }    // OMEGA DOT (radians/sec), GAL,QZS,BDS
                                                //*****************
        public double Idot { get; set; }        // IDOT (radians/sec), GAL,QZS,BDS
        public byte CodeOnL2 { get; set; }        // Codes on L2 channel
        public int WeekNum { get; set; }        // GPS Week # (to go with TOE) Continuous number, not mod(1024), GAL,QZS,BDS:in GPS time frame
        public bool L2P_Flag { get; set; }       // L2 P data flag
                                                 //*****************
        public double SvAccuracy { get; set; }     // SV accuracy (meters) See GPS ICD 200H Section 20.3.3.3.1.3
        public byte SvHealth { get; set; }       // GPS: SV health (bits 17-22 w 3 sf 1)
                                                 // GLO: 1-bit health flag, 0=healthy, 1=unhealthy
        public double Tgd { get; set; }         // TGD (seconds)
        public int Iodc { get; set; }        // IODC Issue of Data, Clock, BDS:AODC(5bit data)
                                                //*****************
        public int Ttm { get; set; }            // Transmission time of message, GAL:in GPS time frame
        public int FitInterval { get; set; }    // Fit Interval

    }
}
