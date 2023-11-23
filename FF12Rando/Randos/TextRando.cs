using Bartz24.FF12;
using Bartz24.RandoWPF;

namespace FF12Rando;

public class TextRando : Randomizer
{
    public DataStoreBinText TextAbilities = new(true);
    public DataStoreBinText TextEquipment = new(true);
    public DataStoreBinText TextKeyItems = new(true);
    public DataStoreBinText TextLoot = new(true);
    public DataStoreBinText TextMenuMessage = new(true);
    public DataStoreBinText TextMenuCommand = new(true);
    public DataStoreBinText TextAbilityHelp = new(true);
    public TextRando(SeedGenerator randomizers) : base(randomizers) { }

    public override void Load()
    {
        Generator.SetUIProgress("Loading Text Data...", 0, -1);
        TextAbilities.Load("data\\text\\abilities.txt");
        TextEquipment.Load("data\\text\\equipment.txt");
        TextKeyItems.Load("data\\text\\keyitems.txt");
        TextLoot.Load("data\\text\\loot.txt");

        TextMenuMessage.Load($"{Generator.DataOutFolder}\\image\\ff12\\test_battle\\us\\binaryfile\\menu_message.bin.txt");
        TextMenuCommand.Load($"{Generator.DataOutFolder}\\image\\ff12\\test_battle\\us\\binaryfile\\menu_command.bin.txt");
        TextAbilityHelp.Load($"{Generator.DataOutFolder}\\image\\ff12\\test_battle\\us\\binaryfile\\listhelp_ability.bin.txt");
    }
    public override void Randomize()
    {
        Generator.SetUIProgress("Randomizing Text Data...", 0, -1);
    }

    public override void Save()
    {
        Generator.SetUIProgress("Saving Text Data...", 0, -1);
        TextMenuMessage.Save($"{Generator.DataOutFolder}\\image\\ff12\\test_battle\\us\\binaryfile\\menu_message.bin.txt");
        TextMenuCommand.Save($"{Generator.DataOutFolder}\\image\\ff12\\test_battle\\us\\binaryfile\\menu_command.bin.txt");
        TextAbilityHelp.Save($"{Generator.DataOutFolder}\\image\\ff12\\test_battle\\us\\binaryfile\\listhelp_ability.bin.txt");
    }
}
