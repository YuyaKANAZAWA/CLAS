using System.Diagnostics;
using static GrToolBox.Common.CommonDefinitions;
using static GrToolBox.Data.CLAS2.ClasDefinitions;

namespace GrToolBox.Data.CLAS2
{
    public class ClasSSR
    {
        public ST01_Mask? ST01_Mask { get; set; }
        public ST02_Orbit? ST02_Orbit { get; set; }
        public ST03_Clock? ST03_Clock { get; set; }
        public ST04_CodeBias? ST04_CodeBias { get; set; }
        public ST05_PhaseBias? ST05_PhaseBias { get; set; }
        public ST06_CodePhaseBias? ST06_CodePhaseBias { get; set; }
        public ST07_URA? ST07_URA { get; set;}
        
        public ST11_Combined? ST11_Combined { get; set; }

        //public SubType SubTypeWaiting { get; set; } = SubType.Next;      // multiple messageでデータ待ちのSubtypeフラグ

        public SubTypeStatus Status { get; set; } = SubTypeStatus.SearchHeader;

        //private SubType Target { get; set; } = SubType.Next;

        private List<ClasSSRData> Data { get; set; } = new List<ClasSSRData>(); // 衛星数分
        private int NMessage { get; set; } = -1;

        private BitCircularBuffer Bcb { get; set; } = new BitCircularBuffer(12);    // 2^12byte用意


        public bool AddClasMessage(ClasMessage cm)
        {
            NMessage++;
            Debug.WriteLine($">>> Message: {NMessage}");
            //マルチ中かどうかの判断
            ClasHeaderCommon? head = null;

            if(Bcb.NumWritable() > 1695)
            {
                Bcb.AddByteArray(cm.Bytes, 49, 1695);
            }
            else
            {
                return false;   // データadd出来ないときにfalse
            }

            Status = SubTypeStatus.SearchHeader;
            while(Status == SubTypeStatus.SearchHeader)
            {
                head = new ClasHeaderCommon();
                Status = head.Search_Decode(Bcb);

                //Status = SubTypeStatus.SearchHeader;

                if (Status != SubTypeStatus.HeaderDone) break;
                Bcb.SetMark();
                switch (head.Data.ID)
                {
                    case SubType.ST1_Mask:
                        ST01_Mask = new ST01_Mask(head.Data);
                        Status = ST01_Mask.Decode(Bcb, Data);
                        break;
                    case SubType.ST2_Orbit:
                        ST02_Orbit = new ST02_Orbit(head.Data);
                        Status = ST02_Orbit.Decode(Bcb, Data);
                        break;
                    case SubType.ST3_Clock:
                        ST03_Clock = new ST03_Clock(head.Data);
                        Status = ST03_Clock.Decode(Bcb, Data);
                        break;
                    case SubType.ST4_CodeBias:
                        ST04_CodeBias = new ST04_CodeBias(head.Data);
                        Status = ST04_CodeBias.Decode(Bcb, Data);
                        break;
                    case SubType.ST5_PhaseBias:
                        ST05_PhaseBias = new ST05_PhaseBias(head.Data);
                        Status = ST05_PhaseBias.Decode(Bcb, Data);
                        break;
                    case SubType.ST6_CodePhaseBias:
                        ST06_CodePhaseBias = new ST06_CodePhaseBias(head.Data);
                        Status = ST06_CodePhaseBias.Decode(Bcb, Data);
                        break;
                    case SubType.ST7_URA:
                        ST07_URA = new ST07_URA(head.Data);
                        Status = ST07_URA.Decode(Bcb, Data);
                        break;
                    case SubType.ST11_Combined:
                        ST11_Combined = new ST11_Combined(head.Data);
                        Status = ST11_Combined.Decode(Bcb, Data);
                        break;
                }
                if(Status == SubTypeStatus.DataDone)
                {
                    Status = SubTypeStatus.SearchHeader;
                }else if(Status == SubTypeStatus.NeedMoreData)
                {
                    Bcb.BackToMark();
                }
                else
                {
                    Bcb.BackToMark();
                    Bcb.ProceedBit(1);
                }

            }

            return true;


            //while (cm.Pos < 1744)        // 1744: RSCの手前まで
            //{
            //    if (SubTypeWaiting == SubType.Next)
            //    {
            //        head = new ClasSubTypeHeader_base();      // subtype header, common part
            //        if (!head.Search(cm))
            //        {
            //            Debug.WriteLine("RenewClasSsr: invalid header");
            //            //return;
            //            break;
            //        }
            //        Target = head.ID;
            //    }
            //    else
            //    {
            //        head = null;
            //        Target = SubTypeWaiting;
            //    }


            //    switch (Target)
            //    {
            //        case SubType.ST1_Mask:
            //            ST1_Mask.Decode(cm, head);      // IOD SSRのチェック必要
            //            if (ST1_Mask.Status == SubTypeStatus.DecodeDone)
            //            {
            //                SubTypeWaiting = SubType.Next;
            //                Data.AddRange(ST1_Mask.ClasSSRsInThis);
            //                ST1_Mask.DebugDispSSR();
            //                ST2_Orbit = new ST2_Orbit(ST1_Mask);
            //                ST3_Clock = new ST3_Clock(ST1_Mask);
            //                ST4_CodeBias = new ST4_CodeBias(ST1_Mask);
            //            }
            //            break;
            //        case SubType.ST2_Orbit:
            //            if (ST2_Orbit == null) break;
            //            ST2_Orbit.Decode(cm, head);
            //            if(ST2_Orbit.Status == SubTypeStatus.DecodeDone)
            //            {
            //                SubTypeWaiting = SubType.Next;
            //                ST2_Orbit.DebugDispSSR();
            //            }
            //            break;
            //        case SubType.ST3_Clock:
            //            if (ST3_Clock == null) break;
            //            ST3_Clock.Decode(cm, head);
            //            if(ST3_Clock.Status == SubTypeStatus.DecodeDone)
            //            {
            //                SubTypeWaiting = SubType.Next;
            //                ST3_Clock.DebugDispSSR();
            //            }
            //            break;
            //        case SubType.ST4_CodeBias:
            //            if (ST4_CodeBias == null) break;
            //            ST4_CodeBias.Decode(cm, head);
            //            if (ST4_CodeBias.Status == SubTypeStatus.DecodeDone)
            //            {
            //                SubTypeWaiting = SubType.Next;
            //                ST4_CodeBias.DebugDispSSR();
            //            }

            //            break;
            //    }
            //}
        }

        //private int GetNSat()
        //{
        //    return Data.Count;
        //}

        //private int GetNSat(SYS sys)
        //{
        //    return Data.Where(d => d.GnssID == sys).Count();
        //}

    }

    public class ClasSSRData
    {
        public SYS GnssID { get; set; } = SYS.UNKNOWN;
        public int SvN { get; set; } = 0;
        public List<int> IndSigs { get; set; } = new List<int>();               // Sigmask, Cellmaskを反映したsignalのインデックス
        public ushort CellMask { get; set; } = 0;
        public int Iode { get; set; } = -1;                                     // Type2  iodeとDorbをまとめたクラスにして，iodeが変わっても対応できるようにする？
        public double[] Dorb { get; set; } = new double[3] {0.0, 0.0, 0.0};     // 
        public double Dcc0 { get; set; } = 0.0;                                 // Type3
        public List<double> Codb { get; set; } = new List<double>();            // Type4
        public List<C_PBias> Pseb { get; set; } = new List<C_PBias>();      // Type5
        public SatCPBias SatCPBias { get; set; } = new SatCPBias();             // Type6 Satellite Code and Phase Bias
        public URA URA { get; set; } = new URA();                               // Type7
        public CombinedCorrection CombCorr { get; set; } = new CombinedCorrection();    
    }

    public class C_PBias
    {
        /// <summary>
        /// Code Bias data
        /// </summary>
        public double CBias { get; set; }
        /// <summary>
        /// Phase Bias data
        /// </summary>
        public double PBias { get; set; }
        /// <summary>
        /// Phase Discontinuity Indicator
        /// </summary>
        public int Pdi { get; set; }
    }

    public class URA
    {
        public int Class { get; set; }
        public int Value { get; set; }
    }

    /// <summary>
    /// Subtype 11
    /// </summary>
    public class CombinedCorrection
    {
        public int IODE { get; set; } = 0;
        public bool OrbitExisting { get; set; }
        public bool ClockExisting { get; set; }
        public bool NetworkCorrec { get; set; }
        public bool NetSVMaskThis { get; set; }

        /// <summary>
        /// Delta radial, along, corss, clock
        /// </summary>
        public double[] Dorb { get; set; } = new double[3] {0.0, 0.0, 0.0};
        public double Dcc0  = 0.0;
    }

    /// <summary>
    /// Subtype 6
    /// </summary>
    public class SatCPBias
    {
        public bool CodeBiasExisting { get; set; } = false;
        public bool PhaseBiasExisting { get; set; } = false;
        public bool NetworkCorrec { get; set; } = false;
        public int NetworkID { get; set; }
        public bool NetSVMaskThis { get; set; } = false;

        public List<C_PBias> CPBIas { get; set; } = new List<C_PBias>();
    }

    /// <summary>
    /// Subtype 9
    /// </summary>
    public class GridedCorrection
    {

    }
}




/*
 *     clasd(1:40) = struct( ...
        'sysid',     0, ...                    % 0:GPS, 1:GLO, 2:GAL, 3:BDS, 4:QZS
        'satno',     0, ...                    % 0-40
        'sigmask',   0, ...
        'nSig',      0, ...
        'signo',     zeros(MAX_SIG,1), ...
        'cellmask',  0, ...
        'iode',      0, ...                    % Type=2
        'dorb',      zeros(3,1), ...
        'dcc0',      0, ...                    % Type=3
        'codb',      zeros(MAX_SIG,1), ...
        'pb',        zeros(MAX_SIG,1), ...     % Type=5
        'pdi',       zeros(MAX_SIG,1), ...      
        'cpb_netIDf', zeros(MAX_NET,1), ...    % Type=6  netIDのフラグ
        'cpb_cbF',   zeros(MAX_NET,1), ...     %         code bias existing flag: network毎
        'cpb_pbF',   zeros(MAX_NET,1), ...     %         phase bias existing flag: network毎
        'cpb_cb',    zeros(MAX_SIG,MAX_NET), ...     % 
        'cpb_pb',    zeros(MAX_SIG,MAX_NET), ...
        'cpb_pdi',   zeros(MAX_SIG,MAX_NET), ...
        'ura',       zeros(2,1), ...           % Type=7 ura[0]:class, ura[1]:value
        'stc_netIDf', zeros(MAX_NET,1), ...
        'stc_type',  zeros(MAX_NET,1), ...
        'stc_QI',    zeros(2,MAX_NET), ...
        'stc_coef',  zeros(4,MAX_NET), ...
        'grd_netIDf', zeros(MAX_NET,1), ...    % Type=9
        'grd_tropType', zeros(MAX_NET,1), ...  %
        'grd_tropQI', zeros(2,MAX_NET), ...    %
        'grd_tropH',  zeros(MAX_GRID, MAX_NET), ...   % tropは全衛星で各ネット同じ値が入る
        'grd_tropW',  zeros(MAX_GRID, MAX_NET), ...   % tropは全衛星で各ネット同じ値が入る
        'grd_stec',  zeros(MAX_GRID, MAX_NET), ...
        'comb_netIDf', zeros(MAX_NET,1), ...   % Type=11  netIDのフラグ
        'comb_orbF', zeros(MAX_NET,1), ...     %          orbit existing flag: network毎
        'comb_clkF', zeros(MAX_NET,1), ...     %          clock existing flag: network毎
        'comb_iode', zeros(MAX_NET,1), ...     
        'comb_dorb', zeros(3,MAX_NET), ...
        'comb_dcc0', zeros(MAX_NET,1) ...
        );
end
 * 
 * 
 * 
 */