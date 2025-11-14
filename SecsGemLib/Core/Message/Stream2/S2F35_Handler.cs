using SecsGemLib.Gem.Events;
using SecsGemLib.Enums;
using SecsGemLib.Core.Message;

namespace SecsGemLib.Core
{
    [Handler(2, 35)]
    public class S2F35_Handler : IMsgHandler
    {
        public Msg Handle(Msg msg)
        {
            var fmt = MsgParser.Parse<S2F35_Format>(msg);

            ushort dataId = fmt.DataId.Value;

            foreach (var rpt in fmt.Links)
            {
                ushort ceid = rpt.Ceid.Value;
                List<int> reports = rpt.RptId
                    .Select(v => (int)v.Value)
                    .ToList();

                EventTable.Add(ceid, reports.ToArray());
            }

            MsgItem body = MsgItem.B(0);
            return Msg.BuildSecondaryMsg(msg, body);
        }
    }
}
