using Habit_tracker.Models;
using SQLite;

namespace Habit_tracker.Services;

public class DatabaseService
{
    SQLiteAsyncConnection? _database;

    async Task Init()
    {
        if (_database != null)
            return;

        _database = new SQLiteAsyncConnection(Constants.DatabasePath, Constants.Flags);
        await _database.CreateTableAsync<Habit>();
        await _database.CreateTableAsync<Category>();
        await _database.CreateTableAsync<Reward>();
        await _database.CreateTableAsync<HabitLog>();
        await EnsureHabitColumnsAsync();
    }

    async Task EnsureHabitColumnsAsync()
    {
        // Existing installs may lack newer Habit columns — add them safely.
        await TryAddColumnAsync("LastSuccessfulWeekStart", "BIGINT DEFAULT 0");
        await TryAddColumnAsync("ReminderEnabled", "INTEGER DEFAULT 0");
        await TryAddColumnAsync("ReminderTimeTicks", $"BIGINT DEFAULT {TimeSpan.FromHours(9).Ticks}");
    }

    async Task TryAddColumnAsync(string column, string typeSql)
    {
        try
        {
            await _database!.ExecuteAsync($"ALTER TABLE Habit ADD COLUMN {column} {typeSql}");
        }
        catch
        {
            // Column already exists
        }
    }

    // ── Habits ──────────────────────────────────────────────

    public async Task<List<Habit>> GetHabitsAsync()
    {
        await Init();
        return await _database!.Table<Habit>().OrderBy(h => h.Title).ToListAsync();
    }

    public async Task<int> SaveHabitsAsync(Habit habit)
    {
        await Init();
        return await _database!.InsertAsync(habit);
    }

    public async Task<int> UpdateAsync(Habit habit)
    {
        await Init();
        return await _database!.UpdateAsync(habit);
    }

    public async Task<int> DeleteHabitAsync(Habit habit)
    {
        await Init();
        await _database!.Table<HabitLog>().DeleteAsync(l => l.HabitId == habit.Id);
        await _database.Table<Reward>().DeleteAsync(r => r.HabitId == habit.Id);
        return await _database.DeleteAsync(habit);
    }

    // ── Categories ──────────────────────────────────────────

    public async Task<List<Category>> GetCategoriesAsync()
    {
        await Init();
        return await _database!.Table<Category>().OrderBy(c => c.Name).ToListAsync();
    }

    public async Task<int> SaveCategoryAsync(Category category)
    {
        await Init();
        return await _database!.InsertAsync(category);
    }

    public async Task<int> DeleteCategoryAsync(Category category)
    {
        await Init();
        var habits = await _database!.Table<Habit>()
            .Where(h => h.CategoryId == category.Id)
            .ToListAsync();

        foreach (var habit in habits)
        {
            habit.CategoryId = 0;
            await _database.UpdateAsync(habit);
        }

        return await _database.DeleteAsync(category);
    }

    // ── Habit logs (calendar / weekly tally) ────────────────

    public async Task<List<HabitLog>> GetLogsForHabitAsync(int habitId, DateTime from, DateTime to)
    {
        await Init();
        var start = from.Date;
        var end = to.Date.AddDays(1);
        return await _database!.Table<HabitLog>()
            .Where(l => l.HabitId == habitId && l.CompletedDate >= start && l.CompletedDate < end)
            .ToListAsync();
    }

    public async Task<List<HabitLog>> GetLogsSinceAsync(DateTime from)
    {
        await Init();
        var start = from.Date;
        return await _database!.Table<HabitLog>()
            .Where(l => l.CompletedDate >= start)
            .ToListAsync();
    }

    public async Task AddLogAsync(int habitId, DateTime date)
    {
        await Init();
        var day = date.Date;
        var existing = await _database!.Table<HabitLog>()
            .Where(l => l.HabitId == habitId && l.CompletedDate == day)
            .FirstOrDefaultAsync();

        if (existing == null)
            await _database.InsertAsync(new HabitLog { HabitId = habitId, CompletedDate = day });
    }

    public async Task RemoveLogAsync(int habitId, DateTime date)
    {
        await Init();
        var day = date.Date;
        await _database!.Table<HabitLog>()
            .DeleteAsync(l => l.HabitId == habitId && l.CompletedDate == day);
    }

    public async Task<int> CountCompletionsAsync(int habitId, DateTime from, DateTime to)
    {
        await Init();
        var start = from.Date;
        var end = to.Date.AddDays(1);

        if (habitId > 0)
        {
            return await _database!.Table<HabitLog>()
                .Where(l => l.HabitId == habitId && l.CompletedDate >= start && l.CompletedDate < end)
                .CountAsync();
        }

        return await _database!.Table<HabitLog>()
            .Where(l => l.CompletedDate >= start && l.CompletedDate < end)
            .CountAsync();
    }

    // ── Rewards ─────────────────────────────────────────────

    public async Task<List<Reward>> GetRewardsAsync()
    {
        await Init();
        return await _database!.Table<Reward>()
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<int> SaveRewardAsync(Reward reward)
    {
        await Init();
        return await _database!.InsertAsync(reward);
    }

    public async Task<int> UpdateRewardAsync(Reward reward)
    {
        await Init();
        return await _database!.UpdateAsync(reward);
    }

    public async Task<int> DeleteRewardAsync(Reward reward)
    {
        await Init();
        return await _database!.DeleteAsync(reward);
    }
}
