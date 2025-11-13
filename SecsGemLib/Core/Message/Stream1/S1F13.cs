namespace SecsGemLib.Core
{
    public class S1F13
    {
        private readonly Message _message;

        public S1F13()
        {
            // 여기서 S1F13 메시지를 만든다
            MessageItem body = MessageItem.L(
                MessageItem.A("VSP_88D_NEO2_V3"),  // MDLN
                MessageItem.A("3.4.2.230")         // SOFTREV
            );

            _message = Message.Build(
                stream: 1,
                function: 13,
                wbit: true,
                body: body
            );
        }

        public Message Build() => _message;
    }
}
