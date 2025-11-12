using SecsGemLib.Protocols.ControlMessages;
using SecsGemLib.Protocols.DataMessages;
using SecsGemLib.Utils;
using System;
using System.Threading.Tasks;

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

            // 2) Control / Data 분기
            if (MessageInspector.IsControlMsg(msg))
            {
                byte sType = MessageInspector.GetSType(msg);
                switch (sType)
                {
                    case 0x01: // Select.req
                        Logger.Write("[HSMS] Select.req → Select.rsp");
                        await _comm.SendAsync(ControlFactory.BuildSelectRsp(msg));
                        var s1f13 = StreamFactory.Build(stream: 1, function: 13);
                        await Task.Delay(200); // 살짝 텀
                        await _comm.SendAsync(s1f13);
                        SelectCompleted?.Invoke();
                        break;

                    case 0x05: // Linktest.req
                        Logger.Write("[HSMS] Linktest.req → Linktest.rsp");
                        await _comm.SendAsync(ControlFactory.BuildLinktestRsp(msg));
                        break;

                    default:
                        // 기타 Control은 필요 시 확장
                        break;
                }
            }
            else if (MessageInspector.IsDataMsg(msg))
            {
                Message response = MessageRouter.Route(msg);

                if (response is not null)
                {
                    await _comm.SendAsync(response);
                }
            }

            //OtherMessageReceived?.Invoke(data); gsseo
        }
    }
}
