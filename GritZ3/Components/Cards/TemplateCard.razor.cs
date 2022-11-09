using GritZ3.Pages;
using GrToolBox.Settings;
using Microsoft.AspNetCore.Components;
using static GrToolBox.Settings.SettingsDefinitions;

namespace GritZ3.Components.Cards
{
    public partial class TemplateCard
    {

        [Parameter]
        public GrSettings? Stg { get; set; }


        private string Title { get; set; } = "Least Squares";
        private bool DispKfTag { get; set; } = false;
        private string KfOnOff { get; set; } = "OFF";


        protected override async Task OnParametersSetAsync()
        {
            await Task.Run(() => SetDisp());
        }


        private void SetDisp()
        {
            if(Stg?.PositioningMode.EstType == Estimation_Type.LS)
            {
                Title = "LS";
                DispKfTag = false;
                KfOnOff = "OFF";
            }
            else if(Stg?.PositioningMode.EstType == Estimation_Type.Kalman)
            {
                Title = "LS + KF";
                DispKfTag = true;
                KfOnOff = "ON";
            }
        }




    }
}
