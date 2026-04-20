namespace JustMeetinPoint.Maui.Features.Home.Models;

public class MeetingResultModel
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public int DurationSeconds { get; set; }

    public double OriginLatitude { get; set; }
    public double OriginLongitude { get; set; }

    public string MeetingPointName { get; set; } = "Punto de encuentro";
    public string AddressText { get; set; } = "Dirección no disponible";
    public string DistanceText { get; set; } = "Distancia no disponible";
    public string FairnessText { get; set; } = "Información no disponible";

    public List<RoutePointModel> RoutePoints { get; set; } = new();
}