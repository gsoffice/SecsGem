using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecsGemLib
{
    public class Logger
    {
        // 로그 발생 시 외부에 알릴 이벤트
        public static event Action<string> LogWritten;

        public static void Write(string message)
        {
            string msg = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] {message}";
            LogWritten?.Invoke(msg);
        }
    }
}
