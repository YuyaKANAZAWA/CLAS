using GrToolBox.Data.Nmea;
using GrToolBox.Output;
using Microsoft.AspNetCore.Components;

namespace GritZ3.Components.Cards
{
    public partial class NmeaPositionCard
    {
        [Parameter]
        public NmeaBurstData? LatestNmeaBurst { get; set; }
        [Parameter]
        public string? Title { get; set; }
        [Parameter]
        public string? Color { get; set; }
        [Parameter]
        public bool ShowReceivingStatus { get; set; }


        private string Styles { get; set; } = "";
        private string TimeUtc { get; set; } = "N/A";
        private string Lat { get; set; } = "N/A";
        private string Lon { get; set; } = "N/A";
        private string Alt { get; set; } = "N/A";
        private string EllHeight { get; set; } = "N/A";
        private string HDOP { get; set; } = "N/A";
        private string VDOP { get; set; } = "N/A";
        private string ReceivedSentence { get; set; } = "";
        private bool NowReceiving { get; set; } = false;
        protected override async Task OnParametersSetAsync()
        {
            if(LatestNmeaBurst == null)
            {
                NowReceiving = false;
            }
            else
            {
                NowReceiving = true;
                await Task.Run(() => SetNmeaBurstStrings());
            }
        }

        protected override async Task OnInitializedAsync()
        {
            Styles = "width:320px; background-color:" + Color;
        }


        private void SetNmeaBurstStrings()
        {
            TimeUtc = LatestNmeaBurst.IsTimeValid ? $"{LatestNmeaBurst.EpochTime.ToString("G")}" : "N/A";
            Lat = double.IsNaN(LatestNmeaBurst.Lat) ? "N/A" : $"{LatestNmeaBurst.Lat:F9}";
            Lon = double.IsNaN(LatestNmeaBurst.Lon) ? "N/A" : $"{LatestNmeaBurst.Lon:F9}";
            Alt = double.IsNaN(LatestNmeaBurst.Alt) ? "N/A" : $"{LatestNmeaBurst.Alt:F3}";
            EllHeight = double.IsNaN(LatestNmeaBurst.EllHeight) ? "N/A" : $"{LatestNmeaBurst.EllHeight:F3}";
            ReceivedSentence = "";
            if (LatestNmeaBurst.Rmc != null) ReceivedSentence += "RMC";
            if (LatestNmeaBurst.Gga != null)
            {
                if (!string.IsNullOrEmpty(ReceivedSentence)) ReceivedSentence += "+";
                ReceivedSentence += "GGA";
            }
        }



    }
}
