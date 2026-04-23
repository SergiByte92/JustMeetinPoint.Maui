namespace JustMeetinPoint.Maui.Features.Map.Models;

public class RouteLegModel
{
    public string Mode { get; set; } = string.Empty;              // WALK, BUS, RAIL...
    public string FromName { get; set; } = string.Empty;
    public string ToName { get; set; } = string.Empty;

    public string LineName { get; set; } = string.Empty;          // Ej. R2, L1, H12
    public string Headsign { get; set; } = string.Empty;          // Dirección / destino línea

    public int DurationSeconds { get; set; }
    public double DistanceMeters { get; set; }

    public bool IsTransit { get; set; }
    public int StopsCount { get; set; }

    public List<RoutePointModel> GeometryPoints { get; set; } = new();
}