using System.Collections.Generic;
using System.Linq;

namespace FF12Rando;

/// <summary>
/// The player facing wording for every mod in <see cref="FF12Mods"/>, in the order the setup screen
/// lists them. One view per mod, shared by the setup screen and the uninstall screen so the two
/// always describe a mod the same way.
///
/// Adding a mod to <see cref="FF12Mods"/> and giving it an entry here is all that is needed: the
/// uninstall screen builds its buttons from this list.
/// </summary>
public static class FF12ModViews
{
    public static ModStatusView Tools { get; } = new(FF12Mods.Tools)
    {
        InstalledText = "The tools for editing scripts and text are correctly installed.",
        MissingText = "The required tools for editing scripts and text are not detected.\nDownload and then install the tools.",
        ArchiveDialogTitle = "Please select a compressed file of the tools."
    };

    public static ModStatusView FileLoader { get; } = new(FF12Mods.FileLoader)
    {
        InstalledText = "The External File Loader is correctly installed.",
        MissingText = "The required External File Loader files are not detected.\nEither download through the Vortex mod manager,\nor download and then install the loader directly with the buttons to the right.",
        ArchiveDialogTitle = "Please select a compressed file of the file loader."
    };

    public static ModStatusView LuaLoader { get; } = new(FF12Mods.LuaLoader)
    {
        InstalledText = "The Lua Loader is correctly installed.",
        MissingText = "The required Lua Loader files are not detected.\nEither download through the Vortex mod manager,\nor download and then install the loader directly with the buttons to the right.",
        ArchiveDialogTitle = "Please select a compressed file of the lua loader."
    };

    public static ModStatusView Manifesto { get; } = new(FF12Mods.Manifesto)
    {
        InstalledText = "The Insurgent's Manifesto is correctly installed.",
        MissingText = "The Insurgent's Manifesto is not installed.\nEither download through the Vortex mod manager,\nor download and then install the loader directly with the buttons to the right.",
        ExternalText = "The Insurgent's Manifesto is correctly installed through Vortex. Note: Some Manifesto features like Dalan options will not appear in rando.",
        ArchiveDialogTitle = "Please select a compressed file of The Insurgent's Manifesto."
    };

    public static ModStatusView Descriptive { get; } = new(FF12Mods.Descriptive)
    {
        InstalledText = "The Insurgent's Descriptive Inventory is correctly installed.\nNOTE: Make sure you generate a seed after installing this mod!\nIt does not work until generating a seed.",
        MissingText = "The OPTIONAL mod for improved equipment descriptions is not installed.\nEither download through the Vortex mod manager,\nor download and then install the loader directly with the buttons to the right.",
        ArchiveDialogTitle = "Please select a compressed file of The Insurgent's Descriptive Inventory."
    };

    public static IReadOnlyList<ModStatusView> All { get; } = new List<ModStatusView>
    {
        Tools, FileLoader, LuaLoader, Manifesto, Descriptive
    };

    /// <summary>The mods the uninstall screen offers, which is everything worth removing.</summary>
    public static IEnumerable<ModStatusView> Uninstallable => All.Where(v => v.Mod.CanUninstall);

    public static ModStatusView For(FF12Mod mod)
    {
        return All.First(v => v.Mod == mod);
    }
}
