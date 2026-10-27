namespace Habit_tracker.Helpers;

public static class EncouragementQuotes
{
    static readonly string[] Quotes =
    [
        "A missed week isn't failure — it's feedback. Start again with one small step.",
        "Progress isn't perfection. Getting back up is the habit that matters most.",
        "Streaks reset. Your ability to begin again never does.",
        "Be kind to yourself. Consistency is built by returning, not by never falling.",
        "Every champion has off weeks. What counts is that you show up for the next one.",
        "You didn't lose your progress — you learned what this week needed. Try again.",
        "Rest if you need to, then rise. One session is enough to restart the fire.",
        "Missing a goal doesn't erase the days you showed up. Begin where you are."
    ];

    static readonly Random Random = new();

    public static string Next() => Quotes[Random.Next(Quotes.Length)];

    public static string ForHabit(string habitTitle, int previousStreak, bool isDaily)
    {
        var quote = Next();
        var unit = isDaily ? "day" : "week";
        var streakNote = previousStreak > 0
            ? $" Your {previousStreak}-{unit} streak on \"{habitTitle}\" paused — and that's okay."
            : $" \"{habitTitle}\" can start fresh.";
        return quote + streakNote;
    }
}
