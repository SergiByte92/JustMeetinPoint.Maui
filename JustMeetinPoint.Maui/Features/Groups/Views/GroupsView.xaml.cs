using JustMeetinPoint.Maui.Features.Groups.ViewModels;

namespace JustMeetinPoint.Maui.Features.Groups.Views;

public partial class GroupsView : ContentPage
{
    public GroupsView(GroupsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}