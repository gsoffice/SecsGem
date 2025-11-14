using System.Text.Json;

namespace SecsGemLib.Core
{
    public abstract class MsgFormat
    {
        // SxFy 정보
        public abstract int Stream { get; }
        public abstract int Function { get; }

        /// <summary>
        /// Body(MsgItem)만 받아서 포맷 구조로 파싱
        /// 모든 포맷 클래스(S2F33, S2F35 등)는 이걸 구현해야 함
        /// </summary>
        public abstract void Parse(MsgItem body);

        /// <summary>
        /// Msg 전체를 받아 Body만 꺼내서 Parse 호출
        /// </summary>
        public void ParseFrom(Msg msg)
        {
            if (msg.Body == null)
                throw new Exception("Message has no body.");
            Parse(msg.Body);
        }

        /// <summary>
        /// 디버깅용 JSON 출력
        /// </summary>
        public override string ToString()
        {
            return JsonSerializer.Serialize(this, new JsonSerializerOptions
            {
                WriteIndented = true
            });
        }
    }
}
