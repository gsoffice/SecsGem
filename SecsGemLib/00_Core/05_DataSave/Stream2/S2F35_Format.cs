using SecsGemLib.Gem.Events;
using SecsGemLib.Message.Objects;
using static SecsGemLib.Core.Message.S2F33_Format;

namespace SecsGemLib.Core.Message
{
    [DataSave(2, 35)]
    public class S2F35_Format : IMsgDataSaver
    {
        public DATAID DataId { get; private set; } = new();
        public List<LinkEntry> Links { get; private set; } = new();

        public class LinkEntry
        {
            public DATAID DataId { get; private set; } = new();
            public CEID Ceid { get; set; } = new();
            public List<RPTID> RptId { get; set; } = new();            
        }       

        public void Save(Msg msg)
        {
            Links.Clear();

            // ------------------------
            // 1) DATAID
            // ------------------------
            DataId.Parse(msg.Body.Items[0]);

            // ------------------------
            // 2) Event Report Pair
            // ------------------------
            var EventReportList = msg.Body.Items[1];

            foreach (var rptStruct in EventReportList.Items)
            {
                var entry = new LinkEntry();

                // CEID
                entry.Ceid.Parse(rptStruct.Items[0]);

                // RPTID List
                foreach (var rptItem in rptStruct.Items[1].Items)
                {
                    var rpt = new RPTID(rptItem.Format);
                    rpt.Parse(rptItem);
                    entry.RptId.Add(rpt);
                }

                Links.Add(entry);
            }            

            ushort dataId = DataId.Value;

            foreach (var rpt in Links)
            {
                ushort ceid = rpt.Ceid.Value;
                List<int> reports = rpt.RptId
                    .Select(v => (int)v.Value)
                    .ToList();

                EventTable.Add(ceid, reports.ToArray());
            }
        }
    }
}
