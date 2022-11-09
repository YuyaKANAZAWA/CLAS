using GritZ3.Classes;
using GritZ3.Pages;
using GrToolBox.Settings;
using Microsoft.AspNetCore.Components;
using static GrToolBox.Settings.SettingsDefinitions;

namespace GritZ3.Components.Settings
{
    public partial class DrawerSatellite
    {
        [Parameter]
        public GrSettings? Stg { get; set; }
        [Parameter]
        public DrawerInfo DrawerInfo { get; set; }

        [Parameter]
        public EventCallback<int> OnClickCallback { get; set; }


        private SatelliteStatus NowStatus { get; set; } = new();
        private SatelliteStatus KeepStatus { get; set; } = new();

        private List<string> ItemsGps { get; set; } = new List<string>();
        private List<string> ItemsGlo { get; set; } = new List<string>();
        private List<string> ItemsGal { get; set; } = new List<string>();
        private List<string> ItemsQzs { get; set; } = new List<string>();
        private List<string> ItemsBds { get; set; } = new List<string>();
        private List<string> ItemsIrn { get; set; } = new List<string>();
        private List<string> ItemsSbs { get; set; } = new List<string>();



        private void SetReturnStatus(GrSettings stg, SatelliteStatus sts)
        {
            stg.Satellite.ExcludingGps = (List<string>)sts.ValuesGps;
            stg.Satellite.ExcludingGlo = (List<string>)sts.ValuesGlo;
            stg.Satellite.ExcludingGal = (List<string>)sts.ValuesGal;
            stg.Satellite.ExcludingQzs = (List<string>)sts.ValuesQzs;
            stg.Satellite.ExcludingBds = (List<string>)sts.ValuesBds;
            stg.Satellite.ExcludingIrn = (List<string>)sts.ValuesIrn;
            stg.Satellite.ExcludingSbs = (List<string>)sts.ValuesSbs;
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

        protected override void OnInitialized()
        {
            for (int i = 1; i < 33; i++)
            {
                string str = "G" + $"{i:00}";
                ItemsGps.Add(str);
            }
            for (int i = 1; i < 31; i++)
            {
                string str = "R" + $"{i:00}";
                ItemsGlo.Add(str);
            }
            for (int i = 1; i < 37; i++)
            {
                string str = "E" + $"{i:00}";
                ItemsGal.Add(str);
            }
            for (int i = 1; i < 8; i++)
            {
                string str = "J" + $"{i:00}";
                ItemsQzs.Add(str);
            }
            for (int i = 1; i < 38; i++)
            {
                string str = "C" + $"{i:00}";
                ItemsBds.Add(str);
            }
            for (int i = 1; i < 15; i++)
            {
                string str = "I" + $"{i:00}";
                ItemsIrn.Add(str);
            }
            for (int i = 20; i < 59; i++)
            {
                string str = "S" + $"{i:00}";
                ItemsSbs.Add(str);
            }
            for (int i = 83; i < 88; i++)
            {
                string str = "S" + $"{i:00}";
                ItemsSbs.Add(str);
            }
        }


    }


    public class SatelliteStatus
    {
        public IEnumerable<string> ValuesGps { get; set; } = new List<string>();
        public IEnumerable<string> ValuesGlo { get; set; } = new List<string>();
        public IEnumerable<string> ValuesGal { get; set; } = new List<string>();
        public IEnumerable<string> ValuesQzs { get; set; } = new List<string>();
        public IEnumerable<string> ValuesBds { get; set; } = new List<string>();
        public IEnumerable<string> ValuesIrn { get; set; } = new List<string>();
        public IEnumerable<string> ValuesSbs { get; set; } = new List<string>();

        public SatelliteStatus Clone()
        {
            return (SatelliteStatus)MemberwiseClone();
        }

    }


}
