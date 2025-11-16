namespace SecsGemLib.Core
{
    public static class FormatParser
    {
        /// <summary>
        /// EZNET 포맷 텍스트 전체를 파싱해서
        /// (S,F) → 여러 개의 overload FormatNode 들을 매핑
        /// </summary>
        public static Dictionary<(int S, int F), List<FormatNode>> Parse(string content)
        {
            var map = new Dictionary<(int S, int F), List<FormatNode>>();

            var lines = content
                .Replace("\r\n", "\n")
                .Replace('\r', '\n')
                .Split('\n')
                .Select(l => l.Trim())
                .ToList();

            int i = 0;
            while (i < lines.Count)
            {
                string line = lines[i];

                if (string.IsNullOrWhiteSpace(line))
                {
                    i++;
                    continue;
                }

                if (!line.StartsWith("S"))
                {
                    i++;
                    continue;
                }

                // S2F33W, S2F33 같은 라인 파싱
                var (s, f) = ParseStreamFunction(line);

                // 이 (S,F)에 대한 format block 라인들 수집
                i++;
                var blockLines = new List<string>();
                while (i < lines.Count && !lines[i].StartsWith("S"))
                {
                    if (!string.IsNullOrWhiteSpace(lines[i]))
                    {
                        blockLines.Add(lines[i]);
                    }
                    i++;
                }

                if (blockLines.Count == 0)
                {
                    continue;
                }

                var roots = ParseFormatBlock(blockLines);
                if (roots.Count == 0)
                {
                    continue;
                }

                if (!map.TryGetValue((s, f), out var list))
                {
                    list = new List<FormatNode>();
                    map[(s, f)] = list;
                }

                list.AddRange(roots);
            }

            return map;
        }

        private static (int S, int F) ParseStreamFunction(string line)
        {
            // 예: "S2F33W." , "S1F2" 등
            line = line.TrimEnd('.');
            int sIdx = line.IndexOf('S');
            int fIdx = line.IndexOf('F');

            if (sIdx != 0 || fIdx < 0)
                throw new FormatException($"Invalid SF line: {line}");

            int s = int.Parse(line.Substring(1, fIdx - 1));

            int fStart = fIdx + 1;
            int fEnd = fStart;
            while (fEnd < line.Length && char.IsDigit(line[fEnd]))
            {
                fEnd++;
            }

            int f = int.Parse(line.Substring(fStart, fEnd - fStart));
            return (s, f);
        }

        private static List<FormatNode> ParseFormatBlock(List<string> lines)
        {
            var roots = new List<FormatNode>();
            var stack = new Stack<FormatNode>();

            foreach (var rawLine in lines)
            {
                var line = rawLine.Trim();

                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                // 마지막에 붙은 "." 제거
                if (line.EndsWith("."))
                {
                    line = line.Substring(0, line.Length - 1).Trim();
                }

                // ">" 단독 라인: list 종료
                if (line == ">")
                {
                    if (stack.Count > 0)
                    {
                        stack.Pop();
                    }
                    continue;
                }

                if (!line.StartsWith("<"))
                {
                    continue;
                }

                // "< ... >" or "< ... " 형태에서 본문 추출
                string inner = line.Trim();

                // 앞쪽 "<" 제거
                if (inner.StartsWith("<"))
                {
                    inner = inner.Substring(1);
                }

                // ">" 이전까지만 취함
                int gt = inner.IndexOf('>');
                bool hasClosing = gt >= 0;
                if (hasClosing)
                {
                    inner = inner.Substring(0, gt);
                }

                inner = inner.Trim();
                if (string.IsNullOrEmpty(inner))
                {
                    continue;
                }

                var node = ParseNode(inner);

                if (stack.Count == 0)
                {
                    roots.Add(node);
                }
                else
                {
                    stack.Peek().Children.Add(node);
                }

                // L 타입이며 children 이 뒤에 이어질 경우 stack 에 push
                if (node.IsList && !hasClosing)
                {
                    stack.Push(node);
                }
            }

            return roots;
        }

        private static FormatNode ParseNode(string inner)
        {
            // 예:
            // "L[2]"
            // "L[n]"
            // "L"
            // "U4  SVID"
            // "A[6]  MDLN"
            // "VAR ECID"
            // "VARIANT SV"

            var node = new FormatNode();

            var parts = inner
                .Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

            string typeToken = parts[0];
            string name = (parts.Length > 1) ? parts[1] : string.Empty;

            node.Name = name;

            // 타입 토큰에서 길이 / count 추출
            int bracket = typeToken.IndexOf('[');
            if (bracket >= 0)
            {
                string baseType = typeToken.Substring(0, bracket);
                string bracketContent = typeToken.Substring(bracket + 1);
                if (bracketContent.EndsWith("]"))
                {
                    bracketContent = bracketContent.Substring(0, bracketContent.Length - 1);
                }

                node.TypeKeyword = baseType.ToUpperInvariant();

                if (string.Equals(bracketContent, "n", StringComparison.OrdinalIgnoreCase))
                {
                    node.VariableCount = true;
                }
                else if (int.TryParse(bracketContent, out int n))
                {
                    if (string.Equals(baseType, "L", StringComparison.OrdinalIgnoreCase))
                    {
                        node.FixedCount = n;
                    }
                    else
                    {
                        node.FixedLength = n;
                    }
                }
            }
            else
            {
                node.TypeKeyword = typeToken.ToUpperInvariant();
            }

            return node;
        }
    }
}
