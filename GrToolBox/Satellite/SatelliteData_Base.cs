using GrToolBox.Common;

namespace GrToolBox.Satellite
{
    public class SatelliteData_Base
    {
        public GrPrn Prn { get; set; }
        //public int Prn { get; set; }
        //public int Isys { get; set; }
        //public char Syschar { get; set; }
        public double[] Pos { get; set; } = null;
        public double Ele { get; set; } = double.NaN;
        public double Azi { get; set; } = double.NaN;
        public int HaveCodeData { get; set; } = 0;      // 測位用データの有無フラグ 3bit: [L5, L2, L1], ex. 0x001: L1データのみ利用可能，0x101: L1，L5データが利用可能
        public int HavePhaseData { get; set; } = 0;
        public int Status { get; set; } = 0;    // enum Sat_Statusで定義された位置がtrueになる

        public double[] Cn0 { get; set; } = { double.NaN, double.NaN, double.NaN };


        public SatelliteData_Base(GrPrn prn)
        {
            this.Prn = prn;
            //this.Syschar = PRN2SYS(prn);
            //this.Isys = ISYS(this.Syschar);
        }

        public SatelliteData_Base() { }


        public void CopyValueFrom(SatelliteData sat)
        {
            Prn = sat.Prn;
            //Isys = sat.Isys;
            //Syschar = sat.Syschar;
            //Pos = new double[3] { sat.Pos[0], sat.Pos[1], sat.Pos[2] };
            Pos = sat.Pos;
            Ele = sat.Ele;
            Azi = sat.Azi;
            HaveCodeData = sat.HaveCode;
            HavePhaseData = sat.HavePhase;
            Status = sat.Status;
        }
    }
}
