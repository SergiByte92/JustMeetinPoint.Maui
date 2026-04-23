using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JustMeetinPoint.Maui.Features.Groups.Services;
using JustMeetinPoint.Maui.Features.Map.Models;
using JustMeetinPoint.Maui.Features.Shared.Services;
using System.Diagnostics;

namespace JustMeetinPoint.Maui.Features.Groups.ViewModels;

[QueryProperty(nameof(GroupCode), "groupCode")]
[QueryProperty(nameof(IsCurrentUserHostRaw), "isCurrentUserHost")]
public partial class GroupLobbyViewModel : ObservableObject
{
    private readonly IGroupService _groupService;
    private readonly IMeetingStateService _meetingStateService;

    private CancellationTokenSource? _autoRefreshCts;
    private bool _hasSentLocation;
    private bool _isAutoRefreshRunning;

    public GroupLobbyViewModel(
        IGroupService groupService,
        IMeetingStateService meetingStateService)
    {
        _groupService = groupService;
        _meetingStateService = meetingStateService;
    }

    #region Properties

    [ObservableProperty] private string groupCode = string.Empty;
    [ObservableProperty] private int memberCount;
    [ObservableProperty] private bool hasStarted;
    [ObservableProperty] private bool isCurrentUserHost;
    [ObservableProperty] private bool isBusy;
    [ObservableProperty] private string errorMessage = string.Empty;

    public string IsCurrentUserHostRaw
    {
        set
        {
            if (bool.TryParse(value, out bool parsed))
            {
                IsCurrentUserHost = parsed;
                OnPropertyChanged(nameof(CanStartGroup));
            }
        }
    }

    // Propiedades calculadas para la UI
    public bool CanStartGroup => IsCurrentUserHost && !HasStarted;
    public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);
    public bool IsCalculating => HasStarted && !HasError;
    public int CurrentStep => HasStarted ? 3 : 2;

    public string ParticipantsText => $"{MemberCount} participante{(MemberCount == 1 ? "" : "s")} conectado{(MemberCount == 1 ? "" : "s")}";
    public string LobbyTitle => HasStarted ? "Calculando punto de encuentro" : "Sala del grupo";

    #endregion

    #region Lifecycle & Logic

    partial void OnGroupCodeChanged(string value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            // Cargamos el lobby inicial
            MainThread.BeginInvokeOnMainThread(async () => await LoadLobbyAsync());
        }
    }

    /// <summary>
    /// Bucle de refresco automático. Se detiene ante cualquier señal de inicio o error.
    /// </summary>
    public void StartAutoRefreshLoop()
    {
        if (_isAutoRefreshRunning || string.IsNullOrWhiteSpace(GroupCode)) return;

        _autoRefreshCts = new CancellationTokenSource();
        _isAutoRefreshRunning = true;
        _ = RunAutoRefreshLoopAsync(_autoRefreshCts.Token);
    }

    public void StopAutoRefreshLoop()
    {
        _autoRefreshCts?.Cancel();
        _autoRefreshCts?.Dispose();
        _autoRefreshCts = null;
        _isAutoRefreshRunning = false;
    }

    private async Task RunAutoRefreshLoopAsync(CancellationToken ct)
    {
        try
        {
            while (!ct.IsCancellationRequested)
            {
                // Solo refrescamos si no estamos ya en proceso de enviar ubicación
                if (!string.IsNullOrWhiteSpace(GroupCode) && !_hasSentLocation)
                {
                    await LoadLobbyAsync();
                }
                await Task.Delay(1500, ct);
            }
        }
        catch (OperationCanceledException) { /* Detención normal */ }
        finally { _isAutoRefreshRunning = false; }
    }

    [RelayCommand]
    private async Task LoadLobbyAsync()
    {
        // Si el grupo ya empezó o estamos ocupados, bloqueamos la entrada
        if (IsBusy || _hasSentLocation || string.IsNullOrWhiteSpace(GroupCode)) return;

        try
        {
            IsBusy = true;
            var lobby = await _groupService.RefreshLobbyAsync(GroupCode, IsCurrentUserHost);

            MemberCount = lobby.MemberCount;
            HasStarted = lobby.HasStarted;

            // PUNTO CLAVE: Si detectamos que ha empezado, disparamos la fase final
            if (HasStarted && !_hasSentLocation)
            {
                _hasSentLocation = true;
                StopAutoRefreshLoop(); // Paramos el bucle antes de que el socket se ensucie

                // Ejecutamos la navegación sin esperar (fire and forget) para liberar el flujo
                _ = Task.Run(async () => await SendCurrentLocationAndNavigateToMapAsync());
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[Lobby] Error: {ex.Message}");
        }
        finally
        {
            // Solo quitamos el Busy si no hemos empezado, para mantener el spinner
            if (!HasStarted) IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task StartAsync()
    {
        if (IsBusy || !CanStartGroup) return;

        try
        {
            IsBusy = true;
            bool started = await _groupService.StartGroupAsync(GroupCode, IsCurrentUserHost);

            if (started)
            {
                HasStarted = true;
                _hasSentLocation = true;
                StopAutoRefreshLoop();
                await SendCurrentLocationAndNavigateToMapAsync();
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = "Error al iniciar grupo.";
        }
        finally { IsBusy = false; }
    }

    private async Task SendCurrentLocationAndNavigateToMapAsync()
    {
        try
        {
            // 1. Obtener GPS (Tarea pesada)
            var location = await Geolocation.Default.GetLocationAsync(
                new GeolocationRequest(GeolocationAccuracy.High, TimeSpan.FromSeconds(10)));

            if (location == null) throw new Exception("GPS no disponible");

            // 2. Comunicar con Servidor y esperar resultado final de ruta
            var result = await _groupService.SendLocationAndWaitResultAsync(
                GroupCode, location.Latitude, location.Longitude);

            if (result != null)
            {
                _meetingStateService.CurrentResult = result;

                // 3. Navegar en el hilo de la UI
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    await Shell.Current.GoToAsync("//main/map");
                });
            }
        }
        catch (Exception ex)
        {
            MainThread.BeginInvokeOnMainThread(() => {
                ErrorMessage = "Fallo al calcular punto de encuentro.";
                _hasSentLocation = false; // Permitimos reintento
                IsBusy = false;
            });
        }
    }

    [RelayCommand]
    private async Task LeaveGroupAsync()
    {
        StopAutoRefreshLoop();
        await _groupService.LeaveGroupAsync(GroupCode);
        await Shell.Current.GoToAsync("//main/groups");
    }

    #endregion
}