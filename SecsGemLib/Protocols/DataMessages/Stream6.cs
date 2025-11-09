using System.Collections.Generic;
using SecsGemLib.Core;
using SecsGemLib.Gem.Data;

namespace SecsGemLib.Protocols.DataMessages
{
    public static class Stream6
    {
        /// <summary>S6F11 - Event Report Send</summary>
        public static Message BuildS6F11(int ceid, IEnumerable<GemReportData> reports)
        {
            var rptList = new List<SecsItem>();

            foreach (var rpt in reports)
            {
                var rptItems = new List<SecsItem> { SecsItem.U4((uint)rpt.ReportId) };
                var valItems = new List<SecsItem>();
                foreach (var v in rpt.Values)
                {
                    valItems.Add(SecsItem.A(v?.ToString() ?? ""));
                }

                rptItems.Add(SecsItem.L(valItems.ToArray()));
                rptList.Add(SecsItem.L(rptItems.ToArray()));
            }

            var body = SecsItem.L(
                SecsItem.U4((uint)ceid),
                SecsItem.L(rptList.ToArray())
            );

            return Message.Build(stream: 6, function: 11, wbit: true, body: body);
        }
    }
}
