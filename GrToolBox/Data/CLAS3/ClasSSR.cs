using System.Diagnostics;
using GrToolBox.Data.GrNavData;
using static GrToolBox.Common.CommonDefinitions;
using static GrToolBox.Data.CLAS3.ClasDefinitions;

namespace GrToolBox.Data.CLAS3
{
    public class ClasSSR 
    {
        public ST01_Mask ST01 { get; set; } = new ST01_Mask();
        public ST02_Orbit ST02 { get; set; } = new ST02_Orbit();
        public ST03_Clock ST03 { get; set; } = new ST03_Clock();
        public ST04_CodeBias ST04 { get; set; } = new ST04_CodeBias();
        public ST05_PhaseBias ST05 { get; set; } = new ST05_PhaseBias();
        public ST06_CodePhaseBias ST06 { get; set; } = new ST06_CodePhaseBias();
        public ST07_URA ST07 { get; set; } = new ST07_URA();
        public ST08_STEC ST08 { get; set; } = new ST08_STEC();
        public ST09_Gridded ST09 { get; set; } = new ST09_Gridded();
        public ST10_ServiceInfo ST10 { get; set; } = new ST10_ServiceInfo();
        public ST11_Combined ST11 { get; set; } = new ST11_Combined();
        public ST12_Atomospheric ST12 { get; set; } = new ST12_Atomospheric();


        public SubTypeStatus Status { get; set; } = SubTypeStatus.SearchHeader;


        private int NMessage { get; set; } = -1;

        private BitCircularBuffer Bcb { get; set; } = new BitCircularBuffer(16);    // 2^12byte用意


        public bool AddClasMessage(ClasMessage cm)
        {
            NMessage++;
            Debug.WriteLine($">>> Message: {NMessage}");
            //マルチ中かどうかの判断
            ClasHeaderCommon? head = null;

            if (Bcb.NumWritable() > 1695)
            {
                Bcb.AddByteArray(cm.Bytes, 49, 1695);
            }
            else
            {
                return false;   // 
            }

            Status = SubTypeStatus.SearchHeader;
            while (Status == SubTypeStatus.SearchHeader)
            {
                Bcb.SetMark();
                head = new ClasHeaderCommon();
                Status = head.Search_Decode(Bcb);

                if (Status != SubTypeStatus.HeaderDone) break;
                switch (head.Data.ID)
                {
                    case SubType.ST1_Mask:
                        Status = ST01.Decode(Bcb, head.Data);
                        break;
                    case SubType.ST2_Orbit:
                        Status = ST02.Decode(Bcb, head.Data, ST01);
                        break;
                    case SubType.ST3_Clock:
                        Status = ST03.Decode(Bcb, head.Data, ST01);
                        break;
                    case SubType.ST4_CodeBias:
                        Status = ST04.Decode(Bcb, head.Data, ST01);
                        break;
                    case SubType.ST5_PhaseBias:
                        Status = ST05.Decode(Bcb, head.Data, ST01);
                        break;
                    case SubType.ST6_CodePhaseBias:
                        Status = ST06.Decode(Bcb, head.Data, ST01);
                        break;
                    case SubType.ST7_URA:
                        Status = ST07.Decode(Bcb, head.Data, ST01);
                        break;
                    case SubType.ST8_STEC:
                        Status = ST08.Decode(Bcb, head.Data, ST01);
                        break;
                    case SubType.ST9_Gridded:
                        Status = ST09.Decode(Bcb, head.Data, ST01);
                        break;
                    case SubType.ST10_ServiceInfo:
                        Status = ST10.Decode(Bcb, head.Data, ST01);
                        break;
                    case SubType.ST11_Combined:
                        Status = ST11.Decode(Bcb, head.Data, ST01);
                        break;
                    case SubType.ST12_Atmospheric:
                        Status = ST12.Decode(Bcb, head.Data, ST01);
                        break;
                }
                if (Status == SubTypeStatus.DataDone)
                {
                    Status = SubTypeStatus.SearchHeader;
                }
                else if (Status == SubTypeStatus.NeedMoreData)
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


        }
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