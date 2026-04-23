namespace JustMeetinPoint.Maui.Features.Map.Models;

/// <summary>
/// Itinerario completo para la UI del mapa.
/// </summary>
public sealed class TransitItineraryModel
{
    public int DurationSeconds { get; set; }
    public double DistanceMeters { get; set; }
    public int TransfersCount { get; set; }

    public List<RouteLegModel> Legs { get; set; } = new();
}