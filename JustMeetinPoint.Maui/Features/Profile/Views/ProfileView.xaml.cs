using JustMeetinPoint.Maui.Features.Profile.ViewModels;

namespace JustMeetinPoint.Maui.Features.Profile.Views;

public partial class ProfileView : ContentPage
{
    public ProfileView()
    {
        InitializeComponent();
        BindingContext = new ProfileViewModel();
    }
}