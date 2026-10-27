using Habit_tracker.Services;

namespace Habit_tracker;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
        ThemeService.Initialize();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(new AppShell());
    }
}
