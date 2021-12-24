using Bartz24.RandoWPF.Data;
using Ookii.Dialogs.Wpf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FF13_2Rando
{
    /// <summary>
    /// Interaction logic for SetupPaths.xaml
    /// </summary>
    public partial class SetupPaths : UserControl
    {
        public string FF13Path { get => SetupData.GetSteamPath("13"); }
        public string FF13_2Path { get => SetupData.GetSteamPath("13-2"); }
        public string LRPath { get => SetupData.GetSteamPath("LR"); }
        public string NovaPath { get => SetupData.GetSteamPath("Nova", false); }

        public SetupPaths()
        {
            InitializeComponent();
            this.DataContext = this;

            SetupData.OutputFolder = @"outdata\Data";

            SetupData.PathFileName = @"data\RandoPaths.csv";
            SetupData.PathRegistrySearch.Add("13", @"\white_data\prog\win\bin\ffxiiiimg.exe");
            SetupData.PathRegistrySearch.Add("13-2", @"\alba_data\prog\win\bin\ffxiii2img.exe");
            SetupData.PathRegistrySearch.Add("LR", @"\LRFF13.exe");

            SetupData.PathRegistrySearch.Keys.ToList().ForEach(s => SetupData.Paths.Add(s, SetupData.GetSteamPath(s)));
            SetupData.Paths.Add("Nova", SetupData.GetSteamPath("Nova", false));
        }
        /*

        private void steamPathLRButton_Click(object sender, RoutedEventArgs e)
        {
            VistaFolderBrowserDialog dialog = new VistaFolderBrowserDialog();
            dialog.Description = "Please select the folder for LR:FF13 Steam.";
            dialog.UseDescriptionForTitle = true;
            if ((bool)dialog.ShowDialog())
            {
                string path = dialog.SelectedPath.Replace("/", "\\") + SetupData.PathRegistrySearch["LR"];
                if (File.Exists(path))
                {
                    SetupData.Paths["LR"] = path + SetupData.PathRegistrySearch["LR"];
                    File.WriteAllLines(SetupData.PathFileName, SetupData.Paths.Select(p => $"{p.Key};{p.Value}"));
                    steamPathLRText.GetBindingExpression(TextBox.TextProperty).UpdateTarget();
                }
                else
                    MessageBox.Show("Make sure the folder is something like 'LIGHTNING RETURNS FINAL FANTASY XIII'.", "The selected folder is not valid");
            }
        }*/

        private void novaPathButton_Click(object sender, RoutedEventArgs e)
        {
            VistaOpenFileDialog dialog = new VistaOpenFileDialog();
            dialog.Title = "Please select the exe for the Nova Chrysalia mod manager.";
            dialog.Filter = "Executable|*.exe";
            dialog.Multiselect = false;
            if ((bool)dialog.ShowDialog())
            {
                string path = dialog.FileName.Replace("/", "\\");
                if (File.Exists(path))
                {
                    SetupData.Paths["Nova"] = path;
                    File.WriteAllLines(SetupData.PathFileName, SetupData.Paths.Select(p => $"{p.Key};{p.Value + (SetupData.PathRegistrySearch.ContainsKey(p.Key) ? SetupData.PathRegistrySearch[p.Key] : "")}"));
                    novaPathText.GetBindingExpression(TextBox.TextProperty).UpdateTarget();
                }
                else
                    MessageBox.Show("Make sure the executable is something like 'NovaChrysalia.exe'.", "The selected executable is not valid");
            }
        }
        /*
        private void steamPath13Button_Click(object sender, RoutedEventArgs e)
        {
            VistaFolderBrowserDialog dialog = new VistaFolderBrowserDialog();
            dialog.Description = "Please select the folder for FF13 Steam.";
            dialog.UseDescriptionForTitle = true;
            if ((bool)dialog.ShowDialog())
            {
                string path = dialog.SelectedPath.Replace("/", "\\") + SetupData.PathRegistrySearch["13"];
                if (File.Exists(path))
                {
                    SetupData.Paths["13"] = path + SetupData.PathRegistrySearch["13"];
                    File.WriteAllLines(SetupData.PathFileName, SetupData.Paths.Select(p => $"{p.Key};{p.Value}"));
                    steamPath13Text.GetBindingExpression(TextBox.TextProperty).UpdateTarget();
                }
                else
                    MessageBox.Show("Make sure the folder is something like 'FINAL FANTASY XIII'.", "The selected folder is not valid");
            }
        }
        */

        private void steamPath13_2Button_Click(object sender, RoutedEventArgs e)
        {
            VistaFolderBrowserDialog dialog = new VistaFolderBrowserDialog();
            dialog.Description = "Please select the folder for FF13-2 Steam.";
            dialog.UseDescriptionForTitle = true;
            if ((bool)dialog.ShowDialog())
            {
                string path = dialog.SelectedPath.Replace("/", "\\") + SetupData.PathRegistrySearch["13-2"];
                if (File.Exists(path))
                {
                    SetupData.Paths["13-2"] = path + SetupData.PathRegistrySearch["13-2"];
                    File.WriteAllLines(SetupData.PathFileName, SetupData.Paths.Select(p => $"{p.Key};{p.Value}"));
                    steamPath13_2Text.GetBindingExpression(TextBox.TextProperty).UpdateTarget();
                }
                else
                    MessageBox.Show("Make sure the folder is something like 'FINAL FANTASY XIII-2'.", "The selected folder is not valid");
            }
        }
    }
}