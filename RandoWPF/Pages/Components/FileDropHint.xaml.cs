using MaterialDesignThemes.Wpf;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Bartz24.RandoWPF;
public partial class FileDropLabel : UserControl
{
    private string idleText = "";
    private bool dragging = false;

    public string IdleText
    {
        get => idleText;
        set
        {
            idleText = value;
            if (!dragging)
            {
                Reset();
            }
        }
    }

    public FileDropLabel()
    {
        InitializeComponent();
        Reset();
    }

    public void ShowDrag(FileDragDropHandler handler, bool supported, string message)
    {
        dragging = true;
        dragDropLabel.Text = message;
        hintIcon.Kind = supported ? PackIconKind.FileDownload : PackIconKind.FileCancelOutline;
        hintIcon.Foreground = handler.GetHighlightBrush(supported);
        hintPanel.Opacity = 1;

        DependencyObject parent = this;
        while (parent != null && parent is not TabItem)
        {
            parent = LogicalTreeHelper.GetParent(parent);
        }

        if (parent is TabItem tab)
        {
            tab.IsSelected = true;
        }

        // Wait for the tab to lay out before scrolling to the hint.
        Dispatcher.BeginInvoke(DispatcherPriority.Loaded, () => BringIntoView());
    }

    public void Reset()
    {
        dragging = false;
        dragDropLabel.Text = idleText;
        hintIcon.Kind = PackIconKind.FileDownloadOutline;
        hintIcon.ClearValue(ForegroundProperty);
        hintPanel.Opacity = 0.7;
    }
}
