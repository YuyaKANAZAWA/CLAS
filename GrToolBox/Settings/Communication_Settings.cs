using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.Ports;
using static GrToolBox.Settings.SettingsDefinitions;

namespace GrToolBox.Settings
{
    public class Communication_Settings
    {
        public Connection_Type Connection { get; set; } = Connection_Type.Serial;
        public SerialPortSettings SerialPort { get; set; } = new();
        public TcpIpSettings TcpClient { get; set; } = new();

        public bool ServerMode { get; set; } = true;
        public TcpIpSettings TcpServer { get; set; } = new();

        public Data_Type DataType { get; set; } = Data_Type.SBF;


    }

    public class SerialPortSettings
    {
        public string PortName { get; set; } = "";
        public int BaudRate { get; set; } = 115200;
        public int DataBits { get; set; } = 8;
        public Parity Parity { get; set; } = Parity.None;
        public StopBits StopBits { get; set; } = StopBits.One;
        public Handshake FlowControl { get; set; } = Handshake.None;
        public int WriteTimeout { get; set; } = 500;
        public int ReceivedBytesThreshold { get; set; } = 64;
        public string InitialSendingCommand { get; set; } = "NONE";

    }

    public class TcpIpSettings
    {
        public int Port { get; set; } = 1234;
        public string IpAddress { get; set; } = "127.0.0.1";
        public int ReadTimeout { get; set; } = 1000;
        public int WtiteTimeout { get; set; } = 1000;
        public string InitialSendingCommand { get; set; } = "NONE";

    }


}
