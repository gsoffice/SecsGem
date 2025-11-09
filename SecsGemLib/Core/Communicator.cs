using SecsGemLib.Utils;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace SecsGemLib.Core
{
    public class Communicator
    {
        private TcpClient _client;
        private NetworkStream _stream;
        private CancellationTokenSource _cts;
        private readonly bool _passive;

        public event Action Connected;
        public event Action Disconnected;
        public event Action<byte[]> DataReceived;

        public bool IsConnected => _client?.Connected ?? false;
        public string RemoteIP { get; }
        public int RemotePort { get; }

        public Communicator(string ip, int port, bool passive = false)
        {
            RemoteIP = ip;
            RemotePort = port;
            _passive = passive;
        }

        public async Task<bool> ConnectAsync()
        {
            _cts = new CancellationTokenSource();
            if (_passive) await StartServerAsync();
            else await StartClientAsync();
            return true;
        }

        private async Task StartClientAsync()
        {
            while (!_cts.Token.IsCancellationRequested)
            {
                try
                {
                    _client = new TcpClient();
                    Logger.Write($"[HSMS] Connecting to {RemoteIP}:{RemotePort}...");
                    await _client.ConnectAsync(IPAddress.Parse(RemoteIP), RemotePort);
                    _stream = _client.GetStream();
                    Logger.Write("[HSMS] Connected.");
                    Connected?.Invoke();
                    _ = Task.Run(() => ReceiveLoop(_cts.Token));
                    return;
                }
                catch
                {
                    Logger.Write("[HSMS] Connect failed. Retrying in 5s...");
                    await Task.Delay(5000, _cts.Token);
                }
            }
        }

        private async Task StartServerAsync()
        {
            TcpListener listener = new(IPAddress.Parse(RemoteIP), RemotePort);
            listener.Start();
            Logger.Write($"[HSMS] Listening on {RemoteIP}:{RemotePort}");

            _ = Task.Run(async () =>
            {
                while (!_cts.Token.IsCancellationRequested)
                {
                    try
                    {
                        Logger.Write("[HSMS] Waiting for Host connection...");
                        _client = await listener.AcceptTcpClientAsync();
                        _stream = _client.GetStream();
                        Logger.Write("[HSMS] Host connected.");
                        Connected?.Invoke();
                        await ReceiveLoop(_cts.Token);
                        Logger.Write("[HSMS] Disconnected. Waiting for reconnect...");
                    }
                    catch (ObjectDisposedException) { break; }
                    catch (Exception ex)
                    {
                        Logger.Write($"[HSMS] Server accept error: {ex.Message}");
                        await Task.Delay(2000);
                    }
                }
            });
        }

        private async Task ReceiveLoop(CancellationToken token)
        {
            byte[] buffer = new byte[8192];
            try
            {
                while (!token.IsCancellationRequested)
                {
                    int read = await _stream.ReadAsync(buffer, 0, buffer.Length, token);
                    if (read == 0) { Disconnect(); return; }

                    byte[] data = new byte[read];
                    Array.Copy(buffer, data, read);

                    var msg = SecsDecoder.Parse(data);
                    Logger.Write($"[Comm] Rx : {ByteHelper.ToHex(data)}");
                    Logger.Write($"[Comm] Rx : {msg}");
                    DataReceived?.Invoke(data);
                }
            }
            catch
            {
                Disconnect(); 
            }
        }

        public async Task SendAsync(byte[] data)
        {
            if (_stream == null || !_client?.Connected == true) return;

            var msg = SecsDecoder.Parse(data);
            Logger.Write($"[Comm] Tx : {ByteHelper.ToHex(data)}");
            Logger.Write($"[Comm] Tx : {msg}");
            await _stream.WriteAsync(data, 0, data.Length);
        }

        public void Disconnect()
        {
            try
            {
                _stream?.Close();
                _client?.Close();
            }
            catch { /* ignore */ }
            Disconnected?.Invoke();
        }
    }
}
