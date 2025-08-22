using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SharpBoxesCore.Helpers.TCP;

public class SocketClientHelper : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;

    protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public Encoding Encoding { get; set; } = Encoding.UTF8; // 默认使用 UTF-8 编码
    private string _serverIp;
    private int _serverPort;
    private string _localIp;
    private int _localPort;
    private TcpClient _client;
    private NetworkStream _stream;
    private bool _reconnectEnabled;
    private List<string> _receivedMessages = new List<string>();
    private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
    private bool _isConnected = false;
    private Thread _reconnectThread;
    private readonly object _lock = new object();


    /// <summary>
    /// 重连间隔，默认为 5 秒
    /// </summary>
    public int ReconnectInterval { get; set; } = 5000;
    public string ServerIp
    {
        get => _serverIp;
        set
        {
            _serverIp = value;
            RaisePropertyChanged();
        }
    }
    public int ServerPort
    {
        get => _serverPort;
        set
        {
            _serverPort = value;
            RaisePropertyChanged();
        }
    }
    public string LocalIp
    {
        get => _localIp;
        set
        {
            _localIp = value;
            RaisePropertyChanged();
        }
    }
    public int LocalPort
    {
        get => _localPort;
        set
        {
            _localPort = value;
            RaisePropertyChanged();
        }
    }

    public SocketClientHelper(
        string serverIp,
        int serverPort,
        string localIp = null,
        int localPort = 0
    )
    {
        ServerIp = serverIp;
        ServerPort = serverPort;
        LocalIp = localIp;
        LocalPort = localPort;
    }

    public void SetReConnectEnable(bool enable)
    {
        _reconnectEnabled = enable;
        if (enable && _reconnectThread == null)
        {
            _reconnectThread = new Thread(ReconnectPolling);
            _reconnectThread.IsBackground = true;
            _reconnectThread.Start();
        }
    }

    public void Connect()
    {
        if (_isConnected)
        {
            throw new InvalidOperationException("Client is already connected.");
        }

        _client = new TcpClient();

        // 绑定到指定的本地 IP 和端口
        if (!string.IsNullOrEmpty(LocalIp) && LocalPort > 0)
        {
            IPAddress localAddress = IPAddress.Parse(LocalIp);
            _client.Client.Bind(new IPEndPoint(localAddress, LocalPort));
        }

        try
        {
            _client.Connect(ServerIp, ServerPort); // 同步连接
            _stream = _client.GetStream();
            _isConnected = true;
            _ = ReceiveDataAsync(_cancellationTokenSource.Token); // 启动异步接收数据
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Connection failed: {ex.Message}");
            _isConnected = false;
            if (_reconnectEnabled)
            {
                Task.Run(() => ReconnectPolling()); // 启动异步重连
            }
        }
    }

    public async Task ConnectAsync()
    {
        if (_isConnected)
        {
            throw new InvalidOperationException("Client is already connected.");
        }

        _client = new TcpClient();

        // 绑定到指定的本地 IP 和端口
        if (!string.IsNullOrEmpty(LocalIp) && LocalPort > 0)
        {
            IPAddress localAddress = IPAddress.Parse(LocalIp);
            _client.Client.Bind(new IPEndPoint(localAddress, LocalPort));
        }

        try
        {
            await _client.ConnectAsync(ServerIp, ServerPort); // 异步连接
            _stream = _client.GetStream();
            _isConnected = true;
            _ = ReceiveDataAsync(_cancellationTokenSource.Token); // 启动异步接收数据
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Connection failed: {ex.Message}");
            _isConnected = false;
            if (_reconnectEnabled)
            {
                Task.Run(() => ReconnectPolling()); // 启动异步重连
            }
        }
    }

    private void ReconnectPolling()
    {
        while (_reconnectEnabled)
        {
            if (!_isConnected)
            {
                try
                {
                    Console.WriteLine("Attempting to reconnect...");
                    _client = new TcpClient();
                    _client.ConnectAsync(ServerIp, ServerPort).Wait();
                    _stream = _client.GetStream();
                    _isConnected = true;
                    _ = ReceiveDataAsync(_cancellationTokenSource.Token);
                    Console.WriteLine("Reconnected successfully.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Reconnect failed: {ex}");
                }
            }
            Thread.Sleep(ReconnectInterval);
        }
    }

    public void Disconnect()
    {
        _reconnectEnabled = false;
        _cancellationTokenSource.Cancel();
        _stream?.Close();
        _client?.Close();
        _isConnected = false;
    }

    private async Task ReceiveDataAsync(CancellationToken cancellationToken)
    {
        byte[] buffer = new byte[1024];
        while (!cancellationToken.IsCancellationRequested && _isConnected)
        {
            try
            {
                int bytesRead = await _stream.ReadAsync(
                    buffer,
                    0,
                    buffer.Length,
                    cancellationToken
                );
                if (bytesRead > 0)
                {
                    string receivedString = Encoding.GetString(buffer, 0, bytesRead);
                    Console.WriteLine($"Received: {receivedString}");
                    lock (_lock)
                    {
                        _receivedMessages.Add(receivedString);
                        Monitor.Pulse(_lock); // 通知等待的线程有新数据到达
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Receive data failed: {ex.Message}");
                _isConnected = false;
                break;
            }
        }
    }

    public async Task SendStringAsync(string message)
    {
        if (!_isConnected)
        {
            throw new InvalidOperationException("Client is not connected.");
        }

        byte[] data = Encoding.GetBytes(message);
        Console.WriteLine($"Sending: {message}");
        await _stream.WriteAsync(data, 0, data.Length);
    }

    public List<string> GetReceiveString()
    {
        lock (_lock)
        {
            return new List<string>(_receivedMessages);
        }
    }

    public string GetNextReceiveString()
    {
        lock (_lock)
        {
            while (_receivedMessages.Count == 0)
            {
                Monitor.Wait(_lock); // 阻塞直到有新数据到达
            }
            string nextMessage = _receivedMessages[0];
            _receivedMessages.RemoveAt(0);
            return nextMessage;
        }
    }

    public void SendString(string message)
    {
        if (!_isConnected)
        {
            throw new InvalidOperationException("Client is not connected.");
        }

        byte[] data = Encoding.GetBytes(message);
        Console.WriteLine($"Sending: {message}");
        _stream.Write(data, 0, data.Length);
    }
}
