using Bartz24.RandoWPF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FF12Rando.Logic;

class AeropassItemReq : ItemReq
{
    public const string RABANASTRE = "80E1";
    public const string NALBINA = "80E2";
    public const string BHUJERBA = "80E3";
    public const string ARCHADES = "80E4";
    public const string BALFONHEIM = "80E5";
    public static ItemReq Aero(string name, bool allowStrahl = true)
    {
        return new AeropassItemReq(name, allowStrahl);
    }

    static AeropassItemReq()
    {
        ItemReq.RegisterMapping("AERO", args => Aero(args[0], args.Count <= 1 || bool.Parse(args[1])));
    }

    public static void Init()
    {
        // Do nothing, just to make sure the static constructor is called
    }

    private static readonly ItemReq BerganReq = ItemReq.Item("Defeat Bergan");
    private static readonly ItemReq CacFlower = ItemReq.Item("8073");
    private static readonly ItemReq EarthTyrant = ItemReq.And(ItemReq.Item("80B1"), ItemReq.Item("80B2"));

    private static readonly Dictionary<string, ItemReq> DestinationReqs = new()
    {
        // Always
        { RABANASTRE, ItemReq.TRUE },
        // Always
        { NALBINA, ItemReq.TRUE },
        // Only alternative is the Strahl
        { BHUJERBA, ItemReq.FALSE },
        // (Fight Bergan or do cactuar side quest or do earth tyrant or fly to Balfonheim) and walk through Sochen
        { ARCHADES, ItemReq.And(ItemReq.Item("80B6"), ItemReq.Or(BerganReq, CacFlower, EarthTyrant, Aero(BALFONHEIM))) },
        // Fight Bergan or do cactuar side quest or do earth tyrant or fly to Archades and walk through Sochen
        { BALFONHEIM, ItemReq.Or(BerganReq, CacFlower, EarthTyrant, ItemReq.And(ItemReq.Item("80B6"), Aero(ARCHADES))) }
    };

    private static readonly Dictionary<string, List<string>> DestinationGraph = new()
    {
        { RABANASTRE, new List<string> { NALBINA, BHUJERBA, ARCHADES } },
        { NALBINA, new List<string> { RABANASTRE, ARCHADES, BALFONHEIM } },
        { BHUJERBA, new List<string> { RABANASTRE, BALFONHEIM } },
        { ARCHADES, new List<string> { RABANASTRE, NALBINA, BALFONHEIM } },
        { BALFONHEIM, new List<string> { NALBINA, ARCHADES, BHUJERBA } }
    };

    private string Destination { get; set; }
    private bool AllowStrahl { get; set; }
    public AeropassItemReq(string destination, bool allowStrahl = true)
    {
        Destination = destination;
        AllowStrahl = allowStrahl;
        if (DestinationReqs != null && !DestinationReqs.ContainsKey(destination))
        {
            throw new ArgumentException("Invalid destination: " + destination);
        }
    }

    protected override List<string> GetPossibleRequirementsImpl()
    {
        return DestinationReqs.Values.SelectMany(r => r.GetPossibleRequirements()).Concat(new List<string>() { "8077" }).Distinct().ToList();
    }

    public override int GetPossibleRequirementsCount()
    {
        return GetPossibleRequirementsImpl().Count;
    }

    protected override bool IsMet(Dictionary<string, int> itemsAvailable)
    {
        return IsDestinationValid(itemsAvailable);
    }

    private bool IsDestinationValid(Dictionary<string, int> itemsAvailable)
    {
        // If we have the Strahl, we can go to any aerodrome
        if (AllowStrahl && ItemReq.Item("8077").IsValid(itemsAvailable))
        {
            return true;
        }

        if (itemsAvailable.GetValueOrDefault(Destination) == 0)
        {
            return false;
        }

        // Refactor below using the directed graph instead
        foreach(string origin in DestinationGraph[Destination])
        {
            if (itemsAvailable.GetValueOrDefault(origin) > 0 && IsOriginAvailable(origin, itemsAvailable))
            {
                return true;
            }
        }

        return false;
    }

    private bool IsOriginAvailable(string origin, Dictionary<string, int> itemsAvailable)
    {
        return DestinationReqs[origin].IsValid(itemsAvailable) || Aero(origin, AllowStrahl).IsValid(itemsAvailable);
    }

    public override string GetDisplay(Func<string, string> itemNameFunc)
    {
        switch(Destination)
        {
            case RABANASTRE:
                return "Rabanastre Aerodrome";
            case NALBINA:
                return "Nalbina Aerodrome";
            case BHUJERBA:
                return "Bhujerba Aerodrome";
            case ARCHADES:
                return "Archades Aerodrome";
            case BALFONHEIM:
                return "Balfonheim Aerodrome";
            default:
                return "Unknown Aerodrome";
        }
    }

    public override int GetDifficulty(Dictionary<string, int> itemsAvailable)
    {
        if (AllowStrahl && ItemReq.Item("8077").IsValid(itemsAvailable))
        {
            return 0;
        }

        if (!IsValid(itemsAvailable))
        {
            return -1;
        }

        switch (Destination)
        {
            case RABANASTRE:
                return 0;
            case NALBINA:
                return 0;
            case BHUJERBA:
                return 1;
            case ARCHADES:
                return 2;
            case BALFONHEIM:
                return 2;
            default:
                return 0;
        }
    }

    public override bool Equals(object obj)
    {
        return obj is AeropassItemReq req &&
               Destination == req.Destination &&
               AllowStrahl == req.AllowStrahl;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Destination, AllowStrahl);
    }
}