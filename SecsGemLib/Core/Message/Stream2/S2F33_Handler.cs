using SecsGemLib.Gem.Variables;
using SecsGemLib.Gem.Events;

namespace SecsGemLib.Core
{
    [Handler(2, 33)]
    public class S2F33_Handler : IMessageHandler
    {
        public Message Handle(Message msg)
        {
            // Body 구조: <L [ DATAID , <L reports> ] >
            if (msg.Body == null || msg.Body.Items.Count < 2)
            {
                return BuildResponse(msg, status: 5); // Parameter Error
            }

            MessageItem dataIdItem = msg.Body.Items[0];
            MessageItem reportList = msg.Body.Items[1];

            ushort dataId = 0;
            if (dataIdItem.Format == MessageItem.DataFormat.U2)
                dataId = BitConverter.ToUInt16(dataIdItem.Data.Reverse().ToArray(), 0);

            // reports = <L a [ <L 2 [ rptid , <L b [svid]]> ] >
            foreach (var rptStruct in reportList.Items)
            {
                if (rptStruct.Items.Count < 2)
                    return BuildResponse(msg, 5);

                var rptIdItem = rptStruct.Items[0];
                var vidListItem = rptStruct.Items[1];

                // RPTID 가져오기
                if (rptIdItem.Format != MessageItem.DataFormat.U2)
                    return BuildResponse(msg, 5);

                ushort rptId = BitConverter.ToUInt16(rptIdItem.Data.Reverse().ToArray(), 0);

                // RPTID 중복 처리?
                if (ReportTable.GetSvids(rptId).Any())
                {
                    return BuildResponse(msg, 4); // Duplicate RPTID
                }

                List<int> svidList = new();

                // SVID 목록
                foreach (var svidItem in vidListItem.Items)
                {
                    if (svidItem.Format != MessageItem.DataFormat.U4 &&
                        svidItem.Format != MessageItem.DataFormat.U2)
                    {
                        return BuildResponse(msg, 5);
                    }

                    long svid = (svidItem.Format == MessageItem.DataFormat.U4)
                        ? BitConverter.ToUInt32(svidItem.Data.Reverse().ToArray(), 0)
                        : BitConverter.ToUInt16(svidItem.Data.Reverse().ToArray(), 0);

                    // SVID 존재 여부 확인
                    if (SvidTable.Get(svid) == null)
                    {
                        return BuildResponse(msg, 2); // Undefined SVID
                    }

                    svidList.Add((int)svid);
                }

                // 정상 → ReportTable에 저장
                ReportTable.Add(rptId, svidList.ToArray());
            }

            // 모두 성공 → STATUS 0
            return BuildResponse(msg, 0);
        }

        private Message BuildResponse(Message req, byte status)
        {
            MessageItem body = MessageItem.B(status);
            return Message.BuildSecondaryMsg(req, body);
        }
    }
}
