using System.Collections.Generic;
using SecsGemLib.Core;
using SecsGemLib.Gem.Data;

namespace SecsGemLib.Protocols.DataMessages
{
    public class Stream6 : IStream
    {
        public int StreamNo => 6;

        /// <summary>S6F11 - Event Report Send</summary>        
        public Message BuildMessage(int function)
        {
            //return function switch
            //{
            //    11 => BuildS6F11(),                
            //    _ => throw new System.NotSupportedException($"S1F{function} not supported")
            //};
            return null;
        }

        public Message BuildMessage(Message msg)
        {
            return null;
        }
    }
}
