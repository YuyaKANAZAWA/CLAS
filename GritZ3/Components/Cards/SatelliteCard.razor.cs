using GritZ3.Pages;
using Microsoft.AspNetCore.Components;
using static GrToolBox.Settings.SettingsDefinitions;
using static GrToolBox.Common.Constants;
using static GrToolBox.Common.CommonDefinitions;
using GrToolBox.Settings;

namespace GritZ3.Components.Cards
{
    public partial class SatelliteCard
    {

        [Parameter]
        public GrSettings Stg { get; set; }

        //private string Title { get; set; } = "Least Squares";
        //private bool DispKfTag { get; set; } = false;
        //private string KfOnOff { get; set; } = "OFF";

        private string ExcludingGps { get; set; } = "NONE";
        private string ExcludingGlo { get; set; } = "NONE";
        private string ExcludingGal { get; set; } = "NONE";
        private string ExcludingQzs { get; set; } = "NONE";
        private string ExcludingBds { get; set; } = "NONE";
        private string ExcludingIrn { get; set; } = "NONE";
        private string ExcludingSbs { get; set; } = "NONE";

        private bool DispTagGps { get; set; } = false;
        private bool DispTagGlo { get; set; } = false;
        private bool DispTagGal { get; set; } = false;
        private bool DispTagQzs { get; set; } = false;
        private bool DispTagBds { get; set; } = false;
        private bool DispTagIrn { get; set; } = false;
        private bool DispTagSbs { get; set; } = false;



        protected override async Task OnParametersSetAsync()
        {
            await Task.Run(() => SetDisp());
            StateHasChanged();
        }

        private string[] ExcludingDisp { get; set; } = new string[MAX_SYS];

        private void SetDisp()
        {
            string[][] excluding = Stg.Satellite.GetExcludingArray();
            for (int i = 0; i < MAX_SYS; i++)
            {
                if (excluding[i].Length > 0)
                {
                    ExcludingDisp[i] = string.Join(", ", excluding[i]);
                }
                else
                {
                    ExcludingDisp[i] = "NONE";
                }
            }

            //ExcludingGps = string.Join(", ", param.Observation.Code[0].Where(s => s != "---"));
            //ExcludingGlo = string.Join(", ", param.Observation.Code[1].Where(s => s != "---"));
            //ExcludingGal = string.Join(", ", param.Observation.Code[2].Where(s => s != "---"));
            //ExcludingQzs = string.Join(", ", param.Observation.Code[3].Where(s => s != "---"));
            //ExcludingBds = string.Join(", ", param.Observation.Code[4].Where(s => s != "---"));
            //ExcludingIrn = string.Join(", ", param.Observation.Code[5].Where(s => s != "---"));
            //ExcludingSbs = string.Join(", ", param.Observation.Code[6].Where(s => s != "---"));

            DispTagGps = ExcludingDisp[0] != "NONE";
            DispTagGlo = ExcludingDisp[1] != "NONE";
            DispTagGal = ExcludingDisp[2] != "NONE";
            DispTagQzs = ExcludingDisp[3] != "NONE";
            DispTagBds = ExcludingDisp[4] != "NONE";
            DispTagIrn = ExcludingDisp[5] != "NONE";
            DispTagSbs = ExcludingDisp[6] != "NONE";

            //if(param.PositioningMode.EstType == Estimation_Type.LS)
            //{
            //    Title = "LS";
            //    DispKfTag = false;
            //    KfOnOff = "OFF";
            //}
            //else if(param.PositioningMode.EstType == Estimation_Type.Kalman)
            //{
            //    Title = "LS + KF";
            //    DispKfTag = true;
            //    KfOnOff = "ON";
            //}
        }




    }
}
