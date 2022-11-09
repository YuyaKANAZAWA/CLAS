using GrToolBox.Communication;
using GrToolBox.Data.Nmea;
using GrToolBox.Settings;
using System.IO.Ports;
using static GrToolBox.Data.Nmea.NmeaConverter;

// シリアルポート名（"COM1"，"/dev/ttyS0"等）を取得
string[] ports = SerialPort.GetPortNames();

// シリアルポート名を画面表示
Console.WriteLine("The following serial ports has been found:");
int portNum = 0;
foreach (string p in ports)
{
    Console.WriteLine($"{portNum}: {p}");
    portNum++;
}

// シリアルポートを選択
Console.Write("Enter port number: ");
string portNumStr = Console.ReadLine();
portNum = int.Parse(portNumStr);

var stg = new SerialPortSettings();
stg.PortName = ports[portNum];
var SD = new SerialDevice(stg);


NmeaConverter NmeaConverter = new NmeaConverter();
NmeaConverter.NewNmeaDataReceived += DispEpochData;

SD.AddConverter(NmeaConverter);

SD.Open();
Console.ReadLine();
SD.Close();


static void DispEpochData(object? sender, NewNmeaDataReceivedEventArgs e)
{
    foreach(var b in e.NewBursts)
    {
        if(b.Rmc != null)   Console.WriteLine(b.Rmc.RawMsg);
        if(b.Gga != null)   Console.WriteLine(b.Gga.RawMsg);
    }
}
