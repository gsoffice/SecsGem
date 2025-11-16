using System.Reflection;
using SecsGemLib.Message.Objects;

namespace SecsGemLib.Core
{
    public static class MsgRouter
    {
        private static readonly Dictionary<(byte S, byte F), IMsgHandler> _handlers = new();

        public static void AutoRegisterHandlers()
        {
            var types = typeof(MsgRouter).Assembly.GetTypes();

            foreach (var t in types)
            {
                var attr = t.GetCustomAttribute<HandlerAttribute>();

                if (attr == null)
                {
                    continue;
                }
                if (!typeof(IMsgHandler).IsAssignableFrom(t))
                {
                    continue;
                }

                var handler = (IMsgHandler)Activator.CreateInstance(t);
                Register(attr.Stream, attr.Function, handler);
            }
        }

        public static void Register(byte stream, byte function, IMsgHandler handler)
        {
            _handlers[(stream, function)] = handler;
        }

        public static Msg Route(Msg msg)
        {
            if (_handlers.TryGetValue((msg.Stream, msg.Function), out var handler))
            {
                return handler.Handle(msg);
            }                

            // 핸들러가 없으면 null (또는 S9F1 에러 자동 리턴 가능)
            return null;
        }
    }
}
