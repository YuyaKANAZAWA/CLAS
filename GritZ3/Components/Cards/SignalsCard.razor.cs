using GritZ3.Pages;
using GrToolBox.Settings;
using Microsoft.AspNetCore.Components;
using static GrToolBox.Settings.SettingsDefinitions;
using static GrToolBox.Common.Constants;
using static GrToolBox.Common.CommonDefinitions;

namespace GritZ3.Components.Cards
{
    public partial class SignalsCard
    {

        [Parameter]
        public GrSettings? Stg { get; set; }

        private string Title { get; set; } = "Elevation mask: ";
        //private bool DispKfTag { get; set; } = false;
        //private string KfOnOff { get; set; } = "OFF";

        private string GpsObs { get; set; } = "";
        private string GloObs { get; set; } = "";
        private string GalObs { get; set; } = "";
        private string QzsObs { get; set; } = "";
        private string BdsObs { get; set; } = "";
        private string IrnObs { get; set; } = "";
        private string SbsObs { get; set; } = "";

        private bool DispTagGps { get; set; } = false;
        private bool DispTagGlo { get; set; } = false;
        private bool DispTagGal { get; set; } = false;
        private bool DispTagQzs { get; set; } = false;
        private bool DispTagBds { get; set; } = false;
        private bool DispTagIrn { get; set; } = false;
        private bool DispTagSbs { get; set; } = false;

        private string[] Obs { get; set; } = new string[MAX_SYS];
        private string[] SysNames { get; set; } = Enum.GetNames(typeof(SYS));

        private List<DispNameObs> DispData { get; set; } = new List<DispNameObs>();

        public SignalsCard()
        {
            for(int i = 0; i < MAX_SYS; i++)
            {
                Obs[i] = "";
            }
        }

        protected override async Task OnParametersSetAsync()
        {
            await Task.Run(() => SetDisp());
        }

        protected override void OnInitialized()
        {
            SetDisp();
        }

        private void SetDisp()
        {
            //Title = "Elevation mask: " + $"{Stg.Satellite.EleMask}" + "[deg]";
            if (Stg==null) return;

            if(Stg.Satellite.ApplyElevationMask && Stg.Satellite.ApplyCnMask)
            {
                Title = "Signal type (C/N0 mask[dB/Hz]),  Elevation mask: " + $"{Stg.Satellite.EleMask}" + "[deg]";
            }
            if(!Stg.Satellite.ApplyElevationMask && Stg.Satellite.ApplyCnMask)
            {
                Title = "Signal type (C/N0 mask[dB/Hz]),  Elevation mask is not applied";
            }
            if(Stg.Satellite.ApplyElevationMask && !Stg.Satellite.ApplyCnMask)
            {
                Title = "Signal type, C/N0 mask is not applied, Elevation mask: " + $"{Stg.Satellite.EleMask}" + "[deg]";
            }
            if(!Stg.Satellite.ApplyElevationMask && !Stg.Satellite.ApplyCnMask)
            {
                Title = "Signal type, C/N0, Elevation masks are not applied";
            }


            DispData.Clear();
            for(int i = 0; i < MAX_SYS; i++)
            {
                DispNameObs data = new DispNameObs();
                data.Name = SysNames[i];
                string description = "Code: ";
                for(int j = 0; j < 3; j++)
                {
                    if (Stg.Observation.Code[i][j] != "---")
                    {
                        description += Stg.Observation.Code[i][j];
                        description += ( "(" + Stg.Satellite.CnMask[i][j] + "), ");
                    }
                }
                if (!string.IsNullOrEmpty(description))
                {
                    int len = description.Length;
                    data.Description = description.Remove(len-2);
                }
                DispData.Add(data);
            }



            GpsObs = string.Join(", ", Stg.Observation.Code[0].Where(s => s != "---"));
            GloObs = string.Join(", ", Stg.Observation.Code[1].Where(s => s != "---"));
            GalObs = string.Join(", ", Stg.Observation.Code[2].Where(s => s != "---"));
            QzsObs = string.Join(", ", Stg.Observation.Code[3].Where(s => s != "---"));
            BdsObs = string.Join(", ", Stg.Observation.Code[4].Where(s => s != "---"));
            IrnObs = string.Join(", ", Stg.Observation.Code[5].Where(s => s != "---"));
            SbsObs = string.Join(", ", Stg.Observation.Code[6].Where(s => s != "---"));

            DispTagGps = !string.IsNullOrEmpty(GpsObs);
            DispTagGlo = !string.IsNullOrEmpty(GloObs);
            DispTagGal = !string.IsNullOrEmpty(GalObs);
            DispTagQzs = !string.IsNullOrEmpty(QzsObs);
            DispTagBds = !string.IsNullOrEmpty(BdsObs);
            DispTagIrn = !string.IsNullOrEmpty(IrnObs);
            DispTagSbs = !string.IsNullOrEmpty(SbsObs);

            //if(Stg.PositioningMode.EstType == Estimation_Type.LS)
            //{
            //    Title = "LS";
            //    DispKfTag = false;
            //    KfOnOff = "OFF";
            //}
            //else if(Stg.PositioningMode.EstType == Estimation_Type.Kalman)
            //{
            //    Title = "LS + KF";
            //    DispKfTag = true;
            //    KfOnOff = "ON";
            //}
        }


        private class DispNameObs
        {
            public string Name { get; set; } = "";
            public string Description { get; set; } = "";
        }

    }
}
