using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecsGemLib.Core
{
    public static class SchemaValidator
    {
        public static bool Validate(MessageItem item, ItemSchema schema, out string error)
        {
            return Validate(item, schema, "$", out error);
        }

        private static bool Validate(MessageItem item, ItemSchema schema, string path, out string error)
        {
            error = "";

            // 1) 포맷 체크
            if (item.Format != schema.Format)
            {
                error = $"{path}: Expected {schema.Format}, but got {item.Format}";
                return false;
            }

            // 2) NON-LIST는 여기서 OK
            if (schema.Format != MessageItem.DataFormat.L)
                return true;

            var items = item.Items;
            int i = 0;  // 실제 Message item index
            int s = 0;  // schema children index

            while (s < schema.Children.Count)
            {
                var childSchema = schema.Children[s];

                if (!childSchema.IsRepeat)
                {
                    // Required element
                    if (i >= items.Count)
                    {
                        error = $"{path}[{i}]: Missing required element";
                        return false;
                    }

                    if (!Validate(items[i], childSchema, $"{path}[{i}]", out error))
                        return false;

                    i++; s++;
                }
                else
                {
                    // Repeat block
                    while (i < items.Count)
                    {
                        if (!Validate(items[i], childSchema, $"{path}[{i}]", out _))
                            break; // 반복 종료

                        i++;
                    }

                    s++; // Next schema node
                }
            }

            // 리스트가 schema보다 더 많은 요소를 가지면 error
            if (i != items.Count)
            {
                error = $"{path}: Unexpected extra items starting from index {i}";
                return false;
            }

            return true;
        }
    }
}
