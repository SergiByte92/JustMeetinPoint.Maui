using JustMeetinPoint.Maui.Features.Profile.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace JustMeetinPoint.Maui.Features.Profile.Views;

public partial class ProfileView : ContentPage
{
    private readonly ProfileViewModel _viewModel;

    public ProfileView()
    {
        InitializeComponent();

        _viewModel = App.Current!
            .Handler!
            .MauiContext!
            .Services
            .GetRequiredService<ProfileViewModel>();

        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadCommand.ExecuteAsync(null);
    }
}