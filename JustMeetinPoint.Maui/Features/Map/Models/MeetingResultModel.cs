namespace JustMeetinPoint.Maui.Features.Map.Models;

/// <summary>
/// Resultado que usa el mapa para pintar punto, ruta, duración y detalle.
/// </summary>
public sealed class MeetingResultModel
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }

    public double OriginLatitude { get; set; }
    public double OriginLongitude { get; set; }

    public int DurationSeconds { get; set; }
    public double DistanceMeters { get; set; }
    public int TransferCount { get; set; }

    public bool HasValidRoute { get; set; }

    public string MeetingPointName { get; set; } = "Punto de encuentro";
    public string AddressText { get; set; } = "Dirección no disponible";
    public string DistanceText { get; set; } = "Distancia no disponible";
    public string FairnessText { get; set; } = "Equilibrio no disponible";

    public List<RoutePointModel>? RoutePoints { get; set; } = new();

    public TransitItineraryModel? Itinerary { get; set; }

    public List<RouteLegModel> Legs { get; set; } = new();
}