using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace Bartz24.RandoWPF;
public class FileDragDropHandler
{
    private static readonly ConditionalWeakTable<Window, FileDragDropHandler> Handlers = new();

    public event Action<FileDragInfo> DragEntered;
    public event Action DragEnded;

    private readonly Window window;
    private readonly List<FileType> fileTypes = new();
    private HighlightAdorner highlight;
    private FileDragInfo current;

    public static FileDragDropHandler GetOrCreateForWindow(Window window)
    {
        return Handlers.GetValue(window, w => new FileDragDropHandler(w));
    }

    private FileDragDropHandler(Window window)
    {
        this.window = window;
        window.AllowDrop = true;
        window.PreviewDragEnter += Window_PreviewDragOver;
        window.PreviewDragOver += Window_PreviewDragOver;
        window.PreviewDragLeave += Window_PreviewDragLeave;
        window.PreviewDrop += Window_PreviewDrop;
    }
    
    public FileType AddFileType(string extension, string description, Action<string> load)
    {
        return AddFileType(new FileType(extension, description, false, paths => load(paths[0])));
    }

    public FileType AddMultiFileType(string extension, string description, Action<IReadOnlyList<string>> load)
    {
        return AddFileType(new FileType(extension, description, true, load));
    }

    private FileType AddFileType(FileType type)
    {
        fileTypes.Add(type);
        return type;
    }

    public void RemoveFileType(FileType type)
    {
        fileTypes.Remove(type);
    }

    private FileDragInfo GetDragInfo(DragEventArgs e)
    {
        List<string> paths = (e.Data.GetData(DataFormats.FileDrop) as string[] ?? Array.Empty<string>()).Where(File.Exists).ToList();
        List<string> exts = paths.Select(p => Path.GetExtension(p).ToLower()).Distinct().ToList();
        if (exts.Count != 1)
        {
            return new(paths, null);
        }

        FileType type = fileTypes.FirstOrDefault(t => t.Extension == exts[0]);
        return new(paths, type != null && (paths.Count == 1 || type.AllowMultiple) ? type : null);
    }

    private void Window_PreviewDragOver(object sender, DragEventArgs e)
    {
        if (!e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            return;
        }

        current ??= StartDrag(e);

        e.Effects = current.IsSupported ? DragDropEffects.Copy : DragDropEffects.None;
        // Handled so text boxes under the cursor don't reject the file.
        e.Handled = true;
    }

    private FileDragInfo StartDrag(DragEventArgs e)
    {
        FileDragInfo info = GetDragInfo(e);

        if (window.Content is UIElement content && AdornerLayer.GetAdornerLayer(content) is AdornerLayer layer)
        {
            highlight = new HighlightAdorner(content, GetHighlightBrush(info.IsSupported));
            layer.Add(highlight);
        }

        BringToFront();
        DragEntered?.Invoke(info);
        return info;
    }

    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    private static extern uint GetWindowThreadProcessId(IntPtr hWnd, IntPtr processId);

    [DllImport("kernel32.dll")]
    private static extern uint GetCurrentThreadId();

    [DllImport("user32.dll")]
    private static extern bool AttachThreadInput(uint idAttach, uint idAttachTo, bool fAttach);

    [DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(IntPtr hWnd);

    private void BringToFront()
    {
        if (window.IsActive)
        {
            return;
        }

        // Windows only lets the app in front (the one being dragged from) hand over focus, so share
        // its input state just long enough to take it. Otherwise the taskbar button only flashes.
        uint foregroundThread = GetWindowThreadProcessId(GetForegroundWindow(), IntPtr.Zero);
        uint thisThread = GetCurrentThreadId();
        bool attached = foregroundThread != 0 && foregroundThread != thisThread && AttachThreadInput(thisThread, foregroundThread, true);

        SetForegroundWindow(new WindowInteropHelper(window).Handle);
        window.Activate();

        if (attached)
        {
            AttachThreadInput(thisThread, foregroundThread, false);
        }
    }

    private void Window_PreviewDragLeave(object sender, DragEventArgs e)
    {
        // Leave also fires when moving between elements inside the window, so only end once the
        // cursor has actually left it.
        if (window.Content is FrameworkElement content)
        {
            Point pos = e.GetPosition(content);
            if (pos.X > 0 && pos.Y > 0 && pos.X < content.ActualWidth && pos.Y < content.ActualHeight)
            {
                return;
            }
        }

        EndDrag();
    }

    private void Window_PreviewDrop(object sender, DragEventArgs e)
    {
        if (!e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            return;
        }

        e.Handled = true;
        FileDragInfo info = GetDragInfo(e);
        EndDrag();

        if (!info.IsSupported)
        {
            RandoUI.ShowTempUIMessage(info.Paths.Count switch
            {
                0 => "Only files can be dragged and dropped.",
                1 => $"{Path.GetFileName(info.Path)} is not a supported file.",
                _ => "Those files can't be dropped."
            });
            return;
        }

        // Load after the drop returns so a message box doesn't hang the window the file came from.
        window.Dispatcher.BeginInvoke(DispatcherPriority.Background, () => info.Type.Load(info.Paths));
    }

    private void EndDrag()
    {
        if (current == null)
        {
            return;
        }

        if (highlight != null)
        {
            AdornerLayer.GetAdornerLayer(highlight.AdornedElement)?.Remove(highlight);
            highlight = null;
        }

        current = null;
        DragEnded?.Invoke();
    }

    public Brush GetHighlightBrush(bool supported)
    {
        return window.TryFindResource(supported ? "PrimaryHueMidBrush" : "MaterialDesignValidationErrorBrush") as Brush
            ?? (supported ? Brushes.DodgerBlue : Brushes.IndianRed);
    }

    private class HighlightAdorner : Adorner
    {
        private const double Thickness = 4;
        private readonly Pen pen;

        public HighlightAdorner(UIElement adornedElement, Brush brush) : base(adornedElement)
        {
            IsHitTestVisible = false;

            Brush pulsing = brush.Clone();
            pulsing.BeginAnimation(Brush.OpacityProperty, new DoubleAnimation(1, 0.25, TimeSpan.FromSeconds(0.6))
            {
                AutoReverse = true,
                RepeatBehavior = RepeatBehavior.Forever,
                EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
            });
            pen = new Pen(pulsing, Thickness);
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            Rect rect = new(AdornedElement.RenderSize);
            rect.Inflate(-Thickness / 2, -Thickness / 2);
            drawingContext.DrawRectangle(null, pen, rect);
        }
    }
}
