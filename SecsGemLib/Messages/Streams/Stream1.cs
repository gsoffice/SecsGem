using SecsGemLib.Messages;
using System;
using System.Text;

namespace SecsGemLib.Messages.Streams
{
    /// <summary>
    /// Stream 1: Equipment Status / Communication 관련 메시지 정의
    /// </summary>
    public class Stream1
    {
        /// <summary>
        /// Function 번호에 따라 메시지 포맷 생성
        /// </summary>
        public Message BuildMessage(int function)
        {
            switch (function)
            {
                case 13:
                    return BuildS1F13();
                default:
                    throw new NotSupportedException($"Stream1 Function {function}은(는) 지원되지 않습니다.");
            }
        }

        private Message BuildS1F13()
        {
            var body = SecsItem.L(
                SecsItem.A("VSP_88D_NEO2_V3"),
                SecsItem.A("3.4.2.230")
            );

            Message msg = Message.Build(
                stream: 1,
                function: 13,
                wbit: true,
                body: body
            );

            return msg;
        }
    }
}
