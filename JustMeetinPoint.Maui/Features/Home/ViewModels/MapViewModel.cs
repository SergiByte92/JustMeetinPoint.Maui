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

    [ObservableProperty] private double latitude;
    [ObservableProperty] private double longitude;
    [ObservableProperty] private int durationSeconds;
    [ObservableProperty] private bool isDefaultMap;

    // Propiedad computada: muestra la duración en formato legible.
    // Si OTP devuelve 0 (emulador sin GPS real o misma ubicación que destino),
    // se muestra un mensaje informativo en vez de "0 segundos".
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
        => OnPropertyChanged(nameof(DurationText));

    partial void OnIsDefaultMapChanged(bool value)
        => OnPropertyChanged(nameof(DurationText));

    public void Load()
    {
        Console.WriteLine($"[MapViewModel] CurrentResult null? {_meetingStateService.CurrentResult == null}");

        if (_meetingStateService.CurrentResult != null)
        {
            Latitude = _meetingStateService.CurrentResult.Latitude;
            Longitude = _meetingStateService.CurrentResult.Longitude;
            DurationSeconds = _meetingStateService.CurrentResult.DurationSeconds;
            IsDefaultMap = false;

            Console.WriteLine($"[MapViewModel] Resultado => {Latitude}, {Longitude}, {DurationSeconds}s");
        }
        else
        {
            Latitude = 41.3874;
            Longitude = 2.1686;
            DurationSeconds = 0;
            IsDefaultMap = true;

            Console.WriteLine("[MapViewModel] Sin resultado. Cargando Barcelona por defecto.");
        }

        OnPropertyChanged(nameof(DurationText));
    }
}