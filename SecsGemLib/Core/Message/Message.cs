using System.Linq;
using System.Text;

namespace SecsGemLib.Core
{
    public class Message
    {
        // -----------------------------
        // HSMS / SECS-II Header Fields
        // -----------------------------
        public ushort DeviceId { get; set; }
        public byte Stream { get; set; }
        public byte Function { get; set; }
        public bool WBit { get; set; }
        public uint SystemBytes { get; set; }
        public byte SType { get; set; }      // 0x00 = Data, others = Control
        public byte PType { get; set; } = 0; // Default HSMS (0)

        // -----------------------------
        // SECS-II Message Content
        // -----------------------------
        public MessageItem Body { get; set; }
        public bool IsPrimary => WBit;
        public bool IsSecondary => !WBit;

        public byte[] _header = System.Array.Empty<byte>();
        public byte[] _prefix = System.Array.Empty<byte>();
        public byte[] _body = System.Array.Empty<byte>();

        // -----------------------------
        // Message Builder
        // -----------------------------
        public static Message Build(int stream, int function, bool wbit, MessageItem body)
        {
            var msg = new Message
            {
                Stream = (byte)stream,
                Function = (byte)function,
                WBit = wbit,
                Body = body,
                SystemBytes = 1,
                DeviceId = 0,   // 장비 / 호스트에 맞게 설정 가능
                SType = 0x00,    // 데이터 메시지
                PType = 0x00
            };

            msg._body = MessageEncoder.EncodeItem(body);
            msg._header = MessageHeader.Build(msg.DeviceId, stream, function, wbit, msg.SystemBytes, msg.SType);
            msg._prefix = MessagePrefix.Build(msg._header, msg._body);

            return msg;
        }

        public byte[] ToBytes() => _prefix.Concat(_header).Concat(_body).ToArray();

        // -----------------------------
        // For Logging
        // -----------------------------
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"[S{Stream}F{Function}{(WBit ? "W" : "")}]");
            sb.AppendLine($"SType={SType:X2}, SystemBytes={SystemBytes}, DevideId={DeviceId}");
            if (Body != null) sb.Append(Body.ToString());
            return sb.ToString();
        }
    }
}
