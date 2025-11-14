using SecsGemLib.Gem.Events;

namespace SecsGemLib.Core.Message
{
    public class S2F35_Format : MsgFormat
    {
        public DATAID DataId { get; private set; } = new();
        public List<LinkEntry> Links { get; private set; } = new();

        public override int Stream => 2;
        public override int Function => 35;

        public class LinkEntry
        {
            public DATAID DataId { get; private set; } = new();
            public CEID Ceid { get; set; } = new();
            public List<RPTID> RptId { get; set; } = new();            
        }

        public override void Parse(MsgItem body)
        {
            Links.Clear();

            // ------------------------
            // 1) DATAID
            // ------------------------
            DataId.Parse(body.Items[0]);

            // ------------------------
            // 2) Event Report Pair
            // ------------------------
            var EventReportList = body.Items[1];

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
        }
    }
}
