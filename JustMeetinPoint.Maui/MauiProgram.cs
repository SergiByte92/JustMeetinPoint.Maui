using CommunityToolkit.Maui;
using JustMeetinPoint.Maui.Features.Auth.Services;
using JustMeetinPoint.Maui.Features.Auth.ViewModels;
using JustMeetinPoint.Maui.Features.Auth.Views;
using JustMeetinPoint.Maui.Features.Dashboard.Views;
using JustMeetinPoint.Maui.Features.Groups.Services;
using JustMeetinPoint.Maui.Features.Groups.ViewModels;
using JustMeetinPoint.Maui.Features.Groups.Views;
using JustMeetinPoint.Maui.Features.Map.ViewModels;
using JustMeetinPoint.Maui.Features.Map.Views;
using JustMeetinPoint.Maui.Features.Profile.Views;
using JustMeetinPoint.Maui.Features.Shared.Services;

namespace JustMeetinPoint.Maui;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit();

        // ── SERVICIOS ─────────────────────────────────────────────────────────
        builder.Services.AddSingleton<IAuthService, SocketAuthService>();
        builder.Services.AddSingleton<IGroupService, GroupService>();
        builder.Services.AddSingleton<IMeetingStateService, MeetingStateService>();

        // ── VIEWMODELS ────────────────────────────────────────────────────────
        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<RegisterViewModel>();
        builder.Services.AddTransient<GroupsViewModel>();
        builder.Services.AddTransient<GroupLobbyViewModel>();
        builder.Services.AddTransient<MapViewModel>();
        builder.Services.AddTransient<CreateGroupViewModel>();

        // ── VIEWS ─────────────────────────────────────────────────────────────
        builder.Services.AddTransient<LoginView>();
        builder.Services.AddTransient<RegisterView>();
        builder.Services.AddTransient<GroupsView>();
        builder.Services.AddTransient<GroupLobbyView>();
        builder.Services.AddTransient<MapView>();
        builder.Services.AddTransient<HomeView>();
        builder.Services.AddTransient<ProfileView>();
        builder.Services.AddTransient<CreateGroupView>();

        return builder.Build();
    }
}