namespace SecsGemLib.Core
{
    /// <summary>
    /// EZNET SECS MESSAGE FORMAT 의 한 노드를 표현
    /// 예: &lt;U4  DATAID&gt;, &lt;L[n]&gt;, &lt;VARIANT SV&gt; 등
    /// </summary>
    public class FormatNode
    {
        /// <summary>
        /// 타입 키워드: L, U1, U2, U4, A, B, BOOLEAN, VAR, VARIANT ...
        /// </summary>
        public string TypeKeyword { get; set; }

        /// <summary>
        /// 필드 명: DATAID, CEID, SVID, VID, DRACK, SV, ...
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// A[40], B[10] 등 고정 길이. 없으면 null.
        /// </summary>
        public int? FixedLength { get; set; }

        /// <summary>
        /// L[2] 같은 고정 element count. 없으면 null.
        /// </summary>
        public int? FixedCount { get; set; }

        /// <summary>
        /// L[n] 여부
        /// </summary>
        public bool VariableCount { get; set; }

        /// <summary>
        /// 자식 노드 리스트 (L 타입일 때만 사용)
        /// </summary>
        public List<FormatNode> Children { get; } = new List<FormatNode>();

        public bool IsList => TypeKeyword == "L";

        /// <summary>
        /// VAR / VARIANT 같은 타입 와일드카드인지 여부
        /// </summary>
        public bool IsVariantType =>
            TypeKeyword == "VAR" || TypeKeyword == "VARIANT";

        public override string ToString()
        {
            return $"{TypeKeyword}{(FixedLength.HasValue ? $"[{FixedLength}]" : "")} {Name}";
        }
    }
}
