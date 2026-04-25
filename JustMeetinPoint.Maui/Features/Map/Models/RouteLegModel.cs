namespace JustMeetinPoint.Maui.Features.Map.Models;

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

    public string NormalizedMode => Mode.Trim().ToUpperInvariant();

    public string ModeIconSource => NormalizedMode switch
    {
        "WALK" or "FOOT" => "walk_mode.svg",
        "BUS" => "bus_mode.svg",
        "RAIL" or "TRAIN" => "train_mode.svg",
        "SUBWAY" or "METRO" => "subway_mode.svg",
        "TRAM" => "tram_mode.svg",
        "BICYCLE" or "BIKE" => "transport_icon.svg",
        "CAR" => "car_mode.svg",
        _ => "route_icon.svg"
    };

    public string ModeText => NormalizedMode switch
    {
        "WALK" or "FOOT" => "A pie",
        "BUS" => "Bus",
        "RAIL" or "TRAIN" => "Tren",
        "SUBWAY" or "METRO" => "Metro",
        "TRAM" => "Tranvía",
        "BICYCLE" or "BIKE" => "Bici",
        "CAR" => "Coche",
        _ => "Ruta"
    };

    public string DurationText
    {
        get
        {
            if (DurationSeconds <= 0)
                return "Duración no disponible";

            int minutes = DurationSeconds / 60;

            if (minutes <= 0)
                return $"{DurationSeconds} seg";

            return $"{minutes} min";
        }
    }

    public string DistanceText
    {
        get
        {
            if (DistanceMeters <= 0)
                return "Distancia no disponible";

            if (DistanceMeters < 1000)
                return $"{DistanceMeters:0} m";

            return $"{DistanceMeters / 1000:0.0} km";
        }
    }

    public string DisplayTitle
    {
        get
        {
            if (!string.IsNullOrWhiteSpace(PublicCode))
                return $"{ModeText} {PublicCode}";

            return ModeText;
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

            if (!string.IsNullOrWhiteSpace(FromName) || !string.IsNullOrWhiteSpace(ToName))
                return $"{FromName} → {ToName}";

            return "Detalle no disponible";
        }
    }
}