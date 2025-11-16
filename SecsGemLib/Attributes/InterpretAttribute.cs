namespace SecsGemLib.Core
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class InterpretAttribute : Attribute
    {
        public byte Stream { get; }
        public byte Function { get; }

        public InterpretAttribute(byte stream, byte function)
        {
            Stream = stream;
            Function = function;
        }
    }
}
