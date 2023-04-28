using Bartz24.RandoWPF;
using System.Windows;
using System.Windows.Controls;

namespace Bartz24.RandoWPF;

/// <summary>
/// Interaction logic for ToggleFlagControl.xaml
/// </summary>
public partial class ToggleFlagControl : UserControl
{
    public static readonly DependencyProperty FlagPropertyProperty =
    DependencyProperty.Register(nameof(FlagProperty), typeof(ToggleFlagProperty), typeof(ToggleFlagControl));

    public ToggleFlagProperty FlagProperty
    {
        get => (ToggleFlagProperty)GetValue(FlagPropertyProperty);
        set => SetValue(FlagPropertyProperty, value);
    }

    public ToggleFlagControl()
    {
        InitializeComponent();
    }
}
