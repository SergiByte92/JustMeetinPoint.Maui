using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JustMeetinPoint.Maui.Features.Home.Models;
using JustMeetinPoint.Maui.Features.Home.Services;

namespace JustMeetinPoint.Maui.Features.Home.ViewModels;

public partial class MapViewModel : ObservableObject
{
    private readonly IMeetingStateService _meetingStateService;

    public MapViewModel(IMeetingStateService meetingStateService)
    {
        _meetingStateService = meetingStateService;
        Load();
    }

    [ObservableProperty] private double latitude;
    [ObservableProperty] private double longitude;
    [ObservableProperty] private int durationSeconds;
    [ObservableProperty] private bool isDefaultMap;

    [ObservableProperty] private double originLatitude;
    [ObservableProperty] private double originLongitude;

    [ObservableProperty] private string meetingPointName = "Punto de encuentro";
    [ObservableProperty] private string addressText = "Dirección no disponible";
    [ObservableProperty] private string distanceText = "Distancia no disponible";
    [ObservableProperty] private string fairnessText = "Equilibrio no disponible";

    [ObservableProperty] private bool isSheetExpanded;

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
        => OnPropertyChanged(nameof(SummaryText));

    [RelayCommand]
    private void ToggleSheet()
    {
        IsSheetExpanded = !IsSheetExpanded;
    }

    public void Load()
    {
        Console.WriteLine($"[MapViewModel] CurrentResult null? {_meetingStateService.CurrentResult == null}");

        if (_meetingStateService.CurrentResult != null)
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

            Console.WriteLine($"[MapViewModel] Resultado => destino: {Latitude}, {Longitude}, {DurationSeconds}s");
            Console.WriteLine($"[MapViewModel] Origen => {OriginLatitude}, {OriginLongitude}");
            Console.WriteLine($"[MapViewModel] RoutePoints => {RoutePoints.Count}");
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

            Console.WriteLine("[MapViewModel] Sin resultado. Cargando Barcelona por defecto.");
        }

        OnPropertyChanged(nameof(DurationText));
        OnPropertyChanged(nameof(SummaryText));
        OnPropertyChanged(nameof(RoutePoints));
    }
}