using Habit_tracker.Services;
using System.Diagnostics;

namespace Habit_tracker;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();

        try
        {
            ThemeService.Initialize();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Theme init failed: {ex}");
        }

        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
        {
            Debug.WriteLine($"Unhandled: {e.ExceptionObject}");
        };

        TaskScheduler.UnobservedTaskException += (_, e) =>
        {
            Debug.WriteLine($"Unobserved task: {e.Exception}");
            e.SetObserved();
        };
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(new AppShell());
    }
}
