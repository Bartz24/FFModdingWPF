using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using System.Windows.Media;

namespace Bartz24.RandoWPF;

/// <summary>
/// Interaction logic for SegmentedProgressBar.xaml
/// </summary>
public partial class SegmentedProgressBar : UserControl, INotifyPropertyChanged
{
    private int _totalSegments;
    private ObservableCollection<SegmentViewModel> _segments;

    public int TotalSegments
    {
        get => _totalSegments;
        set
        {
            if (_totalSegments != value)
            {
                _totalSegments = value;
                OnPropertyChanged();
                UpdateSegments();
            }
        }
    }

    public ObservableCollection<SegmentViewModel> Segments
    {
        get => _segments;
        set
        {
            if (_segments != value)
            {
                _segments = value;
                OnPropertyChanged();
            }
        }
    }

    public SegmentedProgressBar()
    {
        DataContext = this;
        InitializeComponent();
        Segments = new ObservableCollection<SegmentViewModel>();
        TotalSegments = 1;
    }

    private void UpdateSegments()
    {
        Segments.Clear();
        for (int i = 0; i < TotalSegments; i++)
        {
            Segments.Add(new SegmentViewModel
            {
                Value = 0,
                Maximum = 1,
                Indeterminate = false
            });
        }
    }

    public void IncrementProgress()
    {
        SetProgress(GetProgress() + 1, 0);
    }

    public void SetProgress(int segments, double currentSegmentProgress)
    {
        for (int i = 0; i < TotalSegments; i++)
        {
            Segments[i].Value = i < segments ? 1 : (i == segments ? currentSegmentProgress : 0);
            Segments[i].Indeterminate = currentSegmentProgress < 0 && i == segments;
            Segments[i].BackgroundColor = i == segments ? Brushes.DarkGray : Brushes.DarkSlateGray;
        }
    }

    public int GetProgress()
    {
        return Segments.Where(s => s.Value == 1).Count();
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public class SegmentViewModel : INotifyPropertyChanged
{
    private double _value;
    private bool _indeterminate;
    private Brush _backgroundColor;

    public double Value
    {
        get => _value;
        set
        {
            if (_value != value)
            {
                _value = value;
                OnPropertyChanged(nameof(Value));
            }
        }
    }

    public double Maximum { get; set; }

    public bool Indeterminate
    {
        get => _indeterminate;
        set
        {
            if (_indeterminate != value)
            {
                _indeterminate = value;
                OnPropertyChanged(nameof(Indeterminate));
            }
        }
    }

    public Brush BackgroundColor
    {
        get => _backgroundColor;
        set
        {
            if (_backgroundColor != value)
            {
                _backgroundColor = value;
                OnPropertyChanged(nameof(BackgroundColor));
            }
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
