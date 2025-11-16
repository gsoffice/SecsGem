using SecsGemLib.Enums;
using SecsGemLib.Message.Objects;

namespace SecsGemLib.Core.Message
{
    public class RPTID : MsgItem
    {
        public long Value { get; private set; }
        public RPTID(DataFormat fmt) : base(fmt) { }
        public RPTID() : base(DataFormat.U4)
        {
        }

        public RPTID(DataFormat fmt, long val) : this(fmt)
        {
            SetValue(val);
        }

        public void SetValue(long value)
        {
            Value = value;
            Data = BitConverter.GetBytes(value).Reverse().ToArray();
        }

        public override void Parse(MsgItem item)
        {
            if (item.Format != DataFormat.U4)
                throw new Exception("RPTID must be U4");

            Value = BitConverter.ToUInt16(item.Data.Reverse().ToArray(), 0);
            Data = item.Data.ToArray();
        }
    }
}
