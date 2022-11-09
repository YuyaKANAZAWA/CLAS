using GritZ3.Classes;
using GrToolBox.Output;
using Microsoft.AspNetCore.Components;
using static GrToolBox.Common.Constants;
using static GrToolBox.Settings.SettingsDefinitions;

namespace GritZ3.Components.Cards
{
    public partial class GrPositionCard
    {
        [Parameter]
        public EpochPosData? Data { get; set; }
        [Parameter]
        public string Type { get; set; }    // LS, KF, NMEA
        [Parameter]
        public string? Title { get; set; }
        [Parameter]
        public string? Color { get; set; }

        private string Styles { get; set; } = "";

        private string TimeUtc { get; set; } = "N/A";
        private string Lat { get; set; } = "N/A";
        private string Lon { get; set; } = "N/A";
        private string Alt { get; set; } = "N/A";
        private string EllHeight { get; set; } = "N/A";
        private string HDOP { get; set; } = "N/A";
        private string VDOP { get; set; } = "N/A";


        protected override async Task OnInitializedAsync()
        {
            Styles = "width:320px; background-color:" + Color;
        }



        protected override async Task OnParametersSetAsync()
        {
            if (Data != null)
            {
                await Task.Run(() => SetBurstStrings());
            }
        }

        private void SetBurstStrings()
        {
            //TimeUtc = Data.TimeStr;
            TimeUtc = Data.Time.ToString("G");
            Lat = double.IsNaN(Data.Lat) ? "N/A" : $"{Data.Lat:F9}";
            Lon = double.IsNaN(Data.Lon) ? "N/A" : $"{Data.Lon:F9}";
            Alt = double.IsNaN(Data.Alt) ? "N/A" : $"{Data.Alt:F3}";
            EllHeight = double.IsNaN(Data.EllH) ? "N/A" : $"{Data.EllH:F3}";
        }
    }
}
