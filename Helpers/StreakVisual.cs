namespace Habit_tracker.Helpers;

/// <summary>Snapchat/TikTok-style streak tiers — weekly streaks grow the visual.</summary>
public static class StreakVisual
{
    public static string GetEmoji(int streak) => streak switch
    {
        <= 0 => "🌱",
        <= 2 => "🔥",
        <= 6 => "⚡",
        <= 13 => "💪",
        <= 29 => "🌟",
        _ => "🏆"
    };

    public static string GetWeeklyLabel(int streakWeeks, bool weekSecured, int targetPerWeek) => streakWeeks switch
    {
        <= 0 when weekSecured => $"Week secured ({targetPerWeek}/week) — streak starts!",
        <= 0 => $"Aim for {targetPerWeek}× this week",
        1 when weekSecured => "1 week secured 🔥",
        1 => "1-week streak — keep it going",
        <= 2 when weekSecured => $"{streakWeeks}-week streak secured 🔥",
        <= 2 => $"{streakWeeks}-week streak 🔥",
        <= 6 when weekSecured => $"{streakWeeks}-week streak secured ⚡",
        <= 6 => $"{streakWeeks}-week streak ⚡",
        <= 13 => $"{streakWeeks} weeks strong 💪",
        <= 29 => $"{streakWeeks}-week legend 🌟",
        _ => $"{streakWeeks}-week champion 🏆"
    };

    public static string GetDailyLabel(int streak) => streak switch
    {
        <= 0 => "Start your streak",
        1 => "1 day — keep going",
        <= 2 => $"{streak} days 🔥",
        <= 6 => $"{streak}-day streak ⚡",
        <= 13 => $"{streak} days strong 💪",
        <= 29 => $"{streak}-day legend 🌟",
        _ => $"{streak}-day champion 🏆"
    };
}
