using CommunityToolkit.Mvvm.ComponentModel;
using Habit_tracker.Helpers;
using SQLite;

namespace Habit_tracker.Models;

public partial class Habit : ObservableObject
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [ObservableProperty]
    string title = "";

    /// <summary>Optional SMART detail — how/when/what specifically.</summary>
    [ObservableProperty]
    string description = "";

    [ObservableProperty]
    int categoryId;

    [ObservableProperty]
    bool isCompleted;

    /// <summary>Consecutive weeks (or days if no weekly target) meeting the goal.</summary>
    [ObservableProperty]
    int streakCount;

    [ObservableProperty]
    DateTime lastCompletedDate;

    /// <summary>
    /// Times per week goal.
    /// 0 or 7 = daily goal (miss a day → streak breaks immediately).
    /// 1–6 = flexible weekly goal (streak holds once target is met that week).
    /// </summary>
    [ObservableProperty]
    int targetPerWeek;

    /// <summary>Monday of the last week where the weekly target was met.</summary>
    [ObservableProperty]
    DateTime lastSuccessfulWeekStart;

    [ObservableProperty]
    bool reminderEnabled;

    /// <summary>Time of day for the reminder, stored as TimeSpan ticks.</summary>
    [ObservableProperty]
    long reminderTimeTicks = TimeSpan.FromHours(9).Ticks;

    [Ignore]
    public string CategoryName { get; set; } = "";

    [Ignore]
    public string CategoryColor { get; set; } = "#512BD4";

    [Ignore]
    public bool HasCategory => CategoryId > 0 && !string.IsNullOrWhiteSpace(CategoryName);

    [Ignore]
    public bool HasDescription => !string.IsNullOrWhiteSpace(Description);

    /// <summary>Daily goals: every day matters. Miss tomorrow → streak resets.</summary>
    [Ignore]
    public bool IsDailyGoal => TargetPerWeek is 0 or 7;

    /// <summary>Flexible weekly goals (1–6×/week): secure the week, then rest days are OK.</summary>
    [Ignore]
    public bool UsesWeeklyTarget => TargetPerWeek is >= 1 and <= 6;

    [Ignore]
    public bool IsWeekSecured =>
        UsesWeeklyTarget && CompletionsThisWeek >= TargetPerWeek;

    [Ignore]
    public string StreakEmoji => StreakVisual.GetEmoji(StreakCount);

    [Ignore]
    public string StreakDisplay =>
        IsDailyGoal
            ? StreakVisual.GetDailyLabel(StreakCount)
            : StreakVisual.GetWeeklyLabel(StreakCount, IsWeekSecured, TargetPerWeek);

    [Ignore]
    public string WeekTallyDisplay { get; set; } = "○ ○ ○ ○ ○ ○ ○";

    [Ignore]
    public int CompletionsThisWeek { get; set; }

    [Ignore]
    public string WeekProgressLabel =>
        IsDailyGoal
            ? IsCompleted
                ? "Daily goal done today ✓"
                : "Daily goal — miss a day and the streak resets"
            : IsWeekSecured
                ? $"Week secured — {CompletionsThisWeek}/{TargetPerWeek} ✓"
                : $"{CompletionsThisWeek}/{TargetPerWeek} this week";

    [Ignore]
    public TimeSpan ReminderTime
    {
        get => TimeSpan.FromTicks(ReminderTimeTicks);
        set => ReminderTimeTicks = value.Ticks;
    }

    [Ignore]
    public string ReminderLabel =>
        ReminderEnabled
            ? $"Reminder {DateTime.Today.Add(ReminderTime):t}"
            : "No reminder";

    public Habit()
    {
        LastCompletedDate = DateTime.MinValue;
        LastSuccessfulWeekStart = DateTime.MinValue;
        StreakCount = 0;
        IsCompleted = false;
        ReminderEnabled = false;
        ReminderTimeTicks = TimeSpan.FromHours(9).Ticks;
    }

    partial void OnIsCompletedChanged(bool value) => NotifyDisplay();
    partial void OnStreakCountChanged(int value) => NotifyDisplay();
    partial void OnDescriptionChanged(string value) => OnPropertyChanged(nameof(HasDescription));
    partial void OnCategoryIdChanged(int value) => OnPropertyChanged(nameof(HasCategory));
    partial void OnTargetPerWeekChanged(int value) => NotifyDisplay();
    partial void OnLastSuccessfulWeekStartChanged(DateTime value) => NotifyDisplay();
    partial void OnReminderEnabledChanged(bool value) => NotifyDisplay();
    partial void OnReminderTimeTicksChanged(long value) => NotifyDisplay();

    public void NotifyDisplay()
    {
        OnPropertyChanged(nameof(StreakEmoji));
        OnPropertyChanged(nameof(StreakDisplay));
        OnPropertyChanged(nameof(WeekTallyDisplay));
        OnPropertyChanged(nameof(CompletionsThisWeek));
        OnPropertyChanged(nameof(WeekProgressLabel));
        OnPropertyChanged(nameof(HasCategory));
        OnPropertyChanged(nameof(HasDescription));
        OnPropertyChanged(nameof(CategoryName));
        OnPropertyChanged(nameof(CategoryColor));
        OnPropertyChanged(nameof(IsDailyGoal));
        OnPropertyChanged(nameof(UsesWeeklyTarget));
        OnPropertyChanged(nameof(IsWeekSecured));
        OnPropertyChanged(nameof(ReminderTime));
        OnPropertyChanged(nameof(ReminderLabel));
    }
}
