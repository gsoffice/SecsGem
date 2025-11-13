using System;

namespace SecsGemLib.Core
{
    [AttributeUsage(AttributeTargets.Class)]
    public class HandlerAttribute : Attribute
    {
        public byte Stream { get; }
        public byte Function { get; }

        public HandlerAttribute(byte stream, byte function)
        {
            Stream = stream;
            Function = function;
        }
    }
}
