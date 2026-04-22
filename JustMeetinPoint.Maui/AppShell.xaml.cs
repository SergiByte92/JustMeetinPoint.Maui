using JustMeetinPoint.Maui.Features.Groups.Views;

namespace JustMeetinPoint.Maui;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        Routing.RegisterRoute("create-group", typeof(CreateGroupView));
        Routing.RegisterRoute("group-lobby", typeof(GroupLobbyView));
    }
}