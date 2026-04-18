using JustMeetinPoint.Maui.Features.Home.Views;

namespace JustMeetinPoint.Maui;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        Routing.RegisterRoute(nameof(GroupLobbyView), typeof(GroupLobbyView));
    }
}