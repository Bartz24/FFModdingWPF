using Bartz24.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Bartz24.RandoWPF.Data.Areas;
public class AreaGraph
{
    private SeedGenerator SeedGenerator { get; set; }
    public Dictionary<string, Area> Areas { get; set; } = new Dictionary<string, Area>();

    public List<AreaConnection> Connections { get; set; } = new List<AreaConnection>();

    private Func<SeedGenerator, string[], AreaConnection> CreateConnection { get; set; } = (g, s) => new AreaConnection(g, s);

    public AreaGraph(SeedGenerator generator, Func<SeedGenerator, string[], AreaConnection> createFunc = null)
    {
        SeedGenerator = generator;
        if (createFunc != null)
        {
            CreateConnection = createFunc;
        }
    }

    public void ReadFromCSVs(string areaCsv, string areaConnectionsCsv)
    {
        FileHelpers.ReadCSVFile(areaCsv, (row) =>
        {
            Area area = new(row);
            Areas[area.Name] = area;
        }, FileHelpers.CSVFileHeader.HasHeader);

        FileHelpers.ReadCSVFile(areaConnectionsCsv, (row) =>
        {
            AreaConnection connection = CreateConnection(SeedGenerator, row);
            Connections.Add(connection);

            // If it has the "BothWays" trait, add the reverse connection as well
            if (connection.Traits.Contains("BothWays"))
            {
                AreaConnection reverseConnection = connection.CreateReverse();
                Connections.Add(reverseConnection);
            }
        }, FileHelpers.CSVFileHeader.HasHeader);
    }

    public void VerifyIntegrity()
    {
        foreach (var connection in Connections)
        {
            if (!Areas.ContainsKey(connection.FromAreaName))
            {
                throw new Exception($"Area connection has invalid From area: {connection.FromAreaName}");
            }
            if (!Areas.ContainsKey(connection.ToAreaName))
            {
                throw new Exception($"Area connection has invalid To area: {connection.ToAreaName}");
            }
        }
    }

    public List<AreaConnection> GetValidConnectionsFrom(string areaName, ProgressionState state)
    {
        return Connections.Where(c => c.FromAreaName == areaName && c.AreItemReqsMet(state)).ToList();
    }

    public List<AreaConnection> GetValidConnectionsTo(string areaName, ProgressionState state)
    {
        return Connections.Where(c => c.ToAreaName == areaName && c.AreItemReqsMet(state)).ToList();
    }

    public List<Area> GetAllAccessibleAreas(List<string> startAreas, ProgressionState state)
    {
        var tempState = new ProgressionState(state);
        HashSet<string> accessibleAreas = new(startAreas);

        bool foundNewArea = true;
        while (foundNewArea)
        {
            // Update the tempState with any new areas that have been added to accessibleAreas
            tempState.AreasAccessible.UnionWith(accessibleAreas);

            foundNewArea = false;
            foreach (var areaName in accessibleAreas.ToList())
            {
                foreach (var connection in GetValidConnectionsFrom(areaName, tempState))
                {
                    if (!accessibleAreas.Contains(connection.ToAreaName))
                    {
                        accessibleAreas.Add(connection.ToAreaName);
                        foundNewArea = true;
                    }
                }
            }
        }

        return accessibleAreas.Select(areaName => Areas[areaName]).ToList();
    }

    public List<Area> GetAllAccessibleAreas(string startArea, ProgressionState state)
    {
        return GetAllAccessibleAreas(new List<string> { startArea }, state);
    }
}
