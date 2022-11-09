using GrToolBox.Common;
using GrToolBox.Corrections;
using GrToolBox.Data.GrNavData;
using GrToolBox.Data.GrObsData;
using System.Collections.Generic;

namespace GrToolBox.Satellite
{
    public class SatelliteData
    {
        public GrPrn Prn { get; }
        public double[] Pos { get; set; } = new double[3] { double.NaN, double.NaN, double.NaN };
        public double[] PosRot { get; set; } = new double[3] { double.NaN, double.NaN, double.NaN };
        public double[] Vel { get; set; } = new double[3] { double.NaN, double.NaN, double.NaN };
        public double Dtsv_E1_FNAV { get; set; } = double.NaN;
        public double Ele { get; set; } = double.NaN;
        public double Azi { get; set; } = double.NaN;
        public double[] Dcs { get; set; } = new double[3] { double.NaN, double.NaN, double.NaN };

        public List<RawData> Raws { get; set; } = new List<RawData>();

        public GpsNavData GPS_NavData { get; set; } = null;
        public GalNavData GAL_FNavData { get; set; } = null;   // for (E1,E5a)-dual, E5a-single
        public GalNavData GAL_INavData { get; set; } = null;   // for (E1,E5b)-dual, E1, E5b-single
        public QzsNavData QZS_NavData { get; set; } = null;
        public BdsNavData BDS_NavData { get; set; } = null;


        public byte Status { get; set; } = 0;    // enum Sat_Statusで定義された位置がtrueになる
        public byte HaveCode { get; set; } = 0;      // 測位用データの有無フラグ 3bit: [L5, L2, L1], ex. 0b_001: L1データのみ利用可能，0b_101: L1，L5データが利用可能
        public byte HavePhase { get; set; } = 0;
        public byte HaveDoppler { get; set; } = 0;
        public byte HaveIFCode { get; set; } = 0;      // 測位用IFデータの有無フラグ 2bit: [(L1-L5),(L1-L2)], ex. 0b_01: L1-L2のIFデータのみ利用可能，0b_10: L1-L5データが利用可能
        public byte HaveIFPhase { get; set; } = 0;

        // 測位に利用するデータ
        // 擬似距離，位相，ドップラ(3つ)
        public double[] PVT_Code { get; set; } = new double[3] { double.NaN, double.NaN, double.NaN };
        public double[] PVT_Phase { get; set; } = new double[3] { double.NaN, double.NaN, double.NaN };
        public double[] PVT_Doppler { get; set; } = new double[3] { double.NaN, double.NaN, double.NaN };
        public double[] PVT_Cn0 { get; set; } = new double[3] { double.NaN, double.NaN, double.NaN };
        public bool[] PVT_HalfCycleAmb { get; set; } = new bool[3] { false, false, false };     // 位相 half cycle flag
        public string[] PVT_ObsCode { get; set; } = new string[3] { "", "", "" };
        public double[] PVT_Dtsv { get; set; } = new double[3] { double.NaN, double.NaN, double.NaN };
        public double[] PVT_Tgd { get; set; } = new double[3] { double.NaN, double.NaN, double.NaN };
        // 電離層フリー擬似距離，位相（2つ, GPS,QZS:C1-C2,C1-C5, GAL:E1-E5a,E1-E5b, BDS:B1I-B2I,B1I-B3I）
        public double[] PVT_IFCode { get; set; } = new double[2] { double.NaN, double.NaN };
        public double[] PVT_IFPhase { get; set; } = new double[2] { double.NaN, double.NaN };
        public double[] PVT_IFDtsv { get; set; } = new double[2] { double.NaN, double.NaN };

        // 測位に利用するデータ
        // 擬似距離データ(3つ)
        //public double[] Code { get; set; } = new double[3] { double.NaN, double.NaN, double.NaN };
        public int[] LliCode { get; set; } = new int[3] { 0, 0, 0 };
        public int[] SginalStrengthCode { get; set; } = new int[3] { 0, 0, 0 };
        // 位相観測値データ(3つ)
        //public double[] Phase { get; set; } = new double[3] { double.NaN, double.NaN, double.NaN };
        public int[] LliPhase { get; set; } = new int[3] { 0, 0, 0 };
        public int[] SginalStrengthPhase { get; set; } = new int[3] { 0, 0, 0 };
        public bool[] HalfCycleAmb { get; set; } = new bool[3] { false, false, false };     // 位相 half cycle flag

        public double Weight { get; set; } = 1.0;
        public TropData Trop { get; set; } = new TropData();


        public SatelliteData(GrPrn prn)
        {
            Prn = prn;
            //Sys = GetSys(prn);
        }
    }
}
