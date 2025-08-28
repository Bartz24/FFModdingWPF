using Bartz24.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bartz24.RandoWPF.Data.Areas;
public class AreaGraph
{
    private SeedGenerator SeedGenerator { get; set; }
    public Dictionary<string, Area> Areas { get; set; } = new Dictionary<string, Area>();

    public List<AreaConnection> Connections { get; set; } = new List<AreaConnection>();

    public AreaGraph(SeedGenerator generator)
    {
        SeedGenerator = generator;
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
            AreaConnection connection = new(SeedGenerator, row);
            Connections.Add(connection);
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

    public List<AreaConnection> GetValidConnectionsFrom(string areaName, Dictionary<string, int> items)
    {
        return Connections.Where(c => c.FromAreaName == areaName && c.Requirements.IsValid(items)).ToList();
    }

    public List<AreaConnection> GetValidConnectionsTo(string areaName, Dictionary<string, int> items)
    {
        return Connections.Where(c => c.ToAreaName == areaName && c.Requirements.IsValid(items)).ToList();
    }

    public List<Area> GetAllAccessibleAreas(List<string> startAreas, Dictionary<string, int> items)
    {
        HashSet<string> accessibleAreas = new(startAreas);
        Queue<string> areasToCheck = new(startAreas);

        while (areasToCheck.Count > 0)
        {
            string currentArea = areasToCheck.Dequeue();
            foreach (var connection in GetValidConnectionsFrom(currentArea, items))
            {
                if (!accessibleAreas.Contains(connection.ToAreaName))
                {
                    accessibleAreas.Add(connection.ToAreaName);
                    areasToCheck.Enqueue(connection.ToAreaName);
                }
            }
        }

        return accessibleAreas.Select(areaName => Areas[areaName]).ToList();
    }

    public List<Area> GetAllAccessibleAreas(string startArea, Dictionary<string, int> items)
    {
        return GetAllAccessibleAreas(new List<string> { startArea }, items);
    }
}
