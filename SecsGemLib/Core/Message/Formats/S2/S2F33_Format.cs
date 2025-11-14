namespace SecsGemLib.Core.Message
{
    public class S2F33_Format : MsgFormat
    {
        public DATAID DataId { get; private set; } = new();
        public List<ReportEntry> Reports { get; private set; } = new();

        public override int Stream => 2;
        public override int Function => 33;

        public class ReportEntry
        {
            public RPTID RptId { get; set; } = new();
            public List<VID> VidList { get; set; } = new();
        }

        public override void Parse(MsgItem body)
        {
            Reports.Clear();

            // ------------------------
            // 1) DATAID
            // ------------------------
            DataId.Parse(body.Items[0]);

            // ------------------------
            // 2) Report Structure List
            // ------------------------
            var rptList = body.Items[1];

            foreach (var rptStruct in rptList.Items)
            {
                var entry = new ReportEntry();

                // RPTID
                entry.RptId.Parse(rptStruct.Items[0]);

                // VID list
                foreach (var vidItem in rptStruct.Items[1].Items)
                {
                    var vid = new VID(vidItem.Format);
                    vid.Parse(vidItem);
                    entry.VidList.Add(vid);
                }

                Reports.Add(entry);
            }
        }
    }
}
