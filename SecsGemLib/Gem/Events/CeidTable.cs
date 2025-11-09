using System.Collections.Generic;

namespace SecsGemLib.Gem.Events
{
    public class CeidTable
    {
        private readonly Dictionary<int, string> _ceids = new();

        public void Add(int ceid, string name) => _ceids[ceid] = name;
        public string GetName(int ceid) => _ceids.TryGetValue(ceid, out var n) ? n : null;
        public IEnumerable<KeyValuePair<int, string>> GetAll() => _ceids;
    }
}
