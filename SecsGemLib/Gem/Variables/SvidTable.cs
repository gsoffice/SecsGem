using SecsGemLib.Core;
using System;
using System.Collections.Generic;

namespace SecsGemLib.Gem.Variables
{
    public static class SvidTable
    {
        private static readonly Dictionary<long, VariableBase> _svids = new();

        // 🔹 formatString을 자동 매핑하는 Add()
        public static void Add(long svid, string name, string formatString, string unit)
        {
            if (_svids.ContainsKey(svid))
                throw new InvalidOperationException($"SVID {svid} already exists.");

            var format = ParseFormat(formatString);
            _svids[svid] = new VariableBase(svid, name, format, unit);
        }

        public static VariableBase Get(long svid)
            => _svids.TryGetValue(svid, out var v) ? v : null;

        public static IEnumerable<VariableBase> GetAll()
            => _svids.Values;

        public static void UpdateValue(long svid, object newValue)
        {
            if (_svids.TryGetValue(svid, out VariableBase? value))
                value.Data = newValue;
        }

        // 🔹 문자열 포맷 → Enum 매핑
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
                _ => MessageItem.DataFormat.A // 기본값: A
            };
        }
    }
}
