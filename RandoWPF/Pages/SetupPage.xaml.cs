using Ookii.Dialogs.Wpf;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Bartz24.RandoWPF
{
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
        public string Seed
        {
            get => SetupData.Seed; 
            set
            {
                SetupData.Seed = value;
                seedText.GetBindingExpression(TextBox.TextProperty).UpdateTarget();
            }
        }

        public UIElementCollection Children
        {
            get { return (UIElementCollection)GetValue(ChildrenProperty.DependencyProperty); }
            private set { SetValue(ChildrenProperty, value); }
        }
        public SetupPage()
        {
            InitializeComponent();
            this.DataContext = this;
            Children = PART_Host.Children;
            Seed = RandomNum.RandSeed().ToString();
        }

        private void importJSONButton_Click(object sender, RoutedEventArgs e)
        {
            VistaOpenFileDialog dialog = new VistaOpenFileDialog();
            dialog.Title = "Please select a JSON seed.";
            dialog.Multiselect = false;
            dialog.Filter = "JSON|*.json";
            if ((bool)dialog.ShowDialog())
            {
                string path = dialog.FileName.Replace("/", "\\");
                if (File.Exists(path))
                {
                    try
                    {
                        Seed = Flags.LoadSeed(path);
                    }
                    catch
                    {
                        MessageBox.Show("Failed to load the seed file.");
                    }
                }
                else
                    MessageBox.Show("Make sure the JSON file is a seed for rando.", "The selected file is not valid");
            }
        }

        private void importHistoryButton_Click(object sender, RoutedEventArgs e)
        {
            VistaOpenFileDialog dialog = new VistaOpenFileDialog();
            dialog.Title = "Please select a ZIP documentation.";
            dialog.Multiselect = false;
            dialog.Filter = "Zip|*.zip";
            if ((bool)dialog.ShowDialog())
            {
                string path = dialog.FileName.Replace("/", "\\");
                if (File.Exists(path))
                {
                    string outFolder = System.IO.Path.GetTempPath() + @"rando_temp";
                    bool deleteTempFolder = !Directory.Exists(outFolder);
                    if (!Directory.Exists(outFolder))
                        Directory.CreateDirectory(outFolder);
                    try
                    {
                        using (ZipArchive archive = ZipFile.OpenRead(path))
                        {
                            ZipArchiveEntry entry = archive.Entries.First(e => e.Name.EndsWith("_Seed.json"));
                            entry.ExtractToFile(outFolder + @"\seed.json");
                        }
                        Seed = Flags.LoadSeed(outFolder + @"\seed.json");
                    }
                    catch
                    {
                        MessageBox.Show("Failed to load the seed file.");
                    }
                    if (File.Exists(outFolder + @"\seed.json"))
                        File.Delete(outFolder + @"\seed.json");
                    if (deleteTempFolder && Directory.Exists(outFolder))
                        Directory.Delete(outFolder);

                }
                else
                    MessageBox.Show("Make sure the ZIP file is a docs folder for rando.", "The selected file is not valid");
            }
        }

        private void seedButton_Click(object sender, RoutedEventArgs e)
        {
            Seed = RandomNum.RandSeed().ToString();
        }
    }
}