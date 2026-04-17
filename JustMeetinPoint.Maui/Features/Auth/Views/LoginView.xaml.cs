using JustMeetinPoint.Maui.Features.Auth.Services;
using JustMeetinPoint.Maui.Features.Auth.ViewModels;

namespace JustMeetinPoint.Maui.Features.Auth.Views
{
    public partial class LoginView : ContentPage
    {
        public LoginView()
        {
            InitializeComponent();
            BindingContext = new LoginViewModel(new SocketAuthService());
        }
    }
}