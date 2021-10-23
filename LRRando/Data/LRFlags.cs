using Bartz24.RandoWPF;
using Bartz24.RandoWPF.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LRRando
{
    public class LRFlags
    {
        public enum FlagType
        {
            Other
        }
        public class Other
        {
            public static Flag Abilities;
            public static Flag Enemies;
            public static Flag Equip;
            public static Flag Music;
            public static Flag Treasures;
            public static Flag Shops;
            public static Flag Quests;

            internal static void Init()
            {
                Abilities = new Flag()
                {
                    Text = "Abilities",
                    FlagID = "Abilities",
                    DescriptionFormat = ""
                }.Register(FlagType.Other);

                Enemies = new Flag()
                {
                    Text = "Enemies",
                    FlagID = "Enemies",
                    DescriptionFormat = ""
                }.Register(FlagType.Other);

                Equip = new Flag()
                {
                    Text = "Equip",
                    FlagID = "Equip",
                    DescriptionFormat = ""
                }.Register(FlagType.Other);

                Music = new Flag()
                {
                    Text = "Music",
                    FlagID = "Music",
                    DescriptionFormat = ""
                }.Register(FlagType.Other);

                Treasures = new Flag()
                {
                    Text = "Treasures",
                    FlagID = "Treasures",
                    DescriptionFormat = ""
                }.Register(FlagType.Other);

                Shops = new Flag()
                {
                    Text = "Shops",
                    FlagID = "Shops",
                    DescriptionFormat = ""
                }.Register(FlagType.Other);

                Quests = new Flag()
                {
                    Text = "Quests",
                    FlagID = "Quests",
                    DescriptionFormat = ""
                }.Register(FlagType.Other);
            }
        }

        public static void Init()
        {
            Flags.FlagsList.Clear();
            Other.Init();
        }
    }
}

