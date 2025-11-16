using SecsGemLib.Core.Message;
using SecsGemLib.Gem.Events;
using SecsGemLib.Message.Objects;

namespace SecsGemLib.Core
{
    [Handler(2, 33)]
    public class S2F33_Handler : IMsgHandler
    {
        public Msg Handle(Msg msg)
        {
            MsgItem body = MsgItem.B(0);
            return Msg.BuildSecondaryMsg(msg, body);
        }
    }
}
