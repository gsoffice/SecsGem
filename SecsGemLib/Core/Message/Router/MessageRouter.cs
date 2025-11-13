using System.Reflection;

namespace SecsGemLib.Core
{
    public static class MessageRouter
    {
        private static readonly Dictionary<(byte S, byte F), IMessageHandler> _handlers = new();

        public static void AutoRegisterHandlers()
        {
            var types = typeof(MessageRouter).Assembly.GetTypes();

            foreach (var t in types)
            {
                var attr = t.GetCustomAttribute<HandlerAttribute>();
                if (attr == null) continue;

                if (!typeof(IMessageHandler).IsAssignableFrom(t))
                    continue;

                var handler = (IMessageHandler)Activator.CreateInstance(t);
                Register(attr.Stream, attr.Function, handler);
            }
        }

        public static void Register(byte stream, byte function, IMessageHandler handler)
        {
            _handlers[(stream, function)] = handler;
        }

        public static Message Route(Message msg)
        {
            if (_handlers.TryGetValue((msg.Stream, msg.Function), out var handler))
                return handler.Handle(msg);

            // 핸들러가 없으면 null (또는 S9F1 에러 자동 리턴 가능)
            return null;
        }
    }
}
