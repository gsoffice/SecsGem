using SecsGemLib.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecsGemLib.Protocols.DataMessages
{
    public interface IStream
    {
        int StreamNo { get; }
        Message BuildMessage(int function);
    }
}
