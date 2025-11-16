using SecsGemLib.Enums;
using SecsGemLib.Message.Objects;

namespace SecsGemLib.Core.Message
{
    public class DATAID : MsgItem
    {
        public ushort Value { get; private set; }

        public DATAID() : base(DataFormat.U4) { }

        public DATAID(ushort value) : this()
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
                throw new Exception("DATAID must be U2");

            Value = BitConverter.ToUInt16(item.Data.Reverse().ToArray(), 0);
            Data = item.Data.ToArray();
        }
    }
}
