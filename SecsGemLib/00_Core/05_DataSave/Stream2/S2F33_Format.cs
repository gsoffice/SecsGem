using SecsGemLib.Gem.Events;
using SecsGemLib.Message.Objects;

namespace SecsGemLib.Core.Message
{
    [DataSave(2, 33)]
    public class S2F33_Format : IMsgDataSaver
    {
        public DATAID DataId { get; private set; } = new();
        public List<ReportEntry> Reports { get; private set; } = new();

        public class ReportEntry
        {
            public RPTID RptId { get; set; } = new();
            public List<VID> VidList { get; set; } = new();
        }

        public void Save(Msg msg)
        {
            Reports.Clear();

            // ------------------------
            // 1) DATAID
            // ------------------------
            DataId.Parse(msg.Body.Items[0]);

            // ------------------------
            // 2) Report Structure List
            // ------------------------
            var rptList = msg.Body.Items[1];

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

            ushort dataId = DataId.Value;

            foreach (var rpt in Reports)
            {
                long rptid = rpt.RptId.Value;
                List<int> vids = rpt.VidList
                    .Select(v => (int)v.Value)
                    .ToList();

                ReportTable.Add((int)rptid, vids.ToArray());
            }
        }
    }
}
