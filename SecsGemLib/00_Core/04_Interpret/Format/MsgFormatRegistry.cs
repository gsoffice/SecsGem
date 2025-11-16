namespace SecsGemLib.Core
{
    public static class MsgFormatRegistry
    {
        private static readonly Dictionary<(int S, int F), List<FormatNode>> _formats
            = new Dictionary<(int S, int F), List<FormatNode>>();

        public static void LoadFromFile(string path)
        {
            string text = File.ReadAllText(path);
            var parsed = FormatParser.Parse(text);

            _formats.Clear();
            foreach (var kv in parsed)
            {
                _formats[kv.Key] = kv.Value;
            }
        }

        public static List<FormatNode>? Get(byte stream, byte function)
        {
            return _formats.TryGetValue((stream, function), out var list)
                ? list
                : null;
        }
    }
}
