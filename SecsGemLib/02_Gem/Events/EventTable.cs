namespace SecsGemLib.Gem.Events
{
    public static class EventTable
    {
        private static readonly Dictionary<int, List<int>> _events = new();

        public static void Add(int ceid, params int[] rptIds)
            => _events[ceid] = new List<int>(rptIds);

        public static IEnumerable<int> GetReportIds(int ceid)
            => _events.TryGetValue(ceid, out var rptList) ? rptList : new List<int>();

        public static IEnumerable<int> GetAllCeids() => _events.Keys;

        public static bool Exists(int ceid) => _events.ContainsKey(ceid);
    }
}
