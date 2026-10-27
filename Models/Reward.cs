using CommunityToolkit.Mvvm.ComponentModel;
using SQLite;

namespace Habit_tracker.Models;

public partial class Reward : ObservableObject
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [ObservableProperty]
    string title = "";

    [ObservableProperty]
    string description = "";

    /// <summary>Habit this reward is tied to. 0 = overall weekly completions.</summary>
    [ObservableProperty]
    int habitId;

    [ObservableProperty]
    int targetCount = 5;

    /// <summary>Window length in days (7 = this week).</summary>
    [ObservableProperty]
    int periodDays = 7;

    [ObservableProperty]
    bool isClaimed;

    [ObservableProperty]
    DateTime? claimedDate;

    [ObservableProperty]
    DateTime createdAt = DateTime.Now;

    [Ignore]
    public bool HasDescription => !string.IsNullOrWhiteSpace(Description);

    [Ignore]
    public string HabitTitle { get; set; } = "Any habit";

    [Ignore]
    public int CurrentCount { get; set; }

    [Ignore]
    public double Progress => TargetCount <= 0 ? 0 : Math.Min(1.0, (double)CurrentCount / TargetCount);

    [Ignore]
    public bool IsReady => !IsClaimed && CurrentCount >= TargetCount;

    [Ignore]
    public string ProgressLabel =>
        IsClaimed
            ? "Claimed — enjoy it!"
            : IsReady
                ? "Ready to claim!"
                : $"{CurrentCount} of {TargetCount} this week";

    [Ignore]
    public string RuleSummary =>
        $"Do \"{HabitTitle}\" {TargetCount}× in {PeriodDays} days → {Title}";

    partial void OnDescriptionChanged(string value) =>
        OnPropertyChanged(nameof(HasDescription));

    public void NotifyProgress()
    {
        OnPropertyChanged(nameof(CurrentCount));
        OnPropertyChanged(nameof(Progress));
        OnPropertyChanged(nameof(IsReady));
        OnPropertyChanged(nameof(ProgressLabel));
        OnPropertyChanged(nameof(RuleSummary));
        OnPropertyChanged(nameof(HabitTitle));
        OnPropertyChanged(nameof(HasDescription));
    }
}
