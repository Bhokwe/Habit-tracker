using Microsoft.Extensions.Logging;
using Plugin.LocalNotification;

namespace Habit_tracker;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        // Ensure SQLite native binaries are loaded on all platforms
        SQLitePCL.Batteries_V2.Init();

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
        catch
        {
            // Notifications are optional — app must still launch without them
        }

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
