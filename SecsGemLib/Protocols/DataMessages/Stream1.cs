using SecsGemLib.Core;
using SecsGemLib.Enums;
using System.Text;

namespace SecsGemLib.Protocols.DataMessages
{
    /// <summary>Stream 1: Equipment Status / Comm</summary>
    public class Stream1 : IStream
    {
        public int StreamNo => 1;
        public Message BuildMessage(int function)
        {
            return function switch
            {
                3 => BuildS1F4(),
                13 => BuildS1F13(),
                _ => throw new System.NotSupportedException($"S1F{function} not supported")
            };
        }

        private Message BuildS1F4()
        {

            return Message.Build(stream: 1, function: 4, wbit: false, body: body);
        }

        private Message BuildS1F13()
        {
            var body = SecsItem.L(
                SecsItem.A("VSP_88D_NEO2_V3"),
                SecsItem.A("3.4.2.230")
            );

            return Message.Build(stream: 1, function: 13, wbit: true, body: body);
        }
    }
}
