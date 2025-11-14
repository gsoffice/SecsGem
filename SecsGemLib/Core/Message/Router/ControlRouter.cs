namespace SecsGemLib.Core
{
    public static class ControlRouter
    {
        public static Msg Route(Msg msg)
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

        private static Msg BuildSelectRsp(Msg req)
        {
            var body = MsgItem.B(0x00); // Status=0: OK
            var rsp = new Msg
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

            return Msg.BuildControlMsg(rsp);
        }

        private static Msg BuildLinktestRsp(Msg req)
        {
            var rsp = new Msg
            {
                DeviceId = req.DeviceId,
                Stream = 0,
                Function = 0,
                WBit = false,
                SType = 0x06,      // LINKTEST.rsp
                PType = 0x00,
                SystemBytes = req.SystemBytes,
                Body = null
            };

            return Msg.BuildControlMsg(rsp);
        }
    }
}

