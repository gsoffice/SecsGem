namespace SecsGemLib.Message
{
    public static class MsgHeader
    {
        // PType=0x00(Control)/0x01(Data) 는 이 배열 내 4~5 바이트 위치가 아니라,
        // 여기서는 Data 메시지 관점의 10바이트 SECS-II 헤더를 구성한다고 가정.
        // (DeviceId, Stream, Function, PType, SType, SystemBytes)
        public static byte[] Build(ushort deviceId, int stream, int function, bool wbit, uint sysBytes, int sType = 0x00)
        {
            byte streamByte = (byte)(stream | (wbit ? 0x80 : 0x00));
            byte funcByte = (byte)(function & 0x7F);

            return new byte[]
            {
                (byte)(deviceId >> 8),
                (byte)(deviceId & 0xFF),
                streamByte,
                funcByte,
                0x00,                 // PType (0x00 Control / 0x01 Data) - 여기선 Decoder/Validator에서 판단
                (byte)sType,          // SType
                (byte)((sysBytes >> 24) & 0xFF),
                (byte)((sysBytes >> 16) & 0xFF),
                (byte)((sysBytes >> 8) & 0xFF),
                (byte)(sysBytes & 0xFF)
            };
        }
    }
}
