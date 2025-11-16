using SecsGemLib.Message.Objects;
namespace SecsGemLib.Core
{
    public interface IMsgHandler
    {
        Msg Handle(Msg msg);
    }
}
