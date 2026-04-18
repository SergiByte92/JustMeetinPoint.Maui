using JustMeetinPoint.Maui.Features.Home.ViewModels;

namespace JustMeetinPoint.Maui.Features.Home.Views;

public partial class GroupLobbyView : ContentPage
{
    public GroupLobbyView(GroupLobbyViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}