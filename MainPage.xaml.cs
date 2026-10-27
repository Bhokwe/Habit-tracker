using Habit_tracker.ViewModels;

namespace Habit_tracker;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
        var viewModel = new MainViewModel
        {
            ScrollToForm = () => CreateScroll.ScrollToAsync(CreateForm, ScrollToPosition.Start, true)
        };
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is MainViewModel vm)
            await vm.LoadAsync();
    }
}
