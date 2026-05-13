using Avalonia.Controls;
using Avalonia.Interactivity;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;

namespace Forebay.TaskManager.Avalonia.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void ExitMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        Close();
    }

    private async void AboutMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        var box = MessageBoxManager
            .GetMessageBoxStandard("About Forebay Task Manager",
                "Forebay Task Manager\n\n" +
                "A task management application using Forebay storage backend.\n" +
                "Tasks are stored in the cloud and synced across devices.\n\n" +
                "Built with Avalonia UI",
                ButtonEnum.Ok);

        await box.ShowAsync();
    }
}
