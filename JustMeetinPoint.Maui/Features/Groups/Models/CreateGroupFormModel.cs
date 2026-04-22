namespace JustMeetinPoint.Maui.Features.Groups.Models;

public class CreateGroupFormModel
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Method { get; set; } = "centroid";
    public string Category { get; set; } = string.Empty;
}