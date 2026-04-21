namespace JustMeetinPoint.Maui.Features.Groups.Models;

public class GroupLobbyModel
{
    public string GroupCode { get; set; } = string.Empty;
    public int MemberCount { get; set; }
    public bool HasStarted { get; set; }
    public bool IsCurrentUserHost { get; set; }
}