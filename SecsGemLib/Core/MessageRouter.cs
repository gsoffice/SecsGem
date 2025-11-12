using SecsGemLib.Core;
using SecsGemLib.Protocols.DataMessages;
using SecsGemLib.Utils;

namespace SecsGemLib.Core
{
    public static class MessageRouter
    {
        /// <summary>
        /// 수신한 HSMS 데이터를 파싱하여 응답 메시지를 생성해야 하는 경우 생성.
        /// </summary>
        public static Message? Route(Message msg)
        {
            // Secondary나 WBit 없는 Primary는 응답 불필요
            if (!msg.IsPrimary || !msg.WBit)
            {
                Logger.Write($"[Recv] S{msg.Stream}F{msg.Function} no reply required");
                return null;
            }

            IStream stream = StreamFactory.GetStream(msg.Stream);
            if (stream == null)
            {
                Logger.Write($"[Warn] No handler for Stream {msg.Stream}");
                return null;
            }

            // 응답 메시지 생성
            Message reply = stream.BuildMessage(msg);
            return reply;
        }
    }
}
