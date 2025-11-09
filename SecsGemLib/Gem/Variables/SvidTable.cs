using System.Collections.Generic;

namespace SecsGemLib.Gem.Variables
{
    public class SvidTable
    {
        private readonly Dictionary<int, VariableBase> _svids = new();

        public void Add(int id, string name, object value, string desc = "")
            => _svids[id] = new VariableBase(id, name, value, desc);

        public VariableBase Get(int id) => _svids.TryGetValue(id, out var v) ? v : null;

        public IEnumerable<VariableBase> GetAll() => _svids.Values;

        public void UpdateValue(int id, object newValue)
        {
            if (_svids.ContainsKey(id))
                _svids[id].Value = newValue;
        }
    }
}
