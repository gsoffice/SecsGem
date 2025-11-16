using SecsGemLib.Utils;
using SecsGemLib.Message.Objects;

namespace SecsGemLib.Core
{
    public class MsgHandler
    {               
        public event Action<byte[]>? OtherMessageReceived;

        public MsgHandler()
        {           
            Communicator.DataReceived += OnDataReceived;
        }

        private async void OnDataReceived(byte[] data)
        {
            // 패킷체크 => 패킷단위검사 DataLength
            // Todo 데이터 수신할때까지 버퍼에 쌓는 로직 필요
            if (!MsgPacketChecker.CheckPacket(data, out string error))
            {
                Logger.Write($"[HSMS] Invalid format: {error}");
                return;
            }

            // 메세지 디코드 => raw data -> Msg 객체로 변환이 이루어짐
            Msg msg = MsgDecoder.Decode(data);

            // 메세지 해석 (데이터 저장, 메세지를 해석해서 설명을 로그에 남기기 위해)
            if (!MsgInterpreter.Interpret(msg, out var formatError))
            {
                Logger.Write($"[SECS-II] Illegal Data: {formatError}");

                // 여기서 S9F7 만들어서 응답
                //var s9f7 = MsgBuilder.BuildS9F7(msg);  // (Header의 MHEAD 10바이트 넣는 로직은 너가 이미 알고 있을 듯)
                //await Communicator.SendAsync(s9f7);
                return;
            }

            // 데이터 저장
            MsgDataSaver.SaveData(msg);

            // logging
            Logger.Write($"[HSMS:IN] {ByteHelper.ToHex(data)}");
            Logger.Write($"[SECS-II:IN] {msg}");

            // 응답
            MsgResponser.Response(msg);            
        }
    }
}
