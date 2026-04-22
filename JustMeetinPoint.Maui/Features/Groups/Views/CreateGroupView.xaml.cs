using JustMeetinPoint.Maui.Features.Groups.ViewModels;

namespace JustMeetinPoint.Maui.Features.Groups.Views;

public partial class CreateGroupView : ContentPage
{
    public CreateGroupView(CreateGroupViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}