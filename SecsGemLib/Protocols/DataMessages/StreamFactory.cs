using SecsGemLib.Core;
using SecsGemLib.Enums;

namespace SecsGemLib.Protocols.DataMessages
{
    public static class StreamFactory
    {
        public static Message Build(byte[] data)
        {
            Message msg = SecsDecoder.Parse(data);

            if(msg.Stream == 1 && msg.Function == 13)
            {
                var s1 = new Stream1();
                return s1.BuildMessage(13);
            }

            return null;
        }

        public static Message Build(int stream, int function)
        {
            if (stream == 1 && function == 13)
            {
                var s1 = new Stream1();
                return s1.BuildMessage(13);
            }

            return null;
        }

        // 확장 포인트:
        // public static Message BuildS2F41(...) { ... }
        // public static Message BuildS6F11(...) { ... }
    }
}
