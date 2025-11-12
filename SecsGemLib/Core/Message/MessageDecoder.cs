namespace SecsGemLib.Core
{
    public static class MessageDecoder
    {
        /// <summary>
        /// HSMS Raw Data → Message 객체로 디코드
        /// </summary>
        public static Message Decode(byte[] raw)
        {
            if (raw == null || raw.Length < 14)
                throw new ArgumentException("Invalid HSMS message: too short.");

            // ----------------------------
            // [1] Prefix (4 bytes)
            // ----------------------------
            int declaredLength = (raw[0] << 24) | (raw[1] << 16) | (raw[2] << 8) | raw[3];
            if (declaredLength != raw.Length - 4)
                throw new Exception($"Invalid HSMS length prefix: expected {declaredLength}, actual {raw.Length - 4}");

            // ----------------------------
            // [2] Header (10 bytes)
            // ----------------------------
            var header = raw.Skip(4).Take(10).ToArray();

            ushort sessionId = (ushort)((header[0] << 8) | header[1]);
            byte streamByte = header[2];
            byte functionByte = header[3];
            byte pType = header[4];
            byte sType = header[5];
            uint systemBytes = (uint)(header[6] << 24 | header[7] << 16 | header[8] << 8 | header[9]);

            bool wbit = (streamByte & 0x80) != 0;
            int stream = streamByte & 0x7F;
            int function = functionByte & 0x7F;

            // ----------------------------
            // [3] Body
            // ----------------------------
            byte[] body = raw.Skip(14).ToArray();
            MessageItem secsBody = null;

            if (body.Length > 0)
                secsBody = DecodeItem(body, out _);

            // ----------------------------
            // [4] Message 구성
            // ----------------------------
            bool isPrimary = function % 2 == 1;
            bool isSecondary = !isPrimary;

            var msg = new Message
            {
                DeviceId = sessionId,
                Stream = (byte)stream,
                Function = (byte)function,
                WBit = wbit,
                PType = pType,
                SType = sType,
                SystemBytes = systemBytes,
                Body = secsBody
            };

            // 내부 캐시용 (ToBytes 등에서 사용 가능하도록)
            msg._prefix = raw.Take(4).ToArray();
            msg._header = header;
            msg._body = body;

            return msg;
        }

        // -------------------------------------------------------------------
        // 내부: SECS-II Item 트리 디코딩
        // -------------------------------------------------------------------
        private static MessageItem DecodeItem(byte[] buffer, out int consumed)
        {
            consumed = 0;
            if (buffer == null || buffer.Length < 2) return null;

            byte fmtAndLen = buffer[0];
            var format = (MessageItem.DataFormat)(fmtAndLen & 0xFC);
            int lenLen = fmtAndLen & 0x03;

            // 길이값 읽기
            int dataLength = 0;
            for (int i = 0; i < lenLen; i++)
                dataLength = (dataLength << 8) | buffer[1 + i];

            int offset = 1 + lenLen;
            consumed = offset + dataLength;

            // ----------------------------
            // 리스트 타입
            // ----------------------------
            if (format == MessageItem.DataFormat.L)
            {
                var list = new List<MessageItem>();
                int childOffset = offset;

                for (int i = 0; i < dataLength; i++)
                {
                    if (childOffset >= buffer.Length)
                        break;

                    var child = DecodeItem(buffer.Skip(childOffset).ToArray(), out int used);
                    if (child == null) break;

                    list.Add(child);
                    childOffset += used;
                }

                consumed = childOffset;
                return MessageItem.L(list.ToArray());
            }
            // ----------------------------
            // 일반 데이터 타입
            // ----------------------------
            else
            {
                byte[] data = buffer.Skip(offset).Take(dataLength).ToArray();
                var item = new MessageItem(format);
                item.Data = data;
                return item;
            }
        }
    }
}
