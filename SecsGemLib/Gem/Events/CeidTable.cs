using System.Collections.Generic;

namespace SecsGemLib.Gem.Events
{
    public static class CeidTable
    {
        private static readonly Dictionary<int, string> _ceids = new();

        public static void Add(int ceid, string name) => _ceids[ceid] = name;
        public static string GetName(int ceid) => _ceids.TryGetValue(ceid, out var n) ? n : null;
        public static IEnumerable<KeyValuePair<int, string>> GetAll() => _ceids;
    }
}
