using SecsGemLib.Utils;

namespace SecsGemLib.Core
{
    public class MessageHandler
    {
        private readonly Communicator _comm;

        public event Action? SelectCompleted;
        public event Action<byte[]>? OtherMessageReceived;

        public MessageHandler(Communicator communicator)
        {
            _comm = communicator;
            _comm.DataReceived += OnDataReceived;
        }

        private async void OnDataReceived(byte[] data)
        {
            // 로그는 일단 무조건 찍기
            Message msg = MessageDecoder.Decode(data);
            Logger.Write($"[Comm] Rx : {ByteHelper.ToHex(data)}");
            Logger.Write($"[Comm] Rx : {msg}");

            // 1) 포맷 검증은 raw data로 하는게 맞는듯 나중에 검토 해보기
            if (!MessageValidator.Validate(data, out string error))
            {
                Logger.Write($"[HSMS] Invalid format: {error}");
                return;
            }

            Message reply;
            if (MessageInspector.IsControlMsg(msg))
            {
                reply = ControlRouter.Route(msg);

                if (reply != null)
                    await _comm.SendAsync(reply);  // SELECT.rsp 등 전송

                // ★ SELECT.req 처리 후 S1F13 바로 전송
                if (msg.SType == 0x01) // SELECT.req
                {
                    Logger.Write("[HSMS] Select completed → sending S1F13 ...");

                    await Task.Delay(200); // HSMS 권장 짧은 딜레이

                    Message s1f13 = new S1F13().Build();
                    await _comm.SendAsync(s1f13);

                    SelectCompleted?.Invoke();
                }

                return;
            }
            else if (MessageInspector.IsDataMsg(msg))
            {
                reply = MessageRouter.Route(msg);
                if (reply != null)
                {
                    await _comm.SendAsync(reply);
                }
            }
        }
    }
}
