using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using System.Timers;
using Timer = System.Timers.Timer;

namespace KOSTAT_IDReader
{
    /// <summary>
    /// TCP 클라이언트 클래스
    /// </summary>
    public class CNITcpClient : IDisposable
    {
        #region Fields
        private Socket _client;
        private readonly byte[] _receiveBuffer = new byte[4100];
        private readonly string _serverIP;
        private readonly int _port;
        private bool _disposed = false;
        private bool _connected = false;
        
        private readonly Timer _reconnectTimer;
        private readonly ManualResetEvent _waitHandle = new ManualResetEvent(false);
        private readonly Ping _ping = new Ping();
        
        private readonly object _connectionLock = new object();
        private readonly object _receiveLock = new object();
        private readonly object _sendLock = new object();
        #endregion

        #region Properties
        /// <summary>포트 번호</summary>
        public int Port => _port;
        
        /// <summary>서버 IP 주소</summary>
        public string ServerIP => _serverIP;
        
        /// <summary>연결 상태</summary>
        public bool IsConnected => _connected && _client?.Connected == true;
        #endregion

        #region Events
        /// <summary>서버 연결 이벤트</summary>
        public event Action Connected;
        
        /// <summary>서버 연결 해제 이벤트</summary>
        public event Action Disconnected;
        
        /// <summary>데이터 수신 이벤트</summary>
        public event Action<byte[], int> Received;
        
        /// <summary>메시지 이벤트</summary>
        public event Action<string> Message;
        #endregion

        #region Constructor
        public CNITcpClient(string serverIP, int port)
        {
            _serverIP = !string.IsNullOrEmpty(serverIP) ? serverIP : throw new ArgumentException("IP address cannot be null or empty", nameof(serverIP));
            _port = port > 0 && port <= 65535 ? port : throw new ArgumentOutOfRangeException(nameof(port), "Port must be between 1 and 65535");
            
            _reconnectTimer = new Timer(10000) { AutoReset = true };
            _reconnectTimer.Elapsed += OnReconnectTimerElapsed;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// 서버에 연결
        /// </summary>
        public void Connect()
        {
            lock (_connectionLock)
            {
                try
                {
                    if (_disposed)
                        throw new ObjectDisposedException(nameof(CNITcpClient));

                    _waitHandle.Reset();
                    _connected = false;

                    _client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
                    {
                        ReceiveTimeout = 30000,
                        SendTimeout = 30000
                    };

                    if (!_client.Connected)
                    {
                        _client.BeginConnect(_serverIP, _port, OnConnectCallback, _client);

                        if (!_waitHandle.WaitOne(5000, false))
                        {
                            if (!_connected)
                            {
                                _waitHandle.Set();
                                StartReconnectTimer();
                                OnMessage($"{_serverIP}:{_port} 연결 시도 타임아웃");
                            }
                            else
                            {
                                _waitHandle.Set();
                                OnConnected();
                            }
                        }
                    }
                }
                catch (SocketException socketEx) when (socketEx.ErrorCode == 10060)
                {
                    HandleConnectionFailure("연결 타임아웃", socketEx.Message);
                }
                catch (Exception ex)
                {
                    HandleConnectionFailure("연결 오류", ex.Message);
                    CNILog.Write($"Connect Error: {ex.Message} - {ex.StackTrace}", false);
                }
            }
        }

        /// <summary>
        /// 서버에 데이터 전송
        /// </summary>
        /// <param name="data">전송할 데이터</param>
        public void Send(byte[] data)
        {
            lock (_sendLock)
            {
                try
                {
                    if (_disposed || data == null || data.Length == 0)
                        return;

                    if (_client?.Connected == true)
                    {
                        _client.BeginSend(data, 0, data.Length, SocketFlags.None, null, null);
                        OnMessage($"클라이언트 전송: {data.Length} bytes");
                    }
                    else
                    {
                        OnMessage("전송 실패: 연결되지 않음");
                    }
                }
                catch (Exception ex)
                {
                    OnMessage($"전송 오류: {ex.Message}");
                    CNILog.Write($"Send Error: {ex.Message} - {ex.StackTrace}", false);
                }
            }
        }
        #endregion

        #region Private Methods
        private void OnConnectCallback(IAsyncResult ar)
        {
            try
            {
                Socket socket = ar?.AsyncState as Socket;
                if (_disposed || socket == null)
                    return;

                socket.EndConnect(ar);

                if (socket.Connected)
                {
                    _connected = true;
                    StartReconnectTimer();
                    socket.BeginReceive(_receiveBuffer, 0, _receiveBuffer.Length, SocketFlags.None, ReceiveMessage, socket);
                    OnMessage($"{_serverIP}:{_port} 연결 성공");
                }
            }
            catch (Exception ex)
            {
                OnMessage($"연결 콜백 오류: {ex.Message}");
                CNILog.Write($"CallBack Error: {ex.Message} - {ex.StackTrace}", false);
            }
        }

        private void OnReconnectTimerElapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                if (_disposed)
                    return;

                var pingReply = _ping.Send(_serverIP, 3000);
                if (pingReply.Status != IPStatus.Success)
                {
                    HandleConnectionLoss($"Ping 실패: {pingReply.Status}");
                }
                else if (!_connected && !_disposed)
                {
                    _reconnectTimer.Stop();
                    Connect();
                }
            }
            catch (Exception ex)
            {
                OnMessage($"Ping 오류: {ex.Message}");
                CNILog.Write($"Ping Error: {ex.Message} - {ex.StackTrace}", false);
            }
        }

        private void ReceiveMessage(IAsyncResult ar)
        {
            lock (_receiveLock)
            {
                try
                {
                    Socket socket = ar?.AsyncState as Socket;
                    if (_disposed || socket == null || !socket.Connected)
                        return;

                    int length = socket.EndReceive(ar);

                    if (length == 0)
                    {
                        HandleConnectionLoss("서버에서 연결을 끊었습니다");
                        return;
                    }

                    OnReceived(_receiveBuffer, length);

                    // 계속 수신 대기
                    if (socket.Connected && !_disposed)
                        socket.BeginReceive(_receiveBuffer, 0, _receiveBuffer.Length, SocketFlags.None, ReceiveMessage, socket);

                    Array.Clear(_receiveBuffer, 0, length);
                }
                catch (SocketException socketEx)
                {
                    if (socketEx.ErrorCode == 10054 || socketEx.ErrorCode == 10053)
                    {
                        HandleConnectionLoss($"연결이 끊어졌습니다: {socketEx.Message}");
                    }
                    else
                    {
                        HandleConnectionLoss($"수신 오류: {socketEx.Message}");
                        CNILog.Write($"ReceiveMessage Error: {socketEx.Message} - {socketEx.StackTrace}", false);
                    }
                }
                catch (Exception ex)
                {
                    HandleConnectionLoss($"수신 오류: {ex.Message}");
                    CNILog.Write($"ReceiveMessage Error: {ex.Message} - {ex.StackTrace}", false);
                }
            }
        }

        private void HandleConnectionFailure(string errorType, string message)
        {
            OnDisconnected();
            _client?.Close();
            _waitHandle.Set();
            StartReconnectTimer();
            _connected = false;
            OnMessage($"{errorType}: {message}");
        }

        private void HandleConnectionLoss(string message)
        {
            StartReconnectTimer();
            _connected = false;
            _client?.Close();
            OnDisconnected();
            OnMessage(message);
        }

        private void StartReconnectTimer()
        {
            _reconnectTimer.Stop();
            _reconnectTimer.Start();
        }
        #endregion

        #region Event Handlers
        protected virtual void OnConnected() => Connected?.Invoke();
        protected virtual void OnDisconnected() => Disconnected?.Invoke();
        protected virtual void OnReceived(byte[] data, int length) => Received?.Invoke(data, length);
        protected virtual void OnMessage(string message) => Message?.Invoke(message);
        #endregion

        #region IDisposable
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    try
                    {
                        _reconnectTimer?.Stop();
                        _reconnectTimer?.Dispose();

                        if (_client != null)
                        {
                            if (_client.Connected)
                                _client.Shutdown(SocketShutdown.Both);
                            _client.Close();
                            _client = null;
                        }

                        _waitHandle?.Set();
                        _waitHandle?.Dispose();
                        _ping?.Dispose();
                    }
                    catch (Exception ex)
                    {
                        OnMessage($"Dispose 오류: {ex.Message}");
                    }
                }
                _disposed = true;
            }
        }
        #endregion
    }
}
