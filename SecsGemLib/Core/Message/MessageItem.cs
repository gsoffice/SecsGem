using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SecsGemLib.Core
{
    public class MessageItem
    {
        // ---------------- ENUM ----------------
        public enum DataFormat : byte
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

        // --------------- PROPERTIES ----------------
        public DataFormat Format { get; set; }
        public List<MessageItem> Items { get; set; } = new();
        public byte[] Data { get; set; }

        public int NumElements
        {
            get
            {
                if (Data == null || Data.Length == 0)
                    return Format == DataFormat.L ? Items?.Count ?? 0 : 0;

                return Format switch
                {
                    DataFormat.B or DataFormat.A or DataFormat.JIS => Data.Length,
                    DataFormat.I1 or DataFormat.U1 or DataFormat.BOOLEAN => Data.Length,
                    DataFormat.I2 or DataFormat.U2 => Data.Length / 2,
                    DataFormat.I4 or DataFormat.U4 or DataFormat.F4 => Data.Length / 4,
                    DataFormat.I8 or DataFormat.U8 or DataFormat.F8 => Data.Length / 8,
                    DataFormat.L => Items?.Count ?? 0,
                    _ => 1
                };
            }
        }

        public MessageItem(DataFormat fmt) { Format = fmt; }

        // ---------------- FACTORY METHODS ----------------
        public static MessageItem L(params MessageItem[] list)
        {
            var item = new MessageItem(DataFormat.L);
            item.Items.AddRange(list);
            return item;
        }

        public static MessageItem A(string str)
        {
            var item = new MessageItem(DataFormat.A);
            item.Data = Encoding.ASCII.GetBytes(str ?? "");
            return item;
        }

        public static MessageItem JIS(string str)
        {
            var item = new MessageItem(DataFormat.JIS);
            item.Data = Encoding.GetEncoding("shift_jis").GetBytes(str ?? "");
            return item;
        }

        public static MessageItem B(params byte[] bytes)
        {
            var item = new MessageItem(DataFormat.B);
            item.Data = bytes ?? Array.Empty<byte>();
            return item;
        }

        public static MessageItem BOOLEAN(params bool[] values)
        {
            var item = new MessageItem(DataFormat.BOOLEAN);
            item.Data = values.Select(v => (byte)(v ? 1 : 0)).ToArray();
            return item;
        }

        public static MessageItem I1(params sbyte[] values)
        {
            var item = new MessageItem(DataFormat.I1);
            item.Data = values.Select(v => (byte)v).ToArray();
            return item;
        }

        public static MessageItem I2(params short[] values)
        {
            var item = new MessageItem(DataFormat.I2);
            var buf = new List<byte>();
            foreach (var v in values)
                buf.AddRange(BitConverter.GetBytes(v).Reverse());
            item.Data = buf.ToArray();
            return item;
        }

        public static MessageItem I4(params int[] values)
        {
            var item = new MessageItem(DataFormat.I4);
            var buf = new List<byte>();
            foreach (var v in values)
                buf.AddRange(BitConverter.GetBytes(v).Reverse());
            item.Data = buf.ToArray();
            return item;
        }

        public static MessageItem I8(params long[] values)
        {
            var item = new MessageItem(DataFormat.I8);
            var buf = new List<byte>();
            foreach (var v in values)
                buf.AddRange(BitConverter.GetBytes(v).Reverse());
            item.Data = buf.ToArray();
            return item;
        }

        public static MessageItem U1(params byte[] values)
        {
            var item = new MessageItem(DataFormat.U1);
            item.Data = values.ToArray();
            return item;
        }

        public static MessageItem U2(params ushort[] values)
        {
            var item = new MessageItem(DataFormat.U2);
            var buf = new List<byte>();
            foreach (var v in values)
                buf.AddRange(BitConverter.GetBytes(v).Reverse());
            item.Data = buf.ToArray();
            return item;
        }

        public static MessageItem U4(params uint[] values)
        {
            var item = new MessageItem(DataFormat.U4);
            var buf = new List<byte>();
            foreach (var v in values)
                buf.AddRange(BitConverter.GetBytes(v).Reverse());
            item.Data = buf.ToArray();
            return item;
        }

        public static MessageItem U8(params ulong[] values)
        {
            var item = new MessageItem(DataFormat.U8);
            var buf = new List<byte>();
            foreach (var v in values)
                buf.AddRange(BitConverter.GetBytes(v).Reverse());
            item.Data = buf.ToArray();
            return item;
        }

        public static MessageItem F4(params float[] values)
        {
            var item = new MessageItem(DataFormat.F4);
            var buf = new List<byte>();
            foreach (var v in values)
                buf.AddRange(BitConverter.GetBytes(v).Reverse());
            item.Data = buf.ToArray();
            return item;
        }

        public static MessageItem F8(params double[] values)
        {
            var item = new MessageItem(DataFormat.F8);
            var buf = new List<byte>();
            foreach (var v in values)
                buf.AddRange(BitConverter.GetBytes(v).Reverse());
            item.Data = buf.ToArray();
            return item;
        }

        // ---------------- PRETTY PRINT ----------------
        public override string ToString()
        {
            var sb = new StringBuilder();
            FormatItem(this, 0, sb);
            return sb.ToString();
        }

        private static void FormatItem(MessageItem item, int indent, StringBuilder sb)
        {
            string pad = new string(' ', indent * 2);

            if (item.Format == DataFormat.L)
            {
                sb.AppendLine($"{pad}<L[{item.Items.Count}]>");
                foreach (var child in item.Items)
                    FormatItem(child, indent + 1, sb);
                sb.AppendLine($"{pad}>");
                return;
            }

            if (item.Format == DataFormat.A || item.Format == DataFormat.JIS)
            {
                string value = Encoding.ASCII.GetString(item.Data ?? Array.Empty<byte>());
                sb.AppendLine($"{pad}<{item.Format}[{item.Data?.Length}/{item.NumElements}] \"{value}\">");
                return;
            }

            if (item.Data == null || item.Data.Length == 0)
            {
                sb.AppendLine($"{pad}<{item.Format}[0/0]>"); return;
            }

            string valueStr = string.Empty;

            try
            {
                switch (item.Format)
                {
                    case DataFormat.B:
                        valueStr = BitConverter.ToString(item.Data);
                        break;

                    case DataFormat.BOOLEAN:
                        valueStr = string.Join(",", item.Data.Select(b => b != 0));
                        break;

                    case DataFormat.U1:
                        valueStr = string.Join(",", item.Data.Select(b => b.ToString()));
                        break;

                    case DataFormat.U2:
                        valueStr = string.Join(",", Enumerable.Range(0, item.Data.Length / 2)
                            .Select(i => BitConverter.ToUInt16(item.Data.Skip(i * 2).Take(2).Reverse().ToArray(), 0)));
                        break;

                    case DataFormat.U4:
                        valueStr = string.Join(",", Enumerable.Range(0, item.Data.Length / 4)
                            .Select(i => BitConverter.ToUInt32(item.Data.Skip(i * 4).Take(4).Reverse().ToArray(), 0)));
                        break;

                    case DataFormat.U8:
                        valueStr = string.Join(",", Enumerable.Range(0, item.Data.Length / 8)
                            .Select(i => BitConverter.ToUInt64(item.Data.Skip(i * 8).Take(8).Reverse().ToArray(), 0)));
                        break;

                    case DataFormat.I1:
                        valueStr = string.Join(",", item.Data.Select(b => ((sbyte)b).ToString()));
                        break;

                    case DataFormat.I2:
                        valueStr = string.Join(",", Enumerable.Range(0, item.Data.Length / 2)
                            .Select(i => BitConverter.ToInt16(item.Data.Skip(i * 2).Take(2).Reverse().ToArray(), 0)));
                        break;

                    case DataFormat.I4:
                        valueStr = string.Join(",", Enumerable.Range(0, item.Data.Length / 4)
                            .Select(i => BitConverter.ToInt32(item.Data.Skip(i * 4).Take(4).Reverse().ToArray(), 0)));
                        break;

                    case DataFormat.I8:
                        valueStr = string.Join(",", Enumerable.Range(0, item.Data.Length / 8)
                            .Select(i => BitConverter.ToInt64(item.Data.Skip(i * 8).Take(8).Reverse().ToArray(), 0)));
                        break;

                    case DataFormat.F4:
                        valueStr = string.Join(",", Enumerable.Range(0, item.Data.Length / 4)
                            .Select(i => BitConverter.ToSingle(item.Data.Skip(i * 4).Take(4).Reverse().ToArray(), 0)));
                        break;

                    case DataFormat.F8:
                        valueStr = string.Join(",", Enumerable.Range(0, item.Data.Length / 8)
                            .Select(i => BitConverter.ToDouble(item.Data.Skip(i * 8).Take(8).Reverse().ToArray(), 0)));
                        break;

                    default:
                        valueStr = BitConverter.ToString(item.Data);
                        break;
                }
            }
            catch
            {
                valueStr = BitConverter.ToString(item.Data);
            }

            sb.AppendLine($"{pad}<{item.Format}[{item.Data.Length}/{item.NumElements}] {valueStr}>");
        }
    }
}
