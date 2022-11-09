using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GrToolBox.Communication
{
    public class TCPServer
    {
        /* TCP操作 */
        private TcpListener m_listener = null;
        private TcpClient m_client = null;
        private NetworkStream m_tcpStream = null;

        /* 送受信スレッド */
        private Thread m_serverRecvTh = null;
        private Thread m_serverSendTh = null;

        /* 状態フラグ */
        public bool m_running = false;
        public bool m_connected = false;
        private bool m_stopCommand = false;

        /* ログ表示用 */
        private enum LogType { MESSAGE, SEND, RECV };

        /// <summary>
        /// TCPサーバー開始処理
        /// </summary>
        /// <param name="port">接続ポート番号</param>
        /// <returns>処理結果(0=正常 / -1=失敗)</returns>
        public int ServerStart(int port)
        {
            // サーバー状態の初期化
            m_running = true;
            m_connected = false;
            m_stopCommand = false;

            // listenerの初期化
            try
            {
                // 全てのIPアドレスを許可
                m_listener = new TcpListener(IPAddress.Any, port);
                // IPv4のみ
                m_listener.Server.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.IPv6Only, 0);
                // ソケット再利用許可
                m_listener.Server.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.ReuseAddress, true);
                // TTLの初期値を設定
                m_listener.Server.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.IpTimeToLive, 255);
                // listen
                m_listener.Start();
            }
            catch (Exception e)
            {
                // 主にポート番号重複によるバインドエラー
                m_running = false;
                return -1;
            }

            // サーバー(受信スレッド)起動
            m_serverRecvTh = new Thread(ServerRecvThread);
            m_serverRecvTh.Start();

            // サーバー(送信スレッド)起動
//            m_serverSendTh = new Thread(ServerSendThread);
//            m_serverSendTh.Start();
            return 0;
        }

        /// <summary>
        /// サーバー受信スレッド
        /// </summary>
        private void ServerRecvThread()
        {
            // 終了指令を受信するまで継続
            while (!m_stopCommand)
            {
                try
                {
                    // 未接続
                    if (!m_connected)
                    {
                        // 接続待ち(ブロッキング)
                        // ※ 画面より切断時(m_listener.Stop())は例外句に飛ぶ
                        LogWrite(LogType.MESSAGE, $"接続待ち...");
                        m_client = m_listener.AcceptTcpClient();

                        // 接続完了
                        LogWrite(LogType.MESSAGE, $"接続されました クライアント[{m_client.Client.RemoteEndPoint.ToString()}]");
                        m_connected = true;

                        // 送受信用stream(送受信タイムアウト)の設定
                        m_tcpStream = m_client.GetStream();
                        m_tcpStream.ReadTimeout = 5000;
                        m_tcpStream.WriteTimeout = 5000;
                    }

                    // 接続済み
                    if (m_connected)
                    {
                        // 受信処理
                        ProcessRecv();
                    }
                }
                catch (Exception e)
                {
                    // 受信待ち中に画面から切断された場合
                    break;
                }
            }

            // クライアント終了処理
            if (m_client != null)
            {
                m_tcpStream.Close();
                m_client.Close();
                m_client = null;
            }
        }

        /// <summary>
        /// データ受信処理
        /// </summary>
        public void ProcessRecv()
        {
            try
            {
                // 受信処理
                byte[] recvBuffer = new byte[256];
                int recvLength = m_tcpStream.Read(recvBuffer, 0, recvBuffer.Length);

                // クライアントから切断された場合
                if (recvLength == 0)
                {
                    ServerReset();
                    return;
                }

                // データ解析処理
                LogWrite(LogType.RECV, "", recvBuffer, recvLength);
                // ParseRecvData(m_recv_buffer, m_recv_offset);
            }
            catch (System.IO.IOException e)
            {
                // 受信タイムアウト
                LogWrite(LogType.MESSAGE, $"受信タイムアウト");
            }
            catch (Exception e)
            {
                // その他の例外
                ServerReset();
            }
        }

        /// <summary>
        /// サーバー送信スレッド
        /// </summary>
        private void ServerSendThread()
        {
            // 送信中の切断実施のキャッチ
            try
            {
                while (!m_stopCommand)
                {
                    // 1s周期
                    Thread.Sleep(1000);

                    // 未接続
                    if (!m_connected)
                    {
                        // 接続待ち
                        continue;
                    }

                    //byte[] sendData = new byte[1024];
                    byte[] sendData = new byte[] { 1, 2 };
                    int sendSize = 2;
                    m_tcpStream.Write(sendData, 0, sendSize);


                    // ここに送信処理を書く
                    // byte[] sendData = new byte[1024];
                    // int sendSize = XXX;
                    // m_tcp_stream.Write(sendData, 0, sendSize);
                }
            }
            catch (Exception e)
            {
            }
        }

        public void ServerSendThread2(string sendSt)
        {
            try
            {
                if (m_connected)
                {
                    Encoding enc = Encoding.UTF8;
                    var b = enc.GetBytes(sendSt);
                    m_tcpStream.Write(b, 0, b.Length);
                }
            }
            catch (Exception e)
            {
            }

        }

        /// <summary>
        /// TCPサーバーリセット処理
        /// </summary>
        private void ServerReset()
        {
            LogWrite(LogType.MESSAGE, "切断されました");

            // 「未接続」に更新
            m_connected = false;

            // クライアントクローズ
            if (m_client != null)
            {
                m_tcpStream.Close();
                m_client.Close();
                m_client = null;
            }
        }

        /// <summary>
        /// 通信ログ表示
        /// </summary>
        /// <param name="log">ログ（文字列）</param>
        /// <param name="logBuff">ログ（電文）</param>
        /// <param name="size">ログ（電文サイズ）</param>
        private void LogWrite(LogType type, string log, byte[] logBuff = null, int size = 0)
        {
            string result = DateTime.Now.ToString("[yyyy/MM/dd hh:mm:ss]");
            switch (type)
            {
                // ログ（文字列）
                case LogType.MESSAGE:
                    result += " " + log;
                    Console.WriteLine(result);
                    break;

                // ログ（送受信電文）
                case LogType.SEND:
                case LogType.RECV:
                    if (logBuff != null && size > 0)
                    {
                        result += type == LogType.SEND ? "[S]" : "[R] ";
                        for (int i = 0; i < size; i++)
                        {
                            result += string.Format("{0,2:X2}", logBuff[i]);
                        }
                        Console.WriteLine(result);
                    }
                    break;
                default:
                    break;
            }
        }
    }
}
