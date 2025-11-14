using SecsGemLib.Utils;

namespace SecsGemLib.Core
{
    public class MsgHandler
    {
        private readonly Communicator _comm;

        public event Action? SelectCompleted;
        public event Action<byte[]>? OtherMessageReceived;

        public MsgHandler(Communicator communicator)
        {
            _comm = communicator;
            _comm.DataReceived += OnDataReceived;
        }

        private async void OnDataReceived(byte[] data)
        {
            // 로그는 일단 무조건 찍기
            Msg msg = MsgDecoder.Decode(data);

            // Todo            
            // 1.포맷 체크            
            if (!MsgValidator.Validate(data, out string error))
            {
                Logger.Write($"[HSMS] Invalid format: {error}");
                return;
            }

            // 2.메세지 해석 (데이터 저장, 메세지를 해석해서 설명을 로그에 남기기 위해)
            msg = MsgInterpreter.Interpret(msg);



            Logger.Write($"[HSMS:IN] {ByteHelper.ToHex(data)}");
            Logger.Write($"[SECS-II:IN] {msg}");



            Msg reply;
            if (MsgInspector.IsControlMsg(msg))
            {
                reply = ControlRouter.Route(msg);

                if (reply != null)
                    await _comm.SendAsync(reply);  // SELECT.rsp 등 전송

                // ★ SELECT.req 처리 후 S1F13 바로 전송
                if (msg.SType == 0x01) // SELECT.req
                {
                    Logger.Write("[HSMS] Select completed → sending S1F13 ...");

                    await Task.Delay(200); // HSMS 권장 짧은 딜레이

                    Msg s1f13 = new S1F13().Build();
                    await _comm.SendAsync(s1f13);

                    SelectCompleted?.Invoke();
                }

                return;
            }
            else if (MsgInspector.IsDataMsg(msg))
            {
                reply = MsgRouter.Route(msg);
                if (reply != null)
                {
                    await _comm.SendAsync(reply);
                }
            }
        }
    }
}
