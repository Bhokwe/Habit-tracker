using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Habit_tracker.Models;
using Habit_tracker.Services;

namespace Habit_tracker.ViewModels;

public partial class RewardsViewModel : ObservableObject
{
    readonly DatabaseService _db = new();

    public ObservableCollection<Reward> Rewards { get; } = [];
    public ObservableCollection<HabitOption> HabitOptions { get; } = [];

    [ObservableProperty] string newRewardTitle = "";
    [ObservableProperty] string newRewardDescription = "";
    [ObservableProperty] string newTargetText = "5";
    [ObservableProperty] HabitOption? selectedHabit;
    [ObservableProperty] bool hasRewards;
    [ObservableProperty] string insightSummary = "Set a reward to motivate consistency.";

    public async Task LoadAsync()
    {
        var habits = await _db.GetHabitsAsync();
        HabitOptions.Clear();
        HabitOptions.Add(new HabitOption { Id = 0, Title = "Any habit (overall)" });
        foreach (var h in habits)
            HabitOptions.Add(new HabitOption { Id = h.Id, Title = h.Title });

        SelectedHabit ??= HabitOptions.FirstOrDefault();

        var rewards = await _db.GetRewardsAsync();
        var weekStart = StartOfWeek(DateTime.Today);
        var today = DateTime.Today;

        Rewards.Clear();
        foreach (var reward in rewards)
        {
            reward.HabitTitle = HabitOptions.FirstOrDefault(h => h.Id == reward.HabitId)?.Title ?? "Any habit";
            reward.CurrentCount = await _db.CountCompletionsAsync(reward.HabitId, weekStart, today);
            reward.NotifyProgress();
            Rewards.Add(reward);
        }

        HasRewards = Rewards.Count > 0;
        UpdateInsight();
    }

    [RelayCommand]
    async Task AddReward()
    {
        var title = NewRewardTitle.Trim();
        if (string.IsNullOrWhiteSpace(title))
        {
            await Shell.Current.DisplayAlert("Missing reward", "What will you treat yourself to?", "OK");
            return;
        }

        if (!int.TryParse(NewTargetText.Trim(), out var target) || target < 1)
            target = 5;
        if (target > 30) target = 30;

        var reward = new Reward
        {
            Title = title,
            Description = NewRewardDescription.Trim(),
            HabitId = SelectedHabit?.Id ?? 0,
            TargetCount = target,
            PeriodDays = 7,
            CreatedAt = DateTime.Now
        };

        await _db.SaveRewardAsync(reward);
        NewRewardTitle = string.Empty;
        NewRewardDescription = string.Empty;
        NewTargetText = "5";
        await LoadAsync();
    }

    [RelayCommand]
    async Task ClaimReward(Reward? reward)
    {
        if (reward == null || reward.IsClaimed) return;

        if (!reward.IsReady)
        {
            await Shell.Current.DisplayAlert(
                "Not yet",
                $"You've got {reward.CurrentCount} of {reward.TargetCount}. Keep going!",
                "OK");
            return;
        }

        var confirm = await Shell.Current.DisplayAlert(
            "Claim reward",
            $"You've earned \"{reward.Title}\". Claim it and enjoy?",
            "Claim",
            "Not yet");

        if (!confirm) return;

        reward.IsClaimed = true;
        reward.ClaimedDate = DateTime.Now;
        await _db.UpdateRewardAsync(reward);
        reward.NotifyProgress();
        UpdateInsight();
    }

    [RelayCommand]
    async Task DeleteReward(Reward? reward)
    {
        if (reward == null) return;

        var confirm = await Shell.Current.DisplayAlert(
            "Remove reward",
            $"Remove \"{reward.Title}\"?",
            "Remove",
            "Cancel");

        if (!confirm) return;

        await _db.DeleteRewardAsync(reward);
        Rewards.Remove(reward);
        HasRewards = Rewards.Count > 0;
        UpdateInsight();
    }

    void UpdateInsight()
    {
        var active = Rewards.Where(r => !r.IsClaimed).ToList();
        if (active.Count == 0)
        {
            InsightSummary = Rewards.Count == 0
                ? "Example: Gym 5× this week → Ice cream. Make it specific and attainable."
                : "All rewards claimed. Set a new one to keep momentum.";
            return;
        }

        var ready = active.Count(r => r.IsReady);
        var closest = active.OrderByDescending(r => r.Progress).First();
        InsightSummary = ready > 0
            ? $"{ready} reward{(ready == 1 ? "" : "s")} ready to claim — you earned {(ready == 1 ? "it" : "them")}!"
            : $"Closest: {closest.Title} ({closest.CurrentCount}/{closest.TargetCount}). Areas with lower progress need a little more love.";
    }

    static DateTime StartOfWeek(DateTime date)
    {
        var diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
        return date.Date.AddDays(-diff);
    }
}

public class HabitOption
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public override string ToString() => Title;
}
