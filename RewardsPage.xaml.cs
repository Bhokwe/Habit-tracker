using Habit_tracker.ViewModels;

namespace Habit_tracker;

public partial class RewardsPage : ContentPage
{
    public RewardsPage()
    {
        InitializeComponent();
        BindingContext = new RewardsViewModel();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is RewardsViewModel vm)
            await vm.LoadAsync();
    }
}
