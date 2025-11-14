using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecsGemLib.Utils
{
    public static class Logger
    {
        private static readonly object _lock = new();
        private static string _logFile = "SecsGemLog.txt";
        public static event Action<string> EventHandler;

        public static void Write(string msg)
        {            
            lock (_lock)
            {
                string line = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] {msg}";
                EventHandler.Invoke(line);                
            }
        }
    }
}
