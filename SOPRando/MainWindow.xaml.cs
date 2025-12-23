using Bartz24.Data;
using Bartz24.Memory;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SOPRando;
/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private const string MAX_LEVEL_VAR = "MaxJobLevel";
    private readonly int[] MAX_LEVELS = [10, 30, 55, 80, 99, 120, 135, 150, 175, 200, 230, 250, 270, 285, 300];
    private readonly string[] DIFFICULTIES = ["HARD", "CHAOS", "BAHAMUT", "GILGAMESH", "LUFENIA"];
    private readonly int[] MAX_JOB_AFFINITIES = [400, 600, 800];

    MemoryReaderWriter? memory;

    public Dictionary<int, AreaData> areaData = new();
    public Dictionary<int, JobData> jobData = new();

    //
    // Current game state
    //
    private HashSet<int> jobsUnlocked = new();
    private int maxJobLevel = 99;
    private string maxDifficulty = "HARD";
    private int maxJobAffinity = 400;
    private bool masterPointsUnlocked = false;
    private bool evocationUltimaJobsUnlocked = false;
    private bool higherRarityUpgradesUnlocked = false;
    private bool changeSpecialEffects = false;
    private bool affinityUpgradeUnlocked = false;
    private bool fuseUnlocked = false;
    private bool replicateUnlocked = false;
    private bool manikinMaterialsUnlocked = false;
    private bool dragonKingTrialsUnlocked = false;
    private bool exchangeShopUnlocked = false;

    // UI state
    private List<Task> tasks = new();
    private bool isClosing = false;
    private bool closeRequested;
    private CancellationTokenSource cts = new();


    public MainWindow()
    {
        InitializeComponent();
        Load();
    }

    private void Load()
    {
        FileHelpers.ReadCSVFile(@"data\areaData.csv", row =>
        {
            AreaData a = new(row);
            areaData.Add(a.ID, a);
        }, FileHelpers.CSVFileHeader.HasHeader);

        // Populate map combo box with area names
        foreach (var area in areaData.Values)
        {
            MapComboBox.Items.Add(new ComboBoxItem() { Content = area.Name, Tag = area.ID });
        }

        FileHelpers.ReadCSVFile(@"data\jobData.csv", row =>
        {
            JobData j = new(row);
            jobData.Add(j.ID, j);
        }, FileHelpers.CSVFileHeader.HasHeader);

        // Populate job combo box with job names
        foreach (var job in jobData.Values)
        {
            JobComboBox.Items.Add(new ComboBoxItem() { Content = job.Name, Tag = job.ID });
        }

        // Populate max level combo box
        foreach (var level in MAX_LEVELS)
        {
            MaxJobLevelComboBox.Items.Add(new ComboBoxItem() { Content = level.ToString(), Tag = level });
        }

        // Populate difficulty combo box
        foreach (var difficulty in DIFFICULTIES)
        {
            MaxDifficultyComboBox.Items.Add(new ComboBoxItem() { Content = difficulty, Tag = difficulty });
        }

        // Populate max job affinity combo box
        foreach (var affinity in MAX_JOB_AFFINITIES)
        {
            MaxJobAffinityComboBox.Items.Add(new ComboBoxItem() { Content = affinity.ToString(), Tag = affinity });
        }
    }

    private void ConnectButton_Click(object sender, RoutedEventArgs e)
    {
        if (memory != null)
        {
            return;
        }

        memory = new MemoryReaderWriter("SOPFFO");

        ApplyCodePatches();

        // Populate initial job list
        InitialStateLoad();

        // Start update thread that doesn't prevent UI updates
        tasks.Add(Task.Run(UpdateThreadImpl));

        ConnectButton.Content = "Connected";
        ConnectButton.IsEnabled = false;
    }

    // Wait on tasks to complete when closing
    protected override void OnClosing(CancelEventArgs e)
    {
        if (closeRequested)
            return;

        // stop tasks from starting new work
        isClosing = true;
        cts.Cancel();

        // cancel this close *for now*
        e.Cancel = true;
        closeRequested = true;

        FinishCloseAsync();
    }

    private async Task FinishCloseAsync()
    {
        try
        {
            // snapshot in case collection changes
            Task[] snapshot;
            lock (tasks) snapshot = tasks.ToArray();

            // optionally add a timeout so you never hang forever
            await Task.WhenAll(snapshot);
        }
        catch (Exception ex)
        {
            // TODO: log; decide whether to continue closing anyway
            // swallow if your goal is "close no matter what"
        }
        finally
        {
            // now actually close
            await Dispatcher.InvokeAsync(Close);
        }
    }

    private void InitialStateLoad()
    {
        if (memory == null)
        {
            return;
        }

        // Start at 473E814
        jobsUnlocked.Clear();
        for (int i = 0; i < jobData.Values.Max(j => j.ID) + 1; i++)
        {
            if (!jobData.ContainsKey(i))
            {
                continue;
            }

            byte unlocked = memory.ReadByteFromBase(0x473E814 + i * 0x20);
            if (unlocked == 1)
            {
                jobsUnlocked.Add(i);
                if (jobData.TryGetValue(i, out JobData? job))
                {
                    JobListBox.Items.Add(new ListBoxItem() { Content = $"{job.Name}" });
                }
            }
        }

        // Always unlock Swordsman and Duelist
        jobsUnlocked.Add(jobData.Values.First(j => j.Name == "Swordsman").ID);
        jobsUnlocked.Add(jobData.Values.First(j => j.Name == "Duelist").ID);

        // Max level
        maxJobLevel = memory.GetCustomVariable<int>(MAX_LEVEL_VAR);

        // Difficulty unlocks
        bool chaosUnlocked = memory.ReadByteFromBase(0x4200972) == 1;
        bool bahamutUnlocked = memory.ReadByteFromBase(0x420099E) == 1;
        bool gilgameshUnlocked = memory.ReadByteFromBase(0x42009A0) == 1;
        bool lufeniaUnlocked = memory.ReadByteFromBase(0x42009A2) == 1;
        if (lufeniaUnlocked)
        {
            maxDifficulty = "LUFENIA";
        }
        else if (gilgameshUnlocked)
        {
            maxDifficulty = "GILGAMESH";
        }
        else if (bahamutUnlocked)
        {
            maxDifficulty = "BAHAMUT";
        }
        else if (chaosUnlocked)
        {
            maxDifficulty = "CHAOS";
        }
        else
        {
            maxDifficulty = "HARD";
        }

        // Job affinities
        bool affinity600Unlocked = memory.ReadByteFromBase(0x42009BA) == 1;
        bool affinity800Unlocked = memory.ReadByteFromBase(0x42009D3) == 1;
        if (affinity800Unlocked)
        {
            maxJobAffinity = 800;
        }
        else if (affinity600Unlocked)
        {
            maxJobAffinity = 600;
        }
        else
        {
            maxJobAffinity = 400;
        }

        // Master points 4200960
        masterPointsUnlocked = memory.ReadByteFromBase(0x4200960) == 1;

        // Evocation Ultima jobs 4200998
        evocationUltimaJobsUnlocked = memory.ReadByteFromBase(0x4200998) == 1;

        // Higher Rarity Upgrades 4200975
        higherRarityUpgradesUnlocked = memory.ReadByteFromBase(0x4200975) == 1;

        // Change Special Effects 4200977
        changeSpecialEffects = memory.ReadByteFromBase(0x4200977) == 1;

        // Affinity Upgrade 4200979
        affinityUpgradeUnlocked = memory.ReadByteFromBase(0x4200979) == 1;

        // Fuse 42009B8
        fuseUnlocked = memory.ReadByteFromBase(0x42009B8) == 1;

        // Replicate 42009D2
        replicateUnlocked = memory.ReadByteFromBase(0x42009D2) == 1;

        // Manikin Materials 42009DF
        manikinMaterialsUnlocked = memory.ReadByteFromBase(0x42009DF) == 1;

        // Dragon King Trials 42009A6
        dragonKingTrialsUnlocked = memory.ReadByteFromBase(0x42009A6) == 1;

        // Exchange Shop 420099C
        exchangeShopUnlocked = memory.ReadByteFromBase(0x420099C) == 1;

        Dispatcher.BeginInvoke(() =>
        {
            MaxJobLevelComboBox.SelectedItem = MaxJobLevelComboBox.Items.Cast<ComboBoxItem>().FirstOrDefault(item => (int)item.Tag == maxJobLevel);

            MaxDifficultyComboBox.SelectedItem = MaxDifficultyComboBox.Items.Cast<ComboBoxItem>().FirstOrDefault(item =>
            {
                string difficulty = (string)item.Tag;
                return difficulty == maxDifficulty;
            });

            MaxJobAffinityComboBox.SelectedItem = MaxJobAffinityComboBox.Items.Cast<ComboBoxItem>().FirstOrDefault(item =>
            {
                int affinity = (int)item.Tag;
                return affinity == maxJobAffinity;
            });

            MasterPointsCheckBox.IsChecked = masterPointsUnlocked;
            EvocationUltimaJobsCheckBox.IsChecked = evocationUltimaJobsUnlocked;
            HigherRarityUpgradesCheckBox.IsChecked = higherRarityUpgradesUnlocked;
            ChangeSpecialEffectsCheckBox.IsChecked = changeSpecialEffects;
            AffinityUpgradeCheckBox.IsChecked = affinityUpgradeUnlocked;
            FuseCheckBox.IsChecked = fuseUnlocked;
            ReplicateCheckBox.IsChecked = replicateUnlocked;
            ManikinMaterialsCheckBox.IsChecked = manikinMaterialsUnlocked;
            DragonKingTrialsCheckBox.IsChecked = dragonKingTrialsUnlocked;
            ExchangeShopCheckBox.IsChecked = exchangeShopUnlocked;
        });
    }

    private void ApplyCodePatches()
    {
        if (memory == null)
        {
            return;
        }

        OverrideJobLimitData();
        AddDisableMapsDetour();
        AddJobMaxLevelLimitDetour();

        // Disable Job Unlocks
        memory.WriteBytesToBase(0x617C9F, [0x90, 0x90, 0x90, 0x90]);
    }

    private void OverrideJobLimitData()
    {
        if (memory == null)
        {
            return;
        }

        IntPtr ptr = memory.GetPointerChainFromBase(0x4159C60, 0x0, 0x38, 0x10, 0x18, 0x88, 0x40, 0xA4);

        // Verify the int is 30 or 300
        int currentValue = memory.ReadInt(ptr);
        if (currentValue != 30 && currentValue != 300)
        {
            throw new Exception("Job limit data pointer value is not as expected, possible memory read error.");
        }

        // Loop 0x400 times, moving 0x10 bytes forward each time, checking for any AOB pattern and replacing the next 84 bytes with 300
        string aob = "1E 00 00 00 37 00 00 00 50 00 00 00 63 00 00 00 78 00 00 00 87 00 00 00 96 00 00 00 AF 00 00 00 C8 00 00 00 E6 00 00 00 FA 00 00 00 0E 01 00 00 1D 01 00 00 2C 01 00 00 2C 01 00 00 2C 01 00 00 2C 01 00 00 2C 01 00 00 2C 01 00 00 2C 01 00 00 2C 01 00 00";
        int countsFound = 0;
        for (int i = 0; i < 0x400; i++)
        {
            if (memory.VerifyAOBPattern(ptr + i * 0x10, aob))
            {
                // Replace next 84 bytes with 300 (2C 01 00 00)
                for (int j = 0; j < 21; j++)
                {
                    memory.WriteInt(ptr + i * 0x10 + j * 4, 300);
                }
                countsFound++;
            }
        }

        Trace.WriteLine($"OverrideJobLimitData: Found and replaced {countsFound} job limit data entries.");
    }

    private void AddJobMaxLevelLimitDetour()
    {
        // Job Max Level Detour
        string detourName = "JobMaxLevelLimit";
        IntPtr detourPtr = memory.ModuleBase + 0x43BD25;

        StringBuilder byteBuilder = new();
        byteBuilder.Append("0F B6 C0 8B 05 DF 03 00 00 48 8B 5C 24 20 48 83 C4 28");

        memory.CreateCodeCave14Byte(detourPtr, detourName, 1004, byteBuilder.ToString(), 16);
        memory.RegisterExistingCustomVariable<int>(MAX_LEVEL_VAR, memory.GetCodeCave(detourName) + 1000);

        memory.SetCustomVariable<int>(MAX_LEVEL_VAR, 99);
    }

    private void AddDisableMapsDetour()
    {
        // Disable Maps Detour
        string detourName = "DisableMaps";
        IntPtr detourPtr = memory.ModuleBase + 0x656C14;

        StringBuilder byteBuilder = new();
        byteBuilder.Append("44 8B C7 8B F2 81 FE 95 0A 00 00 0F 84 B9 03 00 00 81 FE E1 10 00 00 0F 84 AD 03 00 00 81 FE 5F 1F 00 00 0F 84 A1 03 00 00 81 FE 68 15 00 00 0F 84 95 03 00 00 81 FE 6C 09 00 00 0F 84 89 03 00 00 81 FE 3B 06 00 00 0F 84 7D 03 00 00 81 FE 82 24 00 00 0F 84 71 03 00 00 81 FE 23 11 00 00 0F 84 65 03 00 00 81 FE 5C 26 00 00 0F 84 59 03 00 00 81 FE D0 02 00 00 0F 84 4D 03 00 00 81 FE 2D 16 00 00 0F 84 41 03 00 00 81 FE E6 0A 00 00 0F 84 35 03 00 00 81 FE 0E 15 00 00 0F 84 29 03 00 00 81 FE 5D 26 00 00 0F 84 1D 03 00 00 81 FE 19 11 00 00 0F 84 11 03 00 00 81 FE B2 11 00 00 0F 84 05 03 00 00 81 FE 4C 1E 00 00 0F 84 F9 02 00 00 81 FE E6 20 00 00 0F 84 ED 02 00 00 81 FE C1 21 00 00 0F 84 E1 02 00 00 81 FE E9 24 00 00 0F 84 D5 02 00 00 81 FE 4D 22 00 00 0F 84 C9 02 00 00 81 FE 87 02 00 00 0F 84 BD 02 00 00 81 FE 73 1B 00 00 0F 84 B1 02 00 00 81 FE C0 1C 00 00 0F 84 A5 02 00 00 81 FE D5 20 00 00 0F 84 99 02 00 00 81 FE 25 08 00 00 0F 84 8D 02 00 00 81 FE 46 18 00 00 0F 84 81 02 00 00 81 FE 0D 27 00 00 0F 84 75 02 00 00 81 FE F9 0E 00 00 0F 84 69 02 00 00 81 FE 2C 17 00 00 0F 84 5D 02 00 00 81 FE 3A 19 00 00 0F 84 51 02 00 00 81 FE 7E 1F 00 00 0F 84 45 02 00 00 81 FE 82 1D 00 00 0F 84 39 02 00 00 81 FE B0 12 00 00 0F 84 2D 02 00 00 81 FE 33 19 00 00 0F 84 21 02 00 00 81 FE 6B 03 00 00 0F 84 15 02 00 00 81 FE 06 06 00 00 0F 84 09 02 00 00 81 FE EA 23 00 00 0F 84 FD 01 00 00 81 FE E8 12 00 00 0F 84 F1 01 00 00 81 FE 7E 1E 00 00 0F 84 E5 01 00 00 81 FE 6E 19 00 00 0F 84 D9 01 00 00 81 FE 8B 24 00 00 0F 84 CD 01 00 00 81 FE 90 18 00 00 0F 84 C1 01 00 00 81 FE 17 25 00 00 0F 84 B5 01 00 00 81 FE 92 22 00 00 0F 84 A9 01 00 00 81 FE CC 13 00 00 0F 84 9D 01 00 00 81 FE 47 1D 00 00 0F 84 91 01 00 00 81 FE 0B 1D 00 00 0F 84 85 01 00 00 81 FE 87 21 00 00 0F 84 79 01 00 00 81 FE E3 11 00 00 0F 84 6D 01 00 00 81 FE C8 03 00 00 0F 84 61 01 00 00 81 FE 0A 10 00 00 0F 84 55 01 00 00 81 FE C9 1D 00 00 0F 84 49 01 00 00 81 FE 31 1E 00 00 0F 84 3D 01 00 00 81 FE 15 1A 00 00 0F 84 31 01 00 00 81 FE FB 10 00 00 0F 84 25 01 00 00 81 FE 56 14 00 00 0F 84 19 01 00 00 81 FE D3 23 00 00 0F 84 0D 01 00 00 81 FE 06 16 00 00 0F 84 01 01 00 00 81 FE 49 14 00 00 0F 84 F5 00 00 00 81 FE 44 1D 00 00 0F 84 E9 00 00 00 81 FE 18 14 00 00 0F 84 DD 00 00 00 81 FE 80 1C 00 00 0F 84 D1 00 00 00 81 FE 2B 03 00 00 0F 84 C5 00 00 00 81 FE 6F 1B 00 00 0F 84 B9 00 00 00 81 FE D7 10 00 00 0F 84 AD 00 00 00 81 FE A6 26 00 00 0F 84 A1 00 00 00 81 FE 8F 21 00 00 0F 84 95 00 00 00 81 FE DB 02 00 00 0F 84 89 00 00 00 81 FE 04 13 00 00 0F 84 7D 00 00 00 81 FE 42 22 00 00 0F 84 71 00 00 00 81 FE 96 1B 00 00 0F 84 65 00 00 00 81 FE CA 1B 00 00 0F 84 59 00 00 00 81 FE 1A 1E 00 00 0F 84 4D 00 00 00 81 FE B2 0B 00 00 0F 84 41 00 00 00 81 FE 9C 06 00 00 0F 84 35 00 00 00 81 FE 94 24 00 00 0F 84 29 00 00 00 81 FE 6E 1D 00 00 0F 84 1D 00 00 00 81 FE BF 1F 00 00 0F 84 11 00 00 00 81 FE 0A 1F 00 00 0F 84 05 00 00 00 E9 05 00 00 00 BE 00 00 00 00 4C 8B 48 08 49 8B A9 28 04 00 00");

        memory.CreateCodeCave14Byte(detourPtr, detourName, 1000, byteBuilder.ToString(), 16);
    }

    private void UpdateThreadImpl()
    {
        while (memory != null && memory.IsProcessRunning() && !isClosing)
        {
            var (maps, count) = ReadMaps();
            var materialItems = ReadMaterialItemData();

            InitialUnlocks();
            ApplyUnlocks();

            Dispatcher.Invoke(() =>
            {
                MapListBox.Items.Clear();
                for (int i = 0; i < count; i++)
                {
                    int mapID = maps[i];
                    if (areaData.TryGetValue(mapID, out AreaData? area))
                    {
                        MapListBox.Items.Add(new ListBoxItem() { Content = $"{area.Name}" });
                    }
                }

                JobListBox.Items.Clear();
                foreach (int jobID in jobsUnlocked)
                {
                    if (jobData.TryGetValue(jobID, out JobData? job))
                    {
                        JobListBox.Items.Add(new ListBoxItem() { Content = $"{job.Name}" });
                    }
                }

                MaterialListBox.Items.Clear();
                foreach (var (id, count) in materialItems)
                {
                    MaterialListBox.Items.Add(new ListBoxItem() { Content = $"ID: {id} - Count: {count}" });
                }
            });

            if (isClosing)
            {
                break;
            }

            Thread.Sleep(500);
        }

        memory?.Dispose();
        memory = null;

        Dispatcher.Invoke(() =>
        {
            ConnectButton.Content = "Connect to Game";
            ConnectButton.IsEnabled = true;
        });
    }

    private (List<int> maps, int count) ReadMaps()
    {
        if (memory == null)
        {
            return (new List<int>(), 0);
        }

        int mapCount = memory.ReadIntFromBase(0x420ED68);
        // If the map count is unreasonably high, throw
        if (mapCount > 200)
        {
            throw new Exception("Map count is unreasonably high, possible memory read error.");
        }

        List<int> mapList = new();
        for (int i = 0; i < mapCount; i++)
        {
            int mapID = memory.ReadIntFromBase(0x4200D68 + i * 56);
            mapList.Add(mapID);
        }

        return (mapList, mapCount);
    }

    private List<(int id, int count)> ReadMaterialItemData()
    {
        if (memory == null)
        {
            return new List<(int id, int count)>();
        }

        List<(int id, int count)> materialItems = new();
        // Read as long as the item ID is not 0
        for (int i = 0; ; i++)
        {
            int itemID = memory.ReadIntFromBase(0x42F6BC0 + i * 328);
            if (itemID == 0)
            {
                break;
            }

            int itemCount = memory.ReadShortFromBase(0x42F6BC0 + i * 328 + 8);
            materialItems.Add((itemID, itemCount));
        }


        materialItems.Reverse();

        return materialItems;
    }

    private void ApplyUnlocks()
    {
        if (memory == null)
        {
            return;
        }

        // Write 0 or 1 to unlock the job at 473E814 + jobID * 20
        foreach (int jobID in jobData.Keys)
        {
            byte unlockValue = (byte)(jobsUnlocked.Contains(jobID) ? 1 : 0);
            memory.WriteByteToBase(0x473E814 + jobID * 0x20, unlockValue);
        }

        // Max Job Level
        memory.SetCustomVariable<int>(MAX_LEVEL_VAR, maxJobLevel);

        // Max Difficulty
        memory.WriteByteToBase(0x4200972, (byte)(maxDifficulty == "CHAOS" || maxDifficulty == "BAHAMUT" || maxDifficulty == "GILGAMESH" || maxDifficulty == "LUFENIA" ? 1 : 0));
        memory.WriteByteToBase(0x420099E, (byte)(maxDifficulty == "BAHAMUT" || maxDifficulty == "GILGAMESH" || maxDifficulty == "LUFENIA" ? 1 : 0));
        memory.WriteByteToBase(0x42009A0, (byte)(maxDifficulty == "GILGAMESH" || maxDifficulty == "LUFENIA" ? 1 : 0));
        memory.WriteByteToBase(0x42009A2, (byte)(maxDifficulty == "LUFENIA" ? 1 : 0));

        // Max Job Affinity
        memory.WriteByteToBase(0x42009BA, (byte)(maxJobAffinity == 600 || maxJobAffinity == 800 ? 1 : 0));
        memory.WriteByteToBase(0x42009D3, (byte)(maxJobAffinity == 800 ? 1 : 0));

        // Master Points and Anima for Job EXP
        memory.WriteByteToBase(0x4200960, (byte)(masterPointsUnlocked ? 1 : 0));

        // Evocation Ultima Jobs
        memory.WriteByteToBase(0x4200998, (byte)(evocationUltimaJobsUnlocked ? 1 : 0));

        // Higher Rarity Upgrades
        memory.WriteByteToBase(0x4200975, (byte)(higherRarityUpgradesUnlocked ? 1 : 0));

        // Change Special Effects
        memory.WriteByteToBase(0x4200977, (byte)(changeSpecialEffects ? 1 : 0));

        // Affinity Upgrade
        memory.WriteByteToBase(0x4200979, (byte)(affinityUpgradeUnlocked ? 1 : 0));

        // Fuse
        memory.WriteByteToBase(0x42009B8, (byte)(fuseUnlocked ? 1 : 0));

        // Replicate
        memory.WriteByteToBase(0x42009D2, (byte)(replicateUnlocked ? 1 : 0));

        // Manikin Materials
        memory.WriteByteToBase(0x42009DF, (byte)(manikinMaterialsUnlocked ? 1 : 0));

        // Dragon King Trials
        memory.WriteByteToBase(0x42009A6, (byte)(dragonKingTrialsUnlocked ? 1 : 0));


        // Dragon king Map option
        if (exchangeShopUnlocked || dragonKingTrialsUnlocked)
        {
            memory.WriteByteToBase(0x42009A5, 1);
        }
        else
        {
            memory.WriteByteToBase(0x42009A5, 0);
        }

        // Dragon king menu stuff once exchange shop is unlocked
        if (exchangeShopUnlocked)
        {
            // Main shop
            memory.WriteByteToBase(0x420099C, 1);
            // Items
            memory.WriteByteToBase(0x42009BB, 1);
            // Relics
            memory.WriteByteToBase(0x42009CE, 1);
            memory.WriteByteToBase(0x42009CF, 1);
        }
        else
        {
            memory.WriteByteToBase(0x420099C, 0);
            memory.WriteByteToBase(0x42009BB, 0);
            memory.WriteByteToBase(0x42009CE, 0);
            memory.WriteByteToBase(0x42009CF, 0);
        }
    }

    private void InitialUnlocks()
    {
        if (memory == null)
        {
            return;
        }

        // Weapon unlocks 11 bytes - 4778E9C
        byte[] unlockBytes = Enumerable.Range(0, 11).Select(i => (byte)1).ToArray();
        memory.WriteBytesToBase(0x4778E9C, unlockBytes);

        // Affinity cap increase - 42009A9
        memory.WriteByteToBase(0x42009A9, 1);
    }

    private void AddMapButton_Click(object sender, RoutedEventArgs e)
    {
        // Get the selected map ID
        if (MapComboBox.SelectedItem is ComboBoxItem selectedItem && memory != null)
        {
            int mapID = (int)selectedItem.Tag;
            var (maps, count) = ReadMaps();

            // If it's a new map, add it to the list
            if (!maps.Contains(mapID))
            {
                // Add new map ID
                memory.WriteIntToBase(0x4200D68 + count * 56, mapID);

                // Set enabled flag
                memory.WriteByteToBase(0x4200D68 + count * 56 + 4, 1);

                // Increment map count
                memory.WriteIntToBase(0x420ED68, count + 1);
            }
        }
    }

    private void AddJobButton_Click(object sender, RoutedEventArgs e)
    {
        // Get the selected job ID
        if (JobComboBox.SelectedItem is ComboBoxItem selectedItem && memory != null)
        {
            int jobID = (int)selectedItem.Tag;
            // If it's a new job, add it to the list
            if (!jobsUnlocked.Contains(jobID))
            {
                jobsUnlocked.Add(jobID);
            }
        }
    }

    private void JobListItem_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (memory == null) return;

        ListBoxItem listBoxItem = sender as ListBoxItem;
        if (listBoxItem == null) return;

        string name = listBoxItem.Content?.ToString();
        if (string.IsNullOrWhiteSpace(name)) return;

        var job = jobData.Values.FirstOrDefault(j => j.Name == name);
        if (job == null) return;

        int jobID = job.ID;

        jobsUnlocked.Remove(jobID);
    }

    private void MapListItem_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (memory == null) return;

        ListBoxItem listBoxItem = sender as ListBoxItem;
        if (listBoxItem == null) return;

        string name = listBoxItem.Content?.ToString();
        if (string.IsNullOrWhiteSpace(name)) return;

        var area = areaData.Values.FirstOrDefault(a => a.Name == name);
        if (area == null) return;

        int mapID = area.ID;

        var (maps, count) = ReadMaps();
        int index = maps.IndexOf(mapID);
        if (index > -1)
        {
            memory.WriteIntToBase(0x4200D68 + index * 56, 0);
        }
    }

    private void MaxJobLevelComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (MaxJobLevelComboBox.SelectedItem is ComboBoxItem selectedItem)
        {
            maxJobLevel = (int)selectedItem.Tag;
        }
    }

    private void MaxDifficultyComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (MaxDifficultyComboBox.SelectedItem is ComboBoxItem selectedItem)
        {
            string difficulty = (string)selectedItem.Tag;
            maxDifficulty = difficulty;
        }
    }

    private void MaxJobAffinityComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (MaxJobAffinityComboBox.SelectedItem is ComboBoxItem selectedItem)
        {
            int affinity = (int)selectedItem.Tag;
            maxJobAffinity = affinity;
        }
    }

    private void MasterPointsCheckBox_Changed(object sender, RoutedEventArgs e)
    {
        masterPointsUnlocked = MasterPointsCheckBox.IsChecked == true;
    }

    private void EvocationUltimaJobsCheckBox_Changed(object sender, RoutedEventArgs e)
    {
        evocationUltimaJobsUnlocked = EvocationUltimaJobsCheckBox.IsChecked == true;
    }

    private void ChangeSpecialEffectsCheckBox_Changed(object sender, RoutedEventArgs e)
    {
        changeSpecialEffects = ChangeSpecialEffectsCheckBox.IsChecked == true;
    }

    private void AffinityUpgradeCheckBox_Changed(object sender, RoutedEventArgs e)
    {
        affinityUpgradeUnlocked = AffinityUpgradeCheckBox.IsChecked == true;
    }

    private void FuseCheckBox_Changed(object sender, RoutedEventArgs e)
    {
        fuseUnlocked = FuseCheckBox.IsChecked == true;
    }

    private void ReplicateCheckBox_Changed(object sender, RoutedEventArgs e)
    {
        replicateUnlocked = ReplicateCheckBox.IsChecked == true;
    }

    private void ManikinMaterialsCheckBox_Changed(object sender, RoutedEventArgs e)
    {
        manikinMaterialsUnlocked = ManikinMaterialsCheckBox.IsChecked == true;
    }

    private void DragonKingTrialsCheckBox_Changed(object sender, RoutedEventArgs e)
    {
        dragonKingTrialsUnlocked = DragonKingTrialsCheckBox.IsChecked == true;
    }

    private void ExchangeShopCheckBox_Changed(object sender, RoutedEventArgs e)
    {
        exchangeShopUnlocked = ExchangeShopCheckBox.IsChecked == true;
    }

    private void HigherRarityUpgradesCheckBox_Changed(object sender, RoutedEventArgs e)
    {
        higherRarityUpgradesUnlocked = HigherRarityUpgradesCheckBox.IsChecked == true;
    }
}