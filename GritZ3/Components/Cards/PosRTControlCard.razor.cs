using AntDesign;
using GritZ3.Classes;
using GritZ3.Pages;
using Microsoft.AspNetCore.Components;
using static GrToolBox.Settings.SettingsDefinitions;


namespace GritZ3.Components.Cards
{
    public partial class PosRTControlCard
    {

        [Parameter]
        public ParamsPosRTControlCard param { get; set; }

        [Parameter]
        public EventCallback<string> OnClickCallback { get; set; }

        [Parameter]
        public EventCallback<string> OnClickRecordingCallback { get; set; }

        [Parameter]
        public EventCallback<string> OnClickFileCallback { get; set; }

        protected override async Task OnParametersSetAsync()
        {
            await Task.Run(() => SetDisp());
        }


        private void OnClickStartButton()
        {
            OnClickCallback.InvokeAsync("Start");
        }

        private void OnClickStopButton()
        {
            OnClickCallback.InvokeAsync("Stop");
        }

        private void OnClickRecordingSwitch()
        {
            param.RecordingSwitch = !param.RecordingSwitch;
            if (param.RecordingSwitch)
            {
                OnClickRecordingCallback.InvokeAsync("StartRec");
            }
            else
            {
                OnClickRecordingCallback.InvokeAsync("StopRec");
            }

        }

        private void OnClickKMLButton()
        {
            OnClickFileCallback.InvokeAsync("KML");
        }

        private void OnClickGPXButton()
        {
            OnClickFileCallback.InvokeAsync("GPX");
        }

        private void OnClickCSVButton()
        {
            OnClickFileCallback.InvokeAsync("CSV");
        }


        private void SetDisp()
        {
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


    public class ParamsPosRTControlCard
    {
        public Stack<AlertData> AlertDatas { get; set; } = new Stack<AlertData>();
        public bool StartButtonDisabled { get; set; } = false;
        public bool StopButtonDisabled { get; set; } = true;
        public bool RecordingSwitch { get; set; } = false;
        public bool RecordingSwitchDisabled { get; set; } = false;
        public string RecordingFileInfo { get; set; } = "";
        public string ConvertedFileInfo { get; set; } = "";

        public PosRTControlFileInfos RecFileInfos { get; set; } = new PosRTControlFileInfos();
        public PosRTControlFileInfos DLFileInfos { get; set; } = new PosRTControlFileInfos();

        public PosRTControlCardBadgeData[] BadgeData { get; set; }

        public ParamsPosRTControlCard(int n)
        {
            BadgeData = new PosRTControlCardBadgeData[n];
            for (int i = 0; i < n; i++)
            {
                BadgeData[i] = new PosRTControlCardBadgeData() { Description = $"{i + 1}: No connection" };
            }
        }

    }

    public class PosRTControlCardBadgeData
    {
        public string Status { get; set; } = "error";       // processing, warning, error
        public string Description { get; set; } = "";
    }

    public class PosRTControlFileInfos
    {
        public bool HaveData { get; set; } = false;
        public bool Converting { get; set; } = false;
        public string FileName { get; set; } = string.Empty;
        public string SaveAsName { get; set;} = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

}