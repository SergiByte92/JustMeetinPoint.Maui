using CommunityToolkit.Mvvm.ComponentModel;
using JustMeetinPoint.Maui.Features.Shared.Services;
using JustMeetinPoint.Maui.Features.Map.Models;


namespace JustMeetinPoint.Maui.Features.Map.ViewModels;

public partial class MapViewModel : ObservableObject
{
    private readonly IMeetingStateService _meetingStateService;

    public MapViewModel(IMeetingStateService meetingStateService)
    {
        _meetingStateService = meetingStateService;
    }

    [ObservableProperty]
    private double latitude;

    [ObservableProperty]
    private double longitude;

    [ObservableProperty]
    private int durationSeconds;

    [ObservableProperty]
    private bool isDefaultMap;

    [ObservableProperty]
    private double originLatitude;

    [ObservableProperty]
    private double originLongitude;

    [ObservableProperty]
    private string meetingPointName = "Punto de encuentro";

    [ObservableProperty]
    private string addressText = "Dirección no disponible";

    [ObservableProperty]
    private string distanceText = "Distancia no disponible";

    [ObservableProperty]
    private string fairnessText = "Equilibrio no disponible";

    [ObservableProperty]
    private bool isSheetExpanded;

    public List<RoutePointModel> RoutePoints { get; private set; } = new();

    public string SummaryText
    {
        get
        {
            if (IsDefaultMap)
                return "Sin datos de ruta";

            return $"{DurationText} · {DistanceText}";
        }
    }

    public string DurationText
    {
        get
        {
            if (IsDefaultMap)
                return "Sin datos de ruta";

            if (DurationSeconds <= 0)
                return "Duración no disponible";

            if (DurationSeconds < 60)
                return $"{DurationSeconds} seg";

            int minutes = DurationSeconds / 60;
            int seconds = DurationSeconds % 60;

            if (seconds == 0)
                return $"{minutes} min";

            return $"{minutes} min {seconds} seg";
        }
    }

    partial void OnDurationSecondsChanged(int value)
    {
        OnPropertyChanged(nameof(DurationText));
        OnPropertyChanged(nameof(SummaryText));
    }

    partial void OnIsDefaultMapChanged(bool value)
    {
        OnPropertyChanged(nameof(DurationText));
        OnPropertyChanged(nameof(SummaryText));
    }

    partial void OnDistanceTextChanged(string value)
    {
        OnPropertyChanged(nameof(SummaryText));
    }

    public Task Load()
    {
        Console.WriteLine($"[MapViewModel] CurrentResult null? {_meetingStateService.CurrentResult == null}");

        if (_meetingStateService.CurrentResult is not null)
        {
            var result = _meetingStateService.CurrentResult;

            Latitude = result.Latitude;
            Longitude = result.Longitude;
            DurationSeconds = result.DurationSeconds;

            OriginLatitude = result.OriginLatitude;
            OriginLongitude = result.OriginLongitude;

            MeetingPointName = result.MeetingPointName;
            AddressText = result.AddressText;
            DistanceText = result.DistanceText;
            FairnessText = result.FairnessText;

            RoutePoints = result.RoutePoints ?? new List<RoutePointModel>();
            IsDefaultMap = false;
        }
        else
        {
            Latitude = 41.3874;
            Longitude = 2.1686;
            DurationSeconds = 0;

            OriginLatitude = 41.3874;
            OriginLongitude = 2.1686;

            MeetingPointName = "Barcelona";
            AddressText = "Vista por defecto";
            DistanceText = "—";
            FairnessText = "Sin resultado";

            RoutePoints = new List<RoutePointModel>();
            IsDefaultMap = true;
        }

        OnPropertyChanged(nameof(DurationText));
        OnPropertyChanged(nameof(SummaryText));
        OnPropertyChanged(nameof(RoutePoints));

        return Task.CompletedTask;
    }
}