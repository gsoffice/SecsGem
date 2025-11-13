using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecsGemLib.Core
{
    using System.Collections.Generic;
    using System.Linq;

    public class ItemSchema
    {
        public MessageItem.DataFormat Format { get; set; }

        public List<ItemSchema> Children { get; set; } = new();
        public bool IsRepeat { get; set; } = false;  // 가변 반복 허용

        // ------------------------------
        // 공통 생성자
        // ------------------------------
        private static ItemSchema Create(MessageItem.DataFormat fmt, params ItemSchema[] children)
            => new ItemSchema { Format = fmt, Children = children.ToList() };

        // ------------------------------
        // 리스트 타입 (L)
        // ------------------------------
        public static ItemSchema L(params ItemSchema[] children)
            => Create(MessageItem.DataFormat.L, children);

        public ItemSchema Repeat()
        {
            IsRepeat = true;
            return this;
        }

        // ------------------------------
        // CHAR / STRING 계열
        // ------------------------------
        public static ItemSchema A() => Create(MessageItem.DataFormat.A);     // ASCII
        public static ItemSchema JIS() => Create(MessageItem.DataFormat.JIS); // JIS 8-bit
        public static ItemSchema BOOLEAN() => Create(MessageItem.DataFormat.BOOLEAN);

        // ------------------------------
        // BINARY
        // ------------------------------
        public static ItemSchema B() => Create(MessageItem.DataFormat.B);

        // ------------------------------
        // SIGNED INTEGER
        // ------------------------------
        public static ItemSchema I1() => Create(MessageItem.DataFormat.I1);
        public static ItemSchema I2() => Create(MessageItem.DataFormat.I2);
        public static ItemSchema I4() => Create(MessageItem.DataFormat.I4);
        public static ItemSchema I8() => Create(MessageItem.DataFormat.I8);

        // ------------------------------
        // UNSIGNED INTEGER
        // ------------------------------
        public static ItemSchema U1() => Create(MessageItem.DataFormat.U1);
        public static ItemSchema U2() => Create(MessageItem.DataFormat.U2);
        public static ItemSchema U4() => Create(MessageItem.DataFormat.U4);
        public static ItemSchema U8() => Create(MessageItem.DataFormat.U8);

        // ------------------------------
        // FLOATING POINT
        // ------------------------------
        public static ItemSchema F4() => Create(MessageItem.DataFormat.F4);
        public static ItemSchema F8() => Create(MessageItem.DataFormat.F8);
    }
}
