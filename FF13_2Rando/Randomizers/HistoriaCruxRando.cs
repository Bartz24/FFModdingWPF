using Bartz24.Data;
using Bartz24.Docs;
using Bartz24.FF13_2;
using Bartz24.FF13_2_LR;
using Bartz24.FF13Series;
using Bartz24.RandoWPF;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FF13_2Rando;

public partial class HistoriaCruxRando : Randomizer
{
    public DataStoreWDB<DataStoreRGateTable> gateTable = new();
    public DataStoreWDB<DataStoreRGateTable> gateTableOrig = new();

    private byte[] hcParts;

    public Dictionary<string, GateData> gateData = new();
    public Dictionary<string, AreaData> areaData = new();

    public Dictionary<string, string> placement = new();

    private Dictionary<int, string> coordMap = new();

    public string rootLocation;

    public Dictionary<string, int> areaDepths = new();

    public Dictionary<string, TreeNode> shuffledNodes = new();

    public string overrideInitial;

    private bool experimental = true;

    public HistoriaCruxRando(SeedGenerator randomizers) : base(randomizers) { }

    public override void Load()
    {
        RandoUI.SetUIProgressIndeterminate("Loading Historia Crux Data...");
        // Unpack gate table
        gateTable.LoadWDB(Generator, "13-2", @"\db\resident\_wdbpack.bin\r_gatetab.wdb", false);
        gateTableOrig.LoadWDB(Generator, "13-2", @"\db\resident\_wdbpack.bin\r_gatetab.wdb", false);

        gateData.Clear();

        // Unpack gui xgr for gate matrix
        string guiSystemXGRPath = Nova.GetNovaFile("13-2", @"gui\resident\system.win32.xgr", SetupData.Paths["Nova"], SetupData.Paths["13-2"]);
        string guiSystemXRGOutPath = Generator.DataOutFolder + @"\gui\resident\system.win32.xgr";
        FileHelpers.CopyFile(guiSystemXGRPath, guiSystemXRGOutPath);
        Nova.UnpackWPD(guiSystemXRGOutPath, SetupData.Paths["Nova"]);
        string hcPartsPath = Generator.DataOutFolder + @"\gui\resident\_system.win32.xgr\gr_hc_parts.ykd";
        hcParts = File.ReadAllBytes(hcPartsPath);

        FileHelpers.ReadCSVFile(@"data\historia.csv", row =>
        {
            GateData t = new(row);
            gateData.Add(t.ID, t);
        }, FileHelpers.CSVFileHeader.HasHeader);

        FileHelpers.ReadCSVFile(@"data\areas.csv", row =>
        {
            AreaData a = new(row);
            areaData.Add(a.ID, a);
        }, FileHelpers.CSVFileHeader.HasHeader);
    }
    public override void Randomize()
    {
        RandoUI.SetUIProgressIndeterminate("Randomizing Historia Crux Data...");
        if (FF13_2Flags.Other.HistoriaCrux.FlagEnabled)
        {
            FF13_2Flags.Other.HistoriaCrux.SetRand();

            // Update historia.csv to include all gate entries and ykd offset (for ease) - later
            // Add ykd offsets to areas.csv also for ease of maintenance - later

            // Setup initial area nodes with no links
            var nodes = new Dictionary<string, TreeNode>();
            var coordMap = new Dictionary<int, string>();
            var locations = gateTable.Values.Select(g => g.sArea).Concat(gateTable.Values.Select(g => g.sOpenHistoria1.Substring(0, g.sOpenHistoria1.Length - 2))).Distinct().ToList();
            foreach (var area in locations)
            {
                var emptyNode = new TreeNode();
                emptyNode.name = area;
                nodes.Add(area, emptyNode);
            }
            var unplacedStarter = nodes.Keys.ToList();

            // Blank 7 has to be left of root always for now.
            unplacedStarter.Remove(HistoriaCruxConstants.BLANK_7);
            // Remove the dlc group to be re-added later
            if (!FF13_2Flags.Other.RandoDLC.Enabled)
            {
                unplacedStarter.Remove(HistoriaCruxConstants.COLISEUM_DLC);
                unplacedStarter.Remove(HistoriaCruxConstants.VALHALLA_DLC);
                unplacedStarter.Remove(HistoriaCruxConstants.SERENDIPITY_DLC);
            }

            // Place any fixed initial nodes _with coordinates_
            var startingCoords = (2, 6);

            TreeNode rootNode;
            var attempts = 0;
            // Place all remaining nodes
            // position up to outgoing link count on area
            // place with placement logic rather than fixed list
            Dictionary<int, string> finalPlacement;
            do
            {
                var unplaced = new List<string>(unplacedStarter);
                var expectedCount = nodes.Count;
                // DLC areas
                expectedCount -= 1;
                if (!FF13_2Flags.Other.RandoDLC.Enabled)
                {
                    expectedCount -= 3;
                }
                // TODO: redo initial placement flags for flexibility
                if (FF13_2Flags.Other.ForcedStart.Values.IndexOf(FF13_2Flags.Other.ForcedStart.SelectedValue) > 0)
                {
                    unplaced.Remove(HistoriaCruxConstants.NEW_BODHUM_3);
                    var id = CoordsToId(startingCoords.Item1, startingCoords.Item2);
                    coordMap[id] = HistoriaCruxConstants.NEW_BODHUM_3;
                    rootNode = nodes[HistoriaCruxConstants.NEW_BODHUM_3];
                    if (FF13_2Flags.Other.ForcedStart.Values.IndexOf(FF13_2Flags.Other.ForcedStart.SelectedValue) > 1)
                    {
                        unplaced.Remove(HistoriaCruxConstants.BRESHA_RUINS_5);
                        var id2 = CoordsToId(startingCoords.Item1 + 1, startingCoords.Item2);
                        coordMap[id2] = HistoriaCruxConstants.BRESHA_RUINS_5;
                        rootNode = nodes[HistoriaCruxConstants.BRESHA_RUINS_5];
                        nodes[HistoriaCruxConstants.NEW_BODHUM_3].children.Add(rootNode);
                        rootNode.parent = nodes[HistoriaCruxConstants.NEW_BODHUM_3];
                        expectedCount--;
                    }
                }
                else
                {
                    var bannedInitial = new List<string>()
                    {
                        // Banned due to fixed fight requirements making this too difficult/awkward
                        HistoriaCruxConstants.ACADEMIA_400,
                        HistoriaCruxConstants.AUGUSTA_200,
                        // Banned as every check requires moogle hunt level 1
                        HistoriaCruxConstants.OERBA_200,
                        HistoriaCruxConstants.OERBA_300,
                        // Banned due to downstream link constraints
                        HistoriaCruxConstants.SUNLETH_300,
                        HistoriaCruxConstants.YASCHAS_1X,
                        // For obvious reasons...
                        HistoriaCruxConstants.ACADEMIA_500,
                        HistoriaCruxConstants.DYING_WORLD_700,
                        HistoriaCruxConstants.NEW_BODHUM_700
                    };
                    var initial = RandomNum.SelectRandom(unplaced.Where(r =>
                    {
                        if (bannedInitial.Contains(r)) { return false; }
                        // if the area has a fixed inbound route, don't allow it to be the starting point.
                        if (areaData[r].Traits.Contains("FixedInbound")) { return false; }
                        // Allow terminal nodes if DLC shuffle is on
                        return areaData[r].OutgoingLinkCount > 0;
                    })); //pick random starting location based on random
                    unplaced.Remove(initial);
                    var id = CoordsToId(startingCoords.Item1, startingCoords.Item2);
                    coordMap[id] = initial;
                    rootNode = nodes[initial];
                }

                // If the dlc locations are randomised then ensure the "parent" node for them is included in placement logic
                if (FF13_2Flags.Other.RandoDLC.Enabled)
                {
                    rootNode.children.Add(nodes[HistoriaCruxConstants.BLANK_7]);
                    nodes[HistoriaCruxConstants.BLANK_7].parent = rootNode;
                    // Blank 7 is "placed" but doesn't follow the same rules for now, so it looks like one node is missing always.
                    expectedCount++;
                }

                expectedCount--;

                Generator.Logger.LogDebug($"Attempting to place {unplaced.Count} starting on {rootNode.name}");
                var placed = TryPlaceChildrenWithPlacement(rootNode, 2, 6, coordMap, 0, 0, unplaced, nodes, 1.0f, FF13_2Flags.Other.RandoDLC.Enabled ? 1 : 0);

                if (placed.Item1 && placed.Item2.Count == expectedCount && placed.Item3.Count == 0)
                {
                    Generator.Logger.LogDebug($"Placement of crux nodes successful on attempt {attempts}");
                    finalPlacement = placed.Item2;
                    coordMap = finalPlacement;
                    rootLocation = rootNode.name;
                    break;
                }
                else
                {
                    attempts++;
                    Generator.Logger.LogDebug($"Placed {placed.Item2.Count} of {nodes.Count} (expected {expectedCount}) remaining {placed.Item3.Count}");
                    foreach (var node in nodes.Values)
                    {
                        node.children.Clear();
                        node.parent = null;
                    }
                    if (attempts > 10)
                    {
                        throw new Exception("Too many attempts!");
                    }
                }
            } while (true);


            if (!FF13_2Flags.Other.RandoDLC.Enabled)
            {
                var dlcBlankNode = nodes[HistoriaCruxConstants.BLANK_7];
                coordMap[CoordsToId(1, 6)] = HistoriaCruxConstants.BLANK_7;
                rootNode.children.Add(dlcBlankNode);
                dlcBlankNode.parent = rootNode;
                dlcBlankNode.children.Add(nodes[HistoriaCruxConstants.COLISEUM_DLC]);
                nodes[HistoriaCruxConstants.COLISEUM_DLC].parent = dlcBlankNode;
                dlcBlankNode.children.Add(nodes[HistoriaCruxConstants.SERENDIPITY_DLC]);
                nodes[HistoriaCruxConstants.SERENDIPITY_DLC].parent = dlcBlankNode;
                dlcBlankNode.children.Add(nodes[HistoriaCruxConstants.VALHALLA_DLC]);
                nodes[HistoriaCruxConstants.VALHALLA_DLC].parent = dlcBlankNode;
                var dlcUpperId = CoordsToId(0, 6 - 1);
                coordMap[dlcUpperId] = HistoriaCruxConstants.VALHALLA_DLC;
                var dlcLowerId = CoordsToId(1, 6 + 1);
                coordMap[dlcLowerId] = HistoriaCruxConstants.SERENDIPITY_DLC;
                var dlcLeftId = CoordsToId(0, 6);
                coordMap[dlcLeftId] = HistoriaCruxConstants.COLISEUM_DLC;
            }
            this.coordMap = coordMap;
            areaDepths = nodes.Values.ToDictionary(node => node.name, ResolveNodeDepth);

            // Bounds check all coordinates and shift the entire grid over if needed to fit?

            // Replace all historia gate links properly so that the association is correct, and the incoming placement is correct
            // Special case the hmaa_def link to replace whatever initial area is with wherever new bodhum ends up
            // Can we just straight swap the lines here or does that break things? only one way to find out I guess.
            // Might just have a floating line for now?

            //hmaa_def needs to point at initial
            //otherwise update gates based on target location but needs to be _outgoing_
            //need to know which link goes with which gate - associate on fixed ones somehow?
            var unshuffledNodes = new List<string>() { };
            if (!FF13_2Flags.Other.RandoDLC.Enabled)
            {
                unshuffledNodes.Add(HistoriaCruxConstants.BLANK_7);
                unshuffledNodes.Add(HistoriaCruxConstants.COLISEUM_DLC);
                unshuffledNodes.Add(HistoriaCruxConstants.SERENDIPITY_DLC);
                unshuffledNodes.Add(HistoriaCruxConstants.VALHALLA_DLC);
            }
            foreach (var node in nodes.Values)
            {
                if (unshuffledNodes.Contains(node.name))
                {
                    continue;
                }
                Generator.Logger.LogDebug($"Node {node.name} links to {string.Join(",", node.children.Select(s => s.name))}");
                var outgoingLinks = areaData[node.name].OutgoingGates;
                int childOffset = 0;
                // special case for root node DLC 970 placement
                // the zz link is handled outside of this loop, so just skip the child in the node list if its present
                //if(node.name == rootLocation && node.children.Count > outgoingLinks.Count)
                //{
                //    childOffset++;
                //}

                // special case 1x/sunleth because its weird...
                // the void beyond or serendipity is placed first, so go backwards from the bottom for the other outward links
                if (node.name == HistoriaCruxConstants.SUNLETH_300)
                {
                    // blank node
                    //gateTable["hs_snda01_zz"].sOpenHistoria1 = node.children[node.children.Count - 1].name + "_a";
                    // open link
                    gateTable["hs_snda02_gd"].sOpenHistoria1 = node.children[node.children.Count - 1].name + "_a";
                    // void beyond
                    //gateTable["hs_snda03_ac"].sOpenHistoria1 = node.children[node.children.Count - 1].name + "_a";
                    // Flatten unpicked link to not break downstream logic later when the graph is built
                    // serendipity
                    gateTable["hs_snda03_cs"].sOpenHistoria1 = "h_sp_NA0001_a";
                }
                else if (node.name == HistoriaCruxConstants.YASCHAS_1X)
                {
                    // Flatten unpicked link to not break downstream logic later when the graph is built
                    // void beyond
                    gateTable["hs_ghaa01_ac"].sOpenHistoria1 = "h_cs_NA0000_a";
                    // serendipity
                    //gateTable["hs_ghaa01_cs"].sOpenHistoria1 = node.children[node.children.Count - 1].name + "_a";
                    // open link
                    gateTable["hs_ghaa02_gt"].sOpenHistoria1 = node.children[node.children.Count - 1].name + "_a";
                }
                else
                {
                    for (var j = 0; j < outgoingLinks.Count; j++)
                    {
                        var child = node.children[j + childOffset];
                        // special case for root node DLC 970 placement
                        // the zz link is handled outside of this loop, so just skip the child in the node list if its present
                        if (node.name == rootLocation && child.name == HistoriaCruxConstants.BLANK_7)
                        {
                            childOffset++;
                            j--;
                            continue;
                        }
                        var link = outgoingLinks[j];
                        gateTable[link].sOpenHistoria1 = child.name + "_a";
                    }
                }
            }
            gateTable["hs_hmaa_def"].sArea = rootLocation;
            gateTable["hs_hmaa_def"].sOpenHistoria1 = rootLocation + "_a";
            gateTable["hs_hmaa10_zz"].sArea = rootLocation;

            shuffledNodes = new(nodes);

            // Do placement from tree
            var updatedLocations = coordMap.ToDictionary(kvp => kvp.Value, kvp =>
            {
                var baseCoords = MapCoordsToHexGrid(IdToCoords(kvp.Key));
                if (kvp.Value.Contains("_zz_") || kvp.Value.Contains("_sp_"))
                {
                    // blank items are offset by 5 in each direction
                    // Void beyond nodes also do this as they show as blank in the main matrix
                    return (baseCoords.Item1 + 5, baseCoords.Item2 + 5);
                }
                return baseCoords;
            });
            foreach (var (key, offset) in ykdGateOffsets)
            {
                var validation = BitConverter.ToSingle(hcParts.SubArray(offset, 4));
                var originalX = BitConverter.ToSingle(hcParts.SubArray(offset + 0x20, 4));
                var originalY = BitConverter.ToSingle(hcParts.SubArray(offset + 0x24, 4));
                // Hide other nodes in the top right
                var targetCoordsToSet = updatedLocations.GetValueOrDefault(key, (-30, 30));
                var targetX = BitConverter.GetBytes((float)targetCoordsToSet.Item1);
                var targetY = BitConverter.GetBytes((float)targetCoordsToSet.Item2);
                targetX.CopyTo(hcParts, offset + 0x20);
                targetY.CopyTo(hcParts, offset + 0x24);
                Generator.Logger.LogDebug($"Updated crux coords for key {key} (original x {originalX} y {originalY}) -> (new x {targetCoordsToSet.Item1} y {targetCoordsToSet.Item2}).");
            }
            var i = 0;
            foreach (var link in gateTable.Keys.OrderBy(s => s, StringComparer.Ordinal))
            {

                var linkDetails = gateTable[link];
                var left = linkDetails.sArea;
                var right = linkDetails.sOpenHistoria1.Substring(0, linkDetails.sOpenHistoria1.Length - 2);
                // Bodhum 3xx is a fake area, for link purposes just skip through to the next point
                if (right == HistoriaCruxConstants.NEW_BODHUM_3X)
                {
                    right = HistoriaCruxConstants.BLANK_5;
                }
                if (!updatedLocations.ContainsKey(left) || !updatedLocations.ContainsKey(right))
                {
                    Generator.Logger.LogDebug($"Unable to link coords at either end of link {link}");
                    continue;
                }

                // TODO: special case if override initial to ensure initial link gets set properly (validate??)
                if (rootLocation != null && right == HistoriaCruxConstants.NEW_BODHUM_3)
                {
                    right = rootLocation;
                }
                else if (rootLocation != null && right == rootLocation)
                {
                    right = HistoriaCruxConstants.NEW_BODHUM_3;
                }

                // TODO: special case for "magic" links to void beyond/serendipity, need to consider left also
                DataStoreRGateTable incomingLink;
                if (right != HistoriaCruxConstants.SERENDIPITY && right != HistoriaCruxConstants.VOID_BEYOND_A)
                {
                    incomingLink = gateTableOrig.Values.Find(v => v.sOpenHistoria1 == right + "_a");
                }
                else
                {
                    incomingLink = gateTableOrig.Values.Find(v => v.sOpenHistoria1 == right + "_a" && v.sArea == left);
                }

                if (incomingLink == null)
                {
                    Generator.Logger.LogDebug($"Unable to find incoming link entry for right {right}");
                    continue;
                }

                var offset = ykdLinkOffsets.GetValueOrDefault(incomingLink.record, 0);
                if (offset == 0)
                {
                    Generator.Logger.LogDebug($"Unable to locate link offset for link {incomingLink.record}. Links {left} to {right}");
                    continue;
                }
                var validation = BitConverter.ToSingle(hcParts.SubArray(offset, 4));
                // Not always sensible value?
                var originalANgle = BitConverter.ToSingle(hcParts.SubArray(offset + 0x20, 4));
                var originalX = BitConverter.ToSingle(hcParts.SubArray(offset + 0x28, 4));
                var originalY = BitConverter.ToSingle(hcParts.SubArray(offset + 0x2c, 4));
                // Not always 13??
                var originalLen = BitConverter.ToSingle(hcParts.SubArray(offset + 0x48, 4));
                // Might need to further fine tune the adjustments here
                var leftPos = updatedLocations[left];
                if (left.Contains("_sp_") || left.Contains("_zz_"))
                {
                    leftPos = (leftPos.Item1 - 5, leftPos.Item2 - 5);
                }
                var rightPos = updatedLocations[right];
                if (right.Contains("_sp_") || right.Contains("_zz_"))
                {
                    rightPos = (rightPos.Item1 - 5, rightPos.Item2 - 5);
                }
                var midPoint = ((float)(leftPos.Item1 + rightPos.Item1) / 2, ((float)(leftPos.Item2 + rightPos.Item2) / 2) + 7.5f);
                var dy = rightPos.Item2 - leftPos.Item2;
                var dx = rightPos.Item1 - leftPos.Item1;
                double angle;
                double len;
                int mode = 2;
                var pi4 = (float)Math.PI / 4;
                if (mode == 1)
                {
                    angle = Math.Atan2(dy, dx);
                    // Not sure this is quite correct currently
                    len = Math.Sqrt(dx * dx + dy * dy);
                }
                else
                {
                    // Lock angle to just 0, -Pi/4, +Pi/4 based on relative offsets
                    // Don't adjust length in keeping with vanilla
                    // Maybe adjust coords slightly
                    len = 13;
                    if (leftPos.Item2 == rightPos.Item2)
                    {
                        angle = 0;
                    }
                    else if (leftPos.Item1 < rightPos.Item1)
                    {
                        if (leftPos.Item2 > rightPos.Item2)
                        {
                            angle = -pi4;
                        }
                        else
                        {
                            angle = pi4;
                        }
                    }
                    else
                    {
                        if (leftPos.Item2 > rightPos.Item2)
                        {
                            angle = pi4;
                        }
                        else
                        {
                            angle = -pi4;
                        }
                    }
                }
                // These MUST be single precision floats, not double etc or you'll corrupt the structure :)
                var targetX = BitConverter.GetBytes((float)midPoint.Item1);
                var targetY = BitConverter.GetBytes((float)midPoint.Item2);
                var targetAngle = BitConverter.GetBytes((float)angle);
                var targetLength = BitConverter.GetBytes((float)len);
                targetX.CopyTo(hcParts, offset + 0x28);
                targetY.CopyTo(hcParts, offset + 0x2c);
                targetAngle.CopyTo(hcParts, offset + 0x20);
                targetLength.CopyTo(hcParts, offset + 0x48);
                Generator.Logger.LogDebug($"Linking ({leftPos.Item1}, {leftPos.Item2}) to ({rightPos.Item1},{rightPos.Item2})");
                Generator.Logger.LogDebug($"Updated crux link for key {incomingLink.record} - links {left} to {right} (x: {originalX}, y: {originalY}, angle: {originalANgle}, len: {originalLen})" +
                    $" -> (x: {midPoint.Item1}, y: {midPoint.Item2}, angle: {angle}, len: {len}).");
                i++;
            }

            RandomNum.ClearRand();
        }
    }

    public class TreeNode
    {
        public string name;
        // TODO: associate link name with child for easier lookup?
        public List<TreeNode> children = new();
        public TreeNode parent;
    }

    public int ResolveNodeDepth(TreeNode node)
    {
        int depth = 0;
        TreeNode curr = node;
        var seen = new List<TreeNode>() { node };
        while (curr.parent != null)
        {
            if (seen.Contains(curr.parent))
            {
                // loop detected!
                return -1;
            }
            seen.Add(curr.parent);
            curr = curr.parent;
            depth++;
        }
        return depth;
    }

    private List<TreeNode> FlattenChildrenFromNode(TreeNode root)
    {
        var flattened = new List<TreeNode>() { root };
        flattened.AddRange(root.children.SelectMany(FlattenChildrenFromNode));
        return flattened;
    }

    private int ResolveNodeDepthRelative(TreeNode root, TreeNode node)
    {
        int depth = 0;
        TreeNode curr = node;
        while (curr != root)
        {
            if (curr.parent == null)
            {
                return -1;
            }
            curr = curr.parent;
            depth++;
        }
        return depth;
    }

    private int CoordsToId(int x, int y)
    {
        return x * 31 + y;
    }

    private (int, int) IdToCoords(int c)
    {
        return (c / 31, c % 31);
    }

    private (int, int) MapCoordsToHexGrid((int, int) xy)
    {
        var (x, y) = xy;
        return ((x - 3) * 30 - 15 * (y - 6), (5 - y) * 15);
    }

    private readonly Dictionary<string, int> ykdGateOffsets = new Dictionary<string, int>()
    {
        {HistoriaCruxConstants.BLANK_7, 0x60f0 },
        {HistoriaCruxConstants.AUGUSTA_900, 0x6170 },
        {HistoriaCruxConstants.AUGUSTA_200, 0x61f0 },
        {HistoriaCruxConstants.AUGUSTA_300, 0x6270 },
        {HistoriaCruxConstants.ACADEMIA_400, 0x62f0 },
        {HistoriaCruxConstants.ACADEMIA_500, 0x6370 },
        // HC loc is 0100 not 0400
        {HistoriaCruxConstants.ACADEMIA_4XX, 0x63f0 },
        {HistoriaCruxConstants.VILE_PEAKS_10, 0x6470 },
        {HistoriaCruxConstants.VILE_PEAKS_200, 0x64f0 },
        {HistoriaCruxConstants.SERENDIPITY, 0x6570 },
        {HistoriaCruxConstants.COLISEUM, 0x65f0 },
        {HistoriaCruxConstants.SUNLETH_900, 0x6670 },
        {HistoriaCruxConstants.SUNLETH_300, 0x66f0 },
        {HistoriaCruxConstants.SUNLETH_400, 0x6770 },
        {HistoriaCruxConstants.BRESHA_RUINS_100, 0x67f0 },
        {HistoriaCruxConstants.BRESHA_RUINS_300, 0x6870 },
        {HistoriaCruxConstants.BRESHA_RUINS_5, 0x68f0 },
        {HistoriaCruxConstants.NEW_BODHUM_900, 0x6970 },
        {HistoriaCruxConstants.NEW_BODHUM_3, 0x69f0 },
        {HistoriaCruxConstants.NEW_BODHUM_700, 0x6a70 },
        {HistoriaCruxConstants.YASCHAS_100, 0x6af0 },
        {HistoriaCruxConstants.YASCHAS_10, 0x6b70 },
        // 110
        {HistoriaCruxConstants.YASCHAS_110, 0x6bf0 },
        {HistoriaCruxConstants.YASCHAS_1X, 0x6c70 },
        {HistoriaCruxConstants.OERBA_900, 0x6cf0 },
        {HistoriaCruxConstants.OERBA_200, 0x6d70 },
        {HistoriaCruxConstants.OERBA_300, 0x6df0 },
        {HistoriaCruxConstants.OERBA_400, 0x6e70 },
        {HistoriaCruxConstants.DYING_WORLD_700, 0x6ef0 },
        // TODO: not working?
        {HistoriaCruxConstants.DYING_WORLD_900, 0x6f70 },
        {HistoriaCruxConstants.VOID_BEYOND_A, 0x6ff0 },
        {HistoriaCruxConstants.VOID_BEYOND_B, 0x7070 },
        {HistoriaCruxConstants.BLANK_1, 0x70f0 },
        {HistoriaCruxConstants.BLANK_2, 0x7170 },
        {HistoriaCruxConstants.BLANK_3, 0x71f0 },
        {HistoriaCruxConstants.BLANK_4, 0x7270 },
        {HistoriaCruxConstants.BLANK_5, 0x72f0 },
        {HistoriaCruxConstants.BLANK_6, 0x7370 },
        {HistoriaCruxConstants.ARCHYLTE, 0x73f0 },
        {HistoriaCruxConstants.ARCHYLTE_900, 0x7470 },
        {HistoriaCruxConstants.VALHALLA_FINAL, 0x74f0 },
        {HistoriaCruxConstants.VALHALLA_DLC, 0x7570 },
        {HistoriaCruxConstants.SERENDIPITY_DLC, 0x75f0 },
        {HistoriaCruxConstants.COLISEUM_DLC, 0x7670 },
        {HistoriaCruxConstants.BLANK_8, 0x76f0 },
    };

    private readonly Dictionary<string, int> ykdLinkOffsets = new Dictionary<string, int>()
    {
        {"hs_hmaa10_zz", 0x46d8},
        {"hs_gtca94_gt", 0x4758},
        {"hs_acea01_gt", 0x47d8},
        {"hs_ghaa02_gt", 0x4858},
        {"hs_spza02_ac", 0x48d8},
        {"hs_hmha01_ac", 0x4958},
        {"hs_gtca01_aa", 0x49d8},
        {"hs_aaea01_vp", 0x4a58},
        {"hs_gdza01_vp", 0x4ad8},
        {"hs_snda03_cs", 0x4bc8},
        {"hs_ghaa01_cs", 0x4c48},
        {"hs_snda01_cl", 0x4cc8},
        {"hs_snda93_sn", 0x4d48},
        {"hs_bjaa02_sn", 0x4dc8},
        {"hs_gyba01_sn", 0x4e48},
        {"hs_ddha02_bj", 0x4ec8},
        {"hs_bjaa03_bj", 0x4f48},
        {"hs_hmaa01_bj", 0x4fc8},
        {"hs_hpaa95_hm", 0x5048},
        {"hs_ddha01_hm", 0x50c8},
        {"hs_acea02_gy", 0x5148},
        {"hs_bjaa01_gy", 0x51c8},
        {"hs_bjda01_gy", 0x5248},
        {"hs_gwca01_gh", 0x52c8},
        {"hs_gwca92_gw", 0x5348},
        {"hs_gyaa01_gw", 0x53c8},
        {"hs_gtca02_gw", 0x5448},
        {"hs_gwda01_gw", 0x54c8},
        {"hs_hpaa01_dd", 0x5548},
        {"hs_ddha96_dd", 0x55c8},
        {"hs_ghaa01_ac", 0x56b8},
        {"hs_snda03_ac", 0x5738},
        {"hs_aaea02_sp", 0x57b8},
        {"hs_bjaa02_zz", 0x5838},
        {"hs_gwca01_zz", 0x58b8},
        {"hs_snda01_zz", 0x5938},
        {"hs_gtca01_zz", 0x59b8},
        {"hs_spza03_hp", 0x5a38},
        {"hs_ddha01_zz", 0x5ab8},
        {"hs_snda02_gd", 0x5b38},
        {"hs_bjaa91_gd", 0x5bb8},
        {"hs_acfa01_va", 0x5c38},
        {"hs_hmaa11_va", 0x5cb8},
        {"hs_hmaa13_cs", 0x5d38},
        {"hs_hmaa12_cl", 0x5db8},
        {"hs_acfa01_zz", 0x5e38},
    };

    private Dictionary<TreeNode, int> depthList = new();

    private void UnlinkRecursive(TreeNode node)
    {
        node.children.ForEach(child =>
        {
            UnlinkRecursive(child);
            child.parent = null;
        });
        node.children.Clear();
    }

    private bool IsLocationAllowed(string location, TreeNode parent, List<string> placed, Dictionary<string, TreeNode> nodes)
    {
        // Only allow acad 500 to be selected from blank 5 if the flag is enabled
        if (FF13_2Flags.Other.ForceAcadVoidEndgame.Enabled)
        {
            if ((parent.name == HistoriaCruxConstants.BLANK_5) != (location == HistoriaCruxConstants.ACADEMIA_500))
            {
                return false;
            }
        }
        // This need to account for shuffle groups on items and ensure the placement is convergent enough with vanilla locations in mind.
        var areaInfo = areaData[location];
        var outwardGates = gateData.Values.Where(g => g.Location == location).ToList();
        var mogLevel = GetMogLevel(placed);
        var hasGravitons = HasGravitonLocations(placed);
        var hasWild = HasWildArtefacts(placed);

        // If the location being placed is a dependency on some other location link, then it cannot be placed as a child of that link in the tree.
        // e.g. Coliseum cannot be placed as a child of Sunleth 300 -> ZZ 930
        var requirementOf = gateData.Values.Where(g => g.Requirements.Contains(location)).ToList();
        if (requirementOf.Count > 0)
        {
            foreach (var gate in requirementOf)
            {
                var gateOrigin = gate.Location;
                if (placed.Contains(gateOrigin))
                {
                    var relative = ResolveNodeDepthRelative(nodes[gateOrigin], parent);
                    if (relative != -1)
                    {
                        // TODO: this can be ok if the path uses a side chain?
                        // Step through affected nodes, review requirements on links and verify?
                        // e.g. if yaschas 1x is child of sunleth that's valid as long as the child link isn't through the dependent link (in this case void beyond)
                        Generator.Logger.LogDebug($"Location {location} is not allowed here as it must be placed before {gateOrigin}");
                        return false;
                    }
                }
            }
        }

        // TODO: other requirements need to be checked to ensure they don't cross over

        if (outwardGates.Count == 0)
        {
            return true;
        }

        foreach (var gate in outwardGates)
        {
            // TODO: handle by treasure logic now if cores are randomised?
            if (gate.Traits.Contains("Graviton") && !hasGravitons)
            {
                Generator.Logger.LogDebug($"Location {location} requires gravitons but state does not have them!");
                return false;
            }

            // TODO: handle by treasure logic now if wilds are randomised?
            if (gate.Traits.Contains("Wild") && !hasWild)
            {
                Generator.Logger.LogDebug($"Location {location} requires wild artefacts but state does not have them!");
                return false;
            }

            if (gate.MinMogLevel > mogLevel)
            {
                Generator.Logger.LogDebug($"Location {location} requires mog level {gate.MinMogLevel} but state has {mogLevel}");
                return false;
            }
            // Hard code for Bresha 5 wild artefact if key items aren't rando
            if (!FF13_2Flags.Items.Treasures.FlagEnabled || !FF13_2Flags.Items.KeySide.Enabled || TooSmallOfPool())
            {
                if (gate.ItemRequirements.GetPossibleRequirements().Contains("key_lockjail") && 2 > mogLevel)
                {
                    return false;
                }
            }
        }
        return true;
    }

    private (bool, Dictionary<int, string>, List<string>, int) TryPlaceChildrenWithPlacement(TreeNode root, int rootX, int rootY, Dictionary<int, string> placed, int incomingDir, int depth, List<string> remaining, Dictionary<string, TreeNode> allNodes, float branchFactor, int openBranchesOrig)
    {
        // Because this process is depth first its a little awkward.
        // Ideally want to allow sub-branches to grow out to a reasonable depth
        // but need to be careful about overallocating on the initial pass potentially

        // Also need to potentially special case zz nodes etc properly because right now it does not?
        // need to take into consideration fixed links appropriately
        // especially since zz nodes don't have an outgoing link count...

        var openBranches = openBranchesOrig;

        var areaInfo = areaData[root.name];
        if (areaInfo.OutgoingLinkCount == 0)
        {
            Generator.Logger.LogDebug($"Successfully placed terminal node {root.name} at (x: {rootX}, y: {rootY}, depth: {depth}). Placed {placed.Count} Open branches {openBranches}");
            return (true, placed, remaining, openBranchesOrig - 1);
        }
        else
        {
            Generator.Logger.LogDebug($"Starting placement of node {root.name} at (x: {rootX}, y: {rootY}, depth: {depth}) with {areaInfo.OutgoingLinkCount} outgoing links. Placed {placed.Count} Open branches {openBranches}");
            openBranches += areaInfo.OutgoingLinkCount - 1;
            if (depth == 0 && FF13_2Flags.Other.RandoDLC.Enabled)
            {
                // Extra branch if randomising dlc is on
                openBranches++;
            }
        }

        if (root.children.Count != 0 && depth > 0)
        {
            // Should never happen, placement should always be done by this algorithm.
            throw new Exception("Node already has children set!");
        }

        var newPlacement = new Dictionary<int, string>(placed);

        var newRemaining = new List<string>(remaining);

        Func<string, long> weightFunc = o =>
        {
            // Bias the choice based on fixed battle rank, or number of placed locations if the location doesn't have a fixed rank
            // Also bias areas with higher outgoing links earlier
            var g = 0;
            if (areaData.ContainsKey(o))
            {
                var odata = areaData[o];
                // Prefer areas with multiple outputs, with diminsihing returns as the graph widens out
                var depthMod = 12 * branchFactor;
                if (areaInfo.OutgoingLinkCount == 1)
                {
                    // Prefer high branch factor after a straight line
                    g = (int)(odata.OutgoingLinkCount * depthMod);
                }
                else
                {
                    // Prefer low branch factor after a branch
                    g = (int)(Math.Sign(odata.OutgoingLinkCount) * (3 - odata.OutgoingLinkCount) * depthMod);
                }
            }
            return g + 1;
        };

        // Pick outgoing link count nodes from remaining with weighted shuffle here instead.
        var orderedPreference = new List<string>(root.children.Select(n => n.name));
        // Hack for depth 0
        root.children.Clear();

        orderedPreference.AddRange(areaInfo.FixedLinks);
        newRemaining.RemoveAll(areaInfo.FixedLinks.Contains);

        //if(root.name == HistoriaCruxConstants.SUNLETH_300)
        //{
        //    orderedPreference.Add(HistoriaCruxConstants.VOID_BEYOND_A);
        //    newRemaining.Remove(HistoriaCruxConstants.VOID_BEYOND_A);
        //}
        //else if (root.name == HistoriaCruxConstants.YASCHAS_1X)
        //{
        //    orderedPreference.Add(HistoriaCruxConstants.SERENDIPITY);
        //    newRemaining.Remove(HistoriaCruxConstants.SERENDIPITY);
        //}

        // Place end game before the chain gets too crazy
        if (depth >= 8 && newRemaining.Contains(HistoriaCruxConstants.ACADEMIA_4XX))
        {
            orderedPreference.Add(HistoriaCruxConstants.ACADEMIA_4XX);
            newRemaining.Remove(HistoriaCruxConstants.ACADEMIA_4XX);
        }

        var remainingChoices = new List<string>(newRemaining.Where(a => !areaData[a].Traits.Contains("FixedInbound")));

        var locationToPlace = areaInfo.OutgoingLinkCount;

        // Ensure both 970 and the "normal" outgoing links are placed if rando dlc is enabled.
        if (depth == 0 && FF13_2Flags.Other.RandoDLC.Enabled)
        {
            locationToPlace++;
        }

        if (orderedPreference.Count + remainingChoices.Count < locationToPlace)
        {
            Generator.Logger.LogDebug($"Ran out of placeable locations as children for node {root.name} at depth {depth}. placed {placed.Count}, open branches {openBranches} preference {string.Join(",", orderedPreference)} outgoing {areaInfo.OutgoingLinkCount} remaining {string.Join(",", newRemaining)}");
            return (false, placed, remaining, openBranchesOrig);
        }

        var remainingTerminal = remainingChoices.Where(a => areaData[a].OutgoingLinkCount == 0).ToList();
        var remainingNonTerminalCount = remainingChoices.Count - remainingTerminal.Count;

        var removalReasons = new Dictionary<string, string>();

        while (orderedPreference.Count < locationToPlace && remainingChoices.Count > 0)
        {
            // TODO: may need to fiddle with the bias to kill off links?
            // Because this is depth first its very likely to end up trying to place a long line
            // The weighting helps, but might also want to hard kill off chains at some points to force it into line?
            var selection = RandomNum.SelectRandomWeighted(remainingChoices, weightFunc);
            var selectionData = areaData[selection];
            if (selectionData.Traits.Contains("FixedInbound"))
            {
                removalReasons.Add(selection, "FixedInbound");
                remainingChoices.Remove(selection);
                continue;
            }

            if (!IsLocationAllowed(selection, root, placed.Values.Concat(orderedPreference).ToList(), allNodes))
            {
                removalReasons.Add(selection, "NotAllowed");
                remainingChoices.Remove(selection);
                continue;
            }

            // Prevent placement of terminal nodes until all non-terminal nodes placed?
            // Will this lead to overlong chains? probably?

            // TODO: force outgoing placement from void beyond B to be acad 500/endgame?

            // if a terminal node is rolled and we have enough non-terminal nodes to place otherwise
            if (selectionData.OutgoingLinkCount == 0)
            {
                if (selection == HistoriaCruxConstants.AUGUSTA_300 || selection == HistoriaCruxConstants.COLISEUM)
                {
                    // Just allow these to be placed to ensure augusta 200/sunleth 300 respectively don't get locked out
                }
                else if (remainingNonTerminalCount - orderedPreference.Count > locationToPlace)
                {
                    // If we haven't opened up enough branches, don't allow it to be picked
                    if (openBranches < remainingTerminal.Count - 2)
                    {
                        removalReasons.Add(selection, $"Open branches {openBranches} remainingTerminal {remainingTerminal.Count}");
                        remainingChoices.Remove(selection);
                        continue;
                    }
                    // If there are more nodes to place than branches, remove empty nodes to keep placement going.
                    else if (remainingNonTerminalCount > openBranches)
                    {
                        removalReasons.Add(selection, $"Open branches {openBranches} remainingNonTerminal {remainingNonTerminalCount}");
                        remainingChoices.Remove(selection);
                        continue;
                    }
                }
            }

            if (depth < 3 && placed.Count < 8 && selection == HistoriaCruxConstants.ACADEMIA_400)
            {
                removalReasons.Add(selection, $"Acad 400 check, depth {depth} placed {placed.Count}");
                remainingChoices.Remove(selection);
                continue;
            }
            if (depth < 4 && placed.Count < 12 && selection == HistoriaCruxConstants.AUGUSTA_200)
            {
                removalReasons.Add(selection, $"Augusta 200 check, depth {depth} placed {placed.Count}");
                remainingChoices.Remove(selection);
                continue;
            }
            orderedPreference.Add(selection);
            remainingChoices.Remove(selection);
            newRemaining.Remove(selection);
        }

        // Prefer placing the higher amount of follow on links to the right
        orderedPreference = orderedPreference.OrderByDescending(s =>
        {
            // Prioritise placing path to endgame to the right
            if (s == HistoriaCruxConstants.ACADEMIA_4XX || s == HistoriaCruxConstants.DYING_WORLD_700 || s == HistoriaCruxConstants.SERENDIPITY || s == HistoriaCruxConstants.VOID_BEYOND_A)
            {
                return 5;
            }
            return areaData[s].OutgoingLinkCount;
        }).ToList();

        if (orderedPreference.Count < locationToPlace)
        {
            Generator.Logger.LogDebug($"Unable to resolve sufficient allowed locations to be children of {root.name} at depth {depth}. placed {placed.Count}. Resolved allowed {string.Join(",", orderedPreference)} but needed {locationToPlace} (available {newRemaining.Count})");
            Generator.Logger.LogDebug($"Reasons: {string.Join(", ", removalReasons.Select(kvp => kvp.Key + ":" + kvp.Value))}");
            return (false, placed, remaining, openBranchesOrig);
        }

        if (orderedPreference.Count > orderedPreference.Distinct().Count())
        {
            throw new Exception("Collision in ordered preference - what did you do wrong!");
        }

        // check how many adjacent locations are open and ensure there's enough before going much further?
        // should avoid some bouncing around with unecessary work

        // Directional preference maybe should try and place critical path to valhalla more to the right?
        // Potentially invert this to prioritise nodes torwards longest chain endpoint as rightmost first?
        // order children by deepest child to stretch out long chains maybe
        List<int> usedDirs = new List<int>() { 5 - incomingDir };
        var usedUp = false;
        var usedDown = false;
        var usedRight = false;
        var usedLeft = false;
        // Stop ambiguous vertical placements
        if (incomingDir == 4 || incomingDir == 2)
        {
            usedUp = true;
        }
        else if (incomingDir == 3 || incomingDir == 1)
        {
            usedDown = true;
        }
        else if (incomingDir == 5)
        {
            usedRight = true;
        }
        else if (incomingDir == 0)
        {
            usedLeft = true;
        }
        // When on the main branch, pick random Y dir if there's multiple children to place (removed for ease of placement for now...)
        //Bounds checking
        if (rootX >= 18)
        {
            usedRight = true;
        }
        if (rootY < 4)
        {
            usedUp = true;
        }
        if (rootY > 8)
        {
            usedDown = true;
        }
        if (rootX < 2)
        {
            usedLeft = true;
        }
        // TODO: improve directional preference based on context
        // Introduce a "gravity" back towards constrained y value maybe, double check usedUp/Down flags are being set right (might be inverted currently)
        for (var direction = 0; direction < 6; direction++)
        {
            var trueDir = direction;
            var attemptX = rootX;
            var attemptY = rootY;
            var preferredY = 1;
            // Don't allow placement on the incoming direction
            if (trueDir == 5 - incomingDir)
            {
                continue;
            }
            if (usedDown && (trueDir == 4 || trueDir == 2))
            {
                continue;
            }
            if (usedUp && (trueDir == 1 || trueDir == 3))
            {
                continue;
            }
            if (usedRight && trueDir == 0)
            {
                continue;
            }
            if (usedLeft && (trueDir == 5 || trueDir == 3))
            {
                continue;
            }
            if (trueDir == 0)
            {
                attemptX++;
            }
            else if (trueDir == 1)
            {
                attemptY -= preferredY;
            }
            else if (trueDir == 2)
            {
                attemptX++;
                attemptY += preferredY;
            }
            else if (trueDir == 3)
            {
                attemptX--;
                attemptY -= preferredY;
            }
            else if (trueDir == 4)
            {
                attemptY += preferredY;
            }
            else if (trueDir == 5)
            {
                attemptX--;
            }
            var activeChildId = CoordsToId(attemptX, attemptY);
            if (newPlacement.ContainsKey(activeChildId))
            {
                // Don't allow overwriting
                continue;
            }

            // Pick child node from weighted shuffle of outstanding

            var activeChildArea = orderedPreference[root.children.Count];
            // Where do the nodes come from here - pass in?
            // also need to build links between nodes as part of placement.
            TreeNode activeChild = allNodes[activeChildArea];
            if (root.name == HistoriaCruxConstants.NEW_BODHUM_3X && activeChild.name == HistoriaCruxConstants.BLANK_5)
            {
                root.children.Add(activeChild);
                // New Bodhum 3xx is not a real location, so use its slot for the empty node it generates afterwards.
                var parentId = CoordsToId(rootX, rootY);
                newPlacement.Remove(parentId);
                newPlacement[parentId] = activeChild.name;
                activeChild.parent = root;
                var placementResult = TryPlaceChildrenWithPlacement(activeChild, rootX, rootY, newPlacement, trueDir, depth + 1, newRemaining, allNodes, branchFactor, openBranches);
                if (placementResult.Item1)
                {
                    return placementResult;
                }
                else
                {
                    activeChild.parent = null;
                    root.children.Remove(activeChild);
                    return placementResult;
                }
            }
            var possibleMatch = newPlacement.Values.Where(n => n == activeChild.name);
            if (possibleMatch.Count() > 0)
            {
                if (activeChild.name == HistoriaCruxConstants.SERENDIPITY || activeChild.name == HistoriaCruxConstants.VOID_BEYOND_A)
                {
                    // Double links for serendipity/void beyond
                    // place the child and move on. (is this enough?)
                    root.children.Add(activeChild);
                    activeChild.parent = root;
                    if (root.children.Count == locationToPlace)
                    {
                        break;
                    }
                    continue;
                }
                // Node already placed - this should never happen now.
                throw new Exception($"Node {activeChild.name} has already been placed in placement, skipping. Would have been child of {root.name}");
            }
            newPlacement[activeChildId] = activeChild.name;
            root.children.Add(activeChild);
            activeChild.parent = root;
            // TODO: this is effectively depth first.
            // For nodes with high outgoing links, should reserve adjacent locations to ensure placement always works as expected
            // Otherwise its common to run out of space when generating from a high choice root
            var (success, added, remainingFromChild, nowOpen) = TryPlaceChildrenWithPlacement(activeChild, attemptX, attemptY, newPlacement, trueDir, depth + 1, newRemaining, allNodes, branchFactor / (float)locationToPlace, openBranches);
            if (!success)
            {
                activeChild.parent = null;
                newPlacement.Remove(activeChildId);
                root.children.Remove(activeChild);
            }
            else
            {
                newPlacement = added;
                newRemaining = remainingFromChild;
                openBranches = nowOpen;
                usedDirs.Add(trueDir);
                if (root.children.Count == locationToPlace)
                {
                    break;
                }
                if (trueDir == 4 || trueDir == 2)
                {
                    usedDown = true;
                }
                else if (trueDir == 3 || trueDir == 1)
                {
                    usedUp = true;
                }
                else if (trueDir == 0)
                {
                    usedRight = true;
                }
                else if (trueDir == 5)
                {
                    usedLeft = true;
                }
                // Special case these as they technically have 4 outgoing links so things are going to get weird no matter what probably...
                //if (root.name == HistoriaCruxConstants.SUNLETH_300 || root.name == HistoriaCruxConstants.YASCHAS_1X)
                //{
                //    usedUp = false;
                //    usedDown = false;
                //    usedRight = false;
                //    usedLeft = false;
                //}
                // Restart the loop to check all adjacencies properly
                direction = -1;
            }
        }
        if (root.children.Count < locationToPlace)
        {
            Generator.Logger.LogDebug($"Unable to place children of {root.name} (x: {rootX}, y: {rootY}). Placed {root.children.Count} of {locationToPlace} at depth {depth}. placed {placed.Count} - unwound {newPlacement.Count - placed.Count}. Selected {string.Join(",", orderedPreference)}. Used directions {string.Join(",", usedDirs)}");
            // Unlink all children for re-placement
            UnlinkRecursive(root);
            // Potentially need to introduce an offset for an empty node and try again as long as we have some to work with
            return (false, placed, remaining, openBranchesOrig);
        }
        if (depth == 0)
        {
            Generator.Logger.LogDebug($"Unplaced: {string.Join(",", newRemaining)}, openBranches: {openBranches}");
            Generator.Logger.LogDebug($"Node stats: {string.Join(", ", allNodes.Values.Select(node => $"{node.name} - children: {node.children.Count} - expected: {areaData[node.name].OutgoingLinkCount}"))}");
        }
        // Order placement for gatetable allocation
        // Fixed links go first then everything else is kind of whatever tbh
        // Maybe look at further adjustments here?
        // Broken?
        root.children = root.children.OrderByDescending(n =>
        {
            var s = n.name;
            // Fixed links top of the list
            if (areaInfo.FixedLinks.Contains(s))
            {
                return 10 - areaInfo.FixedLinks.IndexOf(s);
            }
            // Prioritise placing path to endgame
            if (s == HistoriaCruxConstants.ACADEMIA_4XX || s == HistoriaCruxConstants.DYING_WORLD_700 || s == HistoriaCruxConstants.SERENDIPITY || s == HistoriaCruxConstants.VOID_BEYOND_A)
            {
                return 5;
            }
            return areaData[s].OutgoingLinkCount;
        }).ToList();
        Generator.Logger.LogDebug($"Finalised placement of {root.name} at (x: {rootX}, y: {rootY}, depth: {depth}) with {locationToPlace} children ({string.Join(",", root.children.Select(s => s.name))}). Placed {newPlacement.Count} open branches {openBranches}");
        return (true, newPlacement, newRemaining, openBranches);
    }

    int shuffleFailures = 0;

    public List<string> GetIDsForOpening(string open, bool orig = true)
    {
        return gateData.Keys.Where(id => (orig ? gateTableOrig[id] : gateTable[id]).sOpenHistoria1.StartsWith(open)).ToList();
    }

    public int GetMogLevel(List<string> available)
    {
        if (!FF13_2Flags.Items.Treasures.FlagEnabled || TooSmallOfPool())
        {
            return available.Contains(HistoriaCruxConstants.ACADEMIA_4XX) && HasGravitonLocations(available) ? 3 : available.Contains(HistoriaCruxConstants.SUNLETH_300) ? 2 : available.Contains(HistoriaCruxConstants.BRESHA_RUINS_5) ? 1 : 0;
        }
        else
        {
            return 3;
        }
    }

    private bool HasGravitonLocations(List<string> available)
    {
        if (!FF13_2Flags.Items.Treasures.FlagEnabled || !FF13_2Flags.Items.KeyGraviton.Enabled || TooSmallOfPool())
        {
            // If graviton cores aren't rando, use normal logic
            List<string> gravitons = new();
            if (GetMogLevel(available) >= 1)
            {
                gravitons.Add(HistoriaCruxConstants.NEW_BODHUM_3); // requires moogle hunt
            }

            if (GetMogLevel(available) >= 1)
            {
                gravitons.Add(HistoriaCruxConstants.BRESHA_RUINS_5); // requires moogle hunt
            }

            if (GetMogLevel(available) >= 1)
            {
                gravitons.Add(HistoriaCruxConstants.OERBA_200); // requires moogle hunt
            }

            if (GetMogLevel(available) >= 1)
            {
                gravitons.Add(HistoriaCruxConstants.ACADEMIA_400); // requires moogle hunt
            }

            if (GetMogLevel(available) >= 1)
            {
                gravitons.Add(HistoriaCruxConstants.YASCHAS_100); // requires moogle hunt
            }

            if (GetMogLevel(available) >= 1)
            {
                gravitons.Add(HistoriaCruxConstants.OERBA_400); // requires moogle hunt
            }

            if (GetMogLevel(available) >= 1)
            {
                gravitons.Add(HistoriaCruxConstants.SUNLETH_400); // requires moogle hunt
            }

            if (available.Intersect(gravitons).Count() < 5)
            {
                return false;
            }
        }

        return true;
    }

    private bool HasWildArtefacts(List<string> available)
    {
        if (!FF13_2Flags.Items.Treasures.FlagEnabled || !FF13_2Flags.Items.KeyWild.Enabled || TooSmallOfPool())
        {
            // If wild artefacts aren't rando, use normal logic
            List<string> wilds = new();
            if (GetMogLevel(available) >= 1)
            {
                wilds.Add(HistoriaCruxConstants.BRESHA_RUINS_5); // requires moogle hunt
            }

            if (GetMogLevel(available) >= 1)
            {
                wilds.Add(HistoriaCruxConstants.BRESHA_RUINS_300); // Bresha 300. requires moogle hunt
            }

            if (GetMogLevel(available) >= 2)
            {
                wilds.Add(HistoriaCruxConstants.OERBA_200); // Oerba 200. requires moogle throw
            }

            if (GetMogLevel(available) >= 2)
            {
                wilds.Add(HistoriaCruxConstants.SUNLETH_300); // Sunleth 300. requires moogle throw
            }

            if (GetMogLevel(available) >= 2)
            {
                wilds.Add(HistoriaCruxConstants.ARCHYLTE); // Archylte. requires moogle throw
            }

            wilds.Add(HistoriaCruxConstants.AUGUSTA_200); // Augusta 200
            if (GetMogLevel(available) >= 1)
            {
                wilds.Add(HistoriaCruxConstants.ACADEMIA_4XX); // Academia 4XX. requires moogle hunt
            }

            if (GetMogLevel(available) >= 2)
            {
                wilds.Add(HistoriaCruxConstants.YASCHAS_100); // Yaschas 100. requires moogle hunt and throw
            }

            if (GetMogLevel(available) >= 1)
            {
                wilds.Add(HistoriaCruxConstants.DYING_WORLD_700); // Dying World 700. requires moogle hunt
            }

            if (available.Contains(HistoriaCruxConstants.YASCHAS_1X))
            {
                wilds.Add(HistoriaCruxConstants.SERENDIPITY); // Serendipity. requires completing Yaschas 1X
            }

            int wildsNeeded = GetWildsNeeded(available);

            if (available.Intersect(wilds).Count() < wildsNeeded)
            {
                return false;
            }
        }

        return true;
    }

    public int GetWildsNeeded(List<string> available)
    {
        return available.SelectMany(l =>
        gateData.Values.Where(g =>
          g.Location == l &&
          g.Traits.Contains("Wild") &&
          g.Requirements.Intersect(available).Count() == g.Requirements.Count &&
          GetMogLevel(available) >= g.MinMogLevel)
        ).Count();
    }

    private bool TooSmallOfPool()
    {
        if (FF13_2Flags.Items.KeyPlaceTreasure.Enabled)
        {
            return false; // There's many treasures
        }

        int size = 0;
        if (FF13_2Flags.Items.KeyWild.Enabled)
        {
            size++;
        }

        if (FF13_2Flags.Items.KeyGraviton.Enabled)
        {
            size++;
        }

        if (FF13_2Flags.Items.KeySide.Enabled)
        {
            size++;
        }

        if (FF13_2Flags.Items.KeyGateSeal.Enabled)
        {
            size++;
        }

        if (FF13_2Flags.Items.KeyArtefact.Enabled)
        {
            size++;
        }

        if (FF13_2Flags.Items.KeyParadox.Enabled)
        {
            size++;
        }

        if (FF13_2Flags.Items.KeyFragment.Enabled)
        {
            size += 2;
        }
        // Academia 4XX can softlock without Brain Blast
        if (FF13_2Flags.Items.KeyPlaceBrainBlast.Enabled)
        {
            size++;
        }

        return size < 5;
    }

    public override Dictionary<string, HTMLPage> GetDocumentation()
    {
        Dictionary<string, HTMLPage> pages = base.GetDocumentation();
        HTMLPage page = new("Historia Crux", "template/documentation.html");

        BattleRando battleRando = Generator.Get<BattleRando>();

        Dictionary<string, int> diffs = battleRando.GetAreaDifficulties();

        page.HTMLElements.Add(new Table("", (new string[] { "Original Gate", "New Location", "Estimated Battle Difficulty of New Location", "Location depth" }).ToList(), (new int[] { 30, 30, 20, 20 }).ToList(),
            gateData.Values.Where(g => !g.Traits.Contains("Paradox")).Select(g =>
          {
              string id = gateTable[g.ID].sOpenHistoria1;
              string shortID = id.Substring(0, id.Length - 2);
              var depth = areaDepths != null && areaDepths.ContainsKey(shortID) ? areaDepths[shortID].ToString() : "Unchanged";
              return (new string[] { g.GateOriginal, areaData[shortID].Name, diffs.ContainsKey(shortID) ? diffs[shortID].ToString() : "-", depth }).ToList();
          }).ToList()));

        if (FF13_2Flags.Other.HistoriaCrux.FlagEnabled)
        {
            // Mostly here for debug for now, may replace with a graphical view at some point?
            page.HTMLElements.Add(new Table("grid", (new string[] { "X", "Y", "Node" }).ToList(), (new int[] { 10, 10, 80 }).ToList(),
                coordMap.Select(kvp =>
                {
                    var (x, y) = MapCoordsToHexGrid(IdToCoords(kvp.Key));
                    return (new string[] { x.ToString(), y.ToString(), kvp.Value }).ToList();
                }).ToList()));
        }

        pages.Add("historia_crux", page);
        return pages;
    }

    public override void Save()
    {
        RandoUI.SetUIProgressIndeterminate("Saving Historia Crux Data...");
        gateTable.SaveWDB(Generator, @"\db\resident\_wdbpack.bin\r_gatetab.wdb");
        SetupData.WPDTracking[Generator.DataOutFolder + @"\db\resident\wdbpack.bin"].Add("r_gatetab.wdb");

        FF13_2RandoExtensions.SaveFile(Generator, @"\gui\resident\_system.win32.xgr\gr_hc_parts.ykd", hcParts);
        Nova.RepackWPD(Generator.DataOutFolder + @"\gui\resident\system.win32.xgr",
            SetupData.Paths["Nova"]);
    }
}
