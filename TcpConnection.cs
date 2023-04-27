using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TcpConnectionNS
{
    public class DisconnectEventArgs : EventArgs
    {
        public bool requested;
    }

    public class LineReceivedEventArgs : EventArgs
    {
        public string line;
    }

    class TcpConnection : IDisposable
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly TcpClient _client;
        private NetworkStream _stream;
        private Task _receiveTask;
        private readonly string _host;
        private readonly int _port;
        private readonly CancellationTokenSource _cts;

        private const int bufferSize = ushort.MaxValue; // 65536

        public bool connected { get { return _client != null && _client.Connected; } }

        public event EventHandler<LineReceivedEventArgs> lineReceived;
        public event EventHandler<DisconnectEventArgs> disconnected;

        public TcpConnection(string host, int port)
        {
            _host = host;
            _port = port;
            _client = new TcpClient();
            _cts = new CancellationTokenSource();
        }

        public async Task<bool> connect()
        {
            IPAddress hostIP;
            if (IPAddress.TryParse(_host, out hostIP))
            {
                try
                {
                    await _client.ConnectAsync(hostIP, _port);
                    _stream = _client.GetStream();
                    logger.Debug($"{_host}:{_port} connected");
                    _receiveTask = ReceiveAsync(_cts.Token);
                    return true;
                } catch (SocketException ex)
                {
                    logger.Error(ex, $"{_host}:{_port} connection exception");
                }
            }
            return false;
        }

        public async Task SendAsync(string message)
        {
            try
            {
                byte[] data = Encoding.ASCII.GetBytes(message);
                await _stream.WriteAsync(data, 0, data.Length).ConfigureAwait(false);
                logger.Debug($"{_host}:{_port} send: {message}");
            } catch (Exception ex) {
                logger.Error(ex, $"{_host}:{_port} write exception\n");
                disconnected?.Invoke(this, new DisconnectEventArgs() { requested = false });
            }
        }

        public async Task sendCommand(string command)
        {
            await SendAsync(command + "\r\n").ConfigureAwait(false);
        }

        private async Task ReceiveAsync(CancellationToken token)
        {
            logger.Debug($"{_host}:{_port} receiving...");
            byte[] buffer = new byte[1024];
            bool disconnectRequested = false;
            StringBuilder stringRead =  new StringBuilder();
            try
            {
                while (true)
                {
                    int bytesRead = await _stream.ReadAsync(buffer, 0, buffer.Length, token).ConfigureAwait(false);
                    if (bytesRead == 0)
                    {
                        logger.Debug($"{_host}:{_port} stream eof, closing connection");
                        Close();
                        break;
                    }
                    int co = 0;
                    logger.Debug($"{_host}:{_port} received {bytesRead} bytes");

                    while (co < bytesRead)
                    {
                        string ch = Encoding.ASCII.GetString(buffer, co++, 1);
                        stringRead.Append(ch);
                        if (ch.Equals("\n"))
                        {
                            string line = stringRead.ToString();
                            logger.Debug($"{_host}:{_port} received: {line}");
                            invokeLineReceived(line);
                            stringRead.Clear();
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
                if (_client.Connected)
                {
                    _stream.Close();
                }
                logger.Debug($"{_host}:{_port} disconnected by request");
                disconnectRequested = true;
            }
            catch (Exception ex)
            {
                if (_client.Connected)
                {
                    _stream.Close();
                }
                logger.Error(ex, $"{_host}:{_port} read exception\n");
            }
            invokeDisconnected(disconnectRequested);
        }

        private void invokeLineReceived(string line)
        {
            _ = Task.Run(() => { 
                try
                {
                    lineReceived?.Invoke(this, new LineReceivedEventArgs() { line = line });
                }
                catch (Exception ex)
                {
                    logger.Error(ex, $"{_host}:{_port} lineReceived handler exception\n");
                }
            });
        }

        private void invokeDisconnected(bool requested)
        {
            _ = Task.Run(() => {
                try
                {
                    disconnected?.Invoke(this, new DisconnectEventArgs() { requested = requested });
                }
                catch (Exception ex)
                {
                    logger.Error(ex, $"{_host}:{_port} disconnected handler exception\n");
                }
            });
        }


        public void Close()
        {
            _cts.Cancel();
            _receiveTask?.Wait();
            _client?.Close();
            _stream = null;
            _receiveTask = null;
        }

        public void Dispose()
        {
            Close();
        }
    }
}
