using GritZ3.Classes;
using GritZ3.Pages;
using GrToolBox.Settings;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System.IO.Ports;

namespace GritZ3.Components.Settings
{
    public partial class DrawerDataSourceSettings
    {
        [Parameter]
        public GrSettings? Stg { get; set; }
        [Parameter]
        public DrawerInfo? DrawerInfo { get; set; }

        [Parameter]
        public EventCallback<int> OnClickCallback { get; set; }

        public DataSourceStatus NowStatus { get; set; } = new();
        public DataSourceStatus KeepStatus { get; set; } = new();

        private async Task Pushed_Cancel()
        {
            await Task.Run(()=> NowStatus = KeepStatus.Clone());
            Close_DataSource();
        }

        private async Task Pushed_Apply()
        {
            await Task.Run(() => KeepStatus = NowStatus.Clone());
            await Task.Run(() => SetReturnStatus(Stg, NowStatus));

            await OnClickCallback.InvokeAsync(1);
            Close_DataSource();
        }

        private void Close_DataSource()
        {
            DrawerInfo.Visible = false;
        }

        private void GetDataType(string s)
        {
            NowStatus.DataType = s;
        }

        private void GetConnectionType(int i)
        {
            NowStatus.CnnectionType = i;
        }

        private void SetReturnStatus(GrSettings stg, DataSourceStatus sts)
        {
            // Data Type
            switch (sts.DataType)
            {
                case "RTCMv3":
                    stg.Communication.DataType = SettingsDefinitions.Data_Type.RTCM3;
                    break;
                case "SBF":
                    stg.Communication.DataType = SettingsDefinitions.Data_Type.SBF;
                    break;
                case "UBX":
                    stg.Communication.DataType = SettingsDefinitions.Data_Type.UBX;
                    break;
            }
            // Connection Type
            switch (sts.CnnectionType)
            {
                case 1:
                    stg.Communication.Connection = SettingsDefinitions.Connection_Type.Serial;
                    break;
                case 2:
                    stg.Communication.Connection = SettingsDefinitions.Connection_Type.TCP;
                    break;
                case 3:
                    stg.Communication.Connection = SettingsDefinitions.Connection_Type.UDP;
                    break;
            }
            // Serial Port Name
            stg.Communication.SerialPort.PortName = sts.SerialPortName;
            // Serial BaudRate
            stg.Communication.SerialPort.BaudRate = int.Parse(sts.SerialBaudRate);
            // Serial DataBits
            stg.Communication.SerialPort.DataBits = int.Parse(sts.SerialDataBits);
            // Serial Parity
            switch (sts.SerialParity)
            {
                case "None":
                    stg.Communication.SerialPort.Parity = Parity.None;
                    break;
                case "Even":
                    stg.Communication.SerialPort.Parity = Parity.Even;
                    break;
                case "Mark":
                    stg.Communication.SerialPort.Parity = Parity.Mark;
                    break;
                case "Odd":
                    stg.Communication.SerialPort.Parity = Parity.Odd;
                    break;
                case "Space":
                    stg.Communication.SerialPort.Parity = Parity.Space;
                    break;
            }
            // Serial Stop Bit
            switch (sts.SerialStopBit)
            {
                case "None":
                    stg.Communication.SerialPort.StopBits = StopBits.None;
                    break;
                case "One":
                    stg.Communication.SerialPort.StopBits = StopBits.One;
                    break;
                case "OnePointFive":
                    stg.Communication.SerialPort.StopBits = StopBits.OnePointFive;
                    break;
                case "Two":
                    stg.Communication.SerialPort.StopBits = StopBits.Two;
                    break;
            }
            // Serial Flow Control
            switch (sts.SerialFlowControl)
            {
                case "None":
                    stg.Communication.SerialPort.FlowControl = Handshake.None;
                    break;
                case "RTS/CTS":
                    stg.Communication.SerialPort.FlowControl = Handshake.RequestToSend;
                    break;
                case "XON/XOFF":
                    stg.Communication.SerialPort.FlowControl = Handshake.XOnXOff;
                    break;
            }
            // IP
            stg.Communication.TcpClient.IpAddress = sts.TcpIpAddress;
            stg.Communication.TcpClient.Port = sts.TcpPortNumber;
            // Initial Command
            stg.Communication.SerialPort.InitialSendingCommand = sts.InitialSengingCommand;
            stg.Communication.TcpClient.InitialSendingCommand = sts.InitialSengingCommand;
        }


        protected override void OnInitialized()
        {
            _commandList = new List<CommandInfo>
                {
                    new CommandInfo {Id = 0, Command = "NONE", Name = "NONE"},
                    new CommandInfo {Id = 1, Command = "esoc, USB1, GPSNav+GLONav+GALNav+QZSNav+BDSNav", Name = "esoc, USB1, GPSNav+GLONav+GALNav+QZSNav+BDSNav (Septentrio/Get Nav data)"},
                    new CommandInfo {Id = 2, Command = "esoc, COM1, GPSNav+GLONav+GALNav+QZSNav+BDSNav", Name = "esoc, COM1, GPSNav+GLONav+GALNav+QZSNav+BDSNav (Septentrio/Get Nav data)"},
                    new CommandInfo {Id = 3, Command = "esoc, IPS1, GPSNav+GLONav+GALNav+QZSNav+BDSNav", Name = "esoc, IPS1, GPSNav+GLONav+GALNav+QZSNav+BDSNav (Septentrio/Get Nav data)"}
                };
            _selectedCommandItem = _commandList.FirstOrDefault();
            _selectedCommandValue = 0;
        }

        protected override async Task OnParametersSetAsync()
        {
            await Task.Run(() => SetDisp());
        }

        private void SetDisp()
        {
            var command = NowStatus.InitialSengingCommand;
            var item = _commandList.Where(s => s.Command == command).FirstOrDefault();
            if (item != null)
            {
                _selectedCommandItem = item;
                _selectedCommandValue = item.Id;
            }
        }



        class CommandInfo
        {
            public int Id { get; set; }
            public string Command { get; set; } = "";
            public string Name { get; set; } = "";
        }
        List<CommandInfo>? _commandList;
        int? _selectedCommandValue;
        CommandInfo? _selectedCommandItem;
        string _name = string.Empty;

        private void OnSelectedCommandItemChangedHandler(CommandInfo value)
        {
            _selectedCommandItem = value;
            NowStatus.InitialSengingCommand = _selectedCommandItem.Command;
        }

        private void AddCommandItem(MouseEventArgs args)
        {
            if (!string.IsNullOrWhiteSpace(_name) && _commandList != null)
            {
                // _nameがipかどうかを確かめること
                if (!string.IsNullOrEmpty(_name))
                {
                    int newId = _commandList.Count + 1;
                    _commandList.Add(new CommandInfo { Id = newId, Command = _name, Name = _name });
                }
                _name = string.Empty;
            }
        }


    }

    public class DataSourceStatus
    {
        public string DataType { get; set; } = "SBF";
        public int CnnectionType { get; set; } = 1;
        public string SerialPortName { get; set; } = "";
        public string SerialBaudRate { get; set; } = "115200";
        public string SerialDataBits { get; set; } = "8";
        public string SerialParity { get; set; } = "None";
        public string SerialStopBit { get; set; } = "One";
        public string SerialFlowControl { get; set; } = "None";
        public string TcpIpAddress { get; set; } = "127.0.0.1";
        public int TcpPortNumber { get; set; } = 52585;
        public string InitialSengingCommand { get; set; } = "NONE";
        public DataSourceStatus Clone()
        {
            return (DataSourceStatus)MemberwiseClone();
        }

    }

}
