using SecsGemLib.Core;

namespace SecsGemLib.Gem.Variables
{
    public static class SvidTable
    {
        private static readonly Dictionary<long, MessageItem> _svids = new();

        // ===============================================================
        // ADD
        // ===============================================================
        public static void Add(long svid, string name, string formatString, string unit)
        {
            if (_svids.ContainsKey(svid))
                throw new InvalidOperationException($"SVID {svid} already exists.");

            var fmt = ParseFormat(formatString);

            var item = new MessageItem(fmt)
            {
                Svid = svid,
                Name = name,
                Unit = unit,
                Description = name,
                RawValue = GetDefaultValue(fmt)
            };

            item.UpdateDataFromRaw();
            _svids[svid] = item;
        }

        // ===============================================================
        // GET
        // ===============================================================
        public static MessageItem? Get(long svid)
            => _svids.TryGetValue(svid, out var v) ? v : null;

        public static IEnumerable<MessageItem> GetAll()
            => _svids.Values;

        // ===============================================================
        // UPDATE VALUE
        // ===============================================================
        public static void UpdateValue(long svid, object newValue)
        {
            if (_svids.TryGetValue(svid, out var item))
            {
                item.RawValue = newValue;
                item.UpdateDataFromRaw();
            }
        }

        // ===============================================================
        // HELPERS
        // ===============================================================
        private static MessageItem.DataFormat ParseFormat(string fmt)
        {
            return fmt.ToUpper() switch
            {
                "A" => MessageItem.DataFormat.A,
                "B" => MessageItem.DataFormat.B,
                "BOOLEAN" => MessageItem.DataFormat.BOOLEAN,

                "U1" => MessageItem.DataFormat.U1,
                "U2" => MessageItem.DataFormat.U2,
                "U4" => MessageItem.DataFormat.U4,
                "U8" => MessageItem.DataFormat.U8,

                "I1" => MessageItem.DataFormat.I1,
                "I2" => MessageItem.DataFormat.I2,
                "I4" => MessageItem.DataFormat.I4,
                "I8" => MessageItem.DataFormat.I8,

                "F4" => MessageItem.DataFormat.F4,
                "F8" => MessageItem.DataFormat.F8,

                "L" => MessageItem.DataFormat.L,
                "OBJECT" => MessageItem.DataFormat.L,

                _ => MessageItem.DataFormat.A
            };
        }

        private static object GetDefaultValue(MessageItem.DataFormat fmt)
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

                MessageItem.DataFormat.BOOLEAN => false,
                MessageItem.DataFormat.A => "",
                MessageItem.DataFormat.L => Array.Empty<MessageItem>(),

                _ => ""
            };
        }
    }
}
