using Bartz24.Data;
using Bartz24.Docs;
using Bartz24.FF13_2;
using Bartz24.FF13_2_LR;
using Bartz24.FF13Series;
using Bartz24.RandoWPF;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Extensions.Logging;
using SharpCompress.Common;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Xps.Packaging;

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

    public HistoriaCruxRando(SeedGenerator randomizers) : base(randomizers) { }

    public override void Load()
    {
        RandoUI.SetUIProgressIndeterminate("Loading Historia Crux Data...");
        // Unpack gate table
        gateTable.LoadDB3(Generator, "13-2", @"\db\resident\_wdbpack.bin\r_gatetab.wdb", false);
        gateTableOrig.LoadDB3(Generator, "13-2", @"\db\resident\_wdbpack.bin\r_gatetab.wdb", false);

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

            List<string> openings = gateData.Keys
                .Where(id => !gateData[id].Traits.Contains("Paradox"))
                .Where(id => !gateData[id].Traits.Contains("Fixed"))
                .Where(id => FF13_2Flags.Other.RandoDLC.Enabled || !gateData[id].Traits.Contains("DLC"))
                .Select(id => gateTable[id].sOpenHistoria1)
                .Select(s => s.Substring(0, s.Length - 2))
                .Distinct().ToList();

            // TODO: figure out a proper way to allow true random starting location
            // Existing algorithm seems to either eat nodes, or create strange loops
            // Need to be able to unravel at the end to set hmaa_def link properly.

            if (FF13_2Flags.Other.ForcedStart.Values.IndexOf(FF13_2Flags.Other.ForcedStart.SelectedValue) > 0)
            {
                openings = openings.Where(o => o != HistoriaCruxConstants.NEW_BODHUM_3).ToList();
            }

            if (FF13_2Flags.Other.ForcedStart.Values.IndexOf(FF13_2Flags.Other.ForcedStart.SelectedValue) > 1)
            {
                openings = openings.Where(o => o != HistoriaCruxConstants.BRESHA_RUINS_5).ToList();
            }

            if(FF13_2Flags.Other.ForcedStart.Values.IndexOf(FF13_2Flags.Other.ForcedStart.SelectedValue) == 0)
            {
                string msg = "No fixed starting location set. This setting is highly unstable and will crash or fail generation if" +
                    "a stable tree is not generated to avoid rolling unbeatable seeds from an item logic perspective.\n" +
                    "You have been warned :)";
                Generator.Logger.LogError(msg);
                MessageBox.Show(msg);
            }

            placement = GetPlacement(new Dictionary<string, string>(), openings, 0).Item2;

            placement.Keys.ToList().ForEach(open =>
            {
                Generator.Logger.LogDebug($"Location {open} placed at {placement[open]}");
                gateTable.Keys.Where(id => gateTableOrig[id].sOpenHistoria1.StartsWith(open)).ToList().ForEach(id => gateTable[id].sOpenHistoria1 = placement[open] + "_a");
            });

            if (placement.ContainsKey("h_hm_AD0003"))
            {
                gateTable["hs_hmaa10_zz"].sArea = placement["h_hm_AD0003"];
            }

            BuildInitialGateTree();
            // Generate updated gate matrix based on shuffled links
            // TODO: may need to track this from earlier in the process to add in dead nodes

            RandomNum.ClearRand();
        }
    }

    public class TreeNode
    {
        public string name;
        public List<TreeNode> children = new();
        public TreeNode parent;
    }

    private int ResolveNodeDepth(TreeNode node)
    {
        int depth = 0;
        TreeNode curr = node;
        var seen = new List<TreeNode>() { node };
        while(curr.parent != null)
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
            if(curr.parent == null)
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

    private (int,int) IdToCoords(int c)
    {
        return (c / 31, c % 31);
    }

    private(int,int) MapCoordsToHexGrid((int, int)xy)
    {
        var (x, y) = xy;
        return ((x - 3) * 30 - 15 * (y-6), (5-y)*15);
    }

    private void BuildInitialGateTree()
    {
        try
        {
            // Build tree structure from gate table
            var nodes = new Dictionary<string, TreeNode>();
            var locations = gateTable.Values.Select(g => g.sArea).Concat(gateTable.Values.Select(g => g.sOpenHistoria1.Substring(0, g.sOpenHistoria1.Length - 2))).Distinct().ToList();
            // Setup nodes
            foreach (var area in locations)
            {
                var emptyNode = new TreeNode();
                emptyNode.name = area;
                nodes.Add(area, emptyNode);
            }
            // Link nodes
            foreach (var node in nodes)
            {
                var children = gateTable.Values.Where(g => g.sArea == node.Value.name).Select(g => g.sOpenHistoria1.Substring(0, g.sOpenHistoria1.Length - 2)).ToList();
                foreach (var child in children)
                {
                    if (child == node.Key)
                    {
                        // Don't allow self-links
                        continue;
                    }
                    var childNode = nodes[child];
                    childNode.parent = node.Value;
                    node.Value.children.Add(childNode);
                }
            }
            // Calculate depth to find root(s) of area tree
            // Also locate endgame and its depth
            var roots = new List<TreeNode>();
            var valhallaDepth = -1;
            foreach (var node in nodes)
            {
                var depth = ResolveNodeDepth(node.Value);
                if (depth == 0)
                {
                    roots.Add(node.Value);
                }
                if (node.Key == HistoriaCruxConstants.VALHALLA_FINAL)
                {
                    valhallaDepth = depth;
                }
                depthList[node.Value] = depth;
            }
            shuffledNodes = new(nodes);
            if (roots.Count > 1)
            {
                Generator.Logger.LogDebug("Multiple roots detected in tree!");
                Generator.Logger.LogDebug(string.Join(", ", depthList.Select(kvp => $"{kvp.Key.name} - {kvp.Value}")));

                // TODO: handle split root scenario? Impossible?
                return;
            }
            else if (roots.Count == 0)
            {
                Generator.Logger.LogDebug("No roots detected in tree!");
                Generator.Logger.LogDebug(string.Join(", ", depthList.Select(kvp => $"{kvp.Key.name} - {kvp.Value}")));
                return;
            }
            areaDepths = depthList.ToDictionary(kvp => kvp.Key.name, kvp => kvp.Value);
            foreach (var node in roots)
            {
                rootLocation = node.name;
                Generator.Logger.LogDebug($"Initial area which is root of crux tree: {rootLocation}");
                if(overrideInitial != null && overrideInitial != rootLocation)
                {
                    Generator.Logger.LogDebug($"Expected initial to be {overrideInitial} but was {rootLocation} ");
                }
                // TODO:
                // Improve y-axis distribution to ensure we keep in range
                // Move dead location coords into box for clarity
                // Run through a seed to validate link correcness/navigation works as expected
                var initialY = 6;
                var rootid = CoordsToId(2, initialY);
                coordMap[rootid] = node.name;
                // DLC placement left of initial root node
                var dlcBlockId = CoordsToId(1, initialY);
                coordMap[dlcBlockId] = HistoriaCruxConstants.BLANK_7;
                var dlcUpperId = CoordsToId(0, initialY - 1);
                coordMap[dlcUpperId] = HistoriaCruxConstants.VALHALLA_DLC;
                var dlcLowerId = CoordsToId(1, initialY + 1);
                coordMap[dlcLowerId] = HistoriaCruxConstants.SERENDIPITY_DLC;
                var dlcLeftId = CoordsToId(0, initialY);
                coordMap[dlcLeftId] = HistoriaCruxConstants.COLISEUM_DLC;
                var (success, added) = TryPlaceChildren(node, 2, initialY, coordMap, 0);
                if (!success)
                {
                    Generator.Logger.LogDebug("Failed to place crux locations!");
                    foreach (var entry in nodes.Values)
                    {
                        Generator.Logger.LogDebug($"Node {entry.name} with children {string.Join(",", entry.children.Select(n => n.name).ToArray())}");
                    }
                    break;
                }
                coordMap.Clear();
                foreach (var entry in added)
                {
                    coordMap.Add(entry.Key, entry.Value);
                }
            }
            var updatedLocations = coordMap.ToDictionary(kvp => kvp.Value, kvp => {
                var baseCoords = MapCoordsToHexGrid(IdToCoords(kvp.Key));
                if (kvp.Value.Contains("_zz_") || kvp.Value.Contains("_sp_"))
                {
                    // blank items are offset by 5 in each direction
                    // Void beyond nodes also do this as they show as blank in the main matrix
                    return (baseCoords.Item1 + 5, baseCoords.Item2 + 5);
                }
                return baseCoords;
                });
            foreach(var (key, offset) in ykdGateOffsets)
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
            foreach(var link in gateTable.Keys.OrderBy(s=>s, StringComparer.Ordinal))
            {

                var linkDetails = gateTable[link];
                var left = linkDetails.sArea;
                var right = linkDetails.sOpenHistoria1.Substring(0, linkDetails.sOpenHistoria1.Length - 2);
                // Bodhum 3xx is a fake area, for link purposes just skip through to the next point
                if(right == HistoriaCruxConstants.NEW_BODHUM_3X)
                {
                    right = HistoriaCruxConstants.BLANK_5;
                }
                if(!updatedLocations.ContainsKey(left) || !updatedLocations.ContainsKey(right))
                {
                    Generator.Logger.LogDebug($"Unable to link coords at either end of link {link}");
                    continue;
                }

                // TODO: special case if override initial to ensure initial link gets set properly
                if (rootLocation != null && right == HistoriaCruxConstants.NEW_BODHUM_3)
                {
                    right = rootLocation;
                } else if (rootLocation != null && right == rootLocation)
                {
                    right = HistoriaCruxConstants.NEW_BODHUM_3;
                }

                    // TODO: special case for "magic" links to void beyond/serendipity, need to consider left also
                    DataStoreRGateTable incomingLink;
                if(right != HistoriaCruxConstants.SERENDIPITY && right != HistoriaCruxConstants.VOID_BEYOND_A)
                {
                    incomingLink = gateTableOrig.Values.Find(v => v.sOpenHistoria1 == right + "_a");
                } else
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
                if(left.Contains("_sp_") || left.Contains("_zz_"))
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
                if( mode== 1)
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
                    } else if (leftPos.Item1 < rightPos.Item1)
                    {
                        if (leftPos.Item2 > rightPos.Item2)
                        {
                            angle = -pi4;
                        } else
                        {
                            angle = pi4;
                        }
                    } else
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
        } catch (Exception e)
        {
            Generator.Logger.LogError("Error when processing!");
            Generator.Logger.LogError(e.Message);
            Generator.Logger.LogError(e.StackTrace);
        }
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

    private (bool, Dictionary<int, string>) TryPlaceChildren(TreeNode root, int rootX, int rootY, Dictionary<int, string> placed, int incomingDir)
    {
        if (root.children.Count == 0)
        {
            return (true, placed);
        }
        var placedChildren = 0;
        var newPlacement = new Dictionary<int, string>(placed);
        var childrenMaxDepth = root.children.Select(node =>
        {
            var childenOfNode = FlattenChildrenFromNode(node);
            return (node, childenOfNode.Select(c => depthList[c]).Max());
        }).ToDictionary();
        var orderedPreference = childrenMaxDepth.OrderByDescending(kv => kv.Value).Select(kv => kv.Key).ToArray();
        // Directional preference maybe should try and place critical path to valhalla more to the right?
        // Potentially invert this to prioritise nodes torwards longest chain endpoint as rightmost first?
        // order children by deepest child to stretch out long chains maybe
        var usedUp = false;
        var usedDown = false;
        var usedRight = false;
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
        // Special case these as they technically have 4 outgoing links so things are going to get weird no matter what probably...
        if (root.name == HistoriaCruxConstants.SUNLETH_300 || root.name == HistoriaCruxConstants.YASCHAS_1X)
        {
            // TODO: ideally want the serendipity and void beyond links to share an up/down direction in this case.
            usedUp = false;
            usedDown = false;
            usedRight = false;
        }
        // When on the main branch, pick random Y dir if there's multiple children to place
        int yBias = 0;
        if (incomingDir == 0 && rootY == 6 && root.children.Count == 2)
        {
            yBias = new List<int>() { -1, 1 }.Shuffle()[0];
            if (yBias == 1)
            {
                usedUp = true;
            }
            else if (yBias == -1)
            {
                usedDown = true;
            }
        }
        //Bounds checking
        if(rootX >= 18)
        {
            usedRight = true;
        }
        if (rootY < 0)
        {
            usedUp = true;
        }
        if (rootY > 7)
        {
            usedDown = true;
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
            if(usedRight && trueDir < 3)
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


            var activeChild = orderedPreference[placedChildren];
            if (root.name == HistoriaCruxConstants.NEW_BODHUM_3X && activeChild.name == HistoriaCruxConstants.BLANK_5)
            {
                // New Bodhum 3xx is not a real location, so use its slot for the empty node it generates afterwards.
                placedChildren++;
                var parentId = CoordsToId(rootX, rootY);
                newPlacement.Remove(parentId);
                newPlacement[parentId] = activeChild.name;
                return TryPlaceChildren(activeChild, rootX, rootY, newPlacement, trueDir);
            }
            var possibleMatch = newPlacement.Values.Where(n => n == activeChild.name);
            if (possibleMatch.Count() > 0)
            {
                // Node is already placed
                placedChildren++;
                if (placedChildren == root.children.Count)
                {
                    break;
                }
                continue;
            }
            newPlacement[activeChildId] = activeChild.name;
            var (success, added) = TryPlaceChildren(activeChild, attemptX, attemptY, newPlacement, trueDir);
            if (!success)
            {
                newPlacement.Remove(activeChildId);
            }
            else
            {
                placedChildren++;
                newPlacement = added;
                if (placedChildren == root.children.Count)
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
                // Special case these as they technically have 4 outgoing links so things are going to get weird no matter what probably...
                if (root.name == HistoriaCruxConstants.SUNLETH_300 || root.name == HistoriaCruxConstants.YASCHAS_1X)
                {
                    usedUp = false;
                    usedDown = false;
                }
                // Restart the loop to check all adjacencies properly
                direction = -1;
            }
        }
        if(placedChildren< root.children.Count)
        {
            Generator.Logger.LogDebug($"Unable to place children of root node {root.name} (x: {rootX}, y: {rootY})");
            // Potentially need to introduce an offset for an empty node and try again as long as we have some to work with
            return (false, null);
        }
        return (true, newPlacement);
    }

    int shuffleFailures = 0;

    private (bool, Dictionary<string, string>) GetPlacement(Dictionary<string, string> soFar, List<string> openings, int depth)
    {
        // Cap horizontal depth somewhat TODO: Doesn't work! Need to propagate the failure up to dump out more things and retry from higher up or it just
        // gets stuck in the while loop where nothing can be placed as its not exhaustive? Should walk back up the tree but seems to not currently...
        // Add in some debug logging to see what it's backtracking up to and figure it out from there.
        List<string> available = GetAvailableLocations(soFar);

        Func<string, long> weightFunc = o =>
        {
            // Bias the choice based on fixed battle rank, or number of placed locations if the location doesn't have a fixed rank
            // Also bias areas with higher outgoing links earlier
            var f = 0;
            var g = 0;
            if (areaData.ContainsKey(o))
            {
                // Battle balance for "cinematic" fights which can't be shuffled currently.
                // Augusta 200 and Acad 400 also fix a lot of the enemy pool, so can't change much in battle balance
                // Bresha 5 and Sunleth 300 both have 3 outgoing links so are very valuable for early variation
                // Archylte might be getting this flag removed if we can solve faeryl
                // Acad 500 is pseudo-fixed currently anyway in the endgame so has no effect here.
                if (areaData[o].Traits.Contains("Cinematic"))
                {
                    // This might bias earlier fixed areas too much, but 2 of them are bresha 5 and sunleth 300 which we want early anyway so it's ok?
                    f = (12 - areaData[o].FixedBattleRank);
                }
                else
                {
                    f = 6;
                }
                // Prefer areas with multiple outputs, but want to also early-bias this, so higher depth removes this effect somewhat
                var depthMod = (int)Math.Max(0, 6 - (depth / 3));
                g = areaData[o].OutgoingLinkCount * depthMod;
            }
            return g + f;
        };

        List<string> hasOption = openings.Where(o => !soFar.ContainsValue(o) && IsAllowed(o, soFar, available)).Shuffle();
        if (hasOption.Count == 0)
        {
            Generator.Logger.LogDebug($"Ran out of placement options at depth {depth}. Placed {soFar.Count} of {openings.Count}");
            return (false, soFar);
        }

        var remainingToShuffle = openings.Where(t => !soFar.ContainsValue(t)).ToList();
        var shuffledRemaining = new List<string>();
        while(remainingToShuffle.Count() > 0)
        {
            var weightedShuffled = RandomNum.SelectRandomWeighted(remainingToShuffle, weightFunc);
            shuffledRemaining.Add(weightedShuffled);
            remainingToShuffle.Remove(weightedShuffled);
        }

        List<string> remaining = shuffledRemaining;

        foreach (string rep in remaining)
        {
            // Gate certain areas to manipulate placement somewhat
            if (rep == HistoriaCruxConstants.ACADEMIA_400 || rep == HistoriaCruxConstants.AUGUSTA_200)
            {
                if (depth <= 3 || soFar.Count <= 6)
                {
                    continue;
                }
            }
            else if (rep == HistoriaCruxConstants.ACADEMIA_4XX)
            {
                if(depth <= 4 || soFar.Count <= 10)
                {
                    continue;
                }
            }
            List<string> possible = openings
                .Where(o => !soFar.ContainsKey(o) && IsAllowed(o, soFar, available))
                .ToList();
            while (possible.Count > 0)
            {
                string next = RandomNum.SelectRandomWeighted(possible, weightFunc);
                soFar.Add(next, rep);
                if (soFar.Count == openings.Count)
                {
                    return (true, soFar);
                }

                (bool, Dictionary<string, string>) result = GetPlacement(soFar, openings, depth+1);
                if (result.Item1)
                {
                    return result;
                }
                else
                {
                    RandoUI.SetUIProgressDeterminate($"Historia Crux Rando Failures: {shuffleFailures}", soFar.Count - 1, openings.Count);
                    possible.Remove(next);
                    soFar.Remove(next);
                }
            }
        }
        shuffleFailures++;
        Generator.Logger.LogDebug($"Shuffle failure at depth {depth}. Placed {soFar.Count} remaining {remaining.Count}");
        return (false, soFar);
    }

    public List<string> GetIDsForOpening(string open, bool orig = true)
    {
        return gateData.Keys.Where(id => (orig ? gateTableOrig[id] : gateTable[id]).sOpenHistoria1.StartsWith(open)).ToList();
    }

    private bool IsAllowed(string open, Dictionary<string, string> soFar, List<string> available)
    {
        foreach (string id in GetIDsForOpening(open))
        {
            if (!available.Contains(gateData[id].Location))
            {
                return false;
            }

            if (available.Intersect(gateData[id].Requirements).Count() != gateData[id].Requirements.Count)
            {
                return false;
            }

            // TODO: handle by treasure logic now if cores are randomised?
            if (gateData[id].Traits.Contains("Graviton") && !HasGravitonLocations(available))
            {
                return false;
            }

            // TODO: handle by treasure logic now if wilds are randomised?
            if (gateData[id].Traits.Contains("Wild") && !HasWildArtefacts(soFar, available))
            {
                return false;
            }

            if (gateData[id].MinMogLevel > GetMogLevel(available))
            {
                return false;
            }
            // Hard code for Bresha 5 wild artefact if key items aren't rando
            if (!FF13_2Flags.Items.Treasures.FlagEnabled || !FF13_2Flags.Items.KeySide.Enabled || TooSmallOfPool())
            {
                if (gateData[id].ItemRequirements.GetPossibleRequirements().Contains("key_lockjail") && 2 > GetMogLevel(available))
                {
                    return false;
                }
            }
        }

        return true;
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

    private bool HasWildArtefacts(Dictionary<string, string> soFar, List<string> available)
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

            if (available.Contains(HistoriaCruxConstants.SUNLETH_300) && available.Contains(HistoriaCruxConstants.ARCHYLTE) &&
                available.Contains(HistoriaCruxConstants.YASCHAS_1X) && available.Contains(HistoriaCruxConstants.COLISEUM))
            {
                wilds.Add(HistoriaCruxConstants.SERENDIPITY); // Serendipity. requires completing Yaschas 1X and Sunleth 300 (which then transitively requires the Hole Gems from coliseum/archylte)
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

    private List<string> GetAvailableLocations(Dictionary<string, string> soFar)
    {
        List<string> list = new()
        {
            "start"
        };
        if (FF13_2Flags.Other.ForcedStart.Values.IndexOf(FF13_2Flags.Other.ForcedStart.SelectedValue) > 0)
        {
            list.Add("h_hm_AD0003");
        }

        if (FF13_2Flags.Other.ForcedStart.Values.IndexOf(FF13_2Flags.Other.ForcedStart.SelectedValue) > 1)
        {
            list.Add("h_bj_AD0005");
        }

        list.AddRange(soFar.Values);

        // Unlock Void after Ch 2
        if (list.Contains("h_gh_AD0010") && list.Contains("h_sn_AD0300") && list.Contains("h_gd_NA0000"))
        {
            list.Add("h_sp_NA0001");
        }

        // Unlock Serendipity after Yaschas 1X and Sunleth 300
        if (list.Contains("h_sn_AD0300") && list.Contains("h_gd_NA0000") && list.Contains("h_gh_AD0010") && list.Contains("h_cl_NA0000"))
        {
            list.Add("h_cs_NA0000");
        }

        // Unlock Dying World/Bodhum 700 after Academia 4XX and Graviton and Mog Level >= 3
        // Currently requires mog level 3 since bodhum 700 artefact requires improved moogle hunt
        if (list.Contains("h_aa_AD0400") && HasGravitonLocations(list) && GetMogLevel(list) >= 3)
        {
            list.Add("h_dd_AD0700");
            list.Add("h_hm_AD0700");
            list.Add("h_zz_NA0950");
        }

        return list.Distinct().ToList();
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
        gateTable.SaveDB3(Generator, @"\db\resident\_wdbpack.bin\r_gatetab.wdb");
        SetupData.WPDTracking[Generator.DataOutFolder + @"\db\resident\wdbpack.bin"].Add("r_gatetab.wdb");

        FF13_2RandoExtensions.SaveFile(Generator, @"\gui\resident\_system.win32.xgr\gr_hc_parts.ykd", hcParts);
        Nova.RepackWPD(Generator.DataOutFolder + @"\gui\resident\system.win32.xgr",
            SetupData.Paths["Nova"]);
    }
}
