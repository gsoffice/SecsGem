using SecsGemLib.Gem.Events;
using SecsGemLib.Gem.Variables;

namespace SecsGemLib.Core
{
    public static class S6F11
    {
        /// <summary>
        /// CEID 발생 시 Host로 전송되는 S6F11 Event Report 메시지 생성
        /// </summary>
        public static Msg Build(int ceid, uint dataId = 0)
        {
            // CEID 존재 여부 확인
            if (!EventTable.Exists(ceid))
            {
                throw new InvalidOperationException($"CEID {ceid} not defined.");
            }                

            // CEID → RPTID 목록
            var rptIds = EventTable.GetReportIds(ceid).ToList();

            // RPTID 구조들을 담을 리스트
            List<MsgItem> rptListItems = new();

            foreach (int rptId in rptIds)
            {
                // RPTID → SVID 목록
                var svidList = ReportTable.GetSvids(rptId).ToList();

                // SVID 값들 변환
                List<MsgItem> valueItems = new();

                foreach (var svid in svidList)
                {
                    var item = SvidTable.Get(svid);

                    if (item == null)
                    {
                        // 존재하지 않는 SVID → 빈 ASCII ("")
                        valueItems.Add(MsgItem.A(""));
                        continue;
                    }
                    
                    valueItems.Add(item);
                }

                // RPTID 구조 = { L 2 RPTID {L b values} }
                MsgItem rptStruct = MsgItem.L(
                    MsgItem.U2((ushort)rptId),
                    MsgItem.L(valueItems.ToArray())
                );

                rptListItems.Add(rptStruct);
            }

            // 최종 S6F11 Body = { DATAID CEID RPT-LIST }
            MsgItem body = MsgItem.L(
                MsgItem.U4(dataId),                // DATAID
                MsgItem.U2((ushort)ceid),          // CEID
                MsgItem.L(rptListItems.ToArray())  // Report list
            );

            // Primary Message이므로 Build 사용
            return Msg.BuildPrimaryMsg(stream: 6, function: 11, wbit: true, body: body);
        }
    }
}
