namespace Habit_tracker.Models;

public class Habit
{
    public string Title { get; set; } = "";
    public bool IsCompleted { get; set; }
    public int StreakCount { get; set; }
    public DateTime LastCompletedDate { get; set; }

    // Constructor to make it easier to create new ones
    public Habit()
    {
        LastCompletedDate = DateTime.MinValue; // Never completed
        StreakCount = 0;
    }
}