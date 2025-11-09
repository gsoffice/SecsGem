using System.IO;
using System.Text.Json;

namespace SecsGemLib.Gem
{
    public class GemConfig
    {
        public GemManager Manager { get; } = new();

        public static GemManager Load(string filePath)
        {
            var json = File.ReadAllText(filePath);
            var cfg = JsonSerializer.Deserialize<GemConfigFile>(json);
            var mgr = new GemManager();

            foreach (var s in cfg.SVID)
                mgr.Svids.Add(s.Id, s.Name, s.Value);

            foreach (var c in cfg.CEID)
                mgr.Ceids.Add(c.Id, c.Name);

            foreach (var r in cfg.REPORT)
                mgr.Reports.Add(r.Id, r.SVIDs.ToArray());

            foreach (var l in cfg.LINK)
                mgr.Links.Link(l.CEID, l.RPTIDs.ToArray());

            return mgr;
        }

        private class GemConfigFile
        {
            public List<SvidDef> SVID { get; set; }
            public List<CeidDef> CEID { get; set; }
            public List<ReportDef> REPORT { get; set; }
            public List<LinkDef> LINK { get; set; }

            public class SvidDef { public int Id; public string Name; public object Value; }
            public class CeidDef { public int Id; public string Name; }
            public class ReportDef { public int Id; public List<int> SVIDs; }
            public class LinkDef { public int CEID; public List<int> RPTIDs; }
        }
    }
}
