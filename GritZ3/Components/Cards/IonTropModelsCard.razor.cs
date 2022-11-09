using GritZ3.Pages;
using GrToolBox.Settings;
using Microsoft.AspNetCore.Components;
using static GrToolBox.Settings.SettingsDefinitions;

namespace GritZ3.Components.Cards
{
    public partial class IonTropModelsCard
    {

        [Parameter]
        public GrSettings? Stg { get; set; }


        private string Title { get; set; } = "Least Squares";
        private bool DispKfTag { get; set; } = false;
        private string KfOnOff { get; set; } = "OFF";
        private string IonoStr { get; set; } = "NONE";
        private string TropStr { get; set; } = "NONE";


        protected override async Task OnParametersSetAsync()
        {
            await Task.Run(() => SetDisp());
        }


        private void SetDisp()
        {
            switch (Stg.Correction.IonType)
            {
                case Ion_Type.None:
                    IonoStr = "NONE";
                    break;
                case Ion_Type.Klob:
                    IonoStr = "GPS-Broadcast";
                    break;
                case Ion_Type.IonFree:
                    IonoStr = "Iono free combination";
                    break;
                case Ion_Type.Ionex:
                    IonoStr = "GIM";
                    break;
                default:
                    IonoStr = "NONE";
                    break;
            }
            //TropStr = "";
            switch (Stg.Correction.TropType)
            {
                case Trop_Type.None:
                    TropStr = "None";
                    break;
                case Trop_Type.Simple:
                    TropStr = "Simple";
                    break;
                case Trop_Type.Saastamoinen:
                    TropStr = "Saastamoinen";
                    break;
                case Trop_Type.MOPS:
                    TropStr = "MOPS";
                    break;
                default:
                    TropStr = "None";
                    break;
            }
            if(TropStr =="Saastamoinen" || TropStr == "MOPS")
            {
                switch (Stg.Correction.TropMap)
                {
                    case Trop_Map.Cos_z:
                        TropStr += " / 1/cos(zenith)";
                        break;
                    case Trop_Map.Chao:
                        TropStr += " / Chao";
                        break;
                    case Trop_Map.Neil:
                        TropStr += " / Neil";
                        break;
                    default:
                        break;
                }
            }
            else
            {
                TropStr += " / NONE";
            }

        }




    }
}
