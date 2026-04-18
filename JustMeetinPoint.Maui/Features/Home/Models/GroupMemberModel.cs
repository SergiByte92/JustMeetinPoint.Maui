namespace JustMeetinPoint.Maui.Features.Home.Models;

public class GroupMemberModel
{
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public bool IsHost { get; set; }
    public bool IsConnected { get; set; }
}