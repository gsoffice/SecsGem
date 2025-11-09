using System;
using System.Collections.Generic;
using SecsGemLib.Gem.Data;

namespace SecsGemLib.Gem.Data
{
    public class GemEventArgs : EventArgs
    {
        public int CEID { get; }
        public DateTime Timestamp { get; } = DateTime.Now;
        public List<GemReportData> Reports { get; }

        public GemEventArgs(int ceid, List<GemReportData> reports)
        {
            CEID = ceid;
            Reports = reports;
        }
    }
}
