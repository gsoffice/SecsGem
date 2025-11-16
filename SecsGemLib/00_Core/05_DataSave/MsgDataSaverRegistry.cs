using System.Reflection;

namespace SecsGemLib.Core
{
    public static class MsgDataSaverRegistry
    {
        private static readonly Dictionary<(byte, byte), IMsgDataSaver> _registry = new();

        public static void AutoRegister()
        {
            var types = typeof(MsgDataSaverRegistry).Assembly.GetTypes();

            foreach (var t in types)
            {
                var attr = t.GetCustomAttribute<DataSaveAttribute>();
                if (attr == null) continue;
                if (!typeof(IMsgDataSaver).IsAssignableFrom(t)) continue;

                var instance = (IMsgDataSaver)Activator.CreateInstance(t);
                _registry[(attr.Stream, attr.Function)] = instance;
            }
        }

        public static IMsgDataSaver? Get(byte stream, byte function)
        {
            _registry.TryGetValue((stream, function), out var dataSaver);
            return dataSaver;
        }
    }
}
