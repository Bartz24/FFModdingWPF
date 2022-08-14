using Bartz24.FF12;
using Bartz24.RandoWPF;
using System;

namespace FF12Rando
{
    public class TextRando : Randomizer
    {
        public DataStoreBinText TextAbilities = new DataStoreBinText(true);
        public DataStoreBinText TextEquipment = new DataStoreBinText(true);
        public DataStoreBinText TextKeyItems = new DataStoreBinText(true);
        public DataStoreBinText TextLoot = new DataStoreBinText(true);
        public DataStoreBinText TextMenuMessage = new DataStoreBinText(true);
        public DataStoreBinText TextMenuCommand = new DataStoreBinText(true);
        public DataStoreBinText TextAbilityHelp = new DataStoreBinText(true);
        public TextRando(RandomizerManager randomizers) : base(randomizers) { }

        public override void Load()
        {
            TextAbilities.Load("data\\text\\abilities.txt");
            TextEquipment.Load("data\\text\\equipment.txt");
            TextKeyItems.Load("data\\text\\keyitems.txt");
            TextLoot.Load("data\\text\\loot.txt");

            TextMenuMessage.Load("outdata\\ps2data\\image\\ff12\\test_battle\\us\\binaryfile\\menu_message.bin.txt");
            TextMenuCommand.Load("outdata\\ps2data\\image\\ff12\\test_battle\\us\\binaryfile\\menu_command.bin.txt");
            TextAbilityHelp.Load("outdata\\ps2data\\image\\ff12\\test_battle\\us\\binaryfile\\listhelp_ability.bin.txt");
        }
        public override void Randomize(Action<int> progressSetter)
        {
        }

        public override void Save()
        {
            TextMenuMessage.Save("outdata\\ps2data\\image\\ff12\\test_battle\\us\\binaryfile\\menu_message.bin.txt");
            TextMenuCommand.Save("outdata\\ps2data\\image\\ff12\\test_battle\\us\\binaryfile\\menu_command.bin.txt");
            TextAbilityHelp.Save("outdata\\ps2data\\image\\ff12\\test_battle\\us\\binaryfile\\listhelp_ability.bin.txt");
        }
    }
}
