using System.Text;
using SecsGemLib.Enums;
using SecsGemLib.Gem.Events;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SecsGemLib.Core.Message
{
    public class CEID : MsgItem
    {
        public ushort Value { get; private set; }

        public CEID() : base(DataFormat.U4) { }

        public CEID(ushort value) : this()
        {
            SetValue(value);
        }

        public void SetValue(ushort value)
        {
            Value = value;
            Data = BitConverter.GetBytes(value).Reverse().ToArray();
        }

        public override void Parse(MsgItem item)
        {
            if (item.Format != DataFormat.U4)
                throw new Exception("CEID must be U4");

            Value = BitConverter.ToUInt16(item.Data.Reverse().ToArray(), 0);
            Data = item.Data.ToArray();
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            var v = CeidTable.GetName(Value);
            sb.AppendLine($"  /** {Value}={v} **/");            
            return sb.ToString();
        }
    }
}
