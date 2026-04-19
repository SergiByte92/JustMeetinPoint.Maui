using CommunityToolkit.Mvvm.ComponentModel;
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

    [ObservableProperty]
    private double latitude;

    [ObservableProperty]
    private double longitude;

    [ObservableProperty]
    private int durationSeconds;

    [ObservableProperty]
    private bool isDefaultMap;

    public void Load()
    {
        Console.WriteLine($"[MapViewModel] CurrentResult null? {_meetingStateService.CurrentResult == null}");

        if (_meetingStateService.CurrentResult != null)
        {
            Latitude = _meetingStateService.CurrentResult.Latitude;
            Longitude = _meetingStateService.CurrentResult.Longitude;
            DurationSeconds = _meetingStateService.CurrentResult.DurationSeconds;
            IsDefaultMap = false;

            Console.WriteLine($"[MapViewModel] Resultado recibido => {Latitude}, {Longitude}, {DurationSeconds}");
        }
        else
        {
            Latitude = 41.3874;
            Longitude = 2.1686;
            DurationSeconds = 0;
            IsDefaultMap = true;

            Console.WriteLine("[MapViewModel] Sin resultado. Cargando Barcelona por defecto.");
        }
    }
}