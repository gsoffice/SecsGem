namespace SecsGemLib.Core
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class DataSaveAttribute : Attribute
    {
        public byte Stream { get; }
        public byte Function { get; }

        public DataSaveAttribute(byte stream, byte function)
        {
            Stream = stream;
            Function = function;
        }
    }
}
