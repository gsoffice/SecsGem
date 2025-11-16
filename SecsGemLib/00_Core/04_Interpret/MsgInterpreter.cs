
using SecsGemLib.Enums;
using SecsGemLib.Message.Objects;

namespace SecsGemLib.Core
{
    public static class MsgInterpreter
    {
        public static bool Interpret(Msg msg, out string formatError)
        {
            formatError = null;

            // 1) 포맷 기반 자동 검사 + Role/Description 부여
            bool ok = MsgFormatInspector.ValidateAndAnnotate(msg, out formatError);
            return ok;
        }        
    }
}
