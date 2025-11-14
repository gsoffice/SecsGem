using SecsGemLib.Enums;

namespace SecsGemLib.Core.Message
{
    public class VID : MsgItem
    {
        public long Value { get; private set; }

        public VID(DataFormat fmt) : base(fmt) { }

        public VID(DataFormat fmt, long val) : this(fmt)
        {
            SetValue(val);
        }

        public void SetValue(long v)
        {
            Value = v;

            switch (Format)
            {
                case DataFormat.U2:
                    Data = BitConverter.GetBytes((ushort)v).Reverse().ToArray();
                    break;

                case DataFormat.U4:
                    Data = BitConverter.GetBytes((uint)v).Reverse().ToArray();
                    break;

                default:
                    throw new Exception("VID must be U2 or U4");
            }
        }

        public override void Parse(MsgItem item)
        {
            if (item.Format == DataFormat.U2)
                Value = BitConverter.ToUInt16(item.Data.Reverse().ToArray(), 0);
            else if (item.Format == DataFormat.U4)
                Value = BitConverter.ToUInt32(item.Data.Reverse().ToArray(), 0);
            else
                throw new Exception("VID must be U2 or U4");

            Data = item.Data.ToArray();
        }
    }
}
