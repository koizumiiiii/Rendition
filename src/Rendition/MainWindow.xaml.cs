using System.Windows;
using Rendition.ViewModels;

namespace Rendition;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        Loaded += OnLoaded;
        Closed += OnClosed;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is MainViewModel vm)
        {
            await vm.AutoLoadModelCommand.ExecuteAsync(null);
        }
    }

    private void OnClosed(object? sender, EventArgs e)
    {
        if (DataContext is MainViewModel vm)
        {
            vm.Dispose();
        }
    }
}
