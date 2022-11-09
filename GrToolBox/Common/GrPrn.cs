using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static GrToolBox.Common.CommonUtilities;
using static GrToolBox.Common.CommonDefinitions;
using static GrToolBox.Data.DataTypeDefinitions;
using System.ComponentModel;
using static GrToolBox.Settings.SettingsDefinitions;

namespace GrToolBox.Common
{
    /// <summary>
    /// PRN処理用クラス
    /// </summary>
    public class GrPrn
    {
        public int N { get; }
        public string Snn { get; }
        public SYS Sys { get; }
        public int ISys { get; }
        public int Offset { get; }
        public char SysChar { get; }

        /// <summary>
        /// PRN(GR)を指定
        /// </summary>
        /// <param name="prn"></param>
        public GrPrn(int prn)
        {
            N = prn;
            Sys = GetSys(prn);
            ISys = GetISys(Sys);
            Offset = GetPrnOffset(Sys);
            SysChar = GetSysChar(Sys);
            Snn = $"{SysChar}{prn - GetPrnOffset(prn):00}";
        }

        /// <summary>
        /// Snn(RINEX)を指定
        /// </summary>
        /// <param name="snn"></param>
        public GrPrn(string snn) : this(Snn2Prn(snn)) { }

        /// <summary>
        /// svidとデータタイプ（SBF, RTCM，NMEA，USX等を指定）
        /// </summary>
        /// <param name="svid"></param>
        /// <param name="type"></param>
        public GrPrn(int svid, Data_Type type)
        {
            switch (type)
            {
                case Data_Type.SBF:
                    var tmp = new GrPrn(Data.SBF.SbfUtilities.SVID2SNN((byte)svid));
                    N = tmp.N;
                    Snn = tmp.Snn;
                    Sys = tmp.Sys;
                    ISys = tmp.ISys;
                    Offset = tmp.Offset;
                    SysChar = tmp.SysChar;
                    break;
            }
        }

        // オブジェクト名だけでアクセスしたら，prn.Nをintで返す
        public static implicit operator int(GrPrn obj)
        {
            return obj.N;
        }
    }
}
