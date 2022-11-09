using GrToolBox.Time;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static GrToolBox.Data.Rinex.RinexUtilities;
using static GrToolBox.Time.TimeUtilities;
using static GrToolBox.Satellite.SatelliteConstants;
using GrToolBox.Data.GrNavData;
using static GrToolBox.Common.CommonDefinitions;
using static GrToolBox.Common.CommonUtilities;

namespace GrToolBox.Data.Rinex
{
    public class RinexNav
    {
        public GrNavData.NavData NavData { get; set; } = new();
        private RinexNavHeader Header { get; set; } = new();

        // ヘッダデータ用クラス
        private class RinexNavHeader
        {
            public string Version { get; set; }
            public char Type { get; set; }
            public char Syst { get; set; }
            public int NHeadLines { get; set; }
            public Dictionary<string, double[]> Ion { get; set; } = new Dictionary<string, double[]>();
            public Dictionary<string, double[]> Tcorr { get; set; } = new Dictionary<string, double[]>();
        }


        public RinexNav(List<string> fnNav)
        {
            foreach (string fn in fnNav)
            {
                var Raw = File.ReadAllLines(fn, Encoding.UTF8);
                ReadHeader(Raw);
                ReadData(Raw);
            }
            //int aaa = 0;
        }


        /*-----------------
         private methods 
        -----------------*/

        // ヘッダ読み込み
        private void ReadHeader(string[] Raw)
        {
            string row, tmp;
            int nLines = Raw.Length;
            int cline = 0;
            while (cline < nLines)
            {
                row = Raw[cline];
                cline++;
                if (row.Contains("END OF HEADER"))
                {
                    break;
                }
                if (row.Contains("RINEX VERSION / TYPE"))
                {
                    Header.Version = row[5..9];
                    Header.Type = row[20];
                    Header.Syst = row[40];  // v3
                    continue;
                }
                if (row.Contains("ION ALPHA"))
                {
                    Check_and_Put(Header.Ion, row, "Alpha", 2, new int[] { 12, 12, 12, 12 });
                    continue;
                }
                if (row.Contains("ION BETA"))
                {
                    Check_and_Put(Header.Ion, row, "Beta", 2, new int[] { 12, 12, 12, 12 });
                    continue;
                }

                if (row.Contains("IONOSPHERIC CORR"))
                {
                    tmp = row[0..4].Trim();
                    switch (tmp)
                    {
                        case "GPSA":
                            Check_and_Put(Header.Ion, row, "GPSA", 5, new int[] { 12, 12, 12, 12 });
                            break;
                        case "GPSB":
                            Check_and_Put(Header.Ion, row, "GPSB", 5, new int[] { 12, 12, 12, 12 });
                            break;
                        case "QZSA":
                            Check_and_Put(Header.Ion, row, "QZSA", 5, new int[] { 12, 12, 12, 12 });
                            break;
                        case "QZSB":
                            Check_and_Put(Header.Ion, row, "QZSB", 5, new int[] { 12, 12, 12, 12 });
                            break;
                        case "GAL":
                            Check_and_Put(Header.Ion, row, "GAL", 5, new int[] { 12, 12, 12, 12 });
                            break;
                        case "BDSA":
                            Check_and_Put(Header.Ion, row, "BDSA", 5, new int[] { 12, 12, 12, 12 });
                            break;
                        case "BDSB":
                            Check_and_Put(Header.Ion, row, "BDSB", 5, new int[] { 12, 12, 12, 12 });
                            break;
                        case "IRNA":
                            Check_and_Put(Header.Ion, row, "IRNA", 5, new int[] { 12, 12, 12, 12 });
                            break;
                        case "IRNB":
                            Check_and_Put(Header.Ion, row, "IRNB", 5, new int[] { 12, 12, 12, 12 });
                            break;
                    }
                    continue;
                }
                if (row.Contains("CORR TO SYSTEM TIME"))
                {      // ver2
                    Check_and_Put(Header.Tcorr, row, "Dtsys", 0, new int[] { 6, 6, 6, 22 });
                    continue;
                }
                if (row.Contains("DELTA-UTC"))
                {   // ver2
                    Check_and_Put(Header.Tcorr, row, "Utc", 3, new int[] { 19, 19, 9, 9 });
                    continue;
                }
                if (row.Contains("TIME SYSTEM CORR"))
                {
                    tmp = row[0..4];
                    switch (tmp)
                    {
                        case "GPUT":
                            Check_and_Put(Header.Tcorr, row, "GPUT", 5, new int[] { 17, 16, 7, 5 });     //後ろの7,5はフォーマット上はintだがdoubleで読む
                            break;
                        case "GPGA":
                            Check_and_Put(Header.Tcorr, row, "GPGA", 5, new int[] { 17, 16, 7, 5 });
                            break;
                        case "GAUT":
                            Check_and_Put(Header.Tcorr, row, "GAUT", 5, new int[] { 17, 16, 7, 5 });
                            break;
                        case "SBUT":
                            Check_and_Put(Header.Tcorr, row, "SBUT", 5, new int[] { 17, 16, 7, 5 });
                            break;
                        case "GLUT":
                            Check_and_Put(Header.Tcorr, row, "GLUT", 5, new int[] { 17, 16, 7, 5 });
                            break;
                        case "GLGP":
                            Check_and_Put(Header.Tcorr, row, "GLGP", 5, new int[] { 17, 16, 7, 5 });
                            break;
                        case "QZUT":
                            Check_and_Put(Header.Tcorr, row, "QZUT", 5, new int[] { 17, 16, 7, 5 });
                            break;
                        case "QZGP":
                            Check_and_Put(Header.Tcorr, row, "QZGP", 5, new int[] { 17, 16, 7, 5 });
                            break;
                        case "BDUT":
                            Check_and_Put(Header.Tcorr, row, "BDUT", 5, new int[] { 17, 16, 7, 5 });
                            break;
                        case "IRUT":
                            Check_and_Put(Header.Tcorr, row, "IRUT", 5, new int[] { 17, 16, 7, 5 });
                            break;
                        case "IRGP":
                            Check_and_Put(Header.Tcorr, row, "IRGP", 5, new int[] { 17, 16, 7, 5 });
                            break;
                    }
                    continue;
                }
                if (row.Contains("LEAP SECONDS"))
                {
                    Check_and_Put(Header.Tcorr, row, "Leap", 0, new int[] { 6, 6, 6, 6 });
                }
            }
            Header.NHeadLines = cline;
        }

        // 複数ファイルを読み込むので，既出か否かをチェックして新しいヘッダ情報のみを格納する
        private void Check_and_Put(Dictionary<string, double[]> Map, string row, string type, int offset, int[] format)
        {
            if (!Map.ContainsKey(type))
            {
                Map.Add(type, new double[] { 0.0, 0.0, 0.0, 0.0 });  //todo: NaNの方が良いか？考える
            }
            double[] test = Map[type];
            if ((test[0] == 0.0) & (test[1] == 0.0) & (test[2] == 0.0) & (test[3] == 0.0))
            {
                double[] read = FormattedreadD(row, offset, format);
                Map[type] = read;
            }
        }


        private void ReadData(string[] Raw)
        {
            int nRawLines = Raw.Length;
            int cline = Header.NHeadLines;
            string row;
            int[][] format = new int[8][];
            int[] offset = new int[8];
            SYS sys = SYS.UNKNOWN;

            // システム識別子の設定（これ以外(ver3)はデータの先頭から決定）
            char syslet = ' ';
            //if (Header.Type == 'G') syslet = 'R';                           // ver2-Glonass
            //if (Header.Type == 'H') syslet = 'S';                           // ver2-SBAS
            //if (Header.Type == 'N' && Header.Syst == ' ') syslet = 'G';     // ver2-GPS
            if (Header.Type == 'G') sys = SYS.GLO;                           // ver2-Glonass
            if (Header.Type == 'H') sys = SYS.SBS;                           // ver2-SBAS
            if (Header.Type == 'N' && Header.Syst == ' ') sys = SYS.GPS;     // ver2-GPS

            //読み込みフォーマット設定  -------------------
            char nVer = Header.Version[0];
            if (nVer == '2')
            {
                offset[0] = 0;
                format[0] = new int[] { 3, 3, 3, 3, 3, 2, 5, 19, 19, 19 };
                for (int i = 1; i < 8; i++)
                {
                    offset[i] = 3;
                    format[i] = new int[] { 19, 19, 19, 19 };
                }
            }
            else if (nVer == '3')
            {
                offset[0] = 1;
                format[0] = new int[] { 3, 5, 3, 3, 3, 3, 2, 19, 19, 19 };
                for (int i = 1; i < 8; i++)
                {
                    offset[i] = 4;
                    format[i] = new int[] { 19, 19, 19, 19 };
                }
            }

            while (cline < nRawLines)
            {
                //1ブロック分データ読み込み  -------------------
                int nLineToBeRead; // = 0;
                double[][] rtemp;
                if (nVer == '3')
                {
                    row = Raw[cline];
                    sys = GetSys(row[0]);
                }
                if (sys == SYS.GLO || sys == SYS.SBS)
                {   //読み取り行数の決定
                    nLineToBeRead = 4;
                    rtemp = new double[5][];    // Glonass, Sbasの場合，追加データ格納のため1行多く用意する
                }
                else
                {
                    nLineToBeRead = 8;
                    rtemp = new double[8][];
                }
                for (int i = 0; i < nLineToBeRead; i++)
                {   //1-8or4行目読み取り
                    row = Raw[cline];
                    cline++;
                    rtemp[i] = FormattedreadD(row, offset[i], format[i]);
                }
                rtemp[0][0] = rtemp[0][0] + GetPrnOffset(sys);  // GrのPRN(通し番号)に変換
                if (nVer == '2')
                {
                    if (rtemp[0][1] < 80) rtemp[0][1] += 2000;
                    else if (rtemp[0][1] >= 80) rtemp[0][1] += 1900;
                }
                if (sys == SYS.GLO)
                {   // Rの場合は5行目データとしてUTC->GPS変換(閏秒除去)したtocと衛星スロットに対応した周波数を格納
                    //var rawT = new TimeGR((int)rtemp[0][1], (int)rtemp[0][2], (int)rtemp[0][3], (int)rtemp[0][4], (int)rtemp[0][5], rtemp[0][6]);
                    //double[] tmp = { -1.0 };

                    //double leap = -1.0;
                    //if (Header.Tcorr.ContainsKey("Leap"))   // leap second の記録があればそれを使う
                    //{
                    //    leap = Header.Tcorr["Leap"][0];
                    //}
                    //if (leap <= 0)                          // leap second の記録が無ければテーブルを参照する
                    //{
                    //    leap = GetLeapSecond(rawT);
                    //}
                    //int Rweek = rawT.Week;
                    //double Rsow = rawT.Sow + leap;
                    //rtemp[4] = new double[4];
                    //rawT = new TimeGR(Rweek, Rsow);
                    //rtemp[4][0] = rawT.Week;  // 5行1項目として閏秒除去後のweekを格納
                    //rtemp[4][1] = rawT.Sow;   // 5行2項目として閏秒除去後のsowを格納
                    //rtemp[4][2] = rtemp[2][3] * FR1SEPARATION + FR1;
                    //rtemp[4][3] = rtemp[2][3] * FR2SEPARATION + FR2;
                }
                if (sys == SYS.SBS)
                {
                    var rawT = new TimeGR((int)rtemp[0][1], (int)rtemp[0][2], (int)rtemp[0][2], (int)rtemp[0][2], (int)rtemp[0][2], rtemp[0][2]);
                    rtemp[4][0] = rawT.Week;  // 5行1項目としてweekを格納
                    rtemp[4][1] = rawT.Sow;   // 5行2項目としてsowを格納
                }

                //int[] result = array.SelectMany(x => x).ToArray();
                //double[] addData = VecFrom2D_D(rtemp);

                // 2次元ジャグ配列を1次元に
                //double[] addData = rtemp.SelectMany(x => x).ToArray();

                //Data.Eph.Add(addData);
                //nRead++;

                // 1ブロック分読み込み完了 GR形式に変換して保持

                switch (sys)
                {
                    case SYS.GPS:
                        NavData.Add(RinGpsNav2GR(rtemp));
                        break;
                    case SYS.GLO:
                        NavData.Add(RinGloNav2GR(rtemp, Header));
                        break;
                    case SYS.GAL:
                        NavData.Add(RinGalNav2GR(rtemp, Header));
                        break;
                    case SYS.QZS:
                        NavData.Add(RinQzsNav2GR(rtemp));
                        break;
                }



            }
            //int aaa = 1;
        }

        private static GpsNavData RinGpsNav2GR(double[][] data)
        {
            GpsNavData nav = new();
            // 1行目
            nav.Prn = new Common.GrPrn((int)data[0][0]);
            nav.Year = (int)data[0][1];
            nav.Month = (int)data[0][2];
            nav.Day = (int)data[0][3];
            nav.Hour = (int)data[0][4];
            nav.Minute = (int)data[0][5];
            nav.Second = data[0][6];
            nav.A0 = data[0][7];
            nav.A1 = data[0][8];
            nav.A2 = data[0][9];
            // 2行目
            nav.Iode = (byte)data[1][0];
            nav.Crs = data[1][1];               // m
            nav.Deln = data[1][2];              // rad/sec
            nav.M0 = data[1][3];                // rad
            // 3行目
            nav.Cuc = data[2][0];               // rad/sec
            nav.Ecc = data[2][1];
            nav.Cus = data[2][2];               // rad
            nav.SqrtA = data[2][3];             // meters^{1/2}
            // 4行目
            nav.Toe = (int)data[3][0];          // Toe Time of Ephemeris (sec of GPS week)
            nav.Cic = data[3][1];               // rad
            nav.Omega0 = data[3][2];            // rad
            nav.Cis = data[3][3];               // rad
            // 5行目
            nav.I0 = data[4][0];                // rad
            nav.Crc = data[4][1];               // meters
            nav.Omega = data[4][2];             // rad
            nav.OmegaDot = data[4][3];          // rad/sec
            // 6行目
            nav.Idot = data[5][0];              // rad/sec
            nav.CodeOnL2 = (byte)data[5][1];
            nav.WeekNum = (int)data[5][2];      // GPS Week # (to go with TOE) Continuous number, not mod(1024)
            //nav.L2P_Flag = (byte)data[5][3];
            nav.L2P_Flag = (data[5][3] == 1.0);
            // 7行目
            nav.SvAccuracy = data[6][0];        // SV accuracy (meters)
            nav.SvHealth = (byte)data[6][1];    // GPS: SV health
            nav.Tgd = data[6][2];               // TGD(sec)
            nav.Iodc = (ushort)data[6][3];      // IODC Issue of Data
            // 7行目
            nav.Ttm = (int)data[7][0];          // Transmission time of message
            nav.FitInterval = (int)data[7][1];  // Fit Interval in hours
            // GRitz specific
            var t = new TimeGR(nav.Year, nav.Month, nav.Day, nav.Hour, nav.Minute, nav.Second);
            nav.WN_toc = t.Week;
            nav.Toc = t.Sow;
            return nav;
        }

        private static GloNavData RinGloNav2GR(double[][] data, RinexNavHeader Header)
        {
            GloNavData nav = new();

            var toc_utc = new TimeGR((int)data[0][1], (int)data[0][2], (int)data[0][3], (int)data[0][4], (int)data[0][5], data[0][6]);
            double leap = -1.0;
            if (Header.Tcorr.ContainsKey("Leap"))   // leap second の記録があればそれを使う
            {
                leap = Header.Tcorr["Leap"][0];
            }
            if (leap <= 0)                          // leap second の記録が無ければテーブルを参照する
            {
                leap = GetLeapSecond(toc_utc);
            }
            var toc_gps = new TimeGR(toc_utc.Week, toc_utc.Sow + leap);
            // 1行目
            nav.Prn = new Common.GrPrn((int)data[0][0]);
            nav.Year = toc_gps.Year;
            nav.Month = toc_gps.Month;
            nav.Day = toc_gps.Day;
            nav.Hour = toc_gps.Hour;
            nav.Minute = toc_gps.Minute;
            nav.Second = toc_gps.Second;
            // 2行目
            nav.X = data[1][0];                 // km
            nav.Dx = data[1][1];                // km/s
            nav.Ddx = data[1][2];               // km/s^2
            nav.SvHealth = (byte)data[1][3];
            // 3行目
            nav.Y = data[2][0];                 // km
            nav.Dy = data[2][1];                // km/s
            nav.Ddy = data[2][2];               // km/s^2
            nav.FreqNr = (int)data[2][3];       // -7..13
            // 4行目
            nav.Z = data[3][0];                 // km
            nav.Dz = data[3][1];                // km/s
            nav.Ddz = data[3][2];               // km/s^2
            nav.E = (byte)data[3][3];           // Age of oper. information (days) (E)
            // GRitz specific
            nav.WN_toc = toc_gps.Week;
            nav.Toc = toc_gps.Sow;
            return nav;
        }

        private static GalNavData RinGalNav2GR(double[][] data, RinexNavHeader Header)
        {
            GalNavData nav = new();
            // 1行目
            nav.Prn = new Common.GrPrn((int)data[0][0]);
            nav.Year = (int)data[0][1];
            nav.Month = (int)data[0][2];
            nav.Day = (int)data[0][3];
            nav.Hour = (int)data[0][4];
            nav.Minute = (int)data[0][5];
            nav.Second = data[0][6];
            nav.A0 = data[0][7];
            nav.A1 = data[0][8];
            nav.A2 = data[0][9];
            // 2行目
            nav.IODNav = (int)data[1][0];
            nav.Crs = data[1][1];               // m
            nav.Deln = data[1][2];              // rad/sec
            nav.M0 = data[1][3];                // rad
            // 3行目
            nav.Cuc = data[2][0];               // rad/sec
            nav.Ecc = data[2][1];
            nav.Cus = data[2][2];               // rad
            nav.SqrtA = data[2][3];             // meters^{1/2}
            // 4行目
            nav.Toe = (int)data[3][0];          // Toe Time of Ephemeris (sec of GPS/GAL week)
            nav.Cic = data[3][1];               // rad
            nav.Omega0 = data[3][2];            // rad
            nav.Cis = data[3][3];               // rad
            // 5行目
            nav.I0 = data[4][0];                // rad
            nav.Crc = data[4][1];               // meters
            nav.Omega = data[4][2];             // rad
            nav.OmegaDot = data[4][3];          // rad/sec
            // 6行目(0)
            nav.Idot = data[5][0];              // rad/sec
            // 6行目(1)
            int s = (int)data[5][1];
            if (s == 258)                       // 0x102 (258) for F/NAV from E5a
            {
                nav.Source = 16;
            }
            else if (s == 513 || s == 516 || s == 517)  // INav
            {   // 0x201 (513) for I/NAV from E1
                // 0x204 (516) for I/NAV from E5b
                // 0x205 (517) for I/NAV from E1+E5b
                nav.Source = 2;
            }
            else                                // invalid
            {
                nav.Source = 0;
            }
            // 6行目(2)
            int wgal = (int)data[5][2];         // GAL Week # (to go with TOE), mod 4096 
                                                // (GPS week#との差は1024, GALは約78年でロールオーバ,2078年頃？)
            nav.WeekNum = wgal + 1024;          // GPS Week # (to go with TOE) Continuous number, not mod(1024)
            // data[5][3] is spare

            // 7行目(0)
            if(nav.Source == 16)
            {
                nav.SISA_L1E5a = data[6][0];
            }
            else if(nav.Source == 2)
            {
                nav.SISA_L1E5b = data[6][0];
            }
            // 7行目(1)
            switch (s)
            {
                case 258:   // 0x102 (258) for F/NAV from E5a
                    nav.Health_E5a_Valid = true;
                    nav.Health_E5b_Valid = false;
                    nav.Health_E1b_Valid = false;
                    break;
                case 513:   // 0x201 (513) for I/NAV from E1
                    nav.Health_E5a_Valid = false;
                    nav.Health_E5b_Valid = false;
                    nav.Health_E1b_Valid = true;
                    break;
                case 516:   // 0x204 (516) for I/NAV from E5b
                    nav.Health_E5a_Valid = false;
                    nav.Health_E5b_Valid = true;
                    nav.Health_E1b_Valid = false;
                    break;
                case 517:   // 0x205 (517) for I/NAV from E1+E5b
                    nav.Health_E5a_Valid = false;
                    nav.Health_E5b_Valid = true;
                    nav.Health_E1b_Valid = true;
                    break;
            }
            ushort health = (ushort)data[6][1];
            nav.Health_E1b_DVS = (health & 0b_0000_0000_0000_0001) > 0;
            nav.Health_E1b_HS = (byte)((health & 0b_0000_0000_0000_0110) >> 1);
            nav.Health_E5a_DVS = (health & 0b_0000_0000_0000_1000) > 0;
            nav.Health_E5a_HS = (byte)((health & 0b_0000_0000_0011_0000) >> 4);
            nav.Health_E5b_DVS = (health & 0b_0000_0000_0100_0000) > 0;
            nav.Health_E5b_HS = (byte)((health & 0b_0000_0001_1000_0000) >> 7);
            // 7行目(2)
            nav.BGD_L1E5a = data[6][2];
            // 7行目(3)
            nav.BGD_L1E5b = data[6][3];
            // 8行目
            nav.Ttm = (int)data[7][0];
            return nav;
        }


        private static QzsNavData RinQzsNav2GR(double[][] data)
        {
            QzsNavData nav = new();
            // 1行目
            nav.Prn = new Common.GrPrn((int)data[0][0]);
            nav.Year = (int)data[0][1];
            nav.Month = (int)data[0][2];
            nav.Day = (int)data[0][3];
            nav.Hour = (int)data[0][4];
            nav.Minute = (int)data[0][5];
            nav.Second = data[0][6];
            nav.A0 = data[0][7];
            nav.A1 = data[0][8];
            nav.A2 = data[0][9];
            // 2行目
            nav.Iode = (byte)data[1][0];
            nav.Crs = data[1][1];               // m
            nav.Deln = data[1][2];              // rad/sec
            nav.M0 = data[1][3];                // rad
            // 3行目
            nav.Cuc = data[2][0];               // rad/sec
            nav.Ecc = data[2][1];
            nav.Cus = data[2][2];               // rad
            nav.SqrtA = data[2][3];             // meters^{1/2}
            // 4行目
            nav.Toe = (int)data[3][0];          // Toe Time of Ephemeris (sec of GPS week)
            nav.Cic = data[3][1];               // rad
            nav.Omega0 = data[3][2];            // rad
            nav.Cis = data[3][3];               // rad
            // 5行目
            nav.I0 = data[4][0];                // rad
            nav.Crc = data[4][1];               // meters
            nav.Omega = data[4][2];             // rad
            nav.OmegaDot = data[4][3];          // rad/sec
            // 6行目
            nav.Idot = data[5][0];              // rad/sec
            nav.CodeOnL2 = (byte)data[5][1];
            nav.WeekNum = (int)data[5][2];      // GPS Week # (to go with TOE) Continuous number, not mod(1024)
            //nav.L2P_Flag = (byte)data[5][3];
            nav.L2P_Flag = (data[5][3] == 1.0);
            // 7行目
            nav.SvAccuracy = data[6][0];        // SV accuracy (meters)
            nav.SvHealth = (byte)data[6][1];    // GPS: SV health
            nav.Tgd = data[6][2];               // TGD(sec)
            nav.Iodc = (ushort)data[6][3];      // IODC Issue of Data
            // 7行目
            nav.Ttm = (int)data[7][0];          // Transmission time of message
            nav.FitInterval = (int)data[7][1];  // Fit Interval in hours
            return nav;
        }


    }
}
