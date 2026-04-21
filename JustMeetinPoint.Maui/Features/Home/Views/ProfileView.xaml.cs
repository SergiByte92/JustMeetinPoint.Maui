using JustMeetinPoint.Maui.Features.Home.ViewModels;

namespace JustMeetinPoint.Maui.Features.Home.Views;

public partial class ProfileView : ContentPage
{
    public ProfileView()
    {
        InitializeComponent();
        BindingContext = new ProfileViewModel();
    }
}