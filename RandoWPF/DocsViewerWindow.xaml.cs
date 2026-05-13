using Microsoft.Web.WebView2.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Bartz24.RandoWPF;

public partial class DocsViewerWindow : Window
{
    private readonly SeedInformation info;
    private readonly List<DocsPageInfo> pages = new();
    private readonly string extractFolder;
    private bool updatingSelection;

    public DocsViewerWindow(SeedInformation info)
    {
        this.info = info;
        extractFolder = Path.Combine(Path.GetTempPath(), "FFModdingWPF", "DocsViewer", Guid.NewGuid().ToString("N"));

        InitializeComponent();

        Title = $"Seed Docs - {info.Seed}";
        Loaded += DocsViewerWindow_Loaded;
        Closing += DocsViewerWindow_Closing;
        Closed += DocsViewerWindow_Closed;
    }

    private async void DocsViewerWindow_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            ExtractDocsArchive();
            LoadPages();
            await InitializeBrowserAsync();
            NavigateToPage(GetDefaultPage());
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to load the docs viewer.\n\n{ex.Message}", "Docs viewer");
            Close();
        }
    }

    private void DocsViewerWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        try
        {
            docsBrowser.Source = new Uri("about:blank");
        }
        catch
        {
            // Ignore browser shutdown issues.
        }
    }

    private async void DocsViewerWindow_Closed(object sender, EventArgs e)
    {
        try
        {
            docsBrowser.Dispose();
        }
        catch
        {
            // Ignore browser disposal issues.
        }

        await CleanupExtractFolderAsync();
    }

    private void ExtractDocsArchive()
    {
        Directory.CreateDirectory(extractFolder);
        ZipFile.ExtractToDirectory(info.DocsArchivePath, extractFolder);
    }

    private void LoadPages()
    {
        pages.Clear();

        foreach (string file in Directory.GetFiles(extractFolder, "*.html", SearchOption.TopDirectoryOnly).OrderBy(f => f))
        {
            pages.Add(new DocsPageInfo(file));
        }

        if (pages.Count == 0)
        {
            throw new InvalidOperationException("No HTML pages were found in the selected docs archive.");
        }
    }

    private DocsPageInfo GetDefaultPage()
    {
        return pages.FirstOrDefault(p => string.Equals(p.FileName, "index.html", StringComparison.OrdinalIgnoreCase))
            ?? pages[0];
    }

    private async Task InitializeBrowserAsync()
    {
        await docsBrowser.EnsureCoreWebView2Async();
        docsBrowser.CoreWebView2.Settings.AreBrowserAcceleratorKeysEnabled = true;
        docsBrowser.CoreWebView2.Settings.AreDefaultContextMenusEnabled = true;
        docsBrowser.CoreWebView2.Settings.AreDevToolsEnabled = false;
        docsBrowser.CoreWebView2.Settings.IsStatusBarEnabled = false;
        docsBrowser.CoreWebView2.Settings.IsZoomControlEnabled = true;
    }

    private void NavigateToPage(DocsPageInfo page)
    {
        if (page == null)
        {
            return;
        }

        docsBrowser.Source = new Uri(page.FullPath);
    }

    private void BackButton_Click(object sender, RoutedEventArgs e)
    {
        if (docsBrowser.CanGoBack)
        {
            docsBrowser.GoBack();
        }
    }

    private void ForwardButton_Click(object sender, RoutedEventArgs e)
    {
        if (docsBrowser.CanGoForward)
        {
            docsBrowser.GoForward();
        }
    }

    private void RefreshButton_Click(object sender, RoutedEventArgs e)
    {
        docsBrowser.Reload();
    }

    private async Task CleanupExtractFolderAsync()
    {
        for (int attempt = 0; attempt < 5; attempt++)
        {
            try
            {
                if (!Directory.Exists(extractFolder))
                {
                    return;
                }

                Directory.Delete(extractFolder, true);
                return;
            }
            catch
            {
                await Task.Delay(150);
            }
        }
    }

    private sealed class DocsPageInfo
    {
        public string FullPath { get; }
        public string FileName { get; }
        public string Name { get; }

        public DocsPageInfo(string fullPath)
        {
            FullPath = fullPath;
            FileName = Path.GetFileName(fullPath);
            Name = Path.GetFileNameWithoutExtension(fullPath);
        }
    }
}
