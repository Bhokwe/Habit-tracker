using Habit_tracker.ViewModels; // Don't forget this using statement!

namespace Habit_tracker;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();

        BindingContext = new MainViewModel();
    }
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        var vm = (MainViewModel)BindingContext;
        await vm.LoadHabits();

    }
}