using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public class SocketServerHelper
{
    private TcpListener _listener;
    private List<ClientInfo> _clients = new List<ClientInfo>();
    private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
    private readonly object _lock = new object();
    public Encoding Encoding { get; set; } = Encoding.UTF8; // 默认使用 UTF-8 编码
    public int ReconnectInterval { get; set; } = 5000; // 重连间隔，默认为 5 秒

    public class ClientInfo
    {
        public TcpClient Client { get; set; }
        public string IP { get; set; }
        public int Port { get; set; }
        public DateTime ConnectionTime { get; set; }
        public List<string> ReceivedMessages { get; set; } = new List<string>();
        public bool IsConnected { get; set; } = false;
        public Thread ReconnectThread { get; set; }
    }

    public async Task OpenAsync(string ip, int port)
    {
        IPAddress ipAddress = IPAddress.Parse(ip); // 解析 IP 地址
        _listener = new TcpListener(ipAddress, port); // 绑定指定 IP 和端口
        _listener.Start(); // 启动监听
        await AcceptClientsAsync(_cancellationTokenSource.Token); // 异步接受客户端连接
    }

    public void Open(string ip, int port)
    {
        IPAddress ipAddress = IPAddress.Parse(ip); // 解析 IP 地址
        _listener = new TcpListener(ipAddress, port); // 绑定指定 IP 和端口
        _listener.Start(); // 同步启动监听
        _ = AcceptClientsAsync(_cancellationTokenSource.Token); // 启动异步接受客户端连接
    }

    private async Task AcceptClientsAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                TcpClient client = await _listener.AcceptTcpClientAsync();
                var clientInfo = new ClientInfo
                {
                    Client = client,
                    IP = ((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString(),
                    Port = ((IPEndPoint)client.Client.RemoteEndPoint).Port,
                    ConnectionTime = DateTime.Now,
                    IsConnected = true,
                };
                lock (_lock)
                {
                    _clients.Add(clientInfo);
                }
                _ = HandleClientAsync(clientInfo, cancellationToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Accept client failed: {ex.Message}");
            }
        }
    }

    private async Task HandleClientAsync(ClientInfo clientInfo, CancellationToken cancellationToken)
    {
        NetworkStream stream = clientInfo.Client.GetStream();
        byte[] buffer = new byte[1024];
        while (!cancellationToken.IsCancellationRequested && clientInfo.IsConnected)
        {
            try
            {
                int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);
                if (bytesRead > 0)
                {
       
                    string receivedString = Encoding.GetString(buffer, 0, bytesRead);
                    Console.WriteLine(
                        $"Received message from {clientInfo.IP}:{clientInfo.Port}: {receivedString}"
                    );
                    lock (_lock)
                    {
                        clientInfo.ReceivedMessages.Add(receivedString);
                        Monitor.Pulse(_lock); // 通知等待的线程有新数据到达
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Handle client failed: {ex.Message}");
                clientInfo.IsConnected = false;
                StartReconnectThread(clientInfo); // 启动重连线程
                break;
            }
        }
    }

    private void StartReconnectThread(ClientInfo clientInfo)
    {
        clientInfo.ReconnectThread = new Thread(() => ReconnectPolling(clientInfo));
        clientInfo.ReconnectThread.IsBackground = true;
        clientInfo.ReconnectThread.Start();
    }

    private void ReconnectPolling(ClientInfo clientInfo)
    {
        while (!clientInfo.IsConnected)
        {
            try
            {
                Console.WriteLine(
                    $"Attempting to reconnect to client {clientInfo.IP}:{clientInfo.Port}..."
                );
                clientInfo.Client = new TcpClient();
                clientInfo.Client.ConnectAsync(clientInfo.IP, clientInfo.Port).Wait();
                clientInfo.IsConnected = true;
                _ = HandleClientAsync(clientInfo, _cancellationTokenSource.Token);
                Console.WriteLine(
                    $"Reconnected to client {clientInfo.IP}:{clientInfo.Port} successfully."
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"Reconnect to client {clientInfo.IP}:{clientInfo.Port} failed: {ex}"
                );
            }
            Thread.Sleep(ReconnectInterval);
        }
    }

    public void Close()
    {
        _cancellationTokenSource.Cancel();
        _listener.Stop();
        lock (_lock)
        {
            foreach (var clientInfo in _clients)
            {
                clientInfo.Client.Close();
                clientInfo.IsConnected = false;
            }
            _clients.Clear();
        }
    }

    public async Task SendMsgAsync(string message)
    {
        lock (_lock)
        {
            foreach (var clientInfo in _clients)
            {
                if (clientInfo.IsConnected)
                {
                    Console.WriteLine(
                        $"Sending message to {clientInfo.IP}:{clientInfo.Port}: {message}"
                    );
                    _ = SendMsgAsync(clientInfo.Client, message);
                }
            }
        }
    }

    public async Task SendMsgAsync(TcpClient client, string message)
    {
        var clientInfo = _clients.Find(c => c.Client == client);
        if (clientInfo == null || !clientInfo.IsConnected)
        {
            throw new InvalidOperationException("Client is not connected.");
        }

        NetworkStream stream = client.GetStream();
        byte[] data = Encoding.GetBytes(message);
        Console.WriteLine($"Sending message to {clientInfo.IP}:{clientInfo.Port}: {message}");
        await stream.WriteAsync(data, 0, data.Length);
    }

    public List<string> GetReceiveString(TcpClient client = null)
    {
        lock (_lock)
        {
            if (client == null && _clients.Count > 0)
            {
                return new List<string>(_clients[0].ReceivedMessages);
            }
            else if (client != null)
            {
                var clientInfo = _clients.Find(c => c.Client == client);
                if (clientInfo != null)
                {
                    return new List<string>(clientInfo.ReceivedMessages);
                }
            }
            return new List<string>();
        }
    }

    public string GetNextReceiveString(TcpClient client = null)
    {
        lock (_lock)
        {
            ClientInfo clientInfo;
            if (client == null && _clients.Count > 0)
            {
                clientInfo = _clients[0];
            }
            else if (client != null)
            {
                clientInfo = _clients.Find(c => c.Client == client);
            }
            else
            {
                throw new InvalidOperationException("No client available.");
            }

            if (clientInfo == null || !clientInfo.IsConnected)
            {
                throw new InvalidOperationException("Client is not connected.");
            }

            while (clientInfo.ReceivedMessages.Count == 0)
            {
                Monitor.Wait(_lock); // 阻塞直到有新数据到达
            }
            string nextMessage = clientInfo.ReceivedMessages[0];
            clientInfo.ReceivedMessages.RemoveAt(0);
            return nextMessage;
        }
    }

    public List<ClientInfo> GetConnectedClients()
    {
        lock (_lock)
        {
            return new List<ClientInfo>(_clients);
        }
    }

    public async Task SendMsgAsync(string ip, int port, string message)
    {
        TcpClient client;
        lock (_lock)
        {
            var clientInfo = _clients.Find(c => c.IP == ip && c.Port == port);
            if (clientInfo == null || !clientInfo.IsConnected)
            {
                throw new InvalidOperationException($"Client {ip}:{port} is not connected.");
            }
            client = clientInfo.Client;
        }

        try
        {
            NetworkStream stream = client.GetStream();
            byte[] data = Encoding.GetBytes(message);
            Console.WriteLine($"Sending message to {ip}:{port}: {message}");
            await stream.WriteAsync(data, 0, data.Length);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to send message to {ip}:{port}: {ex.Message}");
            throw; // 抛出异常以便调用方处理
        }
    }

    public void SendMsg(string message)
    {
        lock (_lock)
        {
            foreach (var clientInfo in _clients)
            {
                if (clientInfo.IsConnected)
                {
                    SendMsg(clientInfo.Client, message);
                }
            }
        }
    }

    public void SendMsg(TcpClient client, string message)
    {
        var clientInfo = _clients.Find(c => c.Client == client);
        if (clientInfo == null || !clientInfo.IsConnected)
        {
            throw new InvalidOperationException("Client is not connected.");
        }

        NetworkStream stream = client.GetStream();
        byte[] data = Encoding.GetBytes(message);
        Console.WriteLine($"Sending message to {clientInfo.IP}:{clientInfo.Port}: {message}");
        stream.Write(data, 0, data.Length);
    }

    public void SendMsg(string ip, int port, string message)
    {
        TcpClient client;
        lock (_lock)
        {
            var clientInfo = _clients.Find(c => c.IP == ip && c.Port == port);
            if (clientInfo == null || !clientInfo.IsConnected)
            {
                throw new InvalidOperationException($"Client {ip}:{port} is not connected.");
            }
            client = clientInfo.Client;
        }

        try
        {
            NetworkStream stream = client.GetStream();
            byte[] data = Encoding.GetBytes(message);
            Console.WriteLine($"Sending message to {ip}:{port}: {message}");
            stream.Write(data, 0, data.Length);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to send message to {ip}:{port}: {ex.Message}");
            throw; // 抛出异常以便调用方处理
        }
    }
}
