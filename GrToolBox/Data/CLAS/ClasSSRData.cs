using System.Diagnostics;
using static GrToolBox.Common.CommonDefinitions;
using static GrToolBox.Data.CLAS.ClasDefinitions;

namespace GrToolBox.Data.CLAS
{
    public class ClasSSR
    {
        public ST1_Mask ST1_Mask { get; set; } = new ST1_Mask();
        public ST2_Orbit? ST2_Orbit { get; set; }
        public ST3_Clock? ST3_Clock { get; set; }
        public ST4_CodeBias? ST4_CodeBias { get; set; }

        public SubType SubTypeWaiting { get; set; } = SubType.Next;      // multiple messageでデータ待ちのSubtypeフラグ

        private SubType Target { get; set; } = SubType.Next;

        private List<ClasSSRData> Data { get; set; } = new List<ClasSSRData>(); // 衛星数分
        private int NMessage { get; set; } = -1;


        public void AddClasMessage(ClasMessage cm)
        {
            NMessage++;
            Debug.WriteLine($">>> Message: {NMessage}");
            //マルチ中かどうかの判断
            ClasSubTypeHeader_base? head = null;

            while (cm.Pos < 1744)        // 1744: RSCの手前まで
            {
                if (SubTypeWaiting == SubType.Next)
                {
                    head = new ClasSubTypeHeader_base();      // subtype header, common part
                    if (!head.Search(cm))
                    {
                        Debug.WriteLine("RenewClasSsr: invalid header");
                        //return;
                        break;
                    }
                    Target = head.ID;
                }
                else
                {
                    head = null;
                    Target = SubTypeWaiting;
                }


                switch (Target)
                {
                    case SubType.ST1_Mask:
                        ST1_Mask.Decode(cm, head);      // IOD SSRのチェック必要
                        if (ST1_Mask.Status == SubTypeStatus.DecodeDone)
                        {
                            SubTypeWaiting = SubType.Next;
                            Data.AddRange(ST1_Mask.ClasSSRsInThis);
                            ST1_Mask.DebugDispSSR();
                            ST2_Orbit = new ST2_Orbit(ST1_Mask);
                            ST3_Clock = new ST3_Clock(ST1_Mask);
                            ST4_CodeBias = new ST4_CodeBias(ST1_Mask);
                        }
                        break;
                    case SubType.ST2_Orbit:
                        if (ST2_Orbit == null) break;
                        ST2_Orbit.Decode(cm, head);
                        if(ST2_Orbit.Status == SubTypeStatus.DecodeDone)
                        {
                            SubTypeWaiting = SubType.Next;
                            ST2_Orbit.DebugDispSSR();
                        }
                        break;
                    case SubType.ST3_Clock:
                        if (ST3_Clock == null) break;
                        ST3_Clock.Decode(cm, head);
                        if(ST3_Clock.Status == SubTypeStatus.DecodeDone)
                        {
                            SubTypeWaiting = SubType.Next;
                            ST3_Clock.DebugDispSSR();
                        }
                        break;
                    case SubType.ST4_CodeBias:
                        if (ST4_CodeBias == null) break;
                        ST4_CodeBias.Decode(cm, head);
                        if (ST4_CodeBias.Status == SubTypeStatus.DecodeDone)
                        {
                            SubTypeWaiting = SubType.Next;
                            ST4_CodeBias.DebugDispSSR();
                        }

                        break;
                }
            }
        }


        private int GetNSat()
        {
            return Data.Count;
        }

        private int GetNSat(SYS sys)
        {
            return Data.Where(d => d.GnssID == sys).Count();
        }

    }

    public class ClasSSRData
    {
        public SYS GnssID { get; set; } = SYS.UNKNOWN;
        public int SvN { get; set; } = 0;
        public List<int> IndSigs { get; set; } = new List<int>();
        public ushort CellMask { get; set; } = 0;
        public int Iode { get; set; } = -1;                                     // Type2  iodeとDorbをまとめたクラスにして，iodeが変わっても対応できるようにする？
        public double[] Dorb { get; set; } = new double[3] {0.0, 0.0, 0.0};     // 
        public double Dcc0 { get; set; } = 0.0;                                 // Type3
        public List<double> Codb { get; set; } = new List<double>();            // Type4
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