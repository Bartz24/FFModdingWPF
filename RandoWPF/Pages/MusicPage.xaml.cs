using Ookii.Dialogs.Wpf;
using SharpCompress.Archives.SevenZip;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace Bartz24.RandoWPF
{
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

        public UIElementCollection Children
        {
            get { return (UIElementCollection)GetValue(ChildrenProperty.DependencyProperty); }
            private set { SetValue(ChildrenProperty, value); }
        }
        public MusicPage()
        {
            InitializeComponent();
            this.DataContext = this;
            Children = PART_Host.Children;
            if (Directory.Exists("data\\musicPacks"))
                UpdateMusicPackList();
        }

        private void UpdateMusicPackList()
        {
            MusicPackList = new ObservableCollection<string>(Directory.GetDirectories("data\\musicPacks").Select(p => System.IO.Path.GetFileName(p)));
            musicList.GetBindingExpression(ListBox.ItemsSourceProperty).UpdateTarget();
        }

        private void AddMusic_Click(object sender, RoutedEventArgs e)
        {
            VistaOpenFileDialog dialog = new VistaOpenFileDialog();
            dialog.Title = "Please select a compressed file of the music.";
            dialog.Multiselect = false;
            dialog.Filter = "7zip|*.7z";
            if ((bool)dialog.ShowDialog())
            {
                string path = dialog.FileName.Replace("/", "\\");
                string name = System.IO.Path.GetFileNameWithoutExtension(path);
                if (File.Exists(path) && !Directory.Exists("data\\musicPacks\\" + name))
                {
                    try
                    {
                        using (var archive = SevenZipArchive.Open(path))
                        using (var reader = archive.ExtractAllEntries())
                        {
                            while (reader.MoveToNextEntry())
                            {
                                if (!reader.Entry.IsDirectory)
                                {
                                    using (var entryStream = reader.OpenEntryStream())
                                    {
                                        string extractedPath = "data\\musicPacks\\" + name + "\\" + reader.Entry.Key;
                                        if (!Directory.Exists(System.IO.Path.GetDirectoryName(extractedPath)))
                                            Directory.CreateDirectory(System.IO.Path.GetDirectoryName(extractedPath));
                                        using (var writeStream = File.OpenWrite(extractedPath))
                                        {
                                            entryStream.CopyTo(writeStream);
                                        }
                                    }
                                }
                            }
                        }

                        MessageBox.Show("Music pack has been successfully installed.");
                        UpdateMusicPackList();
                    }
                    catch
                    {
                        MessageBox.Show("Failed to install the music pack when extracting the files.");
                    }
                }
                else if (Directory.Exists("data\\musicPacks\\" + name))
                    MessageBox.Show("There already exists a music pack with this name.", "The selected file is not valid");
                else
                    MessageBox.Show("Make sure the selected file is a 7z file.", "The selected file is not valid");
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
}