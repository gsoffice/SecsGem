using System.Collections.Generic;

namespace SecsGemLib.Gem.Variables
{
    public static class SvidTable
    {
        private static readonly Dictionary<int, VariableBase> _svids = new();

        public static void Add(int id, string name, object value, string desc = "")
            => _svids[id] = new VariableBase(id, name, value, desc);

        public static VariableBase Get(int id)
            => _svids.TryGetValue(id, out var v) ? v : null;

        public static IEnumerable<VariableBase> GetAll()
            => _svids.Values;

        public static void UpdateValue(int id, object newValue)
        {
            if (_svids.ContainsKey(id))
                _svids[id].Value = newValue;
        }
    }
}
