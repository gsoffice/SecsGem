using System;
using System.Collections.Generic;
using System.Linq;
using SecsGemLib.Core;
using SecsGemLib.Gem.Events;
using SecsGemLib.Gem.Variables;
using SecsGemLib.Gem.Data;
using SecsGemLib.Protocols.DataMessages;
using System.IO;

namespace SecsGemLib.Gem
{
    public class GemManager
    {
        public SvidTable Svids { get; } = new();
        public AlidTable Alids { get; } = new();
        public CeidTable Ceids { get; } = new();
        public ReportTable Reports { get; } = new();
        public EventReportLink Links { get; } = new();

        public event Action<int, byte[]> EventTriggered;

        /// <summary>Trigger CEID event (→ S6F11)</summary>
        public void TriggerEvent(int ceid)
        {
            var rptIds = Links.GetLinkedReports(ceid);
            var reportData = new List<GemReportData>();

            foreach (var rptId in rptIds)
            {
                var svids = Reports.GetSvids(rptId);
                var values = svids.Select(id => Svids.Get(id)?.Value).ToList();
                reportData.Add(new GemReportData(rptId, values));
            }

            var s6f11 = Stream6.BuildS6F11(ceid, reportData);
            EventTriggered?.Invoke(ceid, s6f11.ToBytes());
        }
    }
}
