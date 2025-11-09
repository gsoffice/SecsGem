using SecsGemLib.Streams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static SecsGemLib.Messages.SecsItem;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SecsGemLib.Messages
{
    /// <summary>
    /// HSMS/SECS-II 메시지 조립/인코딩/검증/컨트롤 응답 등을 담당하는 클래스.
    /// 본문(Body)은 SecsItem(순수 데이터 트리)로 보관한다.
    /// </summary>
    public class Message
    {
        // ---------------------------------------------------
        // PROPERTIES (헤더 + 바디)
        // ---------------------------------------------------
        public ushort DeviceId { get; set; }
        public int Stream { get; set; }
        public int Function { get; set; }
        public bool WBit { get; set; }
        public uint SystemBytes { get; set; }
        public SecsItem Body { get; set; }
        public byte[] Prefix { get; private set; } = Array.Empty<byte>();
        public byte[] Header { get; private set; } = Array.Empty<byte>();
        public byte[] byteBody { get; private set; } = Array.Empty<byte>();

        // ---------------------------------------------------
        // CTOR & FACTORY
        // ---------------------------------------------------
        //public Message(ushort deviceId, int stream, int function, bool wbit, uint sysBytes, SecsItem body)
        //{
        //    DeviceId = deviceId;
        //    Stream = stream;
        //    Function = function;
        //    WBit = wbit;
        //    SystemBytes = sysBytes;
        //    Body = body;
        //}

        public Message()
        {

        }

        public static Message Build(int stream, int function, bool wbit, SecsItem body)
        {
            ushort sessionId = 0x00;
            uint sysBytes = 0x00;           
            byte sType = 2;

            // Message 인스턴스 생성
            var msg = new Message();
            msg.Stream = stream;
            msg.Function = function;
            msg.WBit = wbit;
            msg.Body = body;
            // Body / Header / Prefix 각각 생성
            msg.byteBody = BuildBody(body);
            msg.Header = BuildHeader(sessionId, stream, function, wbit, sysBytes, sType);
            msg.Prefix = BuildPrefix(msg.Header, msg.byteBody);

            return msg;            
        }
            

        // ---------------------------------------------------
        // ENCODING (SECS-II Item → byte[])
        // ---------------------------------------------------
        private static byte[] EncodeItem(SecsItem item)
        {
            if (item == null) return Array.Empty<byte>();

            if (item.Format == SecsItem.SecsFormat.L)
            {
                var encodedChildren = item.Items.SelectMany(EncodeItem).ToArray();
                var lenBytes = EncodeLength(item.Items.Count);
                return new byte[] { (byte)((byte)SecsItem.SecsFormat.L | (byte)lenBytes.Length) }
                    .Concat(lenBytes)
                    .Concat(encodedChildren)
                    .ToArray();
            }
            else
            {
                int dataLen = item.Data?.Length ?? 0;
                var lenBytes = EncodeLength(dataLen);
                return new byte[] { (byte)((byte)item.Format | (byte)lenBytes.Length) }
                    .Concat(lenBytes)
                    .Concat(item.Data ?? Array.Empty<byte>())
                    .ToArray();
            }
        }

        /// <summary>
        /// SECS-II Length-of-length 규칙(1~3바이트) 적용.
        /// 0xFF 이하 → 1바이트, 0xFFFF 이하 → 2바이트, 그 외 → 3바이트(big-endian 3바이트).
        /// </summary>
        private static byte[] EncodeLength(int len)
        {
            if (len < 0) len = 0;

            if (len <= 0xFF)
                return new byte[] { (byte)len };

            if (len <= 0xFFFF)
                return BitConverter.GetBytes((ushort)len).Reverse().ToArray(); // 2 bytes

            // 3 bytes (최대 0xFFFFFF까지 표현)
            // BitConverter는 4바이트 int이므로 big-endian 변환 뒤 상위 1바이트를 제외하고 3바이트만 사용
            return BitConverter.GetBytes(len).Reverse().Skip(1).Take(3).ToArray();
        }

        /// <summary>
        /// HSMS Length prefix(4바이트, big-endian)를 앞에 붙인다.
        /// Length 값은 [Header(10)+Body(n)] 길이이며 Length 자기 자신(4바이트)은 포함하지 않는다.
        /// </summary>
        private static byte[] AddLengthPrefix(byte[] dataHeaderPlusBody)
        {
            int len = dataHeaderPlusBody?.Length ?? 0; // header+body
            byte[] lengthField = BitConverter.GetBytes(len).Reverse().ToArray(); // big-endian 4B
            return lengthField.Concat(dataHeaderPlusBody ?? Array.Empty<byte>()).ToArray();
        }

        // ---------------------------------------------------
        // PACKET 만들기 (전송용)
        // ---------------------------------------------------
        public byte[] ToBytes()
        {
            return (Prefix ?? Array.Empty<byte>())
                .Concat(Header ?? Array.Empty<byte>())
                .Concat(byteBody ?? Array.Empty<byte>())
                .ToArray();
        }

        // ---------------------------------------------------
        // LOG 문자열
        // ---------------------------------------------------
        public override string ToString()
        {
            var sb = new StringBuilder();

            string wbit = WBit ? "W" : "";
            sb.AppendLine($"S{Stream}F{Function}{wbit}");

            if (Body != null)
            {
                sb.Append(FormatItem(Body, 0));
            }                

            return sb.ToString();
        }

        private static string FormatItem(SecsItem item, int indent)
        {
            string indentStr = new string(' ', indent * 3);
            var sb = new StringBuilder();

            if (item.Format == SecsItem.SecsFormat.L)
            {
                sb.AppendLine($"{indentStr}<L[{item.Items.Count}]>");
                foreach (var child in item.Items)
                {
                    sb.Append(FormatItem(child, indent + 1));
                }
                sb.AppendLine($"{indentStr}>");
            }
            else if (item.Format == SecsItem.SecsFormat.A)
            {
                string value = Encoding.ASCII.GetString(item.Data ?? Array.Empty<byte>());
                sb.AppendLine($"{indentStr}<A[{item.Data?.Length}/{item.NumElements}] \"{value}\">");
            }
            else
            {
                sb.AppendLine($"{indentStr}<{item.Format}[{item.Data?.Length}/{item.NumElements}]>");
            }

            return sb.ToString();
        }

        // ---------------------------------------------------
        // HSMS HEADER PARSERS / HELPERS
        // ---------------------------------------------------
        public static bool IsControlMessage(byte[] data)
        {
            if (data == null || data.Length < 14) return false;
            byte pType = data[8];
            return pType == 0x00;
        }

        public static bool IsDataMessage(byte[] data)
        {
            if (data == null || data.Length < 14) return false;
            byte pType = data[8];
            return pType == 0x01;
        }

        public static byte GetSType(byte[] data)
        {
            if (data == null || data.Length < 10) return 0;
            return data[9];
        }

        public static uint GetSystemBytes(byte[] data)
        {
            if (data == null || data.Length < 14) return 0;
            return (uint)((data[10] << 24) | (data[11] << 16) | (data[12] << 8) | data[13]);
        }

        public static ushort GetSessionId(byte[] data)
        {
            if (data == null || data.Length < 6) return 0;
            return (ushort)((data[4] << 8) | data[5]);
        }

        /// <summary>
        /// HSMS/SECS-II 패킷 포맷 검증 (간이)
        /// </summary>
        public static bool ValidateFormat(byte[] data, out string error)
        {
            error = null;

            if (data == null)
            {
                error = "데이터가 null입니다.";
                return false;
            }

            if (data.Length < 14)
            {
                error = $"패킷 길이가 너무 짧습니다. (길이: {data.Length})";
                return false;
            }

            // Length 필드 확인 (big-endian)
            int declaredLength = (data[0] << 24) | (data[1] << 16) | (data[2] << 8) | data[3];
            int actualLength = data.Length - 4;
            if (declaredLength != actualLength)
            {
                error = $"Length 필드 불일치: 선언({declaredLength}) vs 실제({actualLength})";
                return false;
            }

            // PType 검사
            byte pType = data[8];
            if (pType != 0x00 && pType != 0x01)
            {
                error = $"PType이 잘못되었습니다. (값: 0x{pType:X2}) - 0x00(Control) 또는 0x01(Data)만 허용";
                return false;
            }

            // Control 메시지의 경우 SType 검사
            byte sType = data[9];
            if (pType == 0x00 && sType != 0x00)
            {
                byte[] validSTypes = { 1, 2, 3, 4, 5, 6, 7, 9 };
                if (!validSTypes.Contains(sType))
                {
                    error = $"Control 메시지의 SType이 잘못되었습니다. (값: {sType})";
                    return false;
                }
            }

            // Data 메시지의 경우 최소한 SECS-II 헤더(10바이트)가 존재해야 함
            if (pType == 0x01 && actualLength < 10)
            {
                error = "Data 메시지의 SECS-II 헤더가 누락되었습니다.";
                return false;
            }

            // SystemBytes 검사 (0은 특수한 경우 외에는 비정상)
            uint sysBytes = GetSystemBytes(data);
            if (sysBytes == 0)
            {
                error = "SystemBytes 값이 0입니다 (비정상 가능성).";
                return false;
            }

            return true;
        }

        /// <summary>
        /// Select.req → Select.rsp (SType=2) 생성
        /// </summary>
        public static Message BuildResponseControlMsg(byte[] req)
        {
            if (req == null || req.Length < 14)
                throw new ArgumentException("잘못된 Select.req 패킷입니다.");

            ushort sessionId = GetSessionId(req);
            uint sysBytes = GetSystemBytes(req);
            byte status = 0;
            byte sType = 0;

            if (IsControlMessage(req))
            {
                if (GetSType(req) == 0x01)
                {
                    sType = 2;
                }
                else if (GetSType(req) == 0x05)
                {
                    sType = 6;
                }
            }

            // Message 인스턴스 생성
            var msg = new Message();

            // Body / Header / Prefix 각각 생성
            msg.byteBody = sType == 2 ? BuildBody(SecsItem.B(status)) : Array.Empty<byte>();
            msg.Header = BuildHeader(sessionId, 0, 0, false, sysBytes, sType);
            msg.Prefix = BuildPrefix(msg.Header, msg.byteBody);

            return msg;
        }

        // ---------------------------------------------------
        // (선택) Auto Message Builder 확장 포인트
        // ---------------------------------------------------
        /// <summary>
        /// 외부에서 Stream별 Body 빌더를 주입받아 Message를 생성한다.
        /// 예) bodyFactory = func(function) => SecsItem.L( ... );
        /// </summary>
        public static Message BuildAutoMessage(int stream, int function)
        {
            switch (stream)
            {
                case 1:
                    var s1 = new Stream1();
                    return s1.BuildMessage(function);

                // case 2:
                // var s2 = new Stream2();
                // return s2.BuildMessage(function);
                //

                default: throw new NotSupportedException($"Stream {stream}은(는) 아직 지원되지 않습니다.");
            }
        }

        // ---------------------------------------------------
        // BUILD HELPERS (Header, Prefix, Body)
        // ---------------------------------------------------
        private static byte[] BuildHeader(ushort deviceId, int stream, int function, bool wbit, uint sysBytes, int sType = 0x00)
        {
            byte streamByte = (byte)(stream | (wbit ? 0x80 : 0x00));
            byte funcByte = (byte)(function & 0x7F);

            return new byte[]
            {
                (byte)(deviceId >> 8),
                (byte)(deviceId & 0xFF),
                streamByte,
                funcByte,
                0x00,        // PType
                (byte)sType, // SType
                (byte)((sysBytes >> 24) & 0xFF),
                (byte)((sysBytes >> 16) & 0xFF),
                (byte)((sysBytes >> 8) & 0xFF),
                (byte)(sysBytes & 0xFF)
            };
        }

        private static byte[] BuildPrefix(byte[] header, byte[] body)
        {
            int len = (header?.Length ?? 0) + (body?.Length ?? 0);
            return BitConverter.GetBytes(len).Reverse().ToArray();
        }

        private static byte[] BuildBody(SecsItem item)
        {
            return item == null ? Array.Empty<byte>() : EncodeItem(item);
        }
    }
}
