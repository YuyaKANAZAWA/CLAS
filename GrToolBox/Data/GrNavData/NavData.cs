using GrToolBox.Common;
using System.Collections.Generic;
using static GrToolBox.Common.CommonDefinitions;

namespace GrToolBox.Data.GrNavData
{
    public class NavData
    {
        public List<GpsNavData> GPS { get; set; } = new List<GpsNavData>();
        public List<GloNavData> GLO { get; set; } = new List<GloNavData>();
        public List<GalNavData> GAL { get; set; } = new List<GalNavData>();
        public List<QzsNavData> QZS { get; set; } = new List<QzsNavData>();
        public List<BdsNavData> BDS { get; set; } = new List<BdsNavData>();
        public List<IrnNavData> IRN { get; set; } = new List<IrnNavData>();
        public List<SbsNavData> SBS { get; set; } = new List<SbsNavData>();

        public NavData() { }

        public void Add(GpsNavData gps) { GPS.Add(gps); }
        public void Add(GloNavData glo) { GLO.Add(glo); }
        public void Add(GalNavData gal) { GAL.Add(gal); }
        public void Add(QzsNavData qzs) { QZS.Add(qzs); }
        public void Add(BdsNavData bds) { BDS.Add(bds); }
        public void Add(IrnNavData irn) { IRN.Add(irn); }
        public void Add(SbsNavData sbs) { SBS.Add(sbs); }

        public void Add(GrNavBase nav)
        {
            switch (nav?.Prn?.Sys)
            {
                case SYS.GPS:
                    Add((GpsNavData)nav);
                    break;
                case SYS.GLO:
                    Add((GloNavData)nav);
                    break;
                case SYS.GAL:
                    Add((GalNavData)nav);
                    break;
                case SYS.QZS:
                    Add((QzsNavData)nav);
                    break;
                case SYS.BDS:
                    Add((BdsNavData)nav);
                    break;
                case SYS.IRN:
                    //Add((IrnNavData)nav);
                    break;
                case SYS.SBS:
                    //Add((IrnNavData)nav);
                    break;
            }
        }



        public void CopyTo(NavData navData)
        {
            foreach(var n in GPS) { navData.Add(n); }
            foreach(var n in GLO) { navData.Add(n); }
            foreach(var n in GAL) { navData.Add(n); }
            foreach(var n in QZS) { navData.Add(n); }
            foreach(var n in BDS) { navData.Add(n); }
            foreach(var n in IRN) { navData.Add(n); }
            foreach(var n in SBS) { navData.Add(n); }
        }

        public void Clear()
        {
            GPS.Clear();
            GLO.Clear();
            GAL.Clear();
            QZS.Clear();
            BDS.Clear();
            IRN.Clear();
            SBS.Clear();
        }
    }


    //public class GpsNavData
    //{
    //    public GrPrn Prn { get; set; }
    //    public double Toc { get; set; }         // Toc(sow); GLO,GAL,QZS,BDS: in GPS time frame
    //    public int WN_toc { get; set; }         // week number associated with toc (continuous); GLO,GAL,QZS,BDS: in GPS time frame
    //    public int Year { get; set; }           // toc year; GLO,GAL,QZS,BDS: in GPS time frame
    //    public int Month { get; set; }          // toc month; GLO,GAL,QZS,BDS: in GPS time frame
    //    public int Day { get; set; }            // toc day; GLO,GAL,QZS,BDS: in GPS time frame
    //    public int Hour { get; set; }           // toc hour; GLO,GAL,QZS,BDS: in GPS time frame
    //    public int Minute { get; set; }         // toc minute; GLO,GAL,QZS,BDS: in GPS time frame
    //    public double Second { get; set; }      // toc second; GLO,GAL,QZS,BDS: in GPS time frame
    //    public double A0 { get; set; }          // SV clock bias (seconds), GAL,QZS,BDS
    //    public double A1 { get; set; }          // SV clock drift (sec/sec), GAL,QZS,BDS
    //    public double A2 { get; set; }          // SV clock drift rate (sec/sec2), GAL,QZS,BDS
    //                                            //*****************
    //    public byte Iode { get; set; }        // IODE Issue of Data, Ephemeris
    //    public double Crs { get; set; }         // Crs (meters), GAL,QZS,BDS
    //    public double Deln { get; set; }        // Delta n (radians/sec), GAL,QZS,BDS
    //    public double M0 { get; set; }          // M0 (radians), GAL,QZS,BDS
    //                                            //*****************
    //    public double Cuc { get; set; }         // Cuc (radians), GAL,QZS,BDS
    //    public double Ecc { get; set; }         // e Eccentricity, GAL,QZS,BDS
    //    public double Cus { get; set; }         // Cus (radians), GAL,QZS,BDS
    //    public double SqrtA { get; set; }       // sqrt(A) (sqrt(m)), GAL,QZS,BDS
    //                                            //*****************
    //    public int Toe { get; set; }            // Toe Time of Ephemeris (sec of GPS week), GAL,QZS,BDS
    //    public double Cic { get; set; }         // Cic (radians), GAL,QZS,BDS
    //    public double Omega0 { get; set; }      // OMEGA0 (radians), GAL,QZS,BDS
    //    public double Cis { get; set; }         // Cis (radians), GAL,QZS,BDS
    //                                            //*****************
    //    public double I0 { get; set; }          // i0 (radians), GAL,QZS,BDS
    //    public double Crc { get; set; }         // Crc (meters), GAL,QZS,BDS
    //    public double Omega { get; set; }       // Omega (radians), GAL,QZS,BDS
    //    public double OmegaDot { get; set; }    // OMEGA DOT (radians/sec), GAL,QZS,BDS
    //                                            //*****************
    //    public double Idot { get; set; }        // IDOT (radians/sec), GAL,QZS,BDS
    //    public byte CodeOnL2 { get; set; }        // Codes on L2 channel
    //    public int WeekNum { get; set; }        // GPS Week # (to go with TOE) Continuous number, not mod(1024), GAL,QZS,BDS:in GPS time frame
    //    public byte L2P_Flag { get; set; }       // L2 P data flag
    //                                             //*****************
    //    public double SvAccuracy { get; set; }     // SV accuracy (meters) See GPS ICD 200H Section 20.3.3.3.1.3
    //    public byte SvHealth { get; set; }       // GPS: SV health (bits 17-22 w 3 sf 1)
    //                                             // GLO: 1-bit health flag, 0=healthy, 1=unhealthy
    //    public double Tgd { get; set; }         // TGD (seconds)
    //    public ushort Iodc { get; set; }        // IODC Issue of Data, Clock, BDS:AODC(5bit data)
    //                                            //*****************
    //    public int Ttm { get; set; }            // Transmission time of message, GAL:in GPS time frame
    //    public int FitInterval { get; set; }    // Fit Interval
    //}

    //public class GloNavData
    //{
    //    // SHARING WITH GPS: Prn(svid; slot num), SOW of toc, Week of toc, YMDHMS of toc, SvHealth 
    //    public GrPrn Prn { get; set; }
    //    public int WN_toc { get; set; }         // week number associated with toc (continuous); GLO,GAL,QZS,BDS: in GPS time frame
    //    public int Year { get; set; }           // toc year; GLO,GAL,QZS,BDS: in GPS time frame
    //    public int Month { get; set; }          // toc month; GLO,GAL,QZS,BDS: in GPS time frame
    //    public int Day { get; set; }            // toc day; GLO,GAL,QZS,BDS: in GPS time frame
    //    public int Hour { get; set; }           // toc hour; GLO,GAL,QZS,BDS: in GPS time frame
    //    public int Minute { get; set; }         // toc minute; GLO,GAL,QZS,BDS: in GPS time frame
    //    public double Second { get; set; }      // toc second; GLO,GAL,QZS,BDS: in GPS time frame
    //    public double Toc { get; set; }         // Toc(sow); GLO,GAL,QZS,BDS: in GPS time frame
    //    public int FreqNr { get; set; }         // -7..13
    //    public double Tau { get; set; }         // \tau_n(tb): time correction to GLONASS time [sec]
    //    public double Gamma { get; set; }       // \gamma_n(tb):relative deviation of predicted carrier frequency [1Hz/Hz]
    //    public double X { get; set; }           // x-component of satellite position in PZ-90.02 [km]
    //    public double Y { get; set; }            
    //    public double Z { get; set; }
    //    public double Dx { get; set; }          // x-component of satellite velocity in PZ-90.02 [km/sec]
    //    public double Dy { get; set; }
    //    public double Dz { get; set; }
    //    public double Ddx { get; set; }         // x-component of satellite acceleration in PZ-90.02 [km/sec^2]
    //    public double Ddy { get; set; }
    //    public double Ddz { get; set; }
    //    public byte E { get; set; }             // age of data [day]
    //    public byte SvHealth { get; set; }      // GLO: 1-bit health flag, 0=healthy, 1=unhealthy
    //}

    //public class GalNavData
    //{
    //    public GrPrn Prn { get; set; }
    //    public double Toc { get; set; }         // Toc(sow); GLO,GAL,QZS,BDS: in GPS time frame
    //    public int WN_toc { get; set; }         // week number associated with toc (continuous); GLO,GAL,QZS,BDS: in GPS time frame
    //    public int Year { get; set; }           // toc year; GLO,GAL,QZS,BDS: in GPS time frame
    //    public int Month { get; set; }          // toc month; GLO,GAL,QZS,BDS: in GPS time frame
    //    public int Day { get; set; }            // toc day; GLO,GAL,QZS,BDS: in GPS time frame
    //    public int Hour { get; set; }           // toc hour; GLO,GAL,QZS,BDS: in GPS time frame
    //    public int Minute { get; set; }         // toc minute; GLO,GAL,QZS,BDS: in GPS time frame
    //    public double Second { get; set; }      // toc second; GLO,GAL,QZS,BDS: in GPS time frame
    //    public double A0 { get; set; }          // SV clock bias (seconds), GAL,QZS,BDS
    //    public double A1 { get; set; }          // SV clock drift (sec/sec), GAL,QZS,BDS
    //    public double A2 { get; set; }          // SV clock drift rate (sec/sec2), GAL,QZS,BDS
    //                                            //*****************
    //    //public byte Iode { get; set; }          // IODE Issue of Data, Ephemeris  ?????
    //    public double Crs { get; set; }         // Crs (meters), GAL,QZS,BDS
    //    public double Deln { get; set; }        // Delta n (radians/sec), GAL,QZS,BDS
    //    public double M0 { get; set; }          // M0 (radians), GAL,QZS,BDS
    //                                            //*****************
    //    public double Cuc { get; set; }         // Cuc (radians), GAL,QZS,BDS
    //    public double Ecc { get; set; }         // e Eccentricity, GAL,QZS,BDS
    //    public double Cus { get; set; }         // Cus (radians), GAL,QZS,BDS
    //    public double SqrtA { get; set; }       // sqrt(A) (sqrt(m)), GAL,QZS,BDS
    //                                            //*****************
    //    public int Toe { get; set; }            // Toe Time of Ephemeris (sec of GPS week), GAL,QZS,BDS
    //    public double Cic { get; set; }         // Cic (radians), GAL,QZS,BDS
    //    public double Omega0 { get; set; }      // OMEGA0 (radians), GAL,QZS,BDS
    //    public double Cis { get; set; }         // Cis (radians), GAL,QZS,BDS
    //                                            //*****************
    //    public double I0 { get; set; }          // i0 (radians), GAL,QZS,BDS
    //    public double Crc { get; set; }         // Crc (meters), GAL,QZS,BDS
    //    public double Omega { get; set; }       // Omega (radians), GAL,QZS,BDS
    //    public double OmegaDot { get; set; }    // OMEGA DOT (radians/sec), GAL,QZS,BDS
    //                                            //*****************
    //    public double Idot { get; set; }        // IDOT (radians/sec), GAL,QZS,BDS
    //    //public byte CodeOnL2 { get; set; }      // Codes on L2 channel  ?????????????
    //    public int WeekNum { get; set; }        // GPS Week # (to go with TOE) Continuous number, not mod(1024), GAL,QZS,BDS:in GPS time frame
    //    //public byte L2P_Flag { get; set; }      // L2 P data flag  ??????????????
    //                                            //*****************
    //    //public double SvAccuracy { get; set; }  // SV accuracy (meters) See GPS ICD 200H Section 20.3.3.3.1.3    ?????????????
    //    //public byte SvHealth { get; set; }      // GPS: SV health (bits 17-22 w 3 sf 1)                            ???????????
    //                                            // GLO: 1-bit health flag, 0=healthy, 1=unhealthy                  ???????????
    //    //public double Tgd { get; set; }         // TGD (seconds)             ???????????????
    //    //public ushort Iodc { get; set; }        // IODC Issue of Data, Clock, BDS:AODC(5bit data)        ???????????????
    //                                            //*****************
    //    public int Ttm { get; set; }            // Transmission time of message, GAL:in GPS time frame
    //    //public int FitInterval { get; set; }    // Fit Interval               ????????????????


    //    public int IODNav { get; set; }
    //    public byte Source { get; set; }
    //    public ushort Health_OSSOL { get; set; }

    //    // Health_OSSOLの内容
    //    public bool Health_E1b_Valid { get; set; }
    //    public bool Health_E1b_DVS { get; set; }
    //    public byte Health_E1b_HS { get; set; }
    //    public bool Health_E5b_Valid { get; set; }
    //    public bool Health_E5b_DVS { get; set; }
    //    public byte Health_E5b_HS { get; set; }
    //    public bool Health_E5a_Valid { get; set; }
    //    public bool Health_E5a_DVS { get; set; }
    //    public byte Health_E5a_HS { get; set; }


    //    public double SISA_L1E5a { get; set; }      // SISA index for dual frequency E1-E5a; transformed into [meters], see ICD, source=16のとき..のはず,
    //    public double SISA_L1E5b { get; set; }      // SISA index for dual frequency E1-E5b; transformed into [meters], see ICD, source=2のとき..のはず,
    //    public double SISA_L1AE6A { get; set; }     // reserved
    //    public double BGD_L1E5a { get; set; }       // E1-E5a Broadcast Group Delay [s]
    //    public double BGD_L1E5b { get; set; }       // E1-E5b Broadcast Group Delay [s]
    //    public double BGD_L1AE6A { get; set; }      // reserved


    //}

    //public class QzsNavData : GpsNavData
    //{

    //}

    //public class BdsNavData
    //{
    //    public GrPrn Prn { get; set; }
    //    public double Toc { get; set; }         // Toc(sow); GLO,GAL,QZS,BDS: in GPS time frame
    //    public int WN_toc { get; set; }         // week number associated with toc (continuous); GLO,GAL,QZS,BDS: in GPS time frame
    //    public int Year { get; set; }           // toc year; GLO,GAL,QZS,BDS: in GPS time frame
    //    public int Month { get; set; }          // toc month; GLO,GAL,QZS,BDS: in GPS time frame
    //    public int Day { get; set; }            // toc day; GLO,GAL,QZS,BDS: in GPS time frame
    //    public int Hour { get; set; }           // toc hour; GLO,GAL,QZS,BDS: in GPS time frame
    //    public int Minute { get; set; }         // toc minute; GLO,GAL,QZS,BDS: in GPS time frame
    //    public double Second { get; set; }      // toc second; GLO,GAL,QZS,BDS: in GPS time frame
    //    public double A0 { get; set; }          // SV clock bias (seconds), GAL,QZS,BDS
    //    public double A1 { get; set; }          // SV clock drift (sec/sec), GAL,QZS,BDS
    //    public double A2 { get; set; }          // SV clock drift rate (sec/sec2), GAL,QZS,BDS
    //                                            //*****************
    //    //public byte Iode { get; set; }        // IODE Issue of Data, Ephemeris
    //    public double Crs { get; set; }         // Crs (meters), GAL,QZS,BDS
    //    public double Deln { get; set; }        // Delta n (radians/sec), GAL,QZS,BDS
    //    public double M0 { get; set; }          // M0 (radians), GAL,QZS,BDS
    //                                            //*****************
    //    public double Cuc { get; set; }         // Cuc (radians), GAL,QZS,BDS
    //    public double Ecc { get; set; }         // e Eccentricity, GAL,QZS,BDS
    //    public double Cus { get; set; }         // Cus (radians), GAL,QZS,BDS
    //    public double SqrtA { get; set; }       // sqrt(A) (sqrt(m)), GAL,QZS,BDS
    //                                            //*****************
    //    public int Toe { get; set; }            // Toe Time of Ephemeris (sec of GPS week), GAL,QZS,BDS
    //    public double Cic { get; set; }         // Cic (radians), GAL,QZS,BDS
    //    public double Omega0 { get; set; }      // OMEGA0 (radians), GAL,QZS,BDS
    //    public double Cis { get; set; }         // Cis (radians), GAL,QZS,BDS
    //                                            //*****************
    //    public double I0 { get; set; }          // i0 (radians), GAL,QZS,BDS
    //    public double Crc { get; set; }         // Crc (meters), GAL,QZS,BDS
    //    public double Omega { get; set; }       // Omega (radians), GAL,QZS,BDS
    //    public double OmegaDot { get; set; }    // OMEGA DOT (radians/sec), GAL,QZS,BDS
    //                                            //*****************
    //    public double Idot { get; set; }        // IDOT (radians/sec), GAL,QZS,BDS
    //    public int WeekNum { get; set; }        // GPS Week # (to go with TOE) Continuous number, not mod(1024), GAL,QZS,BDS:in GPS time frame
    //                                            //*****************
    //    public double SvAccuracy { get; set; }  // SV accuracy (meters) See GPS ICD 200H Section 20.3.3.3.1.3  ???????????????
    //                                            //*****************
    //    public byte SatH1 { get; set; }         // 1bit autonomous health
    //    public int Ttm { get; set; }            // Transmission time of message, GAL:in GPS time frame
    //    public double Tgd1 { get; set; }        // B1/B3 (seconds)
    //    public double Tgd2 { get; set; }        // B2/B3 (seconds)

    //    public int Aodc { get; set; }
    //    public int Aode { get; set; }

    //}

    public class IrnNavData
    {

    }

    public class SbsNavData
    {

    }


    public class ComNavGEJC
    {
        public SYS Sys { get; set; }
        public double A0 { get; set; }
        public double A1 { get; set; }
        public double A2 { get; set; }
        public double Deln { get; set; }
        public double M0 { get; set; }
        public double Ecc { get; set; }
        public double SqrtA { get; set; }
        public double Toe { get; set; }
        public double Toc { get; set; }
        public double Crs { get; set; }
        public double Cuc { get; set; }
        public double Cus { get; set; }
        public double Cic { get; set; }
        public double Omega0 { get; set; }
        public double Cis { get; set; }
        public double I0 { get; set; }
        public double Crc { get; set; }
        public double Omega { get; set; }
        public double OmegaDot { get; set; }
        public double Idot { get; set; }

        public double A { get; set; }
        public double N0 { get; set; }
        public double N { get; set; }
    }

}
