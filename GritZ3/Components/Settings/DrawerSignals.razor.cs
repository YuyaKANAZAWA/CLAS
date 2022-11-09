using GritZ3.Classes;
using GritZ3.Pages;
using GrToolBox.Settings;
using Microsoft.AspNetCore.Components;
using System.ComponentModel;
using static GrToolBox.Common.CommonDefinitions;
using static GrToolBox.Common.CommonUtilities;
using static GrToolBox.Common.Constants;
using static GrToolBox.Data.SBF.SbfUtilities;
using static GrToolBox.Settings.SettingsDefinitions;

namespace GritZ3.Components.Settings
{
    public partial class DrawerSignals
    {
        [Parameter]
        public GrSettings? Stg { get; set; }
        [Parameter]
        public DrawerInfo DrawerInfo { get; set; }

        [Parameter]
        public EventCallback<int> OnClickCallback { get; set; }


        private SignalsStatus NowStatus { get; set; } = new();
        private SignalsStatus KeepStatus { get; set; } = new();

        private void SetReturnStatus(GrSettings stg, SignalsStatus sts)
        {
            for (int i = 0; i < MAX_SYS; i++)
            {
                stg.Observation.Code[i] = new string[] { "---", "---", "---" };
            }

            foreach (var row in sts.CodeSelectedRows)
            {
                int iSys = GetISys(row.Name);
                stg.Observation.Code[iSys] = new string[] { row.Sig1, row.Sig2, row.Sig3 };
            }

            stg.Satellite.ApplyElevationMask = sts.ApplyElevationMask;
            stg.Satellite.EleMask = sts.ElevationMask;

            stg.Satellite.ApplyCnMask = sts.ApplyCnMask;
            if (sts.IndividualCn0)  // 個別指定時
            {
                for(int i = 0; i < MAX_SYS; i++)
                {
                    stg.Satellite.CnMask[i] = new int[] { sts.CnMaskRowDatas[i].Sig1, sts.CnMaskRowDatas[i].Sig2, sts.CnMaskRowDatas[i].Sig3 };
                }
            }
            else    // 一括指定時
            {
                for (int i = 0; i < MAX_SYS; i++)
                {
                    stg.Satellite.CnMask[i] = new int[] { sts.Sig1Cn_InALump, sts.Sig2Cn_InALump, sts.Sig3Cn_InALump };
                }
            }

        }

        private async Task Pushed_Cancel()
        {
            await Task.Run(() => NowStatus = KeepStatus.Clone());
            Close_This();
        }

        private async Task Pushed_Apply()
        {
            await Task.Run(() => KeepStatus = NowStatus.Clone());
            await Task.Run(() => SetReturnStatus(Stg, NowStatus));

            await OnClickCallback.InvokeAsync(1);
            Close_This();
        }


        private void Close_This()
        {
            DrawerInfo.Visible = false;
        }

        public DrawerSignals()
        {
            foreach(SYS s in Enum.GetValues(typeof(SYS)))
            {
                int i = GetISys(s);
                if (i >= 0)
                {
                    NowStatus.CodeRowDatas[i].Name = KeepStatus.CodeRowDatas[i].Name = s;
                    NowStatus.CnMaskRowDatas[i].Name = KeepStatus.CnMaskRowDatas[i].Name = s;
                }
            }

            NowStatus.CodeRowDatas[0].Sig1 = KeepStatus.CodeRowDatas[0].Sig1 = "C1C";

            //NowStatus.CodeRowDatas[0].Name = KeepStatus.CodeRowDatas[0].Name = SYS.GPS;
            //NowStatus.CodeRowDatas[0].Sig1 = KeepStatus.CodeRowDatas[0].Sig1 = "C1C";
            //NowStatus.CodeRowDatas[1].Name = KeepStatus.CodeRowDatas[1].Name = SYS.GLO;
            //NowStatus.CodeRowDatas[2].Name = KeepStatus.CodeRowDatas[2].Name = SYS.GAL;
            //NowStatus.CodeRowDatas[3].Name = KeepStatus.CodeRowDatas[3].Name = SYS.QZS;
            //NowStatus.CodeRowDatas[4].Name = KeepStatus.CodeRowDatas[4].Name = SYS.BDS;
            //NowStatus.CodeRowDatas[5].Name = KeepStatus.CodeRowDatas[5].Name = SYS.IRN;
            //NowStatus.CodeRowDatas[6].Name = KeepStatus.CodeRowDatas[6].Name = SYS.SBS;

            NowStatus.CodeSelectedRows = new SigRowData[] { NowStatus.CodeRowDatas[0] }; // 初期状態はGPS，C1Cのみの選択
            KeepStatus.CodeSelectedRows = new SigRowData[] { KeepStatus.CodeRowDatas[0] }; // 初期状態はGPS，C1Cのみの選択


        }

        protected override async Task OnInitializedAsync()
        {
            if (Stg.Communication.DataType == Data_Type.SBF)
            {
                string[][][] sbfList = GetSbfSignalList();  // [][][]: SYS, (L1,2,5), list
                for (int i = 0; i < MAX_SYS; i++)
                {
                    NowStatus.CodeRowDatas[i].SigList1 = KeepStatus.CodeRowDatas[i].SigList1 = sbfList[i][0];
                    NowStatus.CodeRowDatas[i].SigList2 = KeepStatus.CodeRowDatas[i].SigList2 = sbfList[i][1];
                    NowStatus.CodeRowDatas[i].SigList3 = KeepStatus.CodeRowDatas[i].SigList3 = sbfList[i][2];
                }
            }

            Stg.Satellite.EleMask = NowStatus.ElevationMask;

            for (int i = 0; i < MAX_SYS; i++)
            {
                Stg.Satellite.CnMask[i] = new int[] { NowStatus.Sig1Cn_InALump, NowStatus.Sig2Cn_InALump, NowStatus.Sig3Cn_InALump };
            }
        }

        protected override async Task OnParametersSetAsync()
        {

        }

    }


    public class SignalsStatus
    {
        public IEnumerable<SigRowData> CodeSelectedRows = new SigRowData[1];

        public SigRowData[] CodeRowDatas { get; set; } = new SigRowData[] { new(), new(), new(), new(), new(), new(), new() };

        public CnRowData[] CnMaskRowDatas { get; set; } = new CnRowData[] { new(), new(), new(), new(), new(), new(), new() };

        public bool ApplyElevationMask { get; set; } = true;
        public bool ApplyCnMask { get; set; } = true;
        public bool DisabledElevationMask 
        {
            get { return !ApplyElevationMask; }
        }
        public bool DisabledCnMask 
        {
            get { return !ApplyCnMask; }
        }

        public int ElevationMask { get; set; } = 15;

        public int Sig1Cn_InALump { get; set; } = 30;
        public int Sig2Cn_InALump { get; set; } = 25;
        public int Sig3Cn_InALump { get; set; } = 25;

        public bool IndividualCn0 { get; set; } = false;

        public SignalsStatus Clone()
        {
            return (SignalsStatus)MemberwiseClone();
        }

    }


    public class SigRowData
    {
        [DisplayName("Key")]
        public string Key { get; set; }

        [DisplayName("SYS")]
        public SYS Name { get; set; }

        [DisplayName("Code1")]
        public string Sig1 { get; set; } = "---";

        [DisplayName("Code2")]
        public string Sig2 { get; set; } = "---";

        [DisplayName("Code3")]
        public string Sig3 { get; set; } = "---";

        public string[]? SigList1 { get; set; }
        public string[]? SigList2 { get; set; }
        public string[]? SigList3 { get; set; }
    }

    public class CnRowData
    {
        [DisplayName("Key")]
        public string Key { get; set; }

        [DisplayName("SYS")]
        public SYS Name { get; set; }

        [DisplayName("Sig1")]
        public int Sig1 { get; set; } = 30;

        [DisplayName("Sig2")]
        public int Sig2 { get; set; } = 25;

        [DisplayName("Sig3")]
        public int Sig3 { get; set; } = 25;

    }


}
