namespace JustMeetinPoint.Maui.Features.Map.Models;

/// <summary>
/// Tramo de ruta recibido desde el servidor.
/// Puede representar caminar, bus, metro, tren, etc.
/// </summary>
public sealed class RouteLegModel
{
    public string Mode { get; set; } = string.Empty;

    public string FromName { get; set; } = string.Empty;
    public string ToName { get; set; } = string.Empty;

    public int DurationSeconds { get; set; }
    public double DistanceMeters { get; set; }

    public string? PublicCode { get; set; }
    public string? LineName { get; set; }
    public string? Headsign { get; set; }

    public string? EncodedPolyline { get; set; }

    public string DurationText =>
        DurationSeconds <= 0 ? "Duración no disponible" : $"{DurationSeconds / 60} min";

    public string DistanceText =>
        DistanceMeters <= 0 ? "Distancia no disponible" : $"{DistanceMeters / 1000:0.0} km";

    public string DisplayTitle
    {
        get
        {
            if (Mode.Equals("WALK", StringComparison.OrdinalIgnoreCase))
                return "Caminar";

            if (!string.IsNullOrWhiteSpace(PublicCode))
                return $"{Mode} {PublicCode}";

            return Mode;
        }
    }

    public string DisplaySubtitle
    {
        get
        {
            if (!string.IsNullOrWhiteSpace(Headsign))
                return $"Dirección {Headsign}";

            if (!string.IsNullOrWhiteSpace(LineName))
                return LineName;

            return $"{FromName} → {ToName}";
        }
    }
}