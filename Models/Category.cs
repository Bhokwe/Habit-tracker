using CommunityToolkit.Mvvm.ComponentModel;
using SQLite;

namespace Habit_tracker.Models;

public partial class Category : ObservableObject
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [ObservableProperty]
    string name = "";

    [ObservableProperty]
    string colorHex = "#512BD4";
}
