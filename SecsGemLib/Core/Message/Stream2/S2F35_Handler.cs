using SecsGemLib.Gem.Events;
using SecsGemLib.Gem.Variables;

namespace SecsGemLib.Core
{
    [Handler(2, 35)]
    public class S2F35_Handler : IMessageHandler
    {
        public Message Handle(Message msg)
        {
            // Body = <L [ DATAID , <L [event links] > ] >
            if (msg.Body == null || msg.Body.Items.Count < 2)
                return BuildResponse(msg, 5); // Parameter error

            var dataIdItem = msg.Body.Items[0];
            var eventListItem = msg.Body.Items[1];

            ushort dataId = 0;
            if (dataIdItem.Format == MessageItem.DataFormat.U2)
                dataId = BitConverter.ToUInt16(dataIdItem.Data.Reverse().ToArray(), 0);

            // ------------------------------------------------
            // 반복 처리: CEID + RPTID LIST 묶음들
            // ------------------------------------------------
            foreach (var evStruct in eventListItem.Items)
            {
                // evStruct = <L [ CEID , <L [RPTID list]> ] >
                if (evStruct.Items.Count < 2)
                    return BuildResponse(msg, 5);

                var ceidItem = evStruct.Items[0];
                var rptListItem = evStruct.Items[1];

                // CEID 추출
                if (ceidItem.Format != MessageItem.DataFormat.U2 &&
                    ceidItem.Format != MessageItem.DataFormat.U4)
                    return BuildResponse(msg, 5);

                int ceid = (ceidItem.Format == MessageItem.DataFormat.U2)
                    ? BitConverter.ToUInt16(ceidItem.Data.Reverse().ToArray(), 0)
                    : (int)BitConverter.ToUInt32(ceidItem.Data.Reverse().ToArray(), 0);

                // ------------------------------------------------
                // CEID 존재 여부 확인 (장비 CEID 테이블 있으면 체크)
                // ------------------------------------------------
                // 여기서는 CEID Table이 없으니,
                // EventTable에 없으면 새로 생성해도 되지만
                // GEM 표준상 CEID는 미리 정의되어야 함.
                // 아래 코드 활성화하고 CEID Table을 생성할 수도 있음:
                //
                // if (!CeidTable.Exists(ceid))
                //     return BuildResponse(msg, 7);
                //
                // 일단은 VALID 로 간주함.

                List<int> rptList = new();

                // ------------------------------------------------
                // RPTID 리스트 파싱 및 검증
                // ------------------------------------------------
                foreach (var rptItem in rptListItem.Items)
                {
                    if (rptItem.Format != MessageItem.DataFormat.U2 &&
                        rptItem.Format != MessageItem.DataFormat.U4)
                        return BuildResponse(msg, 5);

                    int rptid = (rptItem.Format == MessageItem.DataFormat.U2)
                        ? BitConverter.ToUInt16(rptItem.Data.Reverse().ToArray(), 0)
                        : (int)BitConverter.ToUInt32(rptItem.Data.Reverse().ToArray(), 0);

                    // RPTID → SVID 리스트 체크 (S2F33에 의해 등록됨)
                    var svids = ReportTable.GetSvids(rptid);
                    if (!svids.Any())
                        return BuildResponse(msg, 8); // Undefined RPTID

                    rptList.Add(rptid);
                }

                // ------------------------------------------------
                // 최종적으로 CEID → RPTID 링크 삽입
                // ------------------------------------------------
                EventTable.Add(ceid, rptList.ToArray());
            }

            // All OK
            return BuildResponse(msg, 0);
        }

        // --------------------------------------------------------------
        // S2F36 응답 생성기
        // --------------------------------------------------------------
        private Message BuildResponse(Message req, byte status)
        {
            MessageItem body = MessageItem.B(status);
            return Message.BuildSecondaryMsg(req, body);
        }
    }
}
