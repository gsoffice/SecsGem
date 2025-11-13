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

        // ----------------------------------------------------
        // Primary 메시지 생성
        // ----------------------------------------------------
        public static Message Build(int stream, int function, bool wbit, MessageItem body)
        {
            var msg = new Message
            {
                Stream = (byte)stream,
                Function = (byte)function,
                WBit = wbit,
                Body = body,
                SystemBytes = 1,
                DeviceId = 0,
                SType = 0x00,
                PType = 0x00
            };

            msg.Encode();
            return msg;
        }

        // ----------------------------------------------------
        // Secondary(응답) 메시지 생성
        // ----------------------------------------------------
        public static Message Build(Message request, MessageItem body)
        {
            var msg = new Message
            {
                DeviceId = request.DeviceId,
                Stream = request.Stream,
                Function = (byte)(request.Function + 1), // 응답 F = 요청 F + 1
                WBit = false,
                Body = body,
                SystemBytes = request.SystemBytes,
                SType = 0x00,
                PType = 0x00
            };

            msg.Encode();
            return msg;
        }

        // ----------------------------------------------------
        // Control(응답) 메시지 생성
        // ----------------------------------------------------
        public static Message BuildControl(Message request)
        {
            var msg = new Message
            {
                DeviceId = request.DeviceId,
                Stream = 0,
                Function = 0,
                WBit = false,
                Body = request.Body,
                SystemBytes = request.SystemBytes,
                SType = request.SType,   // ★ 제일 중요
                PType = 0x00
            };

            msg.Encode();
            return msg;
        }

        // ----------------------------------------------------
        // Header + Prefix + Body encoding 통합 함수
        // ----------------------------------------------------
        private void Encode()
        {
            _body = MessageEncoder.EncodeItem(Body);
            _header = MessageHeader.Build(DeviceId, Stream, Function, WBit, SystemBytes, SType);
            _prefix = MessagePrefix.Build(_header, _body);
        }

        public byte[] ToBytes() => _prefix.Concat(_header).Concat(_body).ToArray();

        // -----------------------------
        // For Logging
        // -----------------------------
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"[S{Stream}F{Function}{(WBit ? "W" : "")}]");
            sb.AppendLine($"SType={SType:X2}, SystemBytes={SystemBytes}, DeviceId={DeviceId}");
            if (Body != null) sb.Append(Body.ToString());
            return sb.ToString();
        }
    }
}
