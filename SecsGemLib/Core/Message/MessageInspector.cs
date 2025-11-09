using SecsGemLib.Utils;

namespace SecsGemLib.Core
{
    public static class MessageInspector
    {
        public static bool IsControlMsg(byte[] data)
        {
            if (data == null || data.Length < 14) return false;
            byte sType = data[9]; // length(4) + header(10) 구조 기준: index 8 = PType
            return sType != 0x00;
        }

        public static bool IsDataMsg(byte[] data)
        {
            if (data == null || data.Length < 14) return false;
            byte sType = data[9];
            return sType == 0x00;
        }

        public static byte GetSType(byte[] data)
        {
            if (data == null || data.Length < 10) return 0;
            return data[9];
        }

        public static ushort GetSessionId(byte[] data)
        {
            if (data == null || data.Length < 6) return 0;
            return ByteHelper.ReadBE16(data[4], data[5]);
        }

        public static uint GetSystemBytes(byte[] data)
        {
            if (data == null || data.Length < 14) return 0;
            return ByteHelper.ReadBE32(data[10], data[11], data[12], data[13]);
        }
    }
}
