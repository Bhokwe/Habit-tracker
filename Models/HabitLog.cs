using SQLite;

namespace Habit_tracker.Models;

/// <summary>One completion on one calendar day for a habit.</summary>
public class HabitLog
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Indexed]
    public int HabitId { get; set; }

    /// <summary>Date-only (time stripped) of the completion.</summary>
    [Indexed]
    public DateTime CompletedDate { get; set; }
}
