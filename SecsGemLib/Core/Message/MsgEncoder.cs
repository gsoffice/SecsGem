using SecsGemLib.Enums;

namespace SecsGemLib.Core
{
    public static class MsgEncoder
    {
        public static byte[] EncodeItem(MsgItem item)
        {
            if (item == null) return Array.Empty<byte>();

            if (item.Format == DataFormat.L)
            {
                var encodedChildren = item.Items.SelectMany(EncodeItem).ToArray();
                var lenBytes = EncodeLength(item.Items.Count);
                return new byte[] { (byte)((byte)item.Format | (byte)lenBytes.Length) }
                    .Concat(lenBytes).Concat(encodedChildren).ToArray();
            }
            else
            {
                int dataLen = item.Data?.Length ?? 0;
                var lenBytes = EncodeLength(dataLen);
                return new byte[] { (byte)((byte)item.Format | (byte)lenBytes.Length) }
                    .Concat(lenBytes).Concat(item.Data ?? Array.Empty<byte>()).ToArray();
            }
        }

        private static byte[] EncodeLength(int len)
        {
            if (len <= 0xFF) return new[] { (byte)len };
            if (len <= 0xFFFF) return BitConverter.GetBytes((ushort)len).Reverse().ToArray();
            return BitConverter.GetBytes(len).Reverse().Skip(1).Take(3).ToArray();
        }
    }
}
