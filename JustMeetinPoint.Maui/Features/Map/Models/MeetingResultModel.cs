using JustMeetinPoint.Maui.Features.Map.Models;

namespace JustMeetinPoint.Maui.Features.Map.Models;

public class MeetingResultModel
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public int DurationSeconds { get; set; }

    public double OriginLatitude { get; set; }
    public double OriginLongitude { get; set; }

    public string MeetingPointName { get; set; } = string.Empty;
    public string AddressText { get; set; } = string.Empty;
    public string DistanceText { get; set; } = string.Empty;
    public string FairnessText { get; set; } = string.Empty;

    public List<RoutePointModel> RoutePoints { get; set; } = new();

    public TransitItineraryModel? Itinerary { get; set; }

    public bool HasValidRoute { get; set; }
    public bool HasRouteDetails => Itinerary is not null && Itinerary.Legs.Count > 0;
}