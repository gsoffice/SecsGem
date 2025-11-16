using System.Text;
using SecsGemLib.Enums;
using SecsGemLib.Gem.Events;
using SecsGemLib.Gem.Variables;

namespace SecsGemLib.Message.Objects
{
    public class MsgItem
    {
        // ===============================================================
        // PROPERTIES : SECS Item + (확장) SVID 메타데이터
        // ===============================================================
        public DataFormat Format { get; set; }
        public List<MsgItem> Items { get; set; } = new();
        public byte[] Data { get; set; } = Array.Empty<byte>();
        public string? Description { get; set; }

        // ---- SVID 메타데이터 ----
        public long? Svid { get; set; }
        public string? Name { get; set; }
        public string? Unit { get; set; }
        public object? _rawValue { get; set; }
        public FieldRole Role { get; set; } = FieldRole.NONE;

        private object _data;

        public MsgItem(DataFormat fmt) => Format = fmt;

        /// <summary>
        /// 기본 Parse: Raw MsgItem을 그대로 복사
        /// Strong Typed Item(DATAID 등)이 override하여 실제 값 변환 처리
        /// </summary>
        public virtual void Parse(MsgItem from)
        {
            Format = from.Format;
            Data = from.Data.ToArray();
            Items = from.Items.ToList();
        }

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
                DataFormat.I4 => 0,
                DataFormat.I8 => (long)0,

                DataFormat.F4 => 0f,
                DataFormat.F8 => 0d,

                DataFormat.BOOLEAN => false,
                DataFormat.B => Array.Empty<byte>(),

                DataFormat.A => "",
                DataFormat.JIS => "",

                DataFormat.L => new List<MsgItem>(),

                _ => ""
            };
        }

        // ===============================================================
        // RAW VALUE → SECS-II Data 변환
        // ===============================================================
        public void UpdateDataFromRaw()
        {
            var coerced = CoerceRawValue(RawValue);

            switch (Format)
            {
                case DataFormat.L:
                    if (coerced is IEnumerable<MsgItem> list)
                        Items = list.ToList();
                    else
                        Items = new List<MsgItem>();
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
        public static MsgItem L(params MsgItem[] list)
        {
            var item = new MsgItem(DataFormat.L);
            item.Items.AddRange(list);
            return item;
        }

        public static MsgItem A(string str)
        {
            var item = new MsgItem(DataFormat.A);
            item.Data = Encoding.ASCII.GetBytes(str ?? "");
            return item;
        }

        public static MsgItem JIS(string str)
        {
            var item = new MsgItem(DataFormat.JIS);
            item.Data = Encoding.GetEncoding("shift_jis").GetBytes(str ?? "");
            return item;
        }

        public static MsgItem B(params byte[] bytes)
        {
            var item = new MsgItem(DataFormat.B);
            item.Data = bytes ?? Array.Empty<byte>();
            return item;
        }

        public static MsgItem BOOLEAN(params bool[] values)
        {
            var item = new MsgItem(DataFormat.BOOLEAN);
            item.Data = values.Select(v => (byte)(v ? 1 : 0)).ToArray();
            return item;
        }

        public static MsgItem I1(params sbyte[] values)
        {
            var item = new MsgItem(DataFormat.I1);
            item.Data = values.Select(v => (byte)v).ToArray();
            return item;
        }

        public static MsgItem I2(params short[] values)
        {
            var item = new MsgItem(DataFormat.I2);
            var buf = new List<byte>();
            foreach (var v in values)
                buf.AddRange(BitConverter.GetBytes(v).Reverse()); // big-endian
            item.Data = buf.ToArray();
            return item;
        }

        public static MsgItem I4(params int[] values)
        {
            var item = new MsgItem(DataFormat.I4);
            var buf = new List<byte>();
            foreach (var v in values)
                buf.AddRange(BitConverter.GetBytes(v).Reverse());
            item.Data = buf.ToArray();
            return item;
        }

        public static MsgItem I8(params long[] values)
        {
            var item = new MsgItem(DataFormat.I8);
            var buf = new List<byte>();
            foreach (var v in values)
                buf.AddRange(BitConverter.GetBytes(v).Reverse());
            item.Data = buf.ToArray();
            return item;
        }

        public static MsgItem U1(params byte[] values)
        {
            var item = new MsgItem(DataFormat.U1);
            item.Data = values?.ToArray() ?? Array.Empty<byte>();
            return item;
        }

        public static MsgItem U2(params ushort[] values)
        {
            var item = new MsgItem(DataFormat.U2);
            var buf = new List<byte>();
            foreach (var v in values)
                buf.AddRange(BitConverter.GetBytes(v).Reverse());
            item.Data = buf.ToArray();
            return item;
        }

        public static MsgItem U4(params uint[] values)
        {
            var item = new MsgItem(DataFormat.U4);
            var buf = new List<byte>();
            foreach (var v in values)
                buf.AddRange(BitConverter.GetBytes(v).Reverse());
            item.Data = buf.ToArray();
            return item;
        }

        public static MsgItem U8(params ulong[] values)
        {
            var item = new MsgItem(DataFormat.U8);
            var buf = new List<byte>();
            foreach (var v in values)
                buf.AddRange(BitConverter.GetBytes(v).Reverse());
            item.Data = buf.ToArray();
            return item;
        }

        public static MsgItem F4(params float[] values)
        {
            var item = new MsgItem(DataFormat.F4);
            var buf = new List<byte>();
            foreach (var v in values)
                buf.AddRange(BitConverter.GetBytes(v).Reverse());
            item.Data = buf.ToArray();
            return item;
        }

        public static MsgItem F8(params double[] values)
        {
            var item = new MsgItem(DataFormat.F8);
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

        private static void FormatItem(MsgItem item, int indent, StringBuilder sb)
        {
            string pad = new string(' ', indent * 2);

            // 리스트 처리
            if (item.Format == DataFormat.L)
            {
                sb.AppendLine($"{pad}<L[{item.Items.Count}]>{FormatDesc(item)}");

                foreach (var c in item.Items)
                {
                    FormatItem(c, indent + 1, sb);
                }

                sb.AppendLine($"{pad}>");
                return;
            }

            string valueStr = ParseValueString(item);
            sb.AppendLine($"{pad}<{item.Format}[{item.Data.Length}/{item.NumElements}] {valueStr}>{FormatDesc(item)}");
        }

        private static string FormatDesc(MsgItem item)
        {
            List<string> parts = new List<string>();

            // 1) 역할 (DataId, Ceid, RptId, Svid 등)
            if (item.Role != FieldRole.NONE)
            {
                parts.Add(item.Role.ToString());
            }

            // 3) SVID: Interpret 단계에서 Role=Svid인 경우
            if (item.Role == FieldRole.SVID)
            {
                uint val = ExtractUInt(item);
                //if (val != 0)
                {
                    MsgItem svidDef = SvidTable.Get(val);
                    if (svidDef != null)
                    {
                        string s = $"{val}=";

                        if (!string.IsNullOrWhiteSpace(svidDef.Name))
                        {
                            s += $"{svidDef.Name}";
                        }

                        if (!string.IsNullOrWhiteSpace(svidDef.Unit))
                        {
                            s += $" ({svidDef.Unit})";
                        }

                        parts.Add(s);
                    }
                    else
                    {
                        svidDef = SvidTable.Get((long)item.Svid);
                        if (svidDef != null)
                        {
                            string s = $"{svidDef.Svid}=";

                            if (!string.IsNullOrWhiteSpace(svidDef.Name))
                            {
                                s += $"{svidDef.Name}";
                            }

                            if (!string.IsNullOrWhiteSpace(svidDef.Unit))
                            {
                                s += $" ({svidDef.Unit})";
                            }

                            parts.Add(s);
                        }
                    }
                }
            }

            // 4) CEID: Interpret 단계에서 Role=Ceid 인 경우
            if (item.Role == FieldRole.CEID)
            {
                uint ceid = ExtractUInt(item);
                if (ceid != 0)
                {
                    string ceName = CeidTable.GetName((int)ceid);
                    if (!string.IsNullOrWhiteSpace(ceName))
                    {
                        parts.Add($"{ceid}={ceName}");
                    }
                    else
                    {
                        parts.Add($"CEID={ceid}");
                    }
                }
            }

            // 5) 기존 Description (SVID 설명 등)
            if (!string.IsNullOrWhiteSpace(item.Description))
            {
                parts.Add(item.Description);
            }

            if (parts.Count == 0)
            {
                return string.Empty;
            }

            return "  /* " + string.Join(", ", parts) + " */";
        }

        private static uint ExtractUInt(MsgItem item)
        {
            if (item.Data == null || item.Data.Length == 0)
            {
                return 0;
            }

            // MsgItem.Data는 big-endian 저장이므로 읽을 때는 reverse
            byte[] bytes = item.Data.Reverse().ToArray();

            switch (item.Format)
            {
                case DataFormat.U1:
                    {
                        return bytes[0];
                    }
                case DataFormat.U2:
                    {
                        if (bytes.Length < 2)
                        {
                            return 0;
                        }
                        return BitConverter.ToUInt16(bytes, 0);
                    }
                case DataFormat.U4:
                    {
                        if (bytes.Length < 4)
                        {
                            return 0;
                        }
                        return BitConverter.ToUInt32(bytes, 0);
                    }
                case DataFormat.U8:
                    {
                        if (bytes.Length < 8)
                        {
                            return 0;
                        }
                        ulong v = BitConverter.ToUInt64(bytes, 0);
                        if (v > uint.MaxValue)
                        {
                            // CEID, SVID는 보통 U2/U4라서 여기까지 올 일 거의 없음
                            return 0;
                        }
                        return (uint)v;
                    }
                default:
                    {
                        return 0;
                    }
            }
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

        private static string ParseValueString(MsgItem item)
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
