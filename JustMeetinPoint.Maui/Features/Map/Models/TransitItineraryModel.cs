namespace JustMeetinPoint.Maui.Features.Map.Models;

public class TransitItineraryModel
{
    public int TotalDurationSeconds { get; set; }
    public double TotalDistanceMeters { get; set; }
    public int TransfersCount { get; set; }

    public List<RouteLegModel> Legs { get; set; } = new();
}