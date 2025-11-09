namespace SecsGemLib.Gem.Data
{
    public class GemReportData
    {
        public int ReportId { get; }
        public List<object> Values { get; }

        public GemReportData(int rptId, List<object> values)
        {
            ReportId = rptId;
            Values = values;
        }
    }
}
