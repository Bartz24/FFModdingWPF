using Bartz24.FF13_2;
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
        var rootNode = apData.AreaGraph["Historia Crux"];
        var startLink = rootNode.links.Where(l => l.link_name == "Start area").FirstOrDefault();
        if(startLink.link_name == null)
        {
            throw new Exception("Missing root link information!");
        }
        rootLocation = startLink.target_area;
        areaDepths = graph.Values.ToDictionary(node => node.name, ResolveNodeDepth);
        // do placement logic here if not vanilla (flag in json? just do it always?)
        // gate table modification, coordinate overrides, line placement etc.
        UpdateGateTable(apData.AreaGraph);
        var updatedLocations = apData.AreaGraph.ToDictionary(kvp => kvp.Key, kvp =>
        {
            var nodeCoords = (kvp.Value.loc_x, kvp.Value.loc_y);
            var baseCoords = MapCoordsToHexGrid(nodeCoords);
            if (kvp.Key.Contains("_zz_") || kvp.Key.Contains("_sp_"))
            {
                // blank items are offset by 5 in each direction
                // Void beyond nodes also do this as they show as blank in the main matrix
                return (baseCoords.Item1 + 5, baseCoords.Item2 + 5);
            }
            return baseCoords;
        });
        UpdateCruxPositions(updatedLocations);
    }

    private void UpdateGateTable(Dictionary<string, FF13_2AreaNode> nodes)
    {
        foreach (var node in nodes)
        {
            if(node.Key == "Historia Crux")
            {
                continue;
            }
            foreach (var link in node.Value.links)
            {
                gateTable[link.link_name].sOpenHistoria1 = link.target_area + "_a";
            }
            if(node.Key == HistoriaCruxConstants.SUNLETH_300)
            {
                gateTable["hs_snda03_cs"].sOpenHistoria1 = "h_sp_NA0001_a";
            }
            if (node.Key == HistoriaCruxConstants.YASCHAS_1X)
            {
                gateTable["hs_ghaa01_ac"].sOpenHistoria1 = "h_cs_NA0000_a";
            }
        }
        gateTable["hs_hmaa_def"].sArea = rootLocation;
        gateTable["hs_hmaa_def"].sOpenHistoria1 = rootLocation + "_a";
        gateTable["hs_hmaa10_zz"].sArea = rootLocation;
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
