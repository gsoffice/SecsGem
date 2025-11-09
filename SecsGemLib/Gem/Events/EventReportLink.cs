namespace SecsGemLib.Gem.Events
{
    public class EventReportLink
    {
        private readonly Dictionary<int, List<int>> _links = new();

        public void Link(int ceid, params int[] rptIds)
        {
            if (!_links.ContainsKey(ceid))
                _links[ceid] = new List<int>();
            _links[ceid].AddRange(rptIds);
        }

        public IEnumerable<int> GetLinkedReports(int ceid)
            => _links.TryGetValue(ceid, out var list) ? list.Distinct() : Enumerable.Empty<int>();
    }
}
