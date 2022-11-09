using GritZ3.Classes;
using GritZ3.Pages;
using GrToolBox.Settings;
using Microsoft.AspNetCore.Components;
using static GrToolBox.Settings.SettingsDefinitions;

namespace GritZ3.Components.Settings
{
    public partial class DrawerIonTropModels
    {
        [Parameter]
        public GrSettings? Stg { get; set; }
        [Parameter]
        public DrawerInfo? DrawerInfo { get; set; }

        [Parameter]
        public EventCallback<int> OnClickCallback { get; set; }


        private IonTropModelStatus NowStatus { get; set; } = new();
        private IonTropModelStatus KeepStatus { get; set; } = new();


        private void SetReturnStatus(GrSettings stg, IonTropModelStatus sts)
        {
            switch (sts.IonoModel)
            {
                case "None":
                    stg.Correction.IonType = Ion_Type.None;
                    break;
                case "GpsBroadcast":
                    stg.Correction.IonType = Ion_Type.Klob;
                    break;
                case "IonFree":
                    stg.Correction.IonType = Ion_Type.IonFree;
                    break;
                case "IONEX":
                    stg.Correction.IonType = Ion_Type.Ionex;
                    break;
                default:
                    stg.Correction.IonType = Ion_Type.None;
                    break;
            }
            switch (sts.TropModel)
            {
                case "None":
                    stg.Correction.TropType = Trop_Type.None;
                    break;
                case "Simple":
                    stg.Correction.TropType = Trop_Type.Simple;
                    break;
                case "Saastamoinen":
                    stg.Correction.TropType = Trop_Type.Saastamoinen;
                    break;
                case "MOPS":
                    stg.Correction.TropType = Trop_Type.MOPS;
                    break;
                default:
                    stg.Correction.TropType = Trop_Type.None;
                    break;
            }
            switch (sts.TropMappingFunction)
            {
                case "cos":
                    stg.Correction.TropMap = Trop_Map.Cos_z;
                    break;
                case "Chao":
                    stg.Correction.TropMap = Trop_Map.Chao;
                    break;
                case "Neil":
                    stg.Correction.TropMap = Trop_Map.Neil;
                    break;
                default:
                    stg.Correction.TropMap = Trop_Map.Neil;
                    break;
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

    }


    public class IonTropModelStatus
    {
        public string IonoModel { get; set; } = "None";     // None, GpsBroadcast, IonFree, IONEX
        public string TropModel { get; set; } = "None";     // None, Simple, Saastamoinen, MOPS
        public string TropMappingFunction { get; set; } = "Neil";     // cos, Chao, Neil


        public IonTropModelStatus Clone()
        {
            return (IonTropModelStatus)MemberwiseClone();
        }

    }


}
