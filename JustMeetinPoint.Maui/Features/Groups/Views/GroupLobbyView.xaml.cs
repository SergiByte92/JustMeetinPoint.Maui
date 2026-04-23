using JustMeetinPoint.Maui.Features.Groups.ViewModels;

namespace JustMeetinPoint.Maui.Features.Groups.Views;

public partial class GroupLobbyView : ContentPage
{
    private readonly GroupLobbyViewModel _viewModel;

    public GroupLobbyView(GroupLobbyViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.StartAutoRefreshLoop();
    }

    protected override void OnDisappearing()
    {
        _viewModel.StopAutoRefreshLoop();
        base.OnDisappearing();
    }
}