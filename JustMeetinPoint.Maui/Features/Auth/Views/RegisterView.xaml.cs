using Microsoft.Maui.Controls;
using JustMeetingPoint.Maui.Features.Auth.ViewModels;

namespace JustMeetingPoint.Maui.Features.Auth.Views
{
    public partial class RegisterView : ContentPage
    {
        public RegisterView()
        {
            InitializeComponent();
            BindingContext = new RegisterViewModel();
        }
    }
}