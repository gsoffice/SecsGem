using System.Linq;
using System.Text;

namespace SecsGemLib.Core
{
    public class Message
    {
        public ushort DeviceId { get; set; }
        public int Stream { get; set; }
        public int Function { get; set; }
        public bool WBit { get; set; }
        public uint SystemBytes { get; set; }
        public SecsItem Body { get; set; }

        private byte[] _header = System.Array.Empty<byte>();
        private byte[] _prefix = System.Array.Empty<byte>();
        private byte[] _body = System.Array.Empty<byte>();

        public static Message Build(int stream, int function, bool wbit, SecsItem body)
        {
            var msg = new Message
            {
                Stream = stream,
                Function = function,
                WBit = wbit,
                Body = body,
                SystemBytes = 1
            };

            msg._body = SecsEncoder.EncodeItem(body);
            msg._header = MessageHeader.Build(0, stream, function, wbit, msg.SystemBytes, 0x00);
            msg._prefix = MessagePrefix.Build(msg._header, msg._body);
            return msg;
        }

        public byte[] ToBytes() => _prefix.Concat(_header).Concat(_body).ToArray();

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"S{Stream}F{Function}{(WBit ? "W" : "")}");
            if (Body != null) sb.Append(Body.ToString());
            return sb.ToString();
        }
    }
}
