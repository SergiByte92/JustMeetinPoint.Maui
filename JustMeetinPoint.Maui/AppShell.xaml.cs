using JustMeetingPoint.Maui.Features.Auth.Views;
using Microsoft.Maui.Controls;

namespace JustMeetingPoint.Maui
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute("register", typeof(RegisterView));
        }
    }
}