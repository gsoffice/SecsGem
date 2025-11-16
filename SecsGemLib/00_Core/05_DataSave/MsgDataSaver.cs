using SecsGemLib.Message.Objects;

namespace SecsGemLib.Core
{
    public static class MsgDataSaver
    {
        public static void SaveData(Msg msg)
        {
            var dataSaver = MsgDataSaverRegistry.Get(msg.Stream, msg.Function);
            if (dataSaver != null)
            {
                dataSaver.Save(msg);
            }
        }
    }
}
