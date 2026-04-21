namespace JustMeetinPoint.Maui.Features.Map.Models;

public sealed class RoutePointModel
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }

    public RoutePointModel()
    {
    }

    public RoutePointModel(double latitude, double longitude)
    {
        Latitude = latitude;
        Longitude = longitude;
    }
}