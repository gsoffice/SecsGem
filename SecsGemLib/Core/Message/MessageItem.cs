using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SecsGemLib.Core
{
    public class MessageItem
    {
        // ===============================================================
        // ENUM : SECS-II DataFormat
        // ===============================================================
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
            U4 = 0xB0,
            U2 = 0x88,
            U1 = 0x8C
        }

        // ===============================================================
        // PROPERTIES : SECS Item + (확장) SVID 메타데이터
        // ===============================================================
        public DataFormat Format { get; set; }
        public List<MessageItem> Items { get; set; } = new();
        public byte[] Data { get; set; } = Array.Empty<byte>();
        public string? Description { get; set; }

        // ---- SVID 메타데이터 ----
        public long? Svid { get; set; }
        public string? Name { get; set; }
        public string? Unit { get; set; }
        public object? RawValue { get; set; }

        public MessageItem(DataFormat fmt) => Format = fmt;

        // ===============================================================
        // RAW VALUE → SECS-II Data 변환
        // ===============================================================
        public void UpdateDataFromRaw()
        {
            if (RawValue == null)
            {
                Data = Array.Empty<byte>();
                return;
            }

            switch (Format)
            {
                // ---------------- LIST (L) ----------------
                case DataFormat.L:
                    if (RawValue is IEnumerable<MessageItem> list)
                        Items = list.ToList();
                    else if (RawValue is MessageItem single)
                        Items = new List<MessageItem> { single };
                    else
                        Items = new List<MessageItem>();

                    Data = Array.Empty<byte>(); // L 타입은 Data 대신 Items 사용
                    return;

                // ---------------- BYTE ARRAY (B) ----------------
                case DataFormat.B:
                    if (RawValue is byte[] bArr)
                        Data = bArr;
                    else if (RawValue is IEnumerable<byte> bEnum)
                        Data = bEnum.ToArray();
                    else
                        Data = new byte[] { Convert.ToByte(RawValue) };
                    return;

                // ---------------- JIS STRING ----------------
                case DataFormat.JIS:
                    Data = Encoding.GetEncoding("shift_jis").GetBytes(RawValue.ToString() ?? "");
                    return;

                // ---------------- ASCII STRING ----------------
                case DataFormat.A:
                    Data = Encoding.ASCII.GetBytes(RawValue.ToString() ?? "");
                    return;

                // ---------------- BOOLEAN ----------------
                case DataFormat.BOOLEAN:
                    Data = new[] { (byte)(Convert.ToBoolean(RawValue) ? 1 : 0) };
                    return;

                // ---------------- UNSIGNED ----------------
                case DataFormat.U1:
                    Data = new[] { Convert.ToByte(RawValue) };
                    return;

                case DataFormat.U2:
                    Data = BitConverter.GetBytes(Convert.ToUInt16(RawValue)).Reverse().ToArray();
                    return;

                case DataFormat.U4:
                    Data = BitConverter.GetBytes(Convert.ToUInt32(RawValue)).Reverse().ToArray();
                    return;

                case DataFormat.U8:
                    Data = BitConverter.GetBytes(Convert.ToUInt64(RawValue)).Reverse().ToArray();
                    return;

                // ---------------- SIGNED ----------------
                case DataFormat.I1:
                    Data = new[] { (byte)Convert.ToSByte(RawValue) };
                    return;

                case DataFormat.I2:
                    Data = BitConverter.GetBytes(Convert.ToInt16(RawValue)).Reverse().ToArray();
                    return;

                case DataFormat.I4:
                    Data = BitConverter.GetBytes(Convert.ToInt32(RawValue)).Reverse().ToArray();
                    return;

                case DataFormat.I8:
                    Data = BitConverter.GetBytes(Convert.ToInt64(RawValue)).Reverse().ToArray();
                    return;

                // ---------------- FLOATING ----------------
                case DataFormat.F4:
                    Data = BitConverter.GetBytes(Convert.ToSingle(RawValue)).Reverse().ToArray();
                    return;

                case DataFormat.F8:
                    Data = BitConverter.GetBytes(Convert.ToDouble(RawValue)).Reverse().ToArray();
                    return;

                // ---------------- 기타 보호 ----------------
                default:
                    throw new NotSupportedException($"Unsupported format {Format} for SVID usage.");
            }
        }


        // ===============================================================
        // FACTORY METHODS (SECS-II Item Builders)
        // ===============================================================
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
                buf.AddRange(BitConverter.GetBytes(v).Reverse()); // big-endian
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
            item.Data = values?.ToArray() ?? Array.Empty<byte>();
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

        // ===============================================================
        // PRETTY PRINT (Description 포함)
        // ===============================================================
        public override string ToString()
        {
            var sb = new StringBuilder();
            FormatItem(this, 0, sb);
            return sb.ToString();
        }

        private static void FormatItem(MessageItem item, int indent, StringBuilder sb)
        {
            string pad = new string(' ', indent * 2);

            // 리스트 처리
            if (item.Format == DataFormat.L)
            {
                sb.AppendLine($"{pad}<L[{item.Items.Count}]>{FormatDesc(item)}");
                foreach (var c in item.Items)
                    FormatItem(c, indent + 1, sb);
                sb.AppendLine($"{pad}>");
                return;
            }

            string valueStr = ParseValueString(item);
            sb.AppendLine($"{pad}<{item.Format}[{item.Data.Length}/{item.NumElements}] {valueStr}>{FormatDesc(item)}");
        }

        private static string FormatDesc(MessageItem item)
        {
            if (string.IsNullOrWhiteSpace(item.Description)) return "";
            return $"  /** {item.Description} **/";
        }

        public int NumElements
        {
            get
            {
                if (Format == DataFormat.L) return Items.Count;
                return itemLengthSwitch(Data?.Length ?? 0);
            }
        }

        private int itemLengthSwitch(int len) =>
            Format switch
            {
                DataFormat.U1 or DataFormat.I1 or DataFormat.BOOLEAN => len,
                DataFormat.U2 or DataFormat.I2 => len / 2,
                DataFormat.U4 or DataFormat.I4 or DataFormat.F4 => len / 4,
                DataFormat.U8 or DataFormat.I8 or DataFormat.F8 => len / 8,
                DataFormat.A or DataFormat.JIS or DataFormat.B => len,
                _ => 1
            };

        private static string ParseValueString(MessageItem item)
        {
            if (item.Data == null || item.Data.Length == 0)
                return "";

            try
            {
                return item.Format switch
                {
                    DataFormat.U1 => item.Data[0].ToString(),
                    DataFormat.U2 => BitConverter.ToUInt16(item.Data.Reverse().ToArray(), 0).ToString(),
                    DataFormat.U4 => BitConverter.ToUInt32(item.Data.Reverse().ToArray(), 0).ToString(),

                    DataFormat.A => Encoding.ASCII.GetString(item.Data),

                    _ => BitConverter.ToString(item.Data)
                };
            }
            catch
            {
                return BitConverter.ToString(item.Data);
            }
        }
    }
}
