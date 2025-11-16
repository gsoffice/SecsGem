using System.Linq;

namespace SecsGemLib.Core
{
    public static class MsgPacketChecker
    {
        public static bool CheckPacket(byte[] data, out string error)
        {
            error = null;
            if (data == null) { error = "null data"; return false; }
            if (data.Length < 14) { error = "too short (<14)"; return false; }

            int declared = (data[0] << 24) | (data[1] << 16) | (data[2] << 8) | data[3];
            int actual = data.Length - 4;
            if (declared != actual)
            {
                error = $"length mismatch declared={declared}, actual={actual}";
                return false;
            }

            byte pType = data[8];
            if (pType != 0x00 && pType != 0x01)
            {
                error = $"invalid PType=0x{pType:X2}";
                return false;
            }

            if (pType == 0x00)
            {
                byte sType = data[9];
                byte[] valid = { 0, 1, 2, 3, 4, 5, 6, 7, 9 };
                if (!valid.Contains(sType))
                {
                    error = $"invalid Control SType={sType}";
                    return false;
                }
            }

            return true;
        }
    }
}
