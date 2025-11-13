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
            B = 0x10,
            BOOLEAN = 0x11,
            A = 0x20,
            JIS = 0x21,
            
            I1 = 0x31,
            I2 = 0x32,
            I4 = 0x34,
            I8 = 0x30,

            F4 = 0x44,
            F8 = 0x40,

            U1 = 0x51,
            U2 = 0xA8,
            U4 = 0xB0,
            U8 = 0x50            
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
        public object? _rawValue { get; set; }

        private object _data;

        public object RawValue
        {
            get => _rawValue;
            set => _rawValue = CoerceRawValue(value);
        }        

        private object CoerceRawValue(object raw)
        {
            if (raw == null)
                return GetDefaultForFormat();

            try
            {
                return Format switch
                {
                    DataFormat.U1 => Convert.ToByte(raw),
                    DataFormat.U2 => Convert.ToUInt16(raw),
                    DataFormat.U4 => Convert.ToUInt32(raw),
                    DataFormat.U8 => Convert.ToUInt64(raw),

                    DataFormat.I1 => Convert.ToSByte(raw),
                    DataFormat.I2 => Convert.ToInt16(raw),
                    DataFormat.I4 => Convert.ToInt32(raw),
                    DataFormat.I8 => Convert.ToInt64(raw),

                    DataFormat.F4 => Convert.ToSingle(raw),
                    DataFormat.F8 => Convert.ToDouble(raw),

                    DataFormat.BOOLEAN => Convert.ToBoolean(raw),

                    DataFormat.A => raw.ToString() ?? "",
                    DataFormat.JIS => raw.ToString() ?? "",

                    // B 타입은 byte 변환 시도
                    DataFormat.B => raw,

                    // L 타입은 MessageItem 리스트 그대로 둠
                    DataFormat.L => raw,

                    _ => raw
                };
            }
            catch
            {
                return GetDefaultForFormat();
            }
        }

        private object GetDefaultForFormat()
        {
            return Format switch
            {
                DataFormat.U1 => (byte)0,
                DataFormat.U2 => (ushort)0,
                DataFormat.U4 => (uint)0,
                DataFormat.U8 => (ulong)0,

                DataFormat.I1 => (sbyte)0,
                DataFormat.I2 => (short)0,
                DataFormat.I4 => (int)0,
                DataFormat.I8 => (long)0,

                DataFormat.F4 => 0f,
                DataFormat.F8 => 0d,

                DataFormat.BOOLEAN => false,
                DataFormat.B => Array.Empty<byte>(),

                DataFormat.A => "",
                DataFormat.JIS => "",

                DataFormat.L => new List<MessageItem>(),

                _ => ""
            };
        }


        public MessageItem(DataFormat fmt) => Format = fmt;

        // ===============================================================
        // RAW VALUE → SECS-II Data 변환
        // ===============================================================
        public void UpdateDataFromRaw()
        {
            var coerced = CoerceRawValue(RawValue);

            switch (Format)
            {
                case DataFormat.L:
                    if (coerced is IEnumerable<MessageItem> list)
                        Items = list.ToList();
                    else
                        Items = new List<MessageItem>();
                    Data = Array.Empty<byte>();
                    return;

                case DataFormat.A:
                    Data = Encoding.ASCII.GetBytes(coerced.ToString() ?? "");
                    return;

                case DataFormat.JIS:
                    Data = Encoding.GetEncoding("shift_jis").GetBytes(coerced.ToString() ?? "");
                    return;

                case DataFormat.BOOLEAN:
                    Data = new[] { (byte)((bool)coerced ? 1 : 0) };
                    return;

                case DataFormat.B:
                    if (coerced is byte[] bArr)
                        Data = bArr;
                    else if (coerced is IEnumerable<byte> bEnum)
                        Data = bEnum.ToArray();
                    else
                        Data = Array.Empty<byte>();
                    return;

                case DataFormat.U1:
                    Data = new[] { (byte)coerced };
                    return;

                case DataFormat.U2:
                    Data = BitConverter.GetBytes((ushort)coerced).Reverse().ToArray();
                    return;

                case DataFormat.U4:
                    Data = BitConverter.GetBytes((uint)coerced).Reverse().ToArray();
                    return;

                case DataFormat.U8:
                    Data = BitConverter.GetBytes((ulong)coerced).Reverse().ToArray();
                    return;

                case DataFormat.I1:
                    Data = new[] { (byte)(sbyte)coerced };
                    return;

                case DataFormat.I2:
                    Data = BitConverter.GetBytes((short)coerced).Reverse().ToArray();
                    return;

                case DataFormat.I4:
                    Data = BitConverter.GetBytes((int)coerced).Reverse().ToArray();
                    return;

                case DataFormat.I8:
                    Data = BitConverter.GetBytes((long)coerced).Reverse().ToArray();
                    return;

                case DataFormat.F4:
                    Data = BitConverter.GetBytes((float)coerced).Reverse().ToArray();
                    return;

                case DataFormat.F8:
                    Data = BitConverter.GetBytes((double)coerced).Reverse().ToArray();
                    return;
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
            return $"  /** {item.Svid} = {item.Description} **/";
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
