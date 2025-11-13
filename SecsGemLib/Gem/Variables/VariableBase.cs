using SecsGemLib.Core;

namespace SecsGemLib.Gem.Variables
{
    public class VariableBase
    {
        public long Svid { get; }
        public string Name { get; }
        public MessageItem.DataFormat Format { get; }
        public string Unit { get; }

        private object _data;

        public object Data
        {
            get => _data;
            set => _data = CoerceValue(value);
        }

        public VariableBase(long svid, string name, MessageItem.DataFormat fmt, string unit)
        {
            Svid = svid;
            Name = name;
            Format = fmt;
            Unit = unit;

            // 초기값: 포맷에 맞는 기본값
            _data = DefaultValue(fmt);
        }

        private object DefaultValue(MessageItem.DataFormat fmt)
        {
            return fmt switch
            {
                MessageItem.DataFormat.U1 => (byte)0,
                MessageItem.DataFormat.U2 => (ushort)0,
                MessageItem.DataFormat.U4 => (uint)0,
                MessageItem.DataFormat.U8 => (ulong)0,
                MessageItem.DataFormat.I1 => (sbyte)0,
                MessageItem.DataFormat.I2 => (short)0,
                MessageItem.DataFormat.I4 => (int)0,
                MessageItem.DataFormat.I8 => (long)0,
                MessageItem.DataFormat.F4 => 0f,
                MessageItem.DataFormat.F8 => 0d,
                MessageItem.DataFormat.B => (byte)0,
                MessageItem.DataFormat.BOOLEAN => false,
                MessageItem.DataFormat.A => "",
                MessageItem.DataFormat.L => new MessageItem[] { },
                _ => ""
            };
        }

        /// <summary>
        /// 업데이트 값이 들어오면 타입에 맞게 자동 변환
        /// </summary>
        private object CoerceValue(object value)
        {
            if (value == null) return DefaultValue(Format);

            try
            {
                return Format switch
                {
                    MessageItem.DataFormat.U1 => Convert.ToByte(value),
                    MessageItem.DataFormat.U2 => Convert.ToUInt16(value),
                    MessageItem.DataFormat.U4 => Convert.ToUInt32(value),
                    MessageItem.DataFormat.U8 => Convert.ToUInt64(value),

                    MessageItem.DataFormat.I1 => Convert.ToSByte(value),
                    MessageItem.DataFormat.I2 => Convert.ToInt16(value),
                    MessageItem.DataFormat.I4 => Convert.ToInt32(value),
                    MessageItem.DataFormat.I8 => Convert.ToInt64(value),

                    MessageItem.DataFormat.F4 => Convert.ToSingle(value),
                    MessageItem.DataFormat.F8 => Convert.ToDouble(value),

                    MessageItem.DataFormat.B => Convert.ToByte(value),
                    MessageItem.DataFormat.BOOLEAN => Convert.ToBoolean(value),

                    MessageItem.DataFormat.A => value.ToString(),
                    MessageItem.DataFormat.L => value, // 리스트는 별도 처리

                    _ => value
                };
            }
            catch
            {
                // 변환 실패 → 기본값
                return DefaultValue(Format);
            }
        }
    }
}
