using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace KOSTAT_IDReader
{
    /// <summary>
    /// TCP 서버 클래스
    /// </summary>
    public class CNITcpServer : IDisposable
    {
        #region Fields
        private Socket _server;
        private Socket _client;
        private readonly byte[] _receiveBuffer = new byte[1075];
        private readonly IPAddress _serverIP;
        private readonly int _port;
        private bool _disposed = false;
        private readonly object _receiveLock = new object();
        #endregion

        #region Properties
        /// <summary>포트 번호</summary>
        public int Port => _port;
        
        /// <summary>IP 주소</summary>
        public string IPAddress => _serverIP?.ToString() ?? "Unknown";
        
        /// <summary>클라이언트 연결 상태</summary>
        public bool IsClientConnected => _client?.Connected == true;
        #endregion

        #region Events
        /// <summary>클라이언트 연결 이벤트</summary>
        public event Action<string> Connected;
        
        /// <summary>클라이언트 연결 해제 이벤트</summary>
        public event Action Disconnected;
        
        /// <summary>데이터 수신 이벤트</summary>
        public event Action<string> Received;
        
        /// <summary>메시지 이벤트</summary>
        public event Action<string> Message;
        #endregion

        #region Constructor
        public CNITcpServer(IPAddress serverIP, int port)
        {
            _serverIP = serverIP ?? throw new ArgumentNullException(nameof(serverIP));
            _port = port > 0 && port <= 65535 ? port : throw new ArgumentOutOfRangeException(nameof(port));
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// 서버 시작 및 클라이언트 연결 대기
        /// </summary>
        public void Listen()
        {
            try
            {
                if (_disposed)
                    throw new ObjectDisposedException(nameof(CNITcpServer));

                _server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                _server.Bind(new IPEndPoint(_serverIP, _port));
                _server.Listen(5);
                _server.BeginAccept(ClientAccepted, _server);
                
                OnMessage($"{_serverIP}:{_port} Server Ready!");
            }
            catch (Exception ex)
            {
                OnMessage($"Server Listen Error: {ex.Message}");
                CNILog.Write($"Server Listen Error: {ex.Message} - {ex.StackTrace}", false);
            }
        }

        /// <summary>
        /// 클라이언트에게 데이터 전송
        /// </summary>
        /// <param name="message">전송할 메시지</param>
        public void Send(string message)
        {
            try
            {
                if (_disposed || string.IsNullOrEmpty(message))
                    return;

                if (_client?.Connected == true)
                {
                    byte[] sendBuffer = Encoding.UTF8.GetBytes(message);
                    _client.BeginSend(sendBuffer, 0, sendBuffer.Length, SocketFlags.None, null, null);
                    OnMessage($"Server Send: {message}");
                }
                else
                {
                    OnMessage("Cannot send: Client not connected");
                }
            }
            catch (Exception ex)
            {
                OnMessage($"Send Error: {ex.Message}");
                CNILog.Write($"Send Error: {ex.Message} - {ex.StackTrace}", false);
            }
        }
        #endregion

        #region Private Methods
        private void ClientAccepted(IAsyncResult ar)
        {
            try
            {
                if (_disposed || ar?.AsyncState == null)
                    return;

                Socket socket = ar.AsyncState as Socket;
                if (socket == null || !socket.IsBound)
                    return;

                _client = socket.EndAccept(ar);

                if (_client != null)
                {
                    _client.BeginReceive(_receiveBuffer, 0, _receiveBuffer.Length, SocketFlags.None, ReceiveMessage, _client);
                    
                    if (_client.Connected)
                        OnConnected(_client.RemoteEndPoint?.ToString() ?? "Unknown");
                }
                
                // 새로운 연결 계속 수락
                if (!_disposed && socket.IsBound)
                    socket.BeginAccept(ClientAccepted, socket);
            }
            catch (Exception ex)
            {
                OnMessage($"Client Accept Error: {ex.Message}");
                CNILog.Write($"Client Accept Error: {ex.Message} - {ex.StackTrace}", false);
            }
        }

        private void ReceiveMessage(IAsyncResult ar)
        {
            lock (_receiveLock)
            {
                try
                {
                    if (_disposed || ar?.AsyncState == null)
                        return;

                    Socket socket = ar.AsyncState as Socket;
                    if (socket == null || !socket.Connected)
                        return;

                    int length = socket.EndReceive(ar);
                    
                    if (length == 0)
                    {
                        OnDisconnected();
                        return;
                    }

                    string receivedData = Encoding.UTF8.GetString(_receiveBuffer, 0, length);
                    OnReceived(receivedData);
                    
                    // 계속 수신 대기
                    if (socket.Connected && !_disposed)
                        socket.BeginReceive(_receiveBuffer, 0, _receiveBuffer.Length, SocketFlags.None, ReceiveMessage, socket);
                }
                catch (SocketException socketEx)
                {
                    if (socketEx.ErrorCode == 10054 || socketEx.ErrorCode == 10053)
                    {
                        // 연결 끊김
                        if (_client != null)
                        {
                            try { _client.Close(); } catch { }
                        }
                        OnDisconnected();
                        OnMessage($"Client disconnected: {socketEx.Message}");
                    }
                    else
                    {
                        OnMessage($"Socket Error: {socketEx.Message}");
                        CNILog.Write($"Socket Error: {socketEx.Message} - {socketEx.StackTrace}", false);
                    }
                }
                catch (Exception ex)
                {
                    if (_client != null)
                    {
                        try { _client.Close(); } catch { }
                    }
                    OnDisconnected();
                    OnMessage($"Receive Error: {ex.Message}");
                    CNILog.Write($"ReceiveMessage Error: {ex.Message} - {ex.StackTrace}", false);
                }
            }
        }
        #endregion

        #region Event Handlers
        protected virtual void OnConnected(string clientInfo) => Connected?.Invoke(clientInfo);
        protected virtual void OnDisconnected() => Disconnected?.Invoke();
        protected virtual void OnReceived(string data) => Received?.Invoke(data);
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
                        if (_client != null)
                        {
                            if (_client.Connected)
                                _client.Shutdown(SocketShutdown.Both);
                            _client.Close();
                            _client = null;
                        }
                        
                        if (_server != null)
                        {
                            _server.Close();
                            _server = null;
                        }
                    }
                    catch (Exception ex)
                    {
                        OnMessage($"Dispose Error: {ex.Message}");
                    }
                }
                _disposed = true;
            }
        }
        #endregion
    }
}
