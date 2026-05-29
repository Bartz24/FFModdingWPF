using Bartz24.RandoWPF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace FF13_2Rando;

public class APHistoriaCruxRando: HistoriaCruxRando
{
    public APHistoriaCruxRando(SeedGenerator generator) : base(generator)
    {

    }

    public override void Randomize()
    {
        // Not currently supported within AP, coming "soon"
        var apData = RandoFlags.GetArchipelagoData<FF13_2ArchipelagoData>();
        var graph = buildGraph(apData.AreaGraph);
        areaDepths = graph.Values.ToDictionary(node => node.name, ResolveNodeDepth);
    }

    private Dictionary<string, TreeNode> buildGraph(Dictionary<string, FF13_2AreaNode> nodeDict)
    {
        Dictionary<string, TreeNode> nodes = new Dictionary<string, TreeNode>();
        foreach(var (name, nodeData) in nodeDict)
        {
            if(name == "Historia Crux")
            {
                // Not a real area, don't include it in the graph so that we start from depth 0 not 1.
                // TODO: add in crux "node" to base crux rando maybe?
                continue;
            }
            var node = new TreeNode();
            node.name = name;
            nodes.Add(name, node);
        }
        foreach (var node in nodes.Values)
        {
            var links = nodeDict[node.name].links;
            foreach (var link in links)
            {
                var child = nodes[link.target_area];
                child.parent = node;
                node.children.Add(child);
            }
        }
        return nodes;
    }
}
