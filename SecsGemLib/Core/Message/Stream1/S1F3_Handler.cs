using SecsGemLib.Gem.Variables;

namespace SecsGemLib.Core
{
    [Handler(1, 3)]
    public class S1F3_Handler : IMessageHandler
    {
        public Message Handle(Message msg)
        {
            // ① 요청받은 SVID 리스트 추출 (빈 경우 전체)
            List<long> requestedSvids = MessageInspector.ExtractSvidList(msg);
            IEnumerable<VariableBase> targets;

            if (requestedSvids.Count == 0)
            {
                targets = SvidTable.GetAll();
            }
            else
            {
                targets = requestedSvids
                    .Select(id => SvidTable.Get(id))
                    .Where(v => v != null);
            }

            // ② 각 VariableBase를 MessageItem으로 변환
            var svList = new List<MessageItem>();

            foreach (var v in targets)
            {
                MessageItem svItem;

                switch (v.Format)
                {
                    case MessageItem.DataFormat.A:
                        svItem = MessageItem.A(v.Data?.ToString() ?? "");
                        break;

                    case MessageItem.DataFormat.B:
                        svItem = v.Data is byte[] bytes
                            ? MessageItem.B(bytes)
                            : MessageItem.B(Array.Empty<byte>());
                        break;

                    case MessageItem.DataFormat.BOOLEAN:
                        bool boolVal = false;
                        if (v.Data is bool b)
                            boolVal = b;
                        else if (v.Data is string s && bool.TryParse(s, out bool parsed))
                            boolVal = parsed;
                        svItem = MessageItem.BOOLEAN(boolVal);
                        break;

                    case MessageItem.DataFormat.U1:
                        svItem = MessageItem.U1(Convert.ToByte(v.Data));
                        break;
                    case MessageItem.DataFormat.U2:
                        svItem = MessageItem.U2(Convert.ToUInt16(v.Data));
                        break;
                    case MessageItem.DataFormat.U4:
                        if (v.Data == null || string.IsNullOrWhiteSpace(v.Data.ToString()))
                            svItem = MessageItem.U4(0);
                        else if (uint.TryParse(v.Data.ToString(), out uint u4))
                            svItem = MessageItem.U4(u4);
                        else
                            svItem = MessageItem.U4(0);
                        break;
                    case MessageItem.DataFormat.U8:
                        svItem = MessageItem.U8(Convert.ToUInt64(v.Data));
                        break;
                    case MessageItem.DataFormat.I1:
                        svItem = MessageItem.I1(Convert.ToSByte(v.Data));
                        break;
                    case MessageItem.DataFormat.I2:
                        svItem = MessageItem.I2(Convert.ToInt16(v.Data));
                        break;
                    case MessageItem.DataFormat.I4:
                        svItem = MessageItem.I4(Convert.ToInt32(v.Data));
                        break;
                    case MessageItem.DataFormat.I8:
                        svItem = MessageItem.I8(Convert.ToInt64(v.Data));
                        break;

                    case MessageItem.DataFormat.F4:
                        svItem = MessageItem.F4(Convert.ToSingle(v.Data));
                        break;
                    case MessageItem.DataFormat.F8:
                        svItem = MessageItem.F8(Convert.ToDouble(v.Data));
                        break;

                    case MessageItem.DataFormat.L:
                        if (v.Data is List<MessageItem> list)
                            svItem = MessageItem.L(list.ToArray());
                        else
                            svItem = MessageItem.L(); // 빈 리스트 대응
                        break;

                    default:
                        svItem = MessageItem.A(v.Data?.ToString() ?? "");
                        break;
                }

                svList.Add(svItem);
            }

            // ③ 최종 바디 구성: { L n SV1 SV2 ... }
            MessageItem body = MessageItem.L(svList.ToArray());

            // ④ 메시지 조립
            return Message.Build(msg, body);
        }
    }
}

