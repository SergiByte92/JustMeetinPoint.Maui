using JustMeetinPoint.Maui.Features.Groups.ViewModels;

namespace JustMeetinPoint.Maui.Features.Groups.Views;

public partial class GroupLobbyView : ContentPage
{
    public GroupLobbyView(GroupLobbyViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}