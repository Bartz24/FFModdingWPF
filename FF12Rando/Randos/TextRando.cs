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
    public TextRando(RandomizerManager randomizers) : base(randomizers) { }

    public override void Load()
    {
        Randomizers.SetUIProgress("Loading Text Data...", 0, -1);
        TextAbilities.Load("data\\text\\abilities.txt");
        TextEquipment.Load("data\\text\\equipment.txt");
        TextKeyItems.Load("data\\text\\keyitems.txt");
        TextLoot.Load("data\\text\\loot.txt");

        TextMenuMessage.Load("outdata\\ps2data\\image\\ff12\\test_battle\\us\\binaryfile\\menu_message.bin.txt");
        TextMenuCommand.Load("outdata\\ps2data\\image\\ff12\\test_battle\\us\\binaryfile\\menu_command.bin.txt");
        TextAbilityHelp.Load("outdata\\ps2data\\image\\ff12\\test_battle\\us\\binaryfile\\listhelp_ability.bin.txt");
    }
    public override void Randomize()
    {
        Randomizers.SetUIProgress("Randomizing Text Data...", 0, -1);
    }

    public override void Save()
    {
        Randomizers.SetUIProgress("Saving Text Data...", 0, -1);
        TextMenuMessage.Save("outdata\\ps2data\\image\\ff12\\test_battle\\us\\binaryfile\\menu_message.bin.txt");
        TextMenuCommand.Save("outdata\\ps2data\\image\\ff12\\test_battle\\us\\binaryfile\\menu_command.bin.txt");
        TextAbilityHelp.Save("outdata\\ps2data\\image\\ff12\\test_battle\\us\\binaryfile\\listhelp_ability.bin.txt");
    }
}
