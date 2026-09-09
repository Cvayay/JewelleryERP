using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using JewelleryERP.ViewModels;

namespace JewelleryERP;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContextChanged += MainWindow_DataContextChanged;
    }

    private void MainWindow_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (e.OldValue is MainViewModel oldViewModel)
        {
            oldViewModel.PropertyChanged -= ViewModel_PropertyChanged;
        }

        if (e.NewValue is MainViewModel viewModel)
        {
            viewModel.PropertyChanged += ViewModel_PropertyChanged;
        }
    }

    private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(MainViewModel.IsNavigationCollapsed)) return;

        var animation = new DoubleAnimation
        {
            To = ((MainViewModel)sender!).IsNavigationCollapsed ? 64 : 232,
            Duration = TimeSpan.FromMilliseconds(180),
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };

        SidebarColumn.BeginAnimation(ColumnDefinition.WidthProperty, null);
        SidebarColumn.BeginAnimation(ColumnDefinition.WidthProperty, new GridLengthAnimation(animation));
    }
}

internal sealed class GridLengthAnimation : AnimationTimeline
{
    private readonly DoubleAnimation _animation;

    public GridLengthAnimation(DoubleAnimation animation) => _animation = animation;

    public override Type TargetPropertyType => typeof(GridLength);

    protected override Freezable CreateInstanceCore() => new GridLengthAnimation(_animation);

    public override object GetCurrentValue(object defaultOriginValue, object defaultDestinationValue,
        AnimationClock animationClock)
    {
        var from = ((GridLength)defaultOriginValue).Value;
        var to = _animation.To ?? ((GridLength)defaultDestinationValue).Value;
        var value = from + ((to - from) * animationClock.CurrentProgress.GetValueOrDefault());
        return new GridLength(value);
    }
}
