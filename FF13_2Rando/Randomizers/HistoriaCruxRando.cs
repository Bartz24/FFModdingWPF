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

            if (FF13_2Flags.Other.ForcedStart.Values.IndexOf(FF13_2Flags.Other.ForcedStart.SelectedValue) > 0)
            {
                openings = openings.Where(o => o != HistoriaCruxConstants.NEW_BODHUM_3).ToList();
            }

            if (FF13_2Flags.Other.ForcedStart.Values.IndexOf(FF13_2Flags.Other.ForcedStart.SelectedValue) > 1)
            {
                openings = openings.Where(o => o != HistoriaCruxConstants.BRESHA_RUINS_5).ToList();
            }

            placement = GetPlacement(new Dictionary<string, string>(), openings, 0).Item2;

            placement.Keys.ToList().ForEach(open =>
            {
                gateTable.Keys.Where(id => gateTableOrig[id].sOpenHistoria1.StartsWith(open)).ToList().ForEach(id => gateTable[id].sOpenHistoria1 = placement[open] + "_a");
            });

            if (placement.ContainsKey(HistoriaCruxConstants.NEW_BODHUM_3))
            {
                gateTable["hs_hmaa10_zz"].sArea = placement[HistoriaCruxConstants.NEW_BODHUM_3];
            }

            // TODO: fix hs_hmaa_def for starting location if no fixed bodhum start

            BuildInitialGateTree();
            // Generate updated gate matrix based on shuffled links
            // TODO: may need to track this from earlier in the process to add in dead nodes

            RandomNum.ClearRand();
        }
    }

    private class TreeNode
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
                // TODO:
                // Improve y-axis distribution to ensure we keep in range
                // Move dead location coords into box for clarity
                // Run through a seed to validate link correcness/navigation works as expected
                var initialY = 6;
                var rootid = CoordsToId(2, initialY);
                coordMap[rootid] = node.name;
                // DLC placement left of initial root node
                var dlcBlockId = CoordsToId(1, initialY);
                coordMap[dlcBlockId] = "h_zz_NA0970";
                var dlcUpperId = CoordsToId(0, initialY - 1);
                coordMap[dlcUpperId] = "h_va_NA0001";
                var dlcLowerId = CoordsToId(1, initialY + 1);
                coordMap[dlcLowerId] = "h_cs_NA0001";
                var dlcLeftId = CoordsToId(0, initialY);
                coordMap[dlcLeftId] = "h_cl_NA0001";
                var (success, added) = TryPlaceChildren(node, 2, initialY, coordMap, 0, 1);
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
                if(right == "h_hp_AD0003")
                {
                    right = "h_zz_NA0950";
                }
                if(!updatedLocations.ContainsKey(left) || !updatedLocations.ContainsKey(right))
                {
                    Generator.Logger.LogDebug($"Unable to link coords at either end of link {link}");
                    continue;
                }

                // TODO: special case for "magic" links to void beyond/serendipity, need to consider left also
                DataStoreRGateTable incomingLink;
                if(right != "h_cs_NA0000" && right != "h_sp_NA0001")
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
                    Generator.Logger.LogDebug($"Unable to locate link offset for link {incomingLink.record}");
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
        {"h_zz_NA0970", 0x60f0 },
        {"h_gt_AD0900", 0x6170 },
        {"h_gt_AD0200", 0x61f0 },
        {"h_gt_AD0300", 0x6270 },
        {"h_ac_AD0400", 0x62f0 },
        {"h_ac_AD0500", 0x6370 },
        // HC loc is 0100 not 0400
        {"h_aa_AD0400", 0x63f0 },
        {"h_vp_AD0010", 0x6470 },
        {"h_vp_AD0200", 0x64f0 },
        {"h_cs_NA0000", 0x6570 },
        {"h_cl_NA0000", 0x65f0 },
        {"h_sn_AD0900", 0x6670 },
        {"h_sn_AD0300", 0x66f0 },
        {"h_sn_AD0400", 0x6770 },
        {"h_bj_AD0100", 0x67f0 },
        {"h_bj_AD0300", 0x6870 },
        {"h_bj_AD0005", 0x68f0 },
        {"h_hm_AD0900", 0x6970 },
        {"h_hm_AD0003", 0x69f0 },
        {"h_hm_AD0700", 0x6a70 },
        {"h_gy_AD0100", 0x6af0 },
        {"h_gy_AD0010", 0x6b70 },
        // 110
        {"h_gy_AD0200", 0x6bf0 },
        {"h_gh_AD0010", 0x6c70 },
        {"h_gw_AD0900", 0x6cf0 },
        {"h_gw_AD0200", 0x6d70 },
        {"h_gw_AD0300", 0x6df0 },
        {"h_gw_AD0400", 0x6e70 },
        {"h_dd_AD0700", 0x6ef0 },
        {"h_dd_AD0900", 0x6f70 },
        {"h_sp_NA0001", 0x6ff0 },
        {"h_sp_NA0100", 0x7070 },
        {"h_zz_NA0910", 0x70f0 },
        {"h_zz_NA0920", 0x7170 },
        {"h_zz_NA0930", 0x71f0 },
        {"h_zz_NA0940", 0x7270 },
        {"h_zz_NA0950", 0x72f0 },
        {"h_zz_NA0960", 0x7370 },
        {"h_gd_NA0000", 0x73f0 },
        {"h_gd_NA0900", 0x7470 },
        {"h_va_NA0000", 0x74f0 },
        {"h_va_NA0001", 0x7570 },
        {"h_cs_NA0001", 0x75f0 },
        {"h_cl_NA0001", 0x7670 },
        {"h_zz_NA0980", 0x76f0 },
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

    private (bool, Dictionary<int, string>) TryPlaceChildren(TreeNode root, int rootX, int rootY, Dictionary<int, string> placed, int incomingDir, int yBias)
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
        // Stop ambiguous vertical placements
        if (incomingDir == 4 || incomingDir == 2)
        {
            usedUp = true;
        }
        else if (incomingDir == 3 || incomingDir == 1)
        {
            usedDown = true;
        }
        // Special case these as they technically have 4 outgoing links so things are going to get weird no matter what probably...
        if(root.name == "h_sn_AD0300" || root.name == "h_gh_AD0010")
        {
            usedUp = false;
            usedDown = false;
        }
        // When on the main branch, pick random Y dir if there's multiple children to place
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
        //Bounds cehcking
        //if(rootY < 0)
        //{
        //    usedDown = true;
        //}
        //if(rootY > 7)
        //{
        //    usedUp = true;
        //}
        for (var direction = 0; direction < 6; direction++)
        {
            var trueDir = direction;
            //if(yBias == -1)
            //{
            //    trueDir = (direction + 3) % 6;
            //}
            var attemptX = rootX;
            var attemptY = rootY;
            var preferredY = 1;
            // TODO: ignoring ybias for now because it doens't really work out nicely.
            var childBias = root.children.Count > 1 ? -yBias : yBias;
            // TODO: vary based on incomingDir?
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
            if (trueDir == 0)
            {
                attemptX++;
            }
            else if (trueDir == 1)
            {
                attemptY += preferredY;
            }
            else if (trueDir == 2)
            {
                attemptX++;
                attemptY -= preferredY;
            }
            else if (trueDir == 3)
            {
                attemptX--;
                attemptY += preferredY;
            }
            else if (trueDir == 4)
            {
                attemptY -= preferredY;
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
            if (root.name == "h_hp_AD0003" && activeChild.name == "h_zz_NA0950")
            {
                // New Bodhum 3xx is not a real location, so use its slot for the empty node it generates afterwards.
                placedChildren++;
                var parentId = CoordsToId(rootX, rootY);
                newPlacement.Remove(parentId);
                newPlacement[parentId] = activeChild.name;
                return TryPlaceChildren(activeChild, rootX, rootY, newPlacement, trueDir, childBias);
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
            var (success, added) = TryPlaceChildren(activeChild, attemptX, attemptY, newPlacement, trueDir, childBias);
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
                if (root.name == "h_sn_AD0300" || root.name == "h_gh_AD0010")
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
        //if(depth >= 14)
        //{
        //    shuffleFailures++;
        //    return (false, soFar);
        //}
        List<string> available = GetAvailableLocations(soFar);
        List<string> remaining = openings.Where(t => !soFar.ContainsValue(t)).Shuffle();

        // Prioritise branched paths over depth, see how the heuristic works out here
        // Want to deprioritise "terminal" nodes until a bit deeper in the chain generally
        // Also maybe need to consider wild artefact stuff?
        // Also need to bias for fixed boss locations (BJ005, SN300, GD000, GT200, AC400, AC500)

        // TODO: optimisation, wasted for loop this either runs always or once?
        foreach (string rep in remaining)
        {
            List<string> possible = openings.Where(o => !soFar.ContainsKey(o) && IsAllowed(o, soFar, available)).Shuffle();
            if (possible.Count == 0)
            {
                return (false, soFar);
            }
        }

        foreach (string rep in remaining)
        {
            List<string> possible = openings.Where(o => !soFar.ContainsKey(o) && IsAllowed(o, soFar, available)).ToList();
            while (possible.Count > 0)
            {
                string next = RandomNum.SelectRandomWeighted(possible, o => {
                    // Bias the choice based on fixed battle rank, or number of placed locations if the location doesn't have a fixed rank
                    // Also bias areas with higher outgoing links earlier
                    var f = 0;
                    var g = 0;
                    if (areaData.ContainsKey(o))
                    {
                        if (areaData[o].Traits.Contains("Cinematic"))
                        {
                            // This might bias earlier fixed areas too much, but 2 of them are bresha 5 and sunleth 300 which we want early anyway so it's ok?
                            f = (20 - areaData[o].FixedBattleRank);
                        } else
                        {
                            f = 20;
                        }
                        g = areaData[o].OutgoingLinkCount * 2;
                    }
                    // Max f is around 16, adjust offset/multipliers if needed to heighten effect.
                    return Math.Max(1, depth + g - f);
                });
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
                    RandoUI.SetUIProgressDeterminate($"Failures {shuffleFailures}: ", soFar.Count - 1, openings.Count);
                    possible.Remove(next);
                    soFar.Remove(next);
                }
            }
        }
        shuffleFailures++;
        return (false, soFar);
    }

    private string SelectNext(IList<string> possible)
    {
        return possible[RandomNum.NextInt(0, possible.Count)];
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

            if (gateData[id].Traits.Contains("Graviton") && !HasGravitonLocations(available))
            {
                return false;
            }

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
        return available.Contains("h_dd_AD0700") ? 3 : available.Contains("h_sn_AD0300") ? 2 : available.Contains("h_bj_AD0005") ? 1 : 0;
    }

    private bool HasGravitonLocations(List<string> available)
    {
        if (!FF13_2Flags.Items.Treasures.FlagEnabled || !FF13_2Flags.Items.KeyGraviton.Enabled || TooSmallOfPool())
        {
            // If graviton cores aren't rando, use normal logic
            List<string> gravitons = new();
            if (GetMogLevel(available) >= 1)
            {
                gravitons.Add("h_hm_AD0003"); // Bodhum 3. requires moogle hunt
            }

            if (GetMogLevel(available) >= 1)
            {
                gravitons.Add("h_bj_AD0005"); // Bresha 5. requires moogle hunt
            }

            if (GetMogLevel(available) >= 1)
            {
                gravitons.Add("h_gw_AD0200"); // Oerba 200. requires moogle hunt
            }

            if (GetMogLevel(available) >= 1)
            {
                gravitons.Add("h_ac_AD0400"); // Academia 400. requires moogle hunt
            }

            if (GetMogLevel(available) >= 1)
            {
                gravitons.Add("h_gy_AD0100"); // Yaschas 100. requires moogle hunt
            }

            if (GetMogLevel(available) >= 1)
            {
                gravitons.Add("h_gw_AD0400"); // Oerba 400. requires moogle hunt
            }

            if (GetMogLevel(available) >= 1)
            {
                gravitons.Add("h_sn_AD0400"); // Sunleth 400. requires moogle hunt
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
                wilds.Add("h_bj_AD0005"); // Bresha 5. requires moogle hunt
            }

            if (GetMogLevel(available) >= 1)
            {
                wilds.Add("h_bj_AD0300"); // Bresha 300. requires moogle hunt
            }

            if (GetMogLevel(available) >= 2)
            {
                wilds.Add("h_gw_AD0200"); // Oerba 200. requires moogle throw
            }

            if (GetMogLevel(available) >= 2)
            {
                wilds.Add("h_sn_AD0300"); // Sunleth 300. requires moogle throw
            }

            if (GetMogLevel(available) >= 2)
            {
                wilds.Add("h_gd_NA0000"); // Archylte. requires moogle throw
            }

            wilds.Add("h_gt_AD0200"); // Augusta 200
            if (GetMogLevel(available) >= 1)
            {
                wilds.Add("h_aa_AD0400"); // Academia 4XX. requires moogle hunt
            }

            if (GetMogLevel(available) >= 2)
            {
                wilds.Add("h_gy_AD0100"); // Yaschas 100. requires moogle hunt and throw
            }

            if (GetMogLevel(available) >= 1)
            {
                wilds.Add("h_dd_AD0700"); // Dying World 700. requires moogle hunt
            }

            if (available.Contains("h_sn_AD0300") && available.Contains("h_gd_NA0000") && available.Contains("h_gh_AD0010") && available.Contains("h_cl_NA0000"))
            {
                wilds.Add("h_cs_NA0000"); // Serendipity. requires completing Yaschas 1X and Sunleth 300
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

        // Unlock Dying World/Bodhum 700 after Academia 4XX and Graviton and Mog Level >= 1
        if (list.Contains("h_aa_AD0400") && HasGravitonLocations(list) && GetMogLevel(list) > 0)
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

        page.HTMLElements.Add(new Table("", (new string[] { "Original Gate", "New Location", "Estimated Battle Difficulty of New Location" }).ToList(), (new int[] { 40, 40, 20 }).ToList(),
            gateData.Values.Where(g => !g.Traits.Contains("Paradox")).Select(g =>
          {
              string id = gateTable[g.ID].sOpenHistoria1;
              string shortID = id.Substring(0, id.Length - 2);
              return (new string[] { g.GateOriginal, areaData[shortID].Name, diffs.ContainsKey(shortID) ? diffs[shortID].ToString() : "-" }).ToList();
          }).ToList()));

        page.HTMLElements.Add(new Table("grid", (new string[] { "X", "Y", "Location" }).ToList(), (new int[] { 10, 10, 80 }).ToList(),
            coordMap.Select(kvp =>
            {
                var (x, y) = MapCoordsToHexGrid(IdToCoords(kvp.Key));
                // TODO: update placement logic properly, or just remove this block entirely?
                return (new string[] { x.ToString(), y.ToString(), kvp.Value }).ToList();
            }).ToList()));

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
