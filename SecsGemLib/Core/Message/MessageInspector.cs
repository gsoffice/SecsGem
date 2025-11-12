namespace SecsGemLib.Core
{
    public static class MessageInspector
    {
        /// <summary>
        /// Control Message 여부 확인 (e.g. SELECT, LINKTEST 등)
        /// </summary>
        public static bool IsControlMsg(Message msg)
        {
            if (msg == null) return false;
            return msg.SType != 0x00;
        }

        /// <summary>
        /// Data Message 여부 확인 (e.g. SxFy 데이터 메시지)
        /// </summary>
        public static bool IsDataMsg(Message msg)
        {
            if (msg == null) return false;
            return msg.SType == 0x00;
        }

        /// <summary>
        /// SType 반환
        /// </summary>
        public static byte GetSType(Message msg)
        {
            return msg?.SType ?? 0x00;
        }

        /// <summary>
        /// Session ID 반환
        /// </summary>
        public static ushort GetSessionId(Message msg)
        {
            return msg?.DeviceId ?? 0;
        }

        /// <summary>
        /// System Bytes 반환
        /// </summary>
        public static uint GetSystemBytes(Message msg)
        {
            return msg?.SystemBytes ?? 0;
        }

        /// <summary>
        /// Body가 리스트(L) 타입인지 확인
        /// </summary>
        private static bool HasList(Message msg)
        {
            if (msg?.Body == null)
                return false;

            return msg.Body.Format == MessageItem.DataFormat.L;
        }

        /// <summary>
        /// Body가 리스트 타입이고, 리스트 내 아이템이 0개인지 확인
        /// </summary>
        public static bool IsEmptyList(Message msg)
        {
            if (!HasList(msg))
                return false;

            return msg.Body.Items == null || msg.Body.Items.Count == 0;
        }

        public static List<long> ExtractSvidList(Message msg)
        {
            var list = new List<long>();

            if (msg?.Body == null || msg.Body.Items.Count == 0)
                return list; // 빈 요청이면 전체 요청 의미

            foreach (var item in msg.Body.Items)
            {
                if (item.Data != null && item.Data.Length > 0)
                {
                    long val = BitConverter.ToUInt32(item.Data.Reverse().ToArray(), 0); // Big-endian
                    list.Add(val);
                }
            }

            return list;
        }
    }
}
