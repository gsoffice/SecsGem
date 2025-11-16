using SecsGemLib.Utils;
using System.Net;
using System.Net.Sockets;
using SecsGemLib.Message.Objects;

namespace SecsGemLib.Core
{
    public static class Communicator
    {
        private static TcpClient _client;
        private static NetworkStream _stream;
        private static CancellationTokenSource _cts;

        private static bool _passive;
        private static string _remoteIp;
        private static int _remotePort;

        private static TcpListener _listener;

        public static event Action Connected;
        public static event Action Disconnected;
        public static event Action<byte[]> DataReceived;

        public static bool IsConnected => _client?.Connected ?? false;

        public static void Configure(string ip, int port, bool passive = false)
        {
            _remoteIp = ip;
            _remotePort = port;
            _passive = passive;
        }

        public static async Task<bool> ConnectAsync()
        {
            if (_remoteIp == null)
                throw new InvalidOperationException("Communicator.Configure() must be called first.");

            _cts = new CancellationTokenSource();

            if (_passive)
                await StartServerLoopAsync();
            else
                await StartClientLoopAsync();

            return true;
        }

        // -----------------------------------------------------
        // ACTIVE MODE (Client)
        // -----------------------------------------------------
        private static async Task StartClientLoopAsync()
        {
            while (!_cts.Token.IsCancellationRequested)
            {
                try
                {
                    _client = new TcpClient();
                    Logger.Write($"[HSMS] Connecting to {_remoteIp}:{_remotePort}...");

                    await _client.ConnectAsync(IPAddress.Parse(_remoteIp), _remotePort);
                    _stream = _client.GetStream();
                    Logger.Write("[HSMS] Connected.");

                    Connected?.Invoke();
                    await ReceiveLoop(_cts.Token);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Logger.Write($"[HSMS] Connect error: {ex.Message}");
                }

                Logger.Write("[HSMS] Retry connection in 5s...");
                await Task.Delay(5000, _cts.Token);
            }
        }

        // -----------------------------------------------------
        // PASSIVE MODE (Server)
        // -----------------------------------------------------
        private static async Task StartServerLoopAsync()
        {
            _listener = new TcpListener(IPAddress.Parse(_remoteIp), _remotePort);
            _listener.Start();
            Logger.Write($"[HSMS] Listening on {_remoteIp}:{_remotePort}");

            _ = Task.Run(async () =>
            {
                while (!_cts.Token.IsCancellationRequested)
                {
                    TcpClient client = null;

                    try
                    {
                        Logger.Write("[HSMS] Waiting for Host connection...");
                        client = await _listener.AcceptTcpClientAsync(_cts.Token);
                    }
                    catch (OperationCanceledException)
                    {
                        break; // 정상 종료
                    }
                    catch (ObjectDisposedException)
                    {
                        break; // Listener가 종료됨
                    }
                    catch (Exception ex)
                    {
                        Logger.Write($"[HSMS] Server accept error: {ex.Message}");
                        await Task.Delay(1000);
                        continue;
                    }

                    if (client == null)
                        continue;

                    _client = client;
                    _stream = _client.GetStream();
                    Logger.Write("[HSMS] Host connected.");
                    Connected?.Invoke();

                    await ReceiveLoop(_cts.Token);

                    Logger.Write("[HSMS] Host disconnected.");
                    Disconnected?.Invoke();

                    // 다음 Loop에서 다시 Accept 대기
                }
            });
        }

        // -----------------------------------------------------
        // RECEIVE LOOP
        // -----------------------------------------------------
        private static async Task ReceiveLoop(CancellationToken token)
        {
            byte[] buffer = new byte[8192];

            try
            {
                while (!token.IsCancellationRequested)
                {
                    int read = await _stream.ReadAsync(buffer, 0, buffer.Length, token);

                    if (read == 0)
                        break; // 연결 끊김

                    var data = new byte[read];
                    Array.Copy(buffer, data, read);
                    DataReceived?.Invoke(data);
                }
            }
            catch (OperationCanceledException)
            {
                // normal
            }
            catch (Exception ex)
            {
                Logger.Write($"[HSMS] Receive error: {ex.Message}");
            }

            CloseClientOnly();  // listener는 유지
        }

        // -----------------------------------------------------
        // SEND
        // -----------------------------------------------------
        public static async Task SendAsync(Msg msg)
        {
            if (_stream == null || _client?.Connected != true || msg == null)
            {
                return;
            }

            string outMessage;
            MsgInterpreter.Interpret(msg, out outMessage);

            var data = msg.ToBytes();
            Logger.Write($"[HSMS:OUT] {ByteHelper.ToHex(data)}");
            Logger.Write($"[SECS-II:OUT] {msg}");

            await _stream.WriteAsync(data, 0, data.Length);
        }

        // -----------------------------------------------------
        // CLOSE: active/passive 동작 구분
        // -----------------------------------------------------
        private static void CloseClientOnly()
        {
            try
            {
                _stream?.Close();
                _client?.Close();
            }
            catch { }

            _stream = null;
            _client = null;
        }

        public static void Disconnect()
        {
            _cts?.Cancel();

            CloseClientOnly();

            try { _listener?.Stop(); } catch { }
        }
    }
}
