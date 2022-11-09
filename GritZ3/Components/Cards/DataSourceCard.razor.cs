using AntDesign;
using GritZ3.Pages;
using GrToolBox.Settings;
using Microsoft.AspNetCore.Components;
using static GrToolBox.Settings.SettingsDefinitions;

namespace GritZ3.Components.Cards
{
    public partial class DataSourceCard
    {
        [Parameter]
        public GrSettings? Stg { get; set; }
        [Parameter]
        public string? Color { get; set; }

        private string Styles { get; set; } = "";


        private string Title { get; set; } = "";
        private PresetColor TagColor { get; set; } = PresetColor.Red;
        private string TagTitle { get; set; } = "";
        private string Row1Name { get; set; } = "";
        private string Row1Value { get; set; } = "";
        private string Row2Name { get; set; } = "";
        private string Row2Value { get; set; } = "";


        protected override async Task OnInitializedAsync()
        {
            Styles = "width:320px; background-color:" + Color;
        }


        protected override async Task OnParametersSetAsync()
        {
            await Task.Run(() => SetDisp());
        }


        private void SetDisp()
        {
            Title = Stg.Communication.Connection.ToString();
            switch (Stg.Communication.Connection)
            {
                case Connection_Type.Serial:
                    Row1Name = "Serial port name: ";
                    Row1Value = Stg.Communication.SerialPort.PortName;
                    Row2Name = "Speed: ";
                    Row2Value = $"{Stg.Communication.SerialPort.BaudRate}";
                    break;
                case Connection_Type.TCP:
                    Row1Name = "IP address: ";
                    Row1Value = Stg.Communication.TcpClient.IpAddress;
                    Row2Name = "Port: ";
                    Row2Value = $"{Stg.Communication.TcpClient.Port}";
                    break;
                case Connection_Type.UDP:
                    break;

            }

            TagTitle = Stg.Communication.DataType.ToString();
            switch (Stg.Communication.DataType)
            {
                case Data_Type.SBF:
                    TagColor = PresetColor.Orange;
                    break;
                case Data_Type.RTCM3:
                    TagColor = PresetColor.GeekBlue;
                    break;
                case Data_Type.UBX:
                    TagColor = PresetColor.Purple;
                    break;
            }

        }
    }
}
