using System.Collections.Generic;

namespace SecsGemLib.Gem.Events
{
    public class ReportTable
    {
        private readonly Dictionary<int, List<int>> _reports = new();

        public void Add(int rptId, params int[] svids)
            => _reports[rptId] = new List<int>(svids);

        public IEnumerable<int> GetSvids(int rptId)
            => _reports.TryGetValue(rptId, out var svids) ? svids : new List<int>();

        public IEnumerable<int> GetAllReportIds() => _reports.Keys;
    }
}
