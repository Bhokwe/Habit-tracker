namespace Habit_tracker.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using SQLite;

public partial class Habit: ObservableObject
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }


    [ObservableProperty]
    string title;

    [ObservableProperty]
    bool isCompleted;

    [ObservableProperty]
    int streakCount;

    [ObservableProperty]
    DateTime lastCompletedDate;

    // Constructor to make it easier to create new ones
    public Habit()
    {
        LastCompletedDate = DateTime.MinValue; // Never completed
        StreakCount = 0;
        isCompleted = false;
    }
}