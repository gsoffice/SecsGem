using SecsGemLib.Core;
using SecsGemLib.Gem.Variables;
using SecsGemLib.Gem.Events;
using System.Text;
using SecsGemLib.Message.Objects;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SecsGemLib
{
    /// <summary>
    /// GEM 상위 클래스 - HSMS 통신 관리 및 추후 SECS-II 메시지 처리 진입점
    /// </summary>
    public class GemApi
    {
        // C++ 콜백 저장: void(byte*, int, long)
        private unsafe static delegate* unmanaged[Cdecl]<byte*, int, long, void> _nativeCallback = null;

        public static unsafe void RegisterNativeCallback(delegate* unmanaged[Cdecl]<byte*, int, long, void> callback)
        {
            _nativeCallback = callback;
        }

        private MsgHandler _hsmsHandler;
        public event Action<byte[]> OnMessageReceive;

        public bool IsConnected => Communicator.IsConnected;

        // 외부로 노출할 이벤트
        public event Action Connected;
        public event Action Disconnected;
        public event Action<string> MessageReceived;

        /// <summary>
        /// GEM 객체 생성
        /// </summary>
        public GemApi()
        {
            MsgRouter.AutoRegisterHandlers();
            MsgDataSaverRegistry.AutoRegister();
            MsgFormatRegistry.LoadFromFile("DATA\\FORMAT.SML");
        }

        /// <summary>
        /// HSMS 통신 연결 시작
        /// </summary>
        public async Task<bool> ConnectAsync(string ip, int port, bool passive = false)
        {
            Communicator.Configure(ip, port, passive);
            _hsmsHandler = new MsgHandler();
            _hsmsHandler.OtherMessageReceived += OnMessageReceived;            

            try
            {
                Communicator.Connected += OnConnected;
                Communicator.Disconnected += () => Disconnected?.Invoke();

                return await Communicator.ConnectAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GEM] 연결 실패: {ex.Message}");
                return false;
            }
        }
        /// <summary>
        /// 연결 완료 시 동작 (장비 모드일 경우 S1F13 전송)
        /// </summary>
        private async void OnConnected()
        {
            Connected?.Invoke();            
            //Console.WriteLine("[GEM] Connection established. Sending S1F13...");
            //await SendMsg(1, 13);            
        }

        /// <summary>
        /// Stream / Function 기반 메시지 전송
        /// </summary>
        public async Task SendMsg(int stream, int function)
        {
            if (!IsConnected) return;

            //Message msg = Message.Build(stream, function); gsseo

            //if (msg == null)
            //{
            //    Console.WriteLine($"[GEM] Stream={stream}, Function={function} 메시지 생성 실패");
            //    return;
            //}

            //await _hsms.SendAsync(msg);
            //Console.WriteLine($"[GEM] S{stream}F{function} Sent");
        }

        /// <summary>
        /// 연결 종료
        /// </summary>
        public void Disconnect()
        {
            Communicator.Disconnect();
        }

        /// <summary>
        /// 내부 수신 처리 → 외부 이벤트로 전달
        /// </summary>
        private unsafe void OnMessageReceived(byte[] data)
        {
            string msg = Encoding.ASCII.GetString(data);
            MessageReceived?.Invoke(msg);

            long msgId = 0;
            if (_nativeCallback != null)
            {
                fixed (byte* p = data)
                {
                    _nativeCallback(p, data.Length, msgId);
                }
            }
        }

        public void AddSvid(long svid, string name, string format, string unit)
        {
            SvidTable.Add(svid, name, format, unit);
        }

        public void AddCeid(int ceid, string name)
        {
            CeidTable.Add(ceid, name);
        }

        public async void SendEventReport(int ceid)
        {
            Msg msg = S6F11.Build(ceid);
            await Communicator.SendAsync(msg);
        }
    }
}
