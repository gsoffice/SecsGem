using SecsGemLib.Core;
using SecsGemLib.Gem.Variables;
using System.Text;

namespace SecsGemLib
{
    /// <summary>
    /// GEM 상위 클래스 - HSMS 통신 관리 및 추후 SECS-II 메시지 처리 진입점
    /// </summary>
    public class GemApi
    {
        private Communicator _hsms;
        private MessageHandler _hsmsHandler;
        public event Action<byte[]> OnMessageReceive;

        public bool IsConnected => _hsms?.IsConnected ?? false;

        // 외부로 노출할 이벤트
        public event Action Connected;
        public event Action Disconnected;
        public event Action<string> MessageReceived;

        /// <summary>
        /// GEM 객체 생성
        /// </summary>
        public GemApi()
        {
           
        }

        /// <summary>
        /// HSMS 통신 연결 시작
        /// </summary>
        public async Task<bool> ConnectAsync(string ip, int port, bool passive = false)
        {
            _hsms = new Communicator(ip, port, passive);
            _hsmsHandler = new MessageHandler(_hsms);
            _hsmsHandler.OtherMessageReceived += OnMessageReceived;

            try
            {
                _hsms.Connected += OnConnected;
                _hsms.Disconnected += () => Disconnected?.Invoke();

                return await _hsms.ConnectAsync();
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
            _hsms?.Disconnect();
        }

        /// <summary>
        /// 내부 수신 처리 → 외부 이벤트로 전달
        /// </summary>
        private void OnMessageReceived(byte[] data)
        {
            string msg = Encoding.ASCII.GetString(data);
            MessageReceived?.Invoke(msg);
        }

        public void AddSvid(int id, string name, string type)
        {
            SvidTable.Add(id, name, type);
        }
    }
}
