namespace SecsGemLib.Core
{
    public static class MsgParser
    {
        public static T Parse<T>(Msg msg) where T : MsgFormat, new()
        {
            var fmt = new T();
            fmt.ParseFrom(msg);
            return fmt;
        }
    }
}
