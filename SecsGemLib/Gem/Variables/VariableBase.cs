using SecsGemLib.Core;

namespace SecsGemLib.Gem.Variables
{
    public class VariableBase
    {
        public long Svid { get; }
        public string Name { get; }
        public MessageItem.DataFormat Format { get; set; }  // 🔹 타입 변경
        public string Unit { get; }
        public object Data { get; set; }                     // 🔹 string → object (리스트/숫자 대응)

        public VariableBase(long svid, string name, MessageItem.DataFormat format, string unit)
        {
            Svid = svid;
            Name = name;
            Format = format;
            Unit = unit;
            Data = "";
        }
    }
}
