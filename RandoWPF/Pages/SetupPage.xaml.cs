using Ookii.Dialogs.Wpf;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Threading;

namespace Bartz24.RandoWPF;

/// <summary>
/// Interaction logic for SetupPage.xaml
/// </summary>
[ContentProperty(nameof(Children))]
public partial class SetupPage : UserControl
{
    public static readonly DependencyPropertyKey ChildrenProperty = DependencyProperty.RegisterReadOnly(
        nameof(Children),
        typeof(UIElementCollection),
        typeof(SetupPage),
        new PropertyMetadata());
    private static bool settingSeed = false;
    private string zipFilter = "ZIP(*.zip)|*.zip";

    private FileDragDropHandler dragDropHandler;
    private FileType jsonFileType, zipFileType, apFileType;
    private string apExtension = null;

    public string Seed
    {
        get => SetupData.Seed;
        set
        {
            if (settingSeed)
            {
                return;
            }

            settingSeed = true;
            SetupData.Seed = value;
            seedText.GetBindingExpression(TextBox.TextProperty).UpdateTarget();
            settingSeed = false;
        }
    }

    public UIElementCollection Children
    {
        get => (UIElementCollection)GetValue(ChildrenProperty.DependencyProperty);
        private set => SetValue(ChildrenProperty, value);
    }
    public SetupPage()
    {
        InitializeComponent();
        DataContext = this;
        Children = PART_Host.Children;
        Seed = RandomNum.RandSeed().ToString();

        iconAP.Visibility = Visibility.Collapsed;
        RandoFlags.SelectedChanged += (s, e) =>
        {
            if (RandoFlags.Mode == RandoFlags.SeedMode.Archipelago)
            {
                seedText.IsEnabled = false;
                seedButton.IsEnabled = false;
                iconAP.Visibility = Visibility.Visible;
            }
            else
            {
                seedText.IsEnabled = true;
                seedButton.IsEnabled = true;
                iconAP.Visibility = Visibility.Collapsed;
            }
        };

        SetupData.SeedChanged += (s, e) => Seed = SetupData.Seed;

        UpdateDropHint();

        // Drops are accepted on the whole window. Wait for it to exist, since this page's tab may not
        // have been shown yet.
        Dispatcher.BeginInvoke(DispatcherPriority.Loaded, AttachDragDrop);
    }

    public void SetAPFileExtension(string ext)
    {
        zipImportText.Text = "Load from ZIP/" + ext.ToUpper().Trim('.');
        zipFilter = "ZIP or " + ext.ToUpper().Trim('.') + "(*.zip;*" + ext.ToLower() + ")|*.zip;*" + ext.ToLower();
        apExtension = ext.ToUpper().Trim('.');

        if (dragDropHandler != null)
        {
            AddAPFileType();
        }

        UpdateDropHint();
    }

    private void AttachDragDrop()
    {
        Window window = Window.GetWindow(this) ?? Application.Current?.MainWindow;
        if (window == null || dragDropHandler != null)
        {
            return;
        }

        dragDropHandler = FileDragDropHandler.GetOrCreateForWindow(window);
        jsonFileType = dragDropHandler.AddFileType(".json", "the seed JSON", LoadSeedFromJson);
        zipFileType = dragDropHandler.AddFileType(".zip", "the seed from the documentation ZIP", LoadSeedFromZip);
        if (apExtension != null)
        {
            AddAPFileType();
        }

        dragDropHandler.DragEntered += DragDrop_DragEntered;
        dragDropHandler.DragEnded += dropHint.Reset;
    }

    private void AddAPFileType()
    {
        if (apFileType != null)
        {
            dragDropHandler.RemoveFileType(apFileType);
        }

        apFileType = dragDropHandler.AddFileType(apExtension, "the Archipelago seed", LoadSeedFromZip);
    }

    private bool IsSeedFileType(FileType type)
    {
        return type != null && (type == jsonFileType || type == zipFileType || type == apFileType);
    }

    private void DragDrop_DragEntered(FileDragInfo info)
    {
        // Other pages can register their own file types on the same window.
        if (info.IsSupported && !IsSeedFileType(info.Type))
        {
            return;
        }

        dropHint.ShowDrag(dragDropHandler, info.IsSupported, info.IsSupported
            ? $"Release to load {info.Type.Description}: {Path.GetFileName(info.Path)}"
            : info.Paths.Count == 1
                ? $"{Path.GetFileName(info.Path)} is not a seed file. Drop {DescribeAcceptedFiles()}."
                : info.Paths.Count > 1
                    ? "Only one seed file can be loaded at a time."
                    : $"Only files can be dropped. Drop {DescribeAcceptedFiles()}.");
    }

    private void UpdateDropHint()
    {
        dropHint.IdleText = $"You can also drag and drop {DescribeAcceptedFiles()} onto this window.";
    }

    private string DescribeAcceptedFiles()
    {
        return apExtension == null
            ? "a seed JSON or documentation ZIP"
            : $"a seed JSON, documentation ZIP, or Archipelago {apExtension}";
    }

    private void importJSONButton_Click(object sender, RoutedEventArgs e)
    {
        VistaOpenFileDialog dialog = new()
        {
            Title = "Please select a JSON seed.",
            Multiselect = false,
            Filter = "JSON|*.json"
        };
        if ((bool)dialog.ShowDialog())
        {
            LoadSeedFromJson(dialog.FileName.Replace("/", "\\"));
        }
    }

    private void LoadSeedFromJson(string path)
    {
        if (File.Exists(path))
        {
            try
            {
                Seed = RandoFlags.LoadSeed(path);
                RandoUI.ShowTempUIMessage($"Set the seed to {Seed} and loaded flags used for the seed!");
            }
            catch (RandoException ex)
            {
                MessageBox.Show(ex.Message, ex.Title);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to load the seed file.\n\n" + ex.StackTrace);
            }
        }
        else
        {
            MessageBox.Show("Make sure the JSON file is a seed for rando.", "The selected file is not valid");
        }
    }

    private void importHistoryButton_Click(object sender, RoutedEventArgs e)
    {
        VistaOpenFileDialog dialog = new()
        {
            Title = "Please select a ZIP documentation or AP world patch.",
            Multiselect = false,
            Filter = zipFilter
        };
        if ((bool)dialog.ShowDialog())
        {
            LoadSeedFromZip(dialog.FileName.Replace("/", "\\"));
        }
    }

    private void LoadSeedFromZip(string path)
    {
        if (File.Exists(path))
        {
            string outFolder = System.IO.Path.GetTempPath() + @"rando_temp";
            bool deleteTempFolder = !Directory.Exists(outFolder);
            if (!Directory.Exists(outFolder))
            {
                Directory.CreateDirectory(outFolder);
            }

            try
            {
                using (ZipArchive archive = ZipFile.OpenRead(path))
                {
                    ZipArchiveEntry entry = archive.Entries.First(e => e.Name.EndsWith("_Seed.json") || e.Name == "seed.json");
                    entry.ExtractToFile(outFolder + @"\seed.json");
                }

                Seed = RandoFlags.LoadSeed(outFolder + @"\seed.json");
                RandoUI.ShowTempUIMessage($"Set the seed to {Seed} and loaded flags used for the seed!");
            }
            catch (RandoException ex)
            {
                MessageBox.Show(ex.Message, ex.Title);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to load the seed file.\n\n" + ex.StackTrace);
            }

            if (File.Exists(outFolder + @"\seed.json"))
            {
                File.Delete(outFolder + @"\seed.json");
            }

            if (deleteTempFolder && Directory.Exists(outFolder))
            {
                Directory.Delete(outFolder);
            }
        }
        else
        {
            MessageBox.Show("Make sure the ZIP file is a docs folder for rando.", "The selected file is not valid");
        }
    }

    private void seedButton_Click(object sender, RoutedEventArgs e)
    {
        Seed = RandomNum.RandSeed().ToString();
    }

    private void importStringButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            FlagStringCompressor compressor = new();
            Seed = RandoFlags.Deserialize(compressor.Decompress(Clipboard.GetText()));
            RandoUI.ShowTempUIMessage($"Set the seed to {Seed} and loaded flags used for the seed!");
        }
        catch
        {
            MessageBox.Show("Failed to load the seed string from your clipboard. Make sure the string is properly copied.");
        }
    }

    private void importSeedHistoryButton_Click(object sender, RoutedEventArgs e)
    {
        if (RandoSeeds.Seeds.Count == 0)
        {
            RandoUI.ShowTempUIMessage("No previous seeds found.");
        }
        else
        {
            RandoUI.SwitchUITab(0);
        }
    }
}
