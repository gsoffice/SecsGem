namespace SecsGemLib.Enums
{
    public enum DataFormat : byte
    {
        L = 0x00,
        B = 0x20,
        BOOLEAN = 0x24,
        A = 0x40,
        JIS = 0x21,

        I1 = 0x64,
        I2 = 0x68,
        I4 = 0x70,
        I8 = 0x60,

        F4 = 0x90,
        F8 = 0x80,

        U1 = 0xA4,
        U2 = 0xA8,
        U4 = 0xB0,
        U8 = 0xA0
    }
}
