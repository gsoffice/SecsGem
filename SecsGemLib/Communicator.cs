using SecsGemLib.Messages;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace SecsGemLib
{
    public class Communicator
    {
        private TcpClient _client;
        private NetworkStream _stream;
        private CancellationTokenSource _cts;
        private bool _passive;

        public event Action Connected;
        public event Action Disconnected;
        public event Action OnSelected;
        public event Action OnEstablish;
        public event Action<byte[]> DataReceived;

        public bool IsConnected => _client?.Connected ?? false;
        public string RemoteIP { get; private set; }
        public int RemotePort { get; private set; }

        public Communicator(string ip, int port, bool passive = false)
        {
            RemoteIP = ip;
            RemotePort = port;
            _passive = passive;
        }

        public async Task<bool> ConnectAsync()
        {
            try
            {
                _cts = new CancellationTokenSource();

                if (_passive)
                {
                    // Server Mode (Passive)
                    TcpListener listener = new TcpListener(IPAddress.Parse(RemoteIP), RemotePort);
                    listener.Start();
                    Console.WriteLine($"[HSMS] Listening on {RemoteIP}:{RemotePort}");

                    _ = Task.Run(async () =>
                    {
                        while (!_cts.Token.IsCancellationRequested)
                        {
                            try
                            {
                                Console.WriteLine("[HSMS] Waiting for Host connection...");
                                _client = await listener.AcceptTcpClientAsync();
                                Console.WriteLine("[HSMS] Host connected!");

                                _stream = _client.GetStream();
                                Connected?.Invoke();
                                await ReceiveLoop(_cts.Token);

                                // 연결이 끊기면 다시 대기
                                Console.WriteLine("[HSMS] Connection lost. Waiting for new Host...");
                            }
                            catch (ObjectDisposedException)
                            {
                                // listener가 중단되면 루프 종료
                                break;
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"[HSMS] Server accept error: {ex.Message}");
                                await Task.Delay(2000);
                            }
                        }
                    });                   
                }
                else
                {
                    // Client Mode (Active)
                    while (!_cts.Token.IsCancellationRequested)
                    {
                        try
                        {
                            _client = new TcpClient();
                            Console.WriteLine($"[HSMS] Trying to connect to {RemoteIP}:{RemotePort}...");
                            await _client.ConnectAsync(IPAddress.Parse(RemoteIP), RemotePort);
                            Console.WriteLine("[HSMS] Connected to Host!");

                            _stream = _client.GetStream();
                            _ = Task.Run(() => ReceiveLoop(_cts.Token));
                            Connected?.Invoke();
                            return true;
                        }
                        catch
                        {
                            Console.WriteLine("[HSMS] Connection failed. Retrying in 5 seconds...");
                            await Task.Delay(5000, _cts.Token);
                        }
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[HSMS] ConnectAsync error: {ex.Message}");
                return false;
            }
        }

        private async Task ReceiveLoop(CancellationToken token)
        {
            byte[] buffer = new byte[8192];
            try
            {
                while (!token.IsCancellationRequested)
                {
                    int read = await _stream.ReadAsync(buffer, 0, buffer.Length, token);
                    if (read == 0)
                    {
                        Disconnect();
                        return;
                    }

                    byte[] data = new byte[read];
                    Array.Copy(buffer, data, read);
                    Logger.Write($"[Comm] Rx : {BitConverter.ToString(data).Replace("-", " ")}");                    
                    DataReceived?.Invoke(data);
                }
            }
            catch
            {
                Disconnect();
            }
        }

        public async Task SendAsync(Message data)
        {
            if (_stream == null || !_client.Connected)
            {
                return;
            }

            Logger.Write($"[Comm] Tx : {data.ToString()}");
            Logger.Write($"[Comm] Tx : {data.ToBytes()}");

            byte[] buffer = data.ToBytes();
            int length = buffer.Length;
            await _stream.WriteAsync(buffer, 0, length);
        }

        public void Disconnect()
        {
            try
            {
                //_cts?.Cancel();
                _stream?.Close();
                _client?.Close();
            }
            catch { }
            Disconnected?.Invoke();
        }
    }
}
