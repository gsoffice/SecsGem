namespace SecsGemLib.Core
{
    public interface IMessageHandler
    {
        Message Handle(Message msg);
    }
}
