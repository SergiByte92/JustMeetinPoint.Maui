using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JustMeetinPoint.Maui.Features.Auth.Services;
using JustMeetinPoint.Maui.Features.Profile.Services;
using JustMeetinPoint.Maui.Features.Shared.Services;

namespace JustMeetinPoint.Maui.Features.Profile.ViewModels;

public partial class ProfileViewModel : ObservableObject
{
    private readonly IProfileService _profileService;
    private readonly IAuthService _authService;
    private readonly IMeetingStateService _meetingStateService;

    [ObservableProperty]
    private string userInitials = "--";

    [ObservableProperty]
    private string fullName = "Cargando...";

    [ObservableProperty]
    private string email = string.Empty;

    [ObservableProperty]
    private string birthDateText = string.Empty;

    [ObservableProperty]
    private string meetingsCreated = "—";

    [ObservableProperty]
    private string completedGroups = "—";

    [ObservableProperty]
    private string avgTravelTimeText = "—";

    [ObservableProperty]
    private string lastActivityText = "—";

    [ObservableProperty]
    private bool isBusy;

    [ObservableProperty]
    private string errorMessage = string.Empty;

    public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);

    public ProfileViewModel(
        IProfileService profileService,
        IAuthService authService,
        IMeetingStateService meetingStateService)
    {
        _profileService = profileService;
        _authService = authService;
        _meetingStateService = meetingStateService;
    }

    partial void OnErrorMessageChanged(string value)
    {
        OnPropertyChanged(nameof(HasError));
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        if (IsBusy)
            return;

        try
        {
            IsBusy = true;
            ErrorMessage = string.Empty;

            var profile = await _profileService.GetProfileAsync();

            FullName = profile.Username;
            Email = profile.Email;
            BirthDateText = profile.BirthDateText;
            UserInitials = BuildInitials(profile.Username);

            LastActivityText = "Sesión actual";
        }
        catch (Exception ex)
        {
            ErrorMessage = "No se pudieron cargar los datos del perfil.";
            Console.WriteLine($"[ProfileViewModel] Error LoadAsync: {ex}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task LogoutAsync()
    {
        try
        {
            _meetingStateService.Clear();
            _authService.Logout();

            await Shell.Current.GoToAsync("//login");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ProfileViewModel] Error LogoutAsync: {ex}");
            ErrorMessage = "No se pudo cerrar la sesión.";
        }
    }

    private static string BuildInitials(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
            return "--";

        var parts = username
            .Trim()
            .Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length == 1)
            return parts[0][..Math.Min(2, parts[0].Length)].ToUpperInvariant();

        return $"{parts[0][0]}{parts[^1][0]}".ToUpperInvariant();
    }
}