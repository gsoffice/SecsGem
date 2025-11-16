using SecsGemLib.Message.Objects;

namespace SecsGemLib.Core
{
    public class S1F13
    {
        private readonly Msg _message;

        public S1F13()
        {
            // 여기서 S1F13 메시지를 만든다
            MsgItem body = MsgItem.L(
                MsgItem.A("VSP_88D_NEO2_V3"),  // MDLN
                MsgItem.A("3.4.2.230")         // SOFTREV
            );

            _message = Msg.BuildPrimaryMsg(
                stream: 1,
                function: 13,
                wbit: true,
                body: body
            );
        }

        public Msg Build() => _message;
    }
}
