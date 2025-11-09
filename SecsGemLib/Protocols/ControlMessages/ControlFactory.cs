using SecsGemLib.Core;
using SecsGemLib.Utils;
using System.Linq;

namespace SecsGemLib.Protocols.ControlMessages
{
    public static class ControlFactory
    {
        // 공통: Control Header + Optional Body + Length Prefix 조합
        private static byte[] BuildControlPacket(ushort sessionId, byte sType, uint sysBytes, byte[] body = null)
        {
            // Control 메시지용 10바이트 헤더 (DeviceId=SessionId)
            var header = MessageHeader.Build(sessionId, 0, 0, false, sysBytes, sType);
            // PType=0x00 으로 표시해야 하므로 header[4] 위치를 0x00 으로 확실히 세팅
            header[4] = 0x00; // PType
            header[5] = sType;

            var bodyBytes = body ?? System.Array.Empty<byte>();
            var prefix = MessagePrefix.Build(header, bodyBytes);
            return ByteHelper.Concat(prefix, header, bodyBytes);
        }

        public static byte[] BuildSelectRsp(byte[] req, byte status = 0x00)
        {
            ushort sessionId = MessageInspector.GetSessionId(req);
            uint sysBytes = MessageInspector.GetSystemBytes(req);
            // Select.rsp 은 Status 1바이트 Body 포함
            var body = SecsEncoder.EncodeItem(SecsItem.B(status));
            return BuildControlPacket(sessionId, sType: 0x02, sysBytes, body);
        }

        public static byte[] BuildLinktestRsp(byte[] req)
        {
            ushort sessionId = MessageInspector.GetSessionId(req);
            uint sysBytes = MessageInspector.GetSystemBytes(req);
            // Linktest.rsp 는 Body 없음
            return BuildControlPacket(sessionId, sType: 0x06, sysBytes, null);
        }
    }
}
