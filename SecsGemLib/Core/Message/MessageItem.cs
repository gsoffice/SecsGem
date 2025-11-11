using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SecsGemLib.Core.Message
{
    public class MessageItem
    {
        // ---------------- ENUM ----------------
        public enum SecsFormat : byte
        {
            L = 0x00,
            B = 0x20,
            BOOLEAN = 0x24,
            A = 0x40,
            JIS = 0x44,
            I8 = 0x60,
            I4 = 0x64,
            I2 = 0x68,
            I1 = 0x6C,
            F8 = 0x70,
            F4 = 0x74,
            U8 = 0x80,
            U4 = 0x84,
            U2 = 0x88,
            U1 = 0x8C
        }

        // --------------- PROPS ----------------
        public SecsFormat Format { get; set; }
        public List<MessageItem> Items { get; set; } = new();
        public byte[] Data { get; set; }

        public int NumElements
        {
            get
            {
                if (Data == null || Data.Length == 0)
                {
                    return Format == SecsFormat.L ? Items?.Count ?? 0 : 0;
                }

                return Format switch
                {
                    SecsFormat.B or SecsFormat.A => Data.Length,
                    SecsFormat.I2 or SecsFormat.U2 => Data.Length / 2,
                    SecsFormat.I4 or SecsFormat.U4 or SecsFormat.F4 => Data.Length / 4,
                    SecsFormat.I8 or SecsFormat.U8 or SecsFormat.F8 => Data.Length / 8,
                    SecsFormat.L => Items?.Count ?? 0,
                    _ => 1
                };
            }
        }

        public MessageItem(SecsFormat fmt) { Format = fmt; }

        // ------------ FACTORIES ---------------
        public static MessageItem L(params MessageItem[] list)
        {
            var item = new MessageItem(SecsFormat.L);
            item.Items.AddRange(list);
            return item;
        }

        public static MessageItem A(string str)
        {
            var item = new MessageItem(SecsFormat.A);
            item.Data = Encoding.ASCII.GetBytes(str ?? "");
            return item;
        }

        public static MessageItem B(params byte[] bytes)
        {
            var item = new MessageItem(SecsFormat.B);
            item.Data = bytes ?? Array.Empty<byte>();
            return item;
        }

        public static MessageItem U4(params uint[] values)
        {
            var item = new MessageItem(SecsFormat.U4);
            var buf = new List<byte>();
            foreach (var v in values)
                buf.AddRange(BitConverter.GetBytes(v).Reverse());
            item.Data = buf.ToArray();
            return item;
        }

        // -------------- Pretty Print ----------
        public override string ToString()
        {
            var sb = new StringBuilder();
            FormatItem(this, 0, sb);
            return sb.ToString();
        }

        private static void FormatItem(MessageItem item, int indent, StringBuilder sb)
        {
            string pad = new string(' ', indent * 2);

            if (item.Format == SecsFormat.L)
            {
                sb.AppendLine($"{pad}<L[{item.Items.Count}]>");
                foreach (var child in item.Items)
                    FormatItem(child, indent + 1, sb);
                sb.AppendLine($"{pad}>");
                return;
            }

            if (item.Format == SecsFormat.A)
            {
                string value = Encoding.ASCII.GetString(item.Data ?? Array.Empty<byte>());
                sb.AppendLine($"{pad}<A[{item.Data?.Length}/{item.NumElements}] \"{value}\">");
                return;
            }

            sb.AppendLine($"{pad}<{item.Format}[{item.Data?.Length}/{item.NumElements}]>");
        }
    }
}
