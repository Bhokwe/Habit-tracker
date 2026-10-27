using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Habit_tracker.Helpers;
using Habit_tracker.Models;
using Habit_tracker.Services;

namespace Habit_tracker.ViewModels;

public partial class MainViewModel : ObservableObject
{
    readonly DatabaseService _db = new();
    readonly ReminderService _reminders = new();

    static readonly string[] CategoryColors =
    [
        "#512BD4", "#2E7D52", "#E07B39", "#1565C0",
        "#C62828", "#6A1B9A", "#00838F", "#F9A825"
    ];

    public ObservableCollection<Habit> Habits { get; } = [];
    public ObservableCollection<Category> Categories { get; } = [];
    public ObservableCollection<string> CategoryFilterNames { get; } = ["All"];

    [ObservableProperty] string newHabitName = "";
    [ObservableProperty] string newHabitDescription = "";
    [ObservableProperty] string newHabitTargetText = "4";
    [ObservableProperty] string newCategoryName = "";
    [ObservableProperty] Category? selectedCategory;
    [ObservableProperty] string selectedFilter = "All";
    [ObservableProperty] bool showAdvancedAdd;
    [ObservableProperty] bool newReminderEnabled;
    [ObservableProperty] TimeSpan newReminderTime = TimeSpan.FromHours(9);
    [ObservableProperty] int completedTodayCount;
    [ObservableProperty] int totalHabitsCount;
    [ObservableProperty] double todayProgress;
    [ObservableProperty] string progressSummary = "No habits yet";
    [ObservableProperty] bool hasHabits;
    [ObservableProperty] string weekOverview = "";
    [ObservableProperty] string encouragementMessage = "";
    [ObservableProperty] bool showEncouragement;
    [ObservableProperty] string todayGreeting = "Hey there";
    [ObservableProperty] string todayDateLabel = "";
    [ObservableProperty] string progressPercentLabel = "0%";

    List<Habit> _allHabits = [];

    public async Task LoadAsync()
    {
        var categories = await _db.GetCategoriesAsync();
        Categories.Clear();
        CategoryFilterNames.Clear();
        CategoryFilterNames.Add("All");
        foreach (var c in categories)
        {
            Categories.Add(c);
            CategoryFilterNames.Add(c.Name);
        }

        var habits = await _db.GetHabitsAsync();
        var today = DateTime.Today;
        var weekStart = StartOfWeek(today);
        var previousWeekStart = weekStart.AddDays(-7);
        var logsFromPrevWeek = await _db.GetLogsSinceAsync(previousWeekStart);

        var encouragementBits = new List<string>();

        _allHabits.Clear();
        foreach (var habit in habits)
        {
            if (habit.IsCompleted && habit.LastCompletedDate.Date < today)
            {
                habit.IsCompleted = false;
                await _db.UpdateAsync(habit);
            }

            ApplyCategory(habit, categories);
            ApplyWeekTally(habit, logsFromPrevWeek, weekStart, today);

            // Daily goals break as soon as a day is missed; weekly goals wait until week-end
            (string Title, int PreviousStreak, bool IsDaily)? broken = habit.IsDailyGoal
                ? await TryBreakDailyStreakAsync(habit, today)
                : await TryBreakStreakForMissedWeekAsync(
                    habit, previousWeekStart, weekStart, logsFromPrevWeek);

            if (broken is { } info)
                encouragementBits.Add(EncouragementQuotes.ForHabit(info.Title, info.PreviousStreak, info.IsDaily));

            _allHabits.Add(habit);
        }

        ApplyFilter();
        UpdateProgress();
        await _reminders.RescheduleAllAsync(_allHabits);

        if (encouragementBits.Count > 0)
        {
            EncouragementMessage = string.Join("\n\n", encouragementBits.Take(2));
            ShowEncouragement = true;
        }
    }

    /// <summary>
    /// Daily goals: if the last completion wasn't today or yesterday, the streak is gone.
    /// </summary>
    async Task<(string Title, int PreviousStreak, bool IsDaily)?> TryBreakDailyStreakAsync(
        Habit habit,
        DateTime today)
    {
        if (!habit.IsDailyGoal || habit.StreakCount <= 0)
            return null;

        var last = habit.LastCompletedDate.Date;
        if (last == DateTime.MinValue.Date)
            return null;

        // Still alive if done today, or done yesterday (can still complete today)
        if (last == today || last == today.AddDays(-1))
            return null;

        var previousStreak = habit.StreakCount;
        habit.StreakCount = 0;
        await _db.UpdateAsync(habit);
        habit.NotifyDisplay();
        return (habit.Title, previousStreak, true);
    }

    /// <summary>
    /// Weekly goals (1–6×/week): only reset after a full week ends without hitting the target.
    /// </summary>
    async Task<(string Title, int PreviousStreak, bool IsDaily)?> TryBreakStreakForMissedWeekAsync(
        Habit habit,
        DateTime previousWeekStart,
        DateTime currentWeekStart,
        IEnumerable<HabitLog> logs)
    {
        if (!habit.UsesWeeklyTarget || habit.StreakCount <= 0)
            return null;

        // Still counting last week as successful — nothing to break
        if (habit.LastSuccessfulWeekStart >= previousWeekStart)
            return null;

        var prevWeekCount = logs.Count(l =>
            l.HabitId == habit.Id
            && l.CompletedDate.Date >= previousWeekStart
            && l.CompletedDate.Date < currentWeekStart);

        if (prevWeekCount >= habit.TargetPerWeek)
        {
            habit.LastSuccessfulWeekStart = previousWeekStart;
            await _db.UpdateAsync(habit);
            return null;
        }

        var previousStreak = habit.StreakCount;
        habit.StreakCount = 0;
        await _db.UpdateAsync(habit);
        habit.NotifyDisplay();
        return (habit.Title, previousStreak, false);
    }

    [RelayCommand]
    void DismissEncouragement()
    {
        ShowEncouragement = false;
        EncouragementMessage = string.Empty;
    }

    [RelayCommand]
    void ToggleAdvancedAdd() => ShowAdvancedAdd = !ShowAdvancedAdd;

    [RelayCommand]
    async Task AddCategory()
    {
        var name = NewCategoryName.Trim();
        if (string.IsNullOrWhiteSpace(name))
            return;

        if (Categories.Any(c => c.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
        {
            await Shell.Current.DisplayAlert("Already exists", $"Category \"{name}\" already exists.", "OK");
            return;
        }

        var color = CategoryColors[Categories.Count % CategoryColors.Length];
        var category = new Category { Name = name, ColorHex = color };
        await _db.SaveCategoryAsync(category);
        Categories.Add(category);
        CategoryFilterNames.Add(category.Name);
        SelectedCategory = category;
        NewCategoryName = string.Empty;
    }

    [RelayCommand]
    async Task DeleteCategory(Category? category)
    {
        if (category == null) return;

        var confirm = await Shell.Current.DisplayAlert(
            "Remove category",
            $"Remove \"{category.Name}\"? Habits in it stay, but lose this category.",
            "Remove",
            "Cancel");

        if (!confirm) return;

        await _db.DeleteCategoryAsync(category);
        Categories.Remove(category);
        CategoryFilterNames.Remove(category.Name);
        if (SelectedCategory?.Id == category.Id)
            SelectedCategory = null;
        if (SelectedFilter == category.Name)
            SelectedFilter = "All";

        await LoadAsync();
    }

    [RelayCommand]
    async Task AddHabit()
    {
        var name = NewHabitName.Trim();
        if (string.IsNullOrWhiteSpace(name))
            return;

        if (_allHabits.Any(h => h.Title.Equals(name, StringComparison.OrdinalIgnoreCase)))
        {
            await Shell.Current.DisplayAlert("Already exists", $"\"{name}\" is already on your list.", "OK");
            return;
        }

        _ = int.TryParse(NewHabitTargetText.Trim(), out var target);
        if (target < 0) target = 0;
        if (target > 7) target = 7;

        var habit = new Habit
        {
            Title = name,
            Description = NewHabitDescription.Trim(),
            CategoryId = SelectedCategory?.Id ?? 0,
            TargetPerWeek = target,
            ReminderEnabled = NewReminderEnabled,
            ReminderTimeTicks = NewReminderTime.Ticks
        };

        await _db.SaveHabitsAsync(habit);
        ApplyCategory(habit, Categories.ToList());
        ApplyWeekTally(habit, [], StartOfWeek(DateTime.Today), DateTime.Today);
        await _reminders.ScheduleAsync(habit);

        // Permission may have turned reminder off
        if (habit.ReminderEnabled != NewReminderEnabled)
            await _db.UpdateAsync(habit);

        _allHabits.Add(habit);
        NewHabitName = string.Empty;
        NewHabitDescription = string.Empty;
        NewHabitTargetText = "4";
        NewReminderEnabled = false;
        NewReminderTime = TimeSpan.FromHours(9);
        ShowAdvancedAdd = false;
        ApplyFilter();
        UpdateProgress();
    }

    [RelayCommand]
    async Task TapHabit(Habit? habit)
    {
        if (habit == null) return;

        var today = DateTime.Today;
        var yesterday = today.AddDays(-1);
        var lastDate = habit.LastCompletedDate.Date;
        var weekStart = StartOfWeek(today);
        var wasSecured = habit.IsWeekSecured;

        if (!habit.IsCompleted)
        {
            habit.IsCompleted = true;
            habit.LastCompletedDate = DateTime.Now;
            await _db.AddLogAsync(habit.Id, today);

            var logs = await _db.GetLogsForHabitAsync(habit.Id, weekStart, today);
            ApplyWeekTally(habit, logs, weekStart, today);

            if (habit.IsDailyGoal)
            {
                // Classic daily streak: consecutive calendar days
                if (lastDate == yesterday)
                    habit.StreakCount++;
                else if (lastDate < yesterday || lastDate == DateTime.MinValue.Date)
                    habit.StreakCount = 1;
            }
            else
            {
                await TrySecureWeeklyStreakAsync(habit, weekStart, wasSecured);
            }
        }
        else
        {
            habit.IsCompleted = false;
            if (lastDate == today)
                habit.LastCompletedDate = DateTime.MinValue;

            await _db.RemoveLogAsync(habit.Id, today);

            var logs = await _db.GetLogsForHabitAsync(habit.Id, weekStart, today);
            ApplyWeekTally(habit, logs, weekStart, today);

            if (habit.IsDailyGoal)
            {
                if (lastDate == today && habit.StreakCount > 0)
                    habit.StreakCount--;
            }
            else if (wasSecured && !habit.IsWeekSecured && habit.LastSuccessfulWeekStart == weekStart)
            {
                if (habit.StreakCount > 0)
                    habit.StreakCount--;

                var previousWeekStart = weekStart.AddDays(-7);
                habit.LastSuccessfulWeekStart = habit.StreakCount > 0
                    ? previousWeekStart
                    : DateTime.MinValue;
            }
        }

        await _db.UpdateAsync(habit);
        habit.NotifyDisplay();
        UpdateProgress();
    }

    async Task TrySecureWeeklyStreakAsync(Habit habit, DateTime weekStart, bool wasAlreadySecured)
    {
        if (!habit.IsWeekSecured || wasAlreadySecured)
            return;

        if (habit.LastSuccessfulWeekStart == weekStart)
            return;

        var previousWeekStart = weekStart.AddDays(-7);

        if (habit.StreakCount == 0
            || habit.LastSuccessfulWeekStart == previousWeekStart
            || habit.LastSuccessfulWeekStart == DateTime.MinValue)
        {
            habit.StreakCount++;
        }
        else if (habit.LastSuccessfulWeekStart < previousWeekStart)
        {
            habit.StreakCount = 1;
        }

        habit.LastSuccessfulWeekStart = weekStart;
        await _db.UpdateAsync(habit);
        habit.NotifyDisplay();
    }

    [RelayCommand]
    async Task DeleteHabit(Habit? habit)
    {
        if (habit == null) return;

        var confirm = await Shell.Current.DisplayAlert(
            "Remove habit",
            $"Remove \"{habit.Title}\"? Related rewards and history for it will also be removed.",
            "Remove",
            "Cancel");

        if (!confirm) return;

        _reminders.Cancel(habit.Id);
        await _db.DeleteHabitAsync(habit);
        _allHabits.Remove(habit);
        Habits.Remove(habit);
        UpdateProgress();
    }

    [RelayCommand]
    async Task EditReminder(Habit? habit)
    {
        if (habit == null) return;

        var action = await Shell.Current.DisplayActionSheet(
            $"Reminder for \"{habit.Title}\"",
            "Cancel",
            habit.ReminderEnabled ? "Turn off reminder" : null,
            habit.ReminderEnabled ? "Change time" : "Turn on reminder");

        if (action is null or "Cancel")
            return;

        if (action == "Turn off reminder")
        {
            habit.ReminderEnabled = false;
            _reminders.Cancel(habit.Id);
            await _db.UpdateAsync(habit);
            habit.NotifyDisplay();
            return;
        }

        // Turn on or change time — ask for HH:mm
        var current = DateTime.Today.Add(habit.ReminderTime).ToString("HH:mm");
        var input = await Shell.Current.DisplayPromptAsync(
            "Reminder time",
            "Enter time as HH:mm (24-hour), e.g. 07:30 or 18:00",
            "Save",
            "Cancel",
            current,
            maxLength: 5,
            keyboard: Keyboard.Plain);

        if (string.IsNullOrWhiteSpace(input))
            return;

        if (!TimeSpan.TryParse(input.Trim(), out var time) || time < TimeSpan.Zero || time >= TimeSpan.FromDays(1))
        {
            await Shell.Current.DisplayAlert("Invalid time", "Please use HH:mm, like 09:00.", "OK");
            return;
        }

        habit.ReminderEnabled = true;
        habit.ReminderTimeTicks = time.Ticks;
        await _reminders.ScheduleAsync(habit);

        if (!habit.ReminderEnabled)
        {
            await Shell.Current.DisplayAlert(
                "Permission needed",
                "Notifications are off for this app. Enable them in system settings to get reminders.",
                "OK");
        }

        await _db.UpdateAsync(habit);
        habit.NotifyDisplay();
    }

    [RelayCommand]
    void FilterBy(string? filter)
    {
        SelectedFilter = string.IsNullOrWhiteSpace(filter) ? "All" : filter;
        ApplyFilter();
    }

    void ApplyFilter()
    {
        Habits.Clear();
        IEnumerable<Habit> query = _allHabits;

        if (SelectedFilter != "All")
            query = query.Where(h => h.CategoryName.Equals(SelectedFilter, StringComparison.OrdinalIgnoreCase));

        foreach (var habit in query.OrderBy(h => h.Title))
            Habits.Add(habit);

        HasHabits = Habits.Count > 0;
    }

    void ApplyCategory(Habit habit, List<Category> categories)
    {
        var cat = categories.FirstOrDefault(c => c.Id == habit.CategoryId);
        habit.CategoryName = cat?.Name ?? "";
        habit.CategoryColor = cat?.ColorHex ?? "#512BD4";
        habit.NotifyDisplay();
    }

    void ApplyWeekTally(Habit habit, IEnumerable<HabitLog> logs, DateTime weekStart, DateTime today)
    {
        var habitLogs = logs
            .Where(l => l.HabitId == habit.Id && l.CompletedDate.Date >= weekStart && l.CompletedDate.Date <= today)
            .Select(l => l.CompletedDate.Date)
            .ToHashSet();

        var dots = new List<string>();
        for (var i = 0; i < 7; i++)
        {
            var day = weekStart.AddDays(i);
            if (day > today)
                dots.Add("·");
            else if (habitLogs.Contains(day))
                dots.Add("●");
            else
                dots.Add("○");
        }

        habit.CompletionsThisWeek = habitLogs.Count;
        habit.WeekTallyDisplay = string.Join(" ", dots);
        habit.NotifyDisplay();
    }

    void UpdateProgress()
    {
        var hour = DateTime.Now.Hour;
        TodayGreeting = hour switch
        {
            < 12 => "Good morning",
            < 17 => "Good afternoon",
            _ => "Good evening"
        };
        TodayDateLabel = DateTime.Today.ToString("dddd, MMM d");

        TotalHabitsCount = _allHabits.Count;
        CompletedTodayCount = _allHabits.Count(h => h.IsCompleted);
        HasHabits = Habits.Count > 0;
        TodayProgress = TotalHabitsCount > 0
            ? (double)CompletedTodayCount / TotalHabitsCount
            : 0;
        ProgressPercentLabel = $"{(int)Math.Round(TodayProgress * 100)}%";

        var weekTotal = _allHabits.Sum(h => h.CompletionsThisWeek);
        var secured = _allHabits.Count(h => h.IsWeekSecured);
        WeekOverview = TotalHabitsCount == 0
            ? "Start small. One habit is enough."
            : $"{weekTotal} this week · {secured} secured";

        ProgressSummary = TotalHabitsCount switch
        {
            0 => "Ready when you are",
            _ when CompletedTodayCount == TotalHabitsCount => "All done — treat yourself kindly",
            _ when CompletedTodayCount == 0 => "One tap to begin today",
            _ => $"{CompletedTodayCount} of {TotalHabitsCount} done"
        };
    }

    static DateTime StartOfWeek(DateTime date)
    {
        var diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
        return date.Date.AddDays(-diff);
    }
}
