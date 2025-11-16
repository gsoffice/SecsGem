using SecsGemLib.Gem.Events;
using SecsGemLib.Enums;
using SecsGemLib.Core.Message;
using SecsGemLib.Message.Objects;

namespace SecsGemLib.Core
{
    [Handler(2, 35)]
    public class S2F35_Handler : IMsgHandler
    {
        public Msg Handle(Msg msg)
        {
            MsgItem body = MsgItem.B(0);
            return Msg.BuildSecondaryMsg(msg, body);
        }
    }
}
