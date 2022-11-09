using GrToolBox.Common;
using GrToolBox.Data.Nmea;
using GrToolBox.Data.SBF;
using GrToolBox.Settings;
using System.Diagnostics;
using System.IO.Ports;

namespace GrToolBox.Communication
{
    public class SerialDevice
    {

        private SerialPort Sp { get; set; }
        private NmeaConverter NmeaConverter { get; set; }
        private SbfConverter SbfConverter { get; set; }
        private CircularBuffer<byte> Cb { get; set; } = new CircularBuffer<byte>(24);   // 2^12

        //private bool FirstFlag { get; set; } = true;
        private string InitialSendingCommand { get; set; }

        /// <summary>
        ///   コンストラクタ
        /// </summary>
        public SerialDevice(SerialPortSettings stg)
        {
            Sp = new SerialPort();
            Sp.PortName = stg.PortName;
            Sp.BaudRate = stg.BaudRate;
            Sp.DataBits = stg.DataBits;
            Sp.Parity = stg.Parity;
            Sp.StopBits = stg.StopBits;
            Sp.WriteTimeout = stg.WriteTimeout;
            Sp.ReceivedBytesThreshold = stg.ReceivedBytesThreshold;
            Sp.DataReceived += new SerialDataReceivedEventHandler(DataReceived);
            InitialSendingCommand = stg.InitialSendingCommand;
        }

        public SerialDevice(GrSettings stg) : this(stg.Communication.SerialPort) { }

        public bool Open()
        {
            try
            {
                Sp.Open();
            }
            catch (System.Exception ex)
            {
                //throw new ApplicationException("Could not open com port", ex);
                return false;
            }
            var tmp = Sp.ReadExisting();    // 一旦読み出してバッファをクリアしておかないと繋がりが悪い

            if (Sp.IsOpen)
            {
                //int nBuf = Sp.BytesToRead;
                //byte[] buf = new byte[nBuf];
                //Sp.Read(buf, 0, nBuf);

                Debug.WriteLine(Sp.PortName + " Opened!!");
                //string str = "esoc, USB1, Rinex \n";
                Debug.WriteLine("Sending Command: " + InitialSendingCommand);
                Sp.Write(InitialSendingCommand + "\n");
            }
            else
            {
                Debug.WriteLine("Failed to open " + Sp.PortName + "!!");
            }

            return true;

        }

        public bool Close()
        {
            Sp.Close();
            return !Sp.IsOpen;
        }

        public bool Dispose()
        {
            Sp.Dispose();
            return !Sp.IsOpen;
        }


        public void AddConverter(NmeaConverter nc)
        {
            NmeaConverter = nc;
        }

        public void AddConverter(SbfConverter sc)
        {
            SbfConverter = sc;
        }


        private void DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            int nBuf = Sp.BytesToRead;
            byte[] buf = new byte[nBuf];
            Sp.Read(buf, 0, nBuf);
            if (NmeaConverter != null) NmeaConverter.AddDataAndAnalysis(buf);
            if (SbfConverter != null) SbfConverter.AddDataAndAnalysis(buf);

            //int spBytes = Sp.BytesToRead;
            //byte[] buf = null;
            //int nBuf = spBytes;
            ////Console.WriteLine(spBytes + " Received");
            //if (spBytes > 64)
            //{
            //    // 全コンバータのうち，バッファに最も余裕のないものにあわせる
            //    if (NmeaConverter != null)   nBuf = NmeaConverter.AvailableBufferSize() < nBuf ? NmeaConverter.AvailableBufferSize() : nBuf;
            //    if (SbfConverter != null)    nBuf = SbfConverter.AvailableBufferSize() < nBuf ? SbfConverter.AvailableBufferSize() : nBuf;


            //    buf = new byte[nBuf];
            //    Sp.Read(buf, 0, nBuf);
            //    //Console.WriteLine( "Data Received" + BitConverter.ToString(buf));

            //    // 各コンバータのCbにデータを追加
            //    if (NmeaConverter != null) NmeaConverter.AddDataAndAnalysis(buf);
            //    if (SbfConverter != null)  SbfConverter.AddDataAndAnalysis(buf);
            //}
        }
    }
}
