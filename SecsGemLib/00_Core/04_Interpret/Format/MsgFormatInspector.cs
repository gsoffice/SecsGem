using SecsGemLib.Enums;
using SecsGemLib.Gem.Events;
using SecsGemLib.Gem.Variables;
using SecsGemLib.Message.Objects;

namespace SecsGemLib.Core
{
    public static class MsgFormatInspector
    {
        /// <summary>
        /// (S,F)에 해당하는 포맷이 있을 경우,
        ///  1) 타입/길이/구조 검사
        ///  2) MsgItem.Role / Description 자동 세팅
        /// 하나라도 통과하면 true, 모두 실패하면 false + error 메세지
        /// </summary>
        public static bool ValidateAndAnnotate(Msg msg, out string error)
        {
            error = null;

            var candidates = MsgFormatRegistry.Get(msg.Stream, msg.Function);
            if (candidates == null || candidates.Count == 0)
            {
                // 포맷 정의가 없으면 검사하지 않음
                return true;
            }

            // 조금 더 복잡한(노드 많은) 포맷을 우선 검사하도록 정렬
            var ordered = candidates
                .OrderByDescending(fmt => CountNodes(fmt))
                .ToList();

            string lastError = null;

            foreach (var fmtRoot in ordered)
            {
                // 원본을 건드리지 않기 위해 clone 위에서 검사
                var cloneBody = CloneItem(msg.Body);
                var path = new List<int>();

                if (ValidateItem(cloneBody, fmtRoot, path, out var err))
                {
                    // 성공한 경우 clone 에서 설정된 메타를 원본에 반영
                    CopyMeta(cloneBody, msg.Body);
                    error = null;
                    return true;
                }

                lastError = err;
            }

            // 모든 오버로드 실패 = Illegal Data (S9F7 대상)
            error = lastError;
            return false;
        }

        // --------------------------------------------------------------------
        // MsgItem 트리와 FormatNode 트리를 비교
        // --------------------------------------------------------------------
        private static bool ValidateItem(MsgItem item, FormatNode fmt, List<int> path, out string error)
        {
            error = null;

            // 1) 타입 검사 (VAR, VARIANT 는 타입 자유)
            if (!fmt.IsVariantType)
            {
                if (!IsTypeCompatible(item.Format, fmt.TypeKeyword))
                {
                    error = $"Type mismatch at path [{string.Join(",", path)}]: " +
                            $"expected {fmt.TypeKeyword}, actual {item.Format}";
                    return false;
                }
            }

            // 2) 길이 검사 (A[n], B[n] 등 고정 길이 지정된 경우만)
            if (fmt.FixedLength.HasValue)
            {
                if (item.Data == null || item.Data.Length != fmt.FixedLength.Value)
                {
                    error = $"Length mismatch at path [{string.Join(",", path)}] " +
                            $"for {fmt.Name}: expected {fmt.FixedLength}, actual {item.Data?.Length ?? 0}";
                    return false;
                }
            }

            // 3) 리스트 검사
            if (fmt.IsList)
            {
                int actualCount = item.Items?.Count ?? 0;

                // 3-1) 명시적 고정 개수 L[k]
                if (fmt.FixedCount.HasValue && !fmt.VariableCount)
                {
                    if (actualCount != fmt.FixedCount.Value)
                    {
                        error = $"List count mismatch at path [{string.Join(",", path)}] " +
                                $"expected {fmt.FixedCount}, actual {actualCount}";
                        return false;
                    }
                }

                // 3-2) 반복 리스트 L[n]
                if (fmt.VariableCount)
                {
                    // L[n]이면 최소 1개 이상은 있어야 정상이라고 간주
                    if (actualCount == 0)
                    {
                        error = $"List expected at least 1 element (L[n]) at path [{string.Join(",", path)}]";
                        return false;
                    }

                    // 보통 L[n] 아래에 자식 포맷이 1개 있고 그게 반복
                    if (fmt.Children.Count == 1)
                    {
                        for (int i = 0; i < actualCount; i++)
                        {
                            path.Add(i);
                            if (!ValidateItem(item.Items[i], fmt.Children[0], path, out error))
                            {
                                path.RemoveAt(path.Count - 1);
                                return false;
                            }
                            path.RemoveAt(path.Count - 1);
                        }

                        // 리스트 반복 구조 검증 끝
                        ApplyMeta(item, fmt);
                        return true;
                    }
                    // 혹시 Children 여러 개인 특수 케이스면 여기로 오는데
                    // 일단 개수만 맞는지 보고 순서대로 비교
                }

                // 3-3) FixedCount/VariableCount 둘 다 없고, Children도 없는 "L" 단독인 경우
                //      => "빈 리스트" 로 간주 (S2F33 첫 번째 포맷의 L 처럼)
                if (!fmt.FixedCount.HasValue && !fmt.VariableCount && fmt.Children.Count == 0)
                {
                    if (actualCount != 0)
                    {
                        error = $"List expected EMPTY at path [{string.Join(",", path)}] but got {actualCount}";
                        return false;
                    }

                    ApplyMeta(item, fmt);
                    return true;
                }

                // 3-4) 자식 리스트 구조 비교
                if (fmt.Children.Count > 0)
                {
                    if (actualCount != fmt.Children.Count && !fmt.VariableCount)
                    {
                        error = $"List child count mismatch at path [{string.Join(",", path)}] " +
                                $"expected {fmt.Children.Count}, actual {actualCount}";
                        return false;
                    }

                    int loopCount = Math.Min(actualCount, fmt.Children.Count);

                    for (int i = 0; i < loopCount; i++)
                    {
                        path.Add(i);
                        if (!ValidateItem(item.Items[i], fmt.Children[i], path, out error))
                        {
                            path.RemoveAt(path.Count - 1);
                            return false;
                        }
                        path.RemoveAt(path.Count - 1);
                    }

                    // 만약 msg.Items 가 더 많으면 그것도 에러
                    if (!fmt.VariableCount && actualCount > fmt.Children.Count)
                    {
                        error = $"Unexpected extra list element at path [{string.Join(",", path)}], count={actualCount}";
                        return false;
                    }
                }
            }

            // 4) 리스트가 아니면 하위 아이템은 없음 (primitive 타입)
            //    여기까지 왔으면 타입/길이/리스트 구조 OK

            // 5) Role / Description 부여 (CEID, SVID 등)
            ApplyMeta(item, fmt);
            return true;
        }

        // --------------------------------------------------------------------
        // 타입 매칭
        // --------------------------------------------------------------------
        private static bool IsTypeCompatible(DataFormat actual, string typeKeyword)
        {
            typeKeyword = typeKeyword.ToUpperInvariant();

            switch (typeKeyword)
            {
                case "L": return actual == DataFormat.L;
                case "A": return actual == DataFormat.A;
                case "B": return actual == DataFormat.B;
                case "BOOLEAN": return actual == DataFormat.BOOLEAN;
                case "U1": return actual == DataFormat.U1;
                case "U2": return actual == DataFormat.U2;
                case "U4": return actual == DataFormat.U4;
                case "U8": return actual == DataFormat.U8;
                case "I1": return actual == DataFormat.I1;
                case "I2": return actual == DataFormat.I2;
                case "I4": return actual == DataFormat.I4;
                case "I8": return actual == DataFormat.I8;
                case "F4": return actual == DataFormat.F4;
                case "F8": return actual == DataFormat.F8;
                // VAR / VARIANT 등은 IsVariantType 에서 이미 허용
                case "VAR":
                case "VARIANT":
                    return true;
                default:
                    // 모르는 타입 키워드는 일단 true (느슨하게)
                    return true;
            }
        }

        // --------------------------------------------------------------------
        // CEID/SVID 등 메타 부여
        // --------------------------------------------------------------------
        private static void ApplyMeta(MsgItem item, FormatNode fmt)
        {
            if (string.IsNullOrWhiteSpace(fmt.Name))
                return;

            string label = fmt.Name.ToUpperInvariant();

            // 기본 포맷 이름 (DATAID, CEID, SVID 등)
            item.Description = fmt.Name;

            // 1) Role 설정
            if (Enum.TryParse<FieldRole>(label, ignoreCase: true, out var parsed))
            {
                item.Role = parsed;
            }
            else
            {
                switch (label)
                {
                    case "VID":
                    case "SVID":
                        item.Role = FieldRole.SVID;
                        break;

                    case "CEID":
                        item.Role = FieldRole.CEID;
                        break;

                    case "RPTID":
                        item.Role = FieldRole.RPTID;
                        break;

                    case "DATAID":
                    case "DATID":
                        item.Role = FieldRole.DATAID;
                        break;
                }
            }

            // 2) 실제 값 읽기
            long num = ExtractUInt(item);

            // --------------------------------------------------------------------
            // 3) 번호=이름 포맷 생성
            // --------------------------------------------------------------------
            switch (item.Role)
            {
                case FieldRole.VID:
                case FieldRole.SVID:
                    {
                        long? svid = num;
                        if(svid == 0)
                        {
                            svid = item.Svid;
                        }
                        var def = SvidTable.Get((long)svid);
                        if (def != null)
                        {
                            item.Description = $"{svid}={def.Name}";
                            item.Svid = svid;
                            item.Name = def.Name;
                            item.Unit = def.Unit;
                        }
                        else
                        {
                            item.Description = $"{num}";
                        }
                        break;
                    }

                case FieldRole.CEID:
                    {
                        string ceName = CeidTable.GetName((int)num);
                        if (!string.IsNullOrWhiteSpace(ceName))
                            item.Description = $"{num}={ceName}";
                        else
                            item.Description = $"{num}";
                        break;
                    }

                case FieldRole.RPTID:
                    {
                        // 혹시 ReportTable에 이름 기능이 있다면 확장 가능
                        item.Description = $"{num}";
                        break;
                    }

                default:
                    // 다른 필드는 fmt.Name 그대로 둠
                    break;
            }
        }

        private static long ExtractUInt(MsgItem item)
        {
            if (item.Data == null || item.Data.Length == 0)
            {
                return 0;
            }

            // MsgItem.Data 는 big-endian 저장
            byte[] bytes = item.Data.Reverse().ToArray();

            try
            {
                switch (item.Format)
                {
                    case DataFormat.U1:
                        return bytes[0];

                    case DataFormat.U2:
                        if (bytes.Length < 2) return 0;
                        return BitConverter.ToUInt16(bytes, 0);

                    case DataFormat.U4:
                        if (bytes.Length < 4) return 0;
                        return BitConverter.ToUInt32(bytes, 0);

                    case DataFormat.U8:
                        if (bytes.Length < 8) return 0;
                        ulong v = BitConverter.ToUInt64(bytes, 0);
                        if (v > uint.MaxValue) return 0;
                        return (uint)v;

                    default:
                        return 0;
                }
            }
            catch
            {
                return 0;
            }
        }

        // --------------------------------------------------------------------
        // 오버로드 포맷 우선순위 계산용: 노드 수 카운트
        // --------------------------------------------------------------------
        private static int CountNodes(FormatNode fmt)
        {
            int count = 1;
            foreach (var c in fmt.Children)
            {
                count += CountNodes(c);
            }
            return count;
        }

        // --------------------------------------------------------------------
        // MsgItem 깊은 복사 (검사용 clone)
        // --------------------------------------------------------------------
        private static MsgItem CloneItem(MsgItem src)
        {
            var dst = new MsgItem(src.Format)
            {
                Data = src.Data?.ToArray() ?? Array.Empty<byte>(),
                Description = src.Description,
                Svid = src.Svid,
                Name = src.Name,
                Unit = src.Unit,
                Role = src.Role
            };

            foreach (var child in src.Items)
            {
                dst.Items.Add(CloneItem(child));
            }

            return dst;
        }

        /// <summary>
        /// clone 에서 설정된 메타정보(Role, Description, Svid, Name, Unit)만
        /// 원본 MsgItem 트리로 반영
        /// </summary>
        private static void CopyMeta(MsgItem from, MsgItem to)
        {
            to.Description = from.Description;
            to.Svid = from.Svid;
            to.Name = from.Name;
            to.Unit = from.Unit;
            to.Role = from.Role;

            int count = Math.Min(from.Items.Count, to.Items.Count);
            for (int i = 0; i < count; i++)
            {
                CopyMeta(from.Items[i], to.Items[i]);
            }
        }
    }
}
