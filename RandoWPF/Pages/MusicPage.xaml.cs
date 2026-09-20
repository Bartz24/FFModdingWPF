using Ookii.Dialogs.Wpf;
using SharpCompress.Archives.SevenZip;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Threading;

namespace Bartz24.RandoWPF;

/// <summary>
/// Interaction logic for MusicPage.xaml
/// </summary>
[ContentProperty(nameof(Children))]
public partial class MusicPage : UserControl
{
    public static readonly DependencyPropertyKey ChildrenProperty = DependencyProperty.RegisterReadOnly(
        nameof(Children),
        typeof(UIElementCollection),
        typeof(MusicPage),
        new PropertyMetadata());
    public ObservableCollection<string> MusicPackList { get; set; } = new ObservableCollection<string>();

    private FileDragDropHandler dragDropHandler;
    private FileType musicPackFileType;

    public UIElementCollection Children
    {
        get => (UIElementCollection)GetValue(ChildrenProperty.DependencyProperty);
        private set => SetValue(ChildrenProperty, value);
    }
    public MusicPage()
    {
        InitializeComponent();
        DataContext = this;
        Children = PART_Host.Children;
        if (Directory.Exists("data\\musicPacks"))
        {
            UpdateMusicPackList();
        }

        // Drops are accepted on the whole window. Wait for it to exist, since this page's tab may not
        // have been shown yet.
        Dispatcher.BeginInvoke(DispatcherPriority.Loaded, AttachDragDrop);
    }

    private void AttachDragDrop()
    {
        Window window = Window.GetWindow(this) ?? Application.Current?.MainWindow;
        if (window == null || dragDropHandler != null)
        {
            return;
        }

        dragDropHandler = FileDragDropHandler.GetOrCreateForWindow(window);
        musicPackFileType = dragDropHandler.AddMultiFileType(".7z", "music packs", InstallMusicPacks);
        dragDropHandler.DragEntered += DragDrop_DragEntered;
        dragDropHandler.DragEnded += dragDropLabel.Reset;
    }

    private void DragDrop_DragEntered(FileDragInfo info)
    {
        if (info.Type != musicPackFileType)
        {
            return;
        }

        List<string> installed = info.Paths.Select(Path.GetFileNameWithoutExtension).Where(IsInstalled).ToList();
        int newCount = info.Paths.Count - installed.Count;

        string message = newCount switch
        {
            0 when installed.Count == 1 => $"Music pack {installed[0]} is already installed.",
            0 => $"Music packs are already installed: {string.Join(", ", installed)}",
            1 when info.Paths.Count == 1 => $"Drop to install the music pack: {Path.GetFileName(info.Path)}",
            1 => "Drop to install 1 music pack",
            _ => $"Drop to install {newCount} music packs"
        };
        if (newCount > 0 && installed.Count > 0)
        {
            message += $" ({string.Join(", ", installed)} already installed)";
        }

        dragDropLabel.ShowDrag(dragDropHandler, newCount > 0, message);
    }

    private void UpdateMusicPackList()
    {
        MusicPackList = new ObservableCollection<string>(Directory.GetDirectories("data\\musicPacks").Select(p => System.IO.Path.GetFileName(p)));
        musicList.GetBindingExpression(ListBox.ItemsSourceProperty).UpdateTarget();
    }

    private void AddMusic_Click(object sender, RoutedEventArgs e)
    {
        VistaOpenFileDialog dialog = new()
        {
            Title = "Please select a compressed file of the music.",
            Multiselect = true,
            Filter = "7zip|*.7z"
        };
        if ((bool)dialog.ShowDialog())
        {
            InstallMusicPacks(dialog.FileNames.Select(p => p.Replace("/", "\\")).ToList());
        }
    }

    private static bool IsInstalled(string name)
    {
        return Directory.Exists("data\\musicPacks\\" + name);
    }

    private void InstallMusicPacks(IReadOnlyList<string> paths)
    {
        List<string> installed = new(), skipped = new(), failed = new(), invalid = new();
        foreach (string path in paths)
        {
            string name = System.IO.Path.GetFileNameWithoutExtension(path);
            if (!File.Exists(path))
            {
                invalid.Add(name);
            }
            else if (IsInstalled(name))
            {
                skipped.Add(name);
            }
            else if (InstallMusicPack(path, name))
            {
                installed.Add(name);
            }
            else
            {
                failed.Add(name);
            }
        }

        if (installed.Count > 0)
        {
            UpdateMusicPackList();
        }

        // One summary for the whole batch rather than a message box per pack.
        List<string> lines = new();
        if (installed.Count > 0)
        {
            lines.Add(installed.Count == 1
                ? $"Music pack {installed[0]} has been successfully installed."
                : $"{installed.Count} music packs have been successfully installed: {string.Join(", ", installed)}");
        }

        if (skipped.Count > 0)
        {
            lines.Add($"A music pack with this name already exists: {string.Join(", ", skipped)}");
        }

        if (failed.Count > 0)
        {
            lines.Add($"Failed to install when extracting the files: {string.Join(", ", failed)}");
        }

        if (invalid.Count > 0)
        {
            lines.Add($"Make sure the selected file is a 7z file: {string.Join(", ", invalid)}");
        }

        bool anyProblems = skipped.Count + failed.Count + invalid.Count > 0;
        MessageBox.Show(string.Join("\n\n", lines), anyProblems ? "Some music packs were not installed" : "Music packs installed");
    }

    private static bool InstallMusicPack(string path, string name)
    {
        try
        {
            using (SevenZipArchive archive = SevenZipArchive.Open(path))
            using (SharpCompress.Readers.IReader reader = archive.ExtractAllEntries())
            {
                while (reader.MoveToNextEntry())
                {
                    if (!reader.Entry.IsDirectory)
                    {
                        using (SharpCompress.Common.EntryStream entryStream = reader.OpenEntryStream())
                        {
                            string extractedPath = "data\\musicPacks\\" + name + "\\" + reader.Entry.Key;
                            if (!Directory.Exists(System.IO.Path.GetDirectoryName(extractedPath)))
                            {
                                Directory.CreateDirectory(System.IO.Path.GetDirectoryName(extractedPath));
                            }

                            using (FileStream writeStream = File.OpenWrite(extractedPath))
                            {
                                entryStream.CopyTo(writeStream);
                            }
                        }
                    }
                }
            }

            return true;
        }
        catch
        {
            return false;
        }
    }

    private void DeleteMusic_Click(object sender, RoutedEventArgs e)
    {
        string selected = musicList.SelectedItem?.ToString();
        if (selected != null)
        {
            if (MessageBox.Show("Are you sure you want to delete this music pack?", "Delete", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                Directory.Delete("data\\musicPacks\\" + selected, true);
                UpdateMusicPackList();
            }
        }
    }
}