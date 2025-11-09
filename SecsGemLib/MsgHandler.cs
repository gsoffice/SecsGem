using SecsGemLib.Messages;
using System;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace SecsGemLib
{
    public class MsgHandler
    {
        private readonly Communicator _comm;

        public event Action SelectCompleted;
        public event Action<byte[]> OtherMessageReceived;

        public MsgHandler(Communicator communicator)
        {
            _comm = communicator;
            _comm.DataReceived += OnDataReceived;
        }

        private async void OnDataReceived(byte[] data)
        {
            // 여기가 data 처음 넘겨 받는 부분

            // 1. 포맷 체크
                // 1.1 포맷이 정상인지 확인
                // 1.2 정상이 아니라면 그에 맞는 S9 시리즈로 응답.

            if (!Message.ValidateFormat(data, out string error))
            {
                Console.WriteLine($"잘못된 포맷: {error}");
                return;
            }

            // 2. 메세지 타입 구분
                // 1.1 컨트롤 / 데이터 메시지 구분하여 각각 응답
                // 1.2 자동응답 Stream Function 인지 구분해서 자동응답이면 여기서 응답하고 아닌경우 사용자에게 던짐

            if (Message.IsControlMessage(data))
            {
                switch (Message.GetSType(data))
                {
                    case 1: // Select.req
                        Console.WriteLine("[HSMS] Select.req received → sending Select.rsp");
                        await _comm.SendAsync(Message.BuildResponseControlMsg(data));
                        await Task.Delay(1000);
                        await _comm.SendAsync(Message.BuildAutoMessage(1, 13));
                        break;                    

                    case 5: // Linktest.req
                        Console.WriteLine("[HSMS] Linktest.req received → replying");
                        await _comm.SendAsync(Message.BuildResponseControlMsg(data));                        
                        break;

                        //default:
                        //    OtherMessageReceived?.Invoke(data);
                        //    break;
                }
            }
            else if (Message.IsDataMessage(data))
            {

            }

            OtherMessageReceived?.Invoke(data);
        }
    }
}
