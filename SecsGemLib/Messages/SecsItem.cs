using SecsGemLib.Streams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SecsGemLib.Messages
{
    public class SecsItem
    {
        // ---------------------------------------------------
        // ENUMS
        // ---------------------------------------------------
        public enum SecsFormat : byte
        {
            L = 0x00,
            B = 0x20,
            BOOLEAN = 0x24,
            A = 0x40,
            JIS = 0x44,
            I8 = 0x60,
            I4 = 0x64,
            I2 = 0x68,
            I1 = 0x6C,
            F8 = 0x70,
            F4 = 0x74,
            U8 = 0x80,
            U4 = 0x84,
            U2 = 0x88,
            U1 = 0x8C
        }

        public int NumElements
        {
            get
            {
                if (Data == null || Data.Length == 0)
                    return 0;

                switch (Format)
                {
                    case SecsFormat.B:   // 1 byte
                    //case SecsFormat.BO:  // 1 byte (Boolean)
                    case SecsFormat.A:   // 1 byte per char
                    //case SecsFormat.J8:  // 1 byte
                        return Data.Length;

                    case SecsFormat.I2:
                    case SecsFormat.U2:
                        return Data.Length / 2;

                    case SecsFormat.I4:
                    case SecsFormat.U4:
                    case SecsFormat.F4:
                        return Data.Length / 4;

                    case SecsFormat.I8:
                    case SecsFormat.U8:
                    case SecsFormat.F8:
                        return Data.Length / 8;

                    case SecsFormat.L:
                        return Items?.Count ?? 0;

                    default:
                        return 1;
                }
            }
        }


        // ---------------------------------------------------
        // PROPERTIES
        // ---------------------------------------------------
        public SecsFormat Format { get; set; }
        public List<SecsItem> Items { get; set; } = new();
        public byte[] Data { get; set; }

        public SecsItem(SecsFormat fmt) { Format = fmt; }

        // ---------------------------------------------------
        // FACTORY METHODS
        // ---------------------------------------------------
        public static SecsItem L(params SecsItem[] list)
        {
            var item = new SecsItem(SecsFormat.L);
            item.Items.AddRange(list);
            return item;
        }

        public static SecsItem A(string str)
        {
            var item = new SecsItem(SecsFormat.A);
            item.Data = Encoding.ASCII.GetBytes(str ?? "");
            return item;
        }

        public static SecsItem B(params byte[] bytes)
        {
            var item = new SecsItem(SecsFormat.B);
            item.Data = bytes ?? Array.Empty<byte>();
            return item;
        }

        public static SecsItem U4(params uint[] values)
        {
            var item = new SecsItem(SecsFormat.U4);
            var buf = new List<byte>();
            foreach (var v in values)
                buf.AddRange(BitConverter.GetBytes(v).Reverse());
            item.Data = buf.ToArray();
            return item;
        }
    }
}
