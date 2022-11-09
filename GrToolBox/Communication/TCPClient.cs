using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using GrToolBox.Data.Nmea;
using GrToolBox.Data.SBF;
using GrToolBox.Settings;

namespace GrToolBox.Communication
{
    public class TCPClient
    {
        // 接続先設定情報
        private TcpIpSettings Stg { get; set; }

        // socket
        private Socket? Socket { get; set; }
        //受信データ保存用
        private MemoryStream? Ms { get; set; }
        //ロック用
        private readonly object syncLock = new object();
        //送受信文字列エンコード
        private Encoding enc = Encoding.UTF8;

        private NmeaConverter? NmeaConverter { get; set; }
        private SbfConverter? SbfConverter { get; set; }

        // 接続状態
        public bool IsClosed
        {
            get { return (Socket == null); }
        }

        private string InitialSendingCommand { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public TCPClient(TcpIpSettings stg)
        {
            this.Stg = stg;
            //Socket生成
            Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            InitialSendingCommand = stg.InitialSendingCommand;
        }

        public TCPClient(GrSettings stg) : this(stg.Communication.TcpClient) { }


        //public TCPClient(Socket sc)
        //{
        //    Socket = sc;
        //}


        //接続断イベント
        public delegate void DisconnectedEventHandler(object sender, EventArgs e);
        public event DisconnectedEventHandler? OnDisconnected;

        //接続OKイベント
        public delegate void ConnectedEventHandler(EventArgs e);
        public event ConnectedEventHandler? OnConnected;

        //データ受信イベント
        public delegate void ReceiveEventHandler(object sender, string e);
        public event ReceiveEventHandler? OnReceiveData;


        // データConverter追加
        public void AddConverter(NmeaConverter nc)
        {
            NmeaConverter = nc;
        }

        public void AddConverter(SbfConverter sc)
        {
            SbfConverter = sc;
        }


        public virtual void Dispose()
        {
            Close();
        }

        public void Close()
        {
            if (Socket == null) return;
            //Debug.WriteLine("Close" + " ThreadID:" + Thread.CurrentThread.ManagedThreadId);
            Socket.Shutdown(SocketShutdown.Both);
            Socket.Close();
            Socket = null;
            if (Ms != null)
            {
                Ms.Close();
                Ms = null;
            }
            //接続断イベント発生
            OnDisconnected(this, new EventArgs());
        }

        /// <summary>
        /// hostに接続
        /// </summary>
        public void Connect()
        {
            Debug.WriteLine("Connect" + " ThreadID:" + Thread.CurrentThread.ManagedThreadId);

            if (Socket == null) return;
            //IP作成
            IPEndPoint ipEnd = new IPEndPoint(Dns.GetHostAddresses(Stg.IpAddress)[0], Stg.Port);
            //ホストに接続
            //Socket.Connect(ipEnd);
            // Connect to the remote endpoint.
            Socket.BeginConnect(ipEnd,
                new AsyncCallback(ConnectCallback), Socket);
        }

        private void ConnectCallback(IAsyncResult ar)
        {
            Debug.WriteLine("ConnectCallback" + " ThreadID:" + Thread.CurrentThread.ManagedThreadId);
            try
            {
                if (ar == null || ar.AsyncState == null) return;
                // Retrieve the socket from the state object.
                Socket client = (Socket)ar.AsyncState;

                // Complete the connection.
                client?.EndConnect(ar);

                Debug.WriteLine("Socket connected to {0}", client?.RemoteEndPoint?.ToString());

                //接続OKイベント発生
                OnConnected(new EventArgs());


                //データ受信開始
                StartReceive();
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
            }
        }

        /// <summary>
        /// データ受信開始
        /// </summary>
        public void StartReceive()
        {
            if(Socket == null) return;
            //Debug.WriteLine("StartReceive" + " ThreadID:" + Thread.CurrentThread.ManagedThreadId);

            //受信バッファ
            byte[] rcvBuff = new byte[131072];
            //受信データ初期化
            Ms = new MemoryStream();

            //非同期データ受信開始
            Socket.BeginReceive(rcvBuff, 0, rcvBuff.Length, SocketFlags.None, new AsyncCallback(ReceiveDataCallback), rcvBuff);

            // 初回コマンド発出
            if (!string.IsNullOrEmpty(InitialSendingCommand) && InitialSendingCommand != "NONE")
            {
                Send(InitialSendingCommand);
            }



        }

        /// <summary>
        /// 非同期データ受信
        /// </summary>
        /// <param name="ar"></param>
        private void ReceiveDataCallback(IAsyncResult ar)
        {
            if(ar == null || ar.AsyncState == null || Socket == null || Ms == null) 
                return;
            //Debug.WriteLine("ReceiveDataCallback" + " ThreadID:" + Thread.CurrentThread.ManagedThreadId);

            int len = -1;
            lock (syncLock)
            {
                if (IsClosed)
                    return;
                //データ受信終了
                len = Socket.EndReceive(ar);
            }

            //切断された
            if (len <= 0)
            {
                Close();
                return;
            }

            //受信データ取り出し
            byte[] rcvBuff = (byte[])ar.AsyncState;
            //受信データ保存
            Ms.Write(rcvBuff, 0, len);
            Ms.Seek(0, SeekOrigin.Begin);

            byte[] buf = null;
            long nBuf = Ms.Length;

            buf = new byte[nBuf];
            Ms.Read(buf, 0, (int)nBuf);

            //Debug.WriteLine("add " + nBuf + "bytes to Cb");

            // 各コンバータのリングバッファにデータを追加
            if (NmeaConverter != null) NmeaConverter.AddDataAndAnalysis(buf);
            if (SbfConverter != null) SbfConverter.AddDataAndAnalysis(buf);

            Ms.Close();
            Ms = new MemoryStream();


            //if (Ms.Length >= 2)
            //{
            //    //\r\nかチェック
            //    Ms.Seek(-2, SeekOrigin.End);
            //    if (Ms.ReadByte() == '\r' && Ms.ReadByte() == '\n')
            //    {
            //        //受信データを文字列に変換
            //        string rsvStr = enc.GetString(Ms.ToArray());
            //        //受信データ初期化
            //        Ms.Close();
            //        Ms = new MemoryStream();

            //        //データ受信イベント発生
            //        OnReceiveData(this, rsvStr);

            //    }
            //    else
            //    {
            //        //ストリーム位置を戻す
            //        Ms.Seek(0, SeekOrigin.End);
            //    }
            //}

            lock (syncLock)
            {
                //非同期受信を再開始
                if (!IsClosed)
                    Socket.BeginReceive(rcvBuff, 0, rcvBuff.Length, SocketFlags.None, new AsyncCallback(ReceiveDataCallback), rcvBuff);
            }
        }


        /// <summary>
        /// データ送信
        /// </summary>
        /// <param name="str"></param>
        public void Send(string str)
        {
            //Debug.WriteLine("Send" + " ThreadID:" + Thread.CurrentThread.ManagedThreadId);

            if (!IsClosed && Socket != null)
            {
                //文字列をBYTE配列に変換
                byte[] sendBytes = enc.GetBytes(str + "\r\n");
                lock (syncLock)
                {
                    //送信
                    //Socket.Send(sendBytes);
                    var aaa = Socket.Send(sendBytes);   // 代入しておかないと返ってこない．要確認　タイミング？
                    //Debug.WriteLine("sent" + str + aaa + "bytes");
                }
            }
        }
    }
}
