using SecsGemLib.Gem.Variables;

namespace SecsGemLib.Core
{
    [Handler(1, 3)]   // S1F3 Selected
    public class S1F3_Handler : IMsgHandler
    {
        public Msg Handle(Msg msg)
        {
            // ① 요청받은 SVID 리스트 추출 (빈 리스트면 전체 SVID 응답)
            List<long> requested = MsgInspector.ExtractSvidList(msg);

            IEnumerable<MsgItem> targets =
                (requested.Count == 0)
                ? SvidTable.GetAll()
                : requested
                    .Select(id => SvidTable.Get(id))
                    .Where(item => item != null)!;

            // ② SVID 값들로 SECS-II 리스트 생성
            List<MsgItem> svItems = new();

            foreach (var sv in targets)
            {
                // RawValue → Data 재생성 (혹시 값이 변경되었을 수 있으니까)
                sv.UpdateDataFromRaw();

                // 새로운 MessageItem 인스턴스 생성
                // (기존 SvidTable 저장된 item 수정 방지)
                var clone = new MsgItem(sv.Format)
                {
                    Data = sv.Data.ToArray(),
                    Description = sv.Description,
                    Name = sv.Name,
                    Unit = sv.Unit,
                    Svid = sv.Svid,
                    RawValue = sv.RawValue
                };

                svItems.Add(clone);
            }

            // ③ 응답 바디 구성: L [ SV1 SV2 ... ]
            MsgItem body = MsgItem.L(svItems.ToArray());

            // ④ 요청 기반으로 응답 메시지 생성 (S1F4)
            return Msg.BuildSecondaryMsg(msg, body);
        }
    }
}
