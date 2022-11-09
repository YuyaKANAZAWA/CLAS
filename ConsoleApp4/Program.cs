using GrToolBox.Communication;
using GrToolBox.Data.Nmea;
using GrToolBox.Settings;
using static GrToolBox.Data.Nmea.NmeaConverter;

TcpIpSettings stg = new TcpIpSettings();
stg.Port = 52585;
//stg.IpAddress = "192.168.3.1";
stg.IpAddress = "133.19.154.242";

TCPClient tCPClient= new TCPClient(stg);

tCPClient.OnConnected += OnConnected;
//tCPClient.OnDisconnected += OnDisconnected;
//tCPClient.OnReceiveData += OnReceiveData;

NmeaConverter NmeaConverter = new NmeaConverter();
tCPClient.AddConverter(NmeaConverter);
NmeaConverter.NewNmeaDataReceived += DispEpochData;

try
{
    //接続処理
    // Connect to the remote endpoint.
    tCPClient.Connect();
}
catch (Exception ex)
{
    Console.WriteLine(ex.Message);
}
Console.ReadLine();
tCPClient.Close();


static void DispEpochData(object? sender, NewNmeaDataReceivedEventArgs e)
{
    foreach (var b in e.NewBursts)
    {
        if (b.Rmc != null) Console.WriteLine(b.Rmc.RawMsg);
        if (b.Gga != null) Console.WriteLine(b.Gga.RawMsg);
    }
}


/** 接続OKイベント **/
void OnConnected(EventArgs e)
{
    //接続OK処理
    Console.WriteLine("tClient_OnConnected" + " ThreadID:" + Thread.CurrentThread.ManagedThreadId);
}


/** データ受信イベント **/
void OnReceiveData(object sender, string e)
{
    //Console.WriteLine("OnReceiveData" + " ThreadID:" + Thread.CurrentThread.ManagedThreadId);
    //別スレッドからくるのでInvokeを使用
    //Console.WriteLine("ReceiveData:" + e + " ThreadID:" + Thread.CurrentThread.ManagedThreadId);
    Console.WriteLine(e);
}
