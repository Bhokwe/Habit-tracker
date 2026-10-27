using Microsoft.Extensions.Logging;
using Plugin.LocalNotification;

namespace Habit_tracker;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        // Ensure SQLite native binaries are loaded on all platforms
        try
        {
            SQLitePCL.Batteries_V2.Init();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"SQLite init failed: {ex}");
        }

        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        try
        {
            builder.UseLocalNotification();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Notifications failed: {ex}");
        }

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
