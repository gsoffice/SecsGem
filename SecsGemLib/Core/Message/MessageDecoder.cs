using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SecsGemLib.Core
{
    public static class MessageDecoder
    {
        public static Message Parse(byte[] raw)
        {
            // HSMS prefix (4 bytes)
            int declaredLength = (raw[0] << 24) | (raw[1] << 16) | (raw[2] << 8) | raw[3];
            if (declaredLength != raw.Length - 4)
                throw new Exception("Invalid HSMS length prefix.");

            // header (next 10 bytes)
            var header = raw.Skip(4).Take(10).ToArray();

            ushort deviceId = (ushort)((header[0] << 8) | header[1]);
            byte streamByte = header[2];
            byte functionByte = header[3];
            bool wbit = (streamByte & 0x80) != 0;
            int stream = streamByte & 0x7F;
            int function = functionByte & 0x7F;
            uint sysBytes = (uint)(header[6] << 24 | header[7] << 16 | header[8] << 8 | header[9]);

            // body
            byte[] body = raw.Skip(14).ToArray();
            SecsItem secsBody = null;
            if (body.Length > 0)
            {
                secsBody = DecodeItem(body, out _);
            }                

            bool isPrimary = function % 2 == 1;
            bool isSecondary = !isPrimary;

            return new Message
            {
                DeviceId = deviceId,
                Stream = stream,
                Function = function,
                WBit = wbit,
                SystemBytes = sysBytes,
                Body = secsBody,
                IsPrimary = isPrimary,
                IsSecondary = isSecondary
            };
        }

        private static SecsItem DecodeItem(byte[] buffer, out int consumed)
        {
            consumed = 0;
            if (buffer.Length < 2) return null;

            byte fmtAndLen = buffer[0];
            var format = (SecsItem.SecsFormat)(fmtAndLen & 0xFC);
            int lenLen = fmtAndLen & 0x03;
            int length = 0;

            for (int i = 0; i < lenLen; i++)
                length = (length << 8) | buffer[1 + i];

            int offset = 1 + lenLen;
            consumed = offset + length;

            if (format == SecsItem.SecsFormat.L)
            {
                var list = new List<SecsItem>();
                int childOffset = offset;
                for (int i = 0; i < length; i++)
                {
                    var child = DecodeItem(buffer.Skip(childOffset).ToArray(), out int used);
                    if (child == null) break;
                    list.Add(child);
                    childOffset += used;
                }
                consumed = childOffset;
                return SecsItem.L(list.ToArray());
            }
            else
            {
                byte[] data = buffer.Skip(offset).Take(length).ToArray();
                var item = new SecsItem(format);
                item.Data = data;
                return item;
            }
        }
    }
}
