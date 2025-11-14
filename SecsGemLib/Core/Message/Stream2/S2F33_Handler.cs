using SecsGemLib.Core.Message;
using SecsGemLib.Gem.Events;

namespace SecsGemLib.Core
{
    [Handler(2, 33)]
    public class S2F33_Handler : IMsgHandler
    {
        public Msg Handle(Msg msg)
        {
            var fmt = MsgParser.Parse<S2F33_Format>(msg);

            ushort dataId = fmt.DataId.Value;

            foreach (var rpt in fmt.Reports)
            {
                long rptid = rpt.RptId.Value;
                List<int> vids = rpt.VidList
                    .Select(v => (int)v.Value)
                    .ToList();

                ReportTable.Add((int)rptid, vids.ToArray());
            }            

            MsgItem body = MsgItem.B(0);
            return Msg.BuildSecondaryMsg(msg, body);
        }
    }
}
