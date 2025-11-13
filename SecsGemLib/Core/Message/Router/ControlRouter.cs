namespace SecsGemLib.Core
{
    public static class ControlRouter
    {
        public static Message Route(Message msg)
        {
            byte sType = msg.SType;

            switch (sType)
            {
                case 0x01: // SELECT.req
                    return BuildSelectRsp(msg);

                case 0x05: // LINKTEST.req
                    return BuildLinktestRsp(msg);

                default:
                    return null; // 기타 Control은 필요 시 확장
            }
        }

        private static Message BuildSelectRsp(Message req)
        {
            var body = MessageItem.B(0x00); // Status=0: OK
            var rsp = new Message
            {
                DeviceId = req.DeviceId,
                Stream = 0,
                Function = 0,
                WBit = false,
                SType = 0x02,      // SELECT.rsp
                PType = 0x00,
                SystemBytes = req.SystemBytes,
                Body = body
            };

            return Message.BuildControl(rsp);
        }

        private static Message BuildLinktestRsp(Message req)
        {
            var rsp = new Message
            {
                DeviceId = req.DeviceId,
                Stream = 0,
                Function = 0,
                WBit = false,
                SType = 0x06,      // LINKTEST.rsp
                PType = 0x00,
                SystemBytes = req.SystemBytes,
                Body = MessageItem.L()
            };
            
            return rsp;
        }
    }
}

