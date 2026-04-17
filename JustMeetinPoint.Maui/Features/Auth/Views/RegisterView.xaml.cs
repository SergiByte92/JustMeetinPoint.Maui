using JustMeetinPoint.Maui.Features.Auth.Services;
using JustMeetinPoint.Maui.Features.Auth.ViewModels;

namespace JustMeetinPoint.Maui.Features.Auth.Views;

public partial class RegisterView : ContentPage
{
    public RegisterView()
    {
        InitializeComponent();
        BindingContext = new RegisterViewModel(new SocketAuthService());
    }
}