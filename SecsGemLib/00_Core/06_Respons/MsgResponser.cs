using SecsGemLib.Message.Objects;
using SecsGemLib.Utils;

namespace SecsGemLib.Core
{
    public static class MsgResponser
    {
        public static event Action? SelectCompleted;
        public async static void Response(Msg msg)
        {
            Msg reply;
            if (MsgInspector.IsControlMsg(msg))
            {
                reply = ControlRouter.Route(msg);
                await Communicator.SendAsync(reply);  // SELECT.rsp 등 전송

                // SELECT.req 처리 후 S1F13 바로 전송
                if (msg.SType == 0x01) // SELECT.req
                {
                    Logger.Write("[HSMS] Select completed → sending S1F13 ...");

                    await Task.Delay(200); // HSMS 권장 짧은 딜레이

                    Msg s1f13 = new S1F13().Build();
                    await Communicator.SendAsync(s1f13);

                    SelectCompleted?.Invoke();
                }
            }
            else if (MsgInspector.IsDataMsg(msg))
            {
                reply = MsgRouter.Route(msg);                
                await Communicator.SendAsync(reply);                
            }
        }
    }
}
