using SecsGemLib.Core;
using SecsGemLib.Utils;

namespace SecsGemLib.Protocols.ControlMessages
{
    public static class ControlFactory
    {
        /// <summary>
        /// 공통 Control Message 생성 (HSMS Header + Body + Prefix)
        /// </summary>
        private static Message BuildControlMessage(ushort sessionId, byte sType, uint sysBytes, MessageItem body = null)
        {
            var msg = new Message
            {
                DeviceId = sessionId,
                Stream = 0,
                Function = 0,
                WBit = false,
                SType = sType,
                PType = 0x00,
                SystemBytes = sysBytes,
                Body = body
            };

            // Body
            msg._body = body != null ? MessageEncoder.EncodeItem(body) : System.Array.Empty<byte>();

            // Header (10 bytes)
            msg._header = MessageHeader.Build(sessionId, 0, 0, false, sysBytes, sType);
            msg._header[4] = 0x00; // PType
            msg._header[5] = sType;

            // Prefix (4 bytes)
            msg._prefix = MessagePrefix.Build(msg._header, msg._body);

            return msg;
        }

        /// <summary>
        /// SELECT.RSP 생성 (S-Type=0x02, 1바이트 Status 포함)
        /// </summary>
        public static Message BuildSelectRsp(Message msg, byte status = 0x00)
        {
            ushort sessionId = msg.DeviceId;
            uint sysBytes = msg.SystemBytes;

            var body = MessageItem.B(status);
            return BuildControlMessage(sessionId, sType: 0x02, sysBytes, body);
        }

        /// <summary>
        /// LINKTEST.RSP 생성 (S-Type=0x06, Body 없음)
        /// </summary>
        public static Message BuildLinktestRsp(Message msg)
        {
            ushort sessionId = msg.DeviceId;
            uint sysBytes = msg.SystemBytes;

            return BuildControlMessage(sessionId, sType: 0x06, sysBytes, null);
        }

        /// <summary>
        /// SEPARATE.RSP, REJECT.RSP 등 다른 Control 메시지 추가 시 동일 패턴 사용
        /// </summary>
    }
}
