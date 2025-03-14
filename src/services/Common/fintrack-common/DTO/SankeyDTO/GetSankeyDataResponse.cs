using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fintrack_common.DTO.SankeyDTO
{
    public class GetSankeyDataResponse
    {
        public List<SankeyNode> Nodes { get; set; } = new List<SankeyNode>();
        public List<SankeyLink> Links { get; set; } = new List<SankeyLink>();
    }

    public class SankeyNode
    {
        public string Name { get; set; } = "";
        public string IdText { get; set; } = "";
    }

    public class SankeyLink
    {
        public int Source { get; set; }
        public string SourceText { get; set; } = "";
        public int Target { get; set; }
        public string TargetText { get; set; } = "";
        public int Value { get; set; }
    }
}
