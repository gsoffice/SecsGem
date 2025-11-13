using System.Collections.Generic;

namespace SecsGemLib.Gem.Events
{
    public static class ReportTable
    {
        private static readonly Dictionary<int, List<int>> _reports = new();

        public static void Add(int rptId, params int[] svids)
            => _reports[rptId] = new List<int>(svids);

        public static IEnumerable<int> GetSvids(int rptId)
            => _reports.TryGetValue(rptId, out var svids) ? svids : new List<int>();

        public static IEnumerable<int> GetAllReportIds() => _reports.Keys;
    }
}
