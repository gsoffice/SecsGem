using SecsGemLib.Core;
using SecsGemLib.Enums;
using System.Reflection;

namespace SecsGemLib.Protocols.DataMessages
{
    public static class StreamFactory
    {
        private static readonly Dictionary<int, IStream> _streams;

        static StreamFactory()
        {
            // 리플렉션으로 IStreamHandler 구현 클래스 전부 자동 등록
            _streams = Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(t => typeof(IStream).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
                .Select(t => (IStream)Activator.CreateInstance(t))
                .ToDictionary(h => h.StreamNo, h => h);
        }

        public static IStream GetStream(int streamNo)
        {
            _streams.TryGetValue(streamNo, out var stream);
            return stream;
        }

        public static Message? Build(int stream, int function)
        {
            if (stream == 1 && function == 13)
            {
                var s1 = new Stream1();
                return s1.BuildMessage(13);
            }

            return null;
        }        
    }
}
