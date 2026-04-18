using JustMeetinPoint.Maui.Features.Home.Models;

namespace JustMeetinPoint.Maui.Features.Home.Services;

public interface IGroupService
{
    Task<GroupLobbyModel> CreateGroupAsync();
    Task<GroupLobbyModel> JoinGroupAsync(string groupCode);
    Task<GroupLobbyModel> RefreshLobbyAsync(string groupCode, bool isCurrentUserHost);
    Task LeaveGroupAsync(string groupCode);
}