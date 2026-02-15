using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using Habit_tracker.Models;
using Habit_tracker.Services;

namespace Habit_tracker.ViewModels;

// 'partial' is required for the Toolkit to generate code for us
public partial class MainViewModel : ObservableObject
{
    // this is so that the MVM uses the actual database service and not fake RAM
    DatabaseService _databaseService = new DatabaseService();
 
    public ObservableCollection<Habit> Habits { get; } = new ObservableCollection<Habit>();

    
    [ObservableProperty]
    string newHabitName="";

    
    [RelayCommand]
    async Task AddHabit()
    {
       
        if (string.IsNullOrWhiteSpace(NewHabitName))
            return;

        // Create the new habit
        var newHabit = new Habit
        {
            Title = NewHabitName
        };

        await _databaseService.SaveHabitsAsync(newHabit);
       
        Habits.Add(newHabit);

       

        // Clear the input box
        NewHabitName = string.Empty;
    }

    public async Task LoadHabits()
    {
        var habits = await _databaseService.GetHabitsAsync();
        Habits.Clear();

        foreach (var habit in habits)
        {
            Habits.Add(habit);
        }
    }

    [RelayCommand]
    async Task TapHabit(Habit habit) 
    {
        if (habit == null) return;

        //check date logic
        var today = DateTime.Today;
        var yesterday = today.AddDays(-1);

        habit.IsCompleted= !habit.IsCompleted;

        if (habit.IsCompleted) 
        { 
            if(habit.LastCompletedDate == yesterday)
            {
                habit.StreakCount++;
            }
            else if (habit.LastCompletedDate < yesterday)
            {
                habit.StreakCount = 1;
            }
            else if (habit.LastCompletedDate.Date == today)
            {
                // Already done today, don't increase streak again
            }

            // Save the date
            habit.LastCompletedDate = DateTime.Now;
        }
        
        //database update logic
        await _databaseService.UpdateAsync(habit);

        //add sound here to play when habit is completed
    }

}