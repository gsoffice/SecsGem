namespace SecsGemLib.Message
{
    public static class MsgPrefix
    {
        public static byte[] Build(byte[] header, byte[] body)
        {
            int len = (header?.Length ?? 0) + (body?.Length ?? 0);
            return BitConverter.GetBytes(len).Reverse().ToArray();
        }
    }
}
