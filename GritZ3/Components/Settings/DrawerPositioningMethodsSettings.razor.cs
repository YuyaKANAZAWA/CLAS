using GritZ3.Classes;
using GritZ3.Pages;
using GrToolBox.Settings;
using Microsoft.AspNetCore.Components;
using static GrToolBox.Settings.SettingsDefinitions;

namespace GritZ3.Components.Settings
{
    public partial class DrawerPositioningMethodsSettings
    {
        [Parameter]
        public GrSettings? Stg { get; set; }
        [Parameter]
        public DrawerInfo? DrawerInfo { get; set; }
        [Parameter]
        public EventCallback<int> OnClickCallback { get; set; }


        private PositioningMethodsStatus NowStatus { get; set; } = new();
        private PositioningMethodsStatus KeepStatus { get; set; } = new();


        private void SetReturnStatus(GrSettings stg, PositioningMethodsStatus sts)
        {
            if (sts.SwitchKF)
            {
                stg.PositioningMode.EstType = Estimation_Type.Kalman;
            }
            else
            {
                stg.PositioningMode.EstType = Estimation_Type.LS;
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
            //param.VisiblePositioningMethods = false;
        }
    }

    public class PositioningMethodsStatus
    {
        public bool SwitchKF { get; set; } = false;

        public PositioningMethodsStatus Clone()
        {
            return (PositioningMethodsStatus)MemberwiseClone();
        }

    }


}
