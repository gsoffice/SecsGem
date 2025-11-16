using SecsGemLib.Enums;
using SecsGemLib.Message.Objects;

namespace SecsGemLib.Gem.Variables
{
    public static class SvidTable
    {
        private static readonly Dictionary<long, MsgItem> _svids = new();

        // ===============================================================
        // ADD
        // ===============================================================
        public static void Add(long svid, string name, string formatString, string unit)
        {
            if (_svids.ContainsKey(svid))
                throw new InvalidOperationException($"SVID {svid} already exists.");

            var fmt = ParseFormat(formatString);

            var item = new MsgItem(fmt)
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
        public static MsgItem? Get(long svid)
            => _svids.TryGetValue(svid, out var v) ? v : null;

        public static IEnumerable<MsgItem> GetAll()
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
        private static DataFormat ParseFormat(string fmt)
        {
            return fmt.ToUpper() switch
            {
                "A" => DataFormat.A,
                "B" => DataFormat.B,
                "BOOLEAN" => DataFormat.BOOLEAN,

                "U1" => DataFormat.U1,
                "U2" => DataFormat.U2,
                "U4" => DataFormat.U4,
                "U8" => DataFormat.U8,

                "I1" => DataFormat.I1,
                "I2" => DataFormat.I2,
                "I4" => DataFormat.I4,
                "I8" => DataFormat.I8,

                "F4" => DataFormat.F4,
                "F8" => DataFormat.F8,

                "L" => DataFormat.L,
                "OBJECT" => DataFormat.L,

                _ => DataFormat.A
            };
        }

        private static object GetDefaultValue(DataFormat fmt)
        {
            return fmt switch
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
                DataFormat.A => "",
                DataFormat.L => Array.Empty<MsgItem>(),

                _ => ""
            };
        }
    }
}
