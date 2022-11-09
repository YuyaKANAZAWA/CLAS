using System.Management;
using System.IO.Ports;

namespace GrToolBox.Communication
{
    public class SerialPortUtilities
    {

        public static List<SerialPortName> SerialPortSearch()
        {

            List<SerialPortName> ss = new();

            if (OperatingSystem.IsWindows())
            {
                // 接続済みデバイス確認
                ManagementClass device = new ManagementClass("Win32_PnPEntity");
                // ポート名&COM番号取得
                foreach (ManagementObject port in device.GetInstances())
                {
                    // ポート名を取得
                    string name = (string)port.GetPropertyValue("Name");
                    if (name == null) continue;
                    // "(COM" に一致する文字列数確認
                    int i = name.IndexOf("(COM");
                    if (i >= 0)
                    {
                        // 取得したポート名とCOM番号をセット
                        string caption = (string)port.GetPropertyValue("Caption");
                        // "COM*"を取り出して配列に追加
                        string portname = name.Substring(i + 1).Replace(")", "");
                        SerialPortName spn = new();
                        spn.Caption = caption;
                        spn.Name = portname;
                        ss.Add(spn);
                    }
                }
            }
            else
            {
                string[] ports = SerialPort.GetPortNames();
                foreach (string port in ports)
                {
                    SerialPortName spn = new();
                    spn.Name = port;
                    spn.Caption = "unknown";
                    ss.Add(spn);
                }
            }
            return ss;
        }

    }


    public class SerialPortName
    {
        public string Caption { get; set; } = String.Empty;
        public string Name { get; set; } = string.Empty;
    }


}
