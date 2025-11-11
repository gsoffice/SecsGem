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
            // 1) 포맷 검증
            if (!MessageValidator.Validate(data, out string error))
            {
                Logger.Write($"[HSMS] Invalid format: {error}");
                return;
            }

            // 2) Control / Data 분기
            if (MessageInspector.IsControlMsg(data))
            {
                byte sType = MessageInspector.GetSType(data);
                switch (sType)
                {
                    case 0x01: // Select.req
                        Logger.Write("[HSMS] Select.req → Select.rsp");
                        await _comm.SendAsync(ControlFactory.BuildSelectRsp(data));
                        var s1f13 = StreamFactory.Build(stream: 1, function: 13).ToBytes();
                        await Task.Delay(200); // 살짝 텀
                        await _comm.SendAsync(s1f13);
                        SelectCompleted?.Invoke();
                        break;

                    case 0x05: // Linktest.req
                        Logger.Write("[HSMS] Linktest.req → Linktest.rsp");
                        await _comm.SendAsync(ControlFactory.BuildLinktestRsp(data));
                        break;

                    default:
                        // 기타 Control은 필요 시 확장
                        break;
                }
            }
            else if (MessageInspector.IsDataMsg(data))
            {
                byte[] msg = MessageRouter.Route(data)?.ToBytes();

                if (msg is not null)
                {
                    await _comm.SendAsync(msg);
                }
            }

            OtherMessageReceived?.Invoke(data);
        }
    }
}
