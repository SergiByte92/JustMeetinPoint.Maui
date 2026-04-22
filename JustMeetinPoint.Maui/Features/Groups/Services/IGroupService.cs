using JustMeetinPoint.Maui.Features.Groups.Models;
using JustMeetinPoint.Maui.Features.Map.Models;

namespace JustMeetinPoint.Maui.Features.Groups.Services;

public interface IGroupService
{
    Task<GroupLobbyModel> CreateGroupAsync(string name, string description, string method, string category);
    Task<GroupLobbyModel> JoinGroupAsync(string groupCode);
    Task<GroupLobbyModel> RefreshLobbyAsync(string groupCode, bool isCurrentUserHost);
    Task LeaveGroupAsync(string groupCode);
    Task<bool> StartGroupAsync(string groupCode, bool isCurrentUserHost);
    Task<MeetingResultModel?> SendLocationAndWaitResultAsync(string groupCode, double latitude, double longitude);
}