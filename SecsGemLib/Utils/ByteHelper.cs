using System;
using System.Linq;

namespace SecsGemLib.Utils
{
    public static class ByteHelper
    {
        public static byte[] BE16(ushort v) => new[] { (byte)(v >> 8), (byte)(v & 0xFF) };
        public static byte[] BE32(uint v) => new[]
        {
            (byte)((v >> 24) & 0xFF),
            (byte)((v >> 16) & 0xFF),
            (byte)((v >> 8) & 0xFF),
            (byte)(v & 0xFF)
        };

        public static ushort ReadBE16(byte hi, byte lo) => (ushort)((hi << 8) | lo);
        public static uint ReadBE32(byte b0, byte b1, byte b2, byte b3)
            => (uint)(b0 << 24 | b1 << 16 | b2 << 8 | b3);

        public static string ToHex(byte[] data) => BitConverter.ToString(data).Replace("-", " ");
        public static byte[] Concat(params byte[][] parts) => parts.SelectMany(p => p ?? Array.Empty<byte>()).ToArray();
    }
}
