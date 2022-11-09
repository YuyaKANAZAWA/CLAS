using System.IO.Ports;

namespace GritZ3.Components.Settings
{
    public partial class SerialPortSettings
    {
        private void GetPortName(string s)
        {
            NowStatus.SerialPortName = s;
            StateHasChanged();
        }

        private void GetBaudRate(string s)
        {
            NowStatus.SerialBaudRate = s;
            StateHasChanged();
        }

        private void GetDataBits(string s)
        {
            NowStatus.SerialDataBits = s;
            StateHasChanged();
        }

        private void GetParity(string s)
        {
            NowStatus.SerialParity = s;
            StateHasChanged();
        }

        private void GetStopBit(string s)
        {
            NowStatus.SerialStopBit = s;
            StateHasChanged();
        }

        private void GetFlowControl(string s)
        {
            NowStatus.SerialFlowControl = s;
            StateHasChanged();
        }
    }
}
