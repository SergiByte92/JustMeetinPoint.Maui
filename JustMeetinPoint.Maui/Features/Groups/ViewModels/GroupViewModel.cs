using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JustMeetinPoint.Maui.Features.Groups.Services;

namespace JustMeetinPoint.Maui.Features.Groups.ViewModels;

public partial class GroupsViewModel : ObservableObject
{
    private readonly IGroupService _groupService;

    public GroupsViewModel(IGroupService groupService)
    {
        _groupService = groupService;
    }

    [ObservableProperty]
    private string joinCode = string.Empty;

    [ObservableProperty]
    private string errorMessage = string.Empty;

    public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);

    partial void OnErrorMessageChanged(string value)
    {
        OnPropertyChanged(nameof(HasError));
    }

    [RelayCommand]
    private async Task CreateGroupAsync()
    {
        try
        {
            ErrorMessage = string.Empty;

            var result = await _groupService.CreateGroupAsync();

            if (result is null || string.IsNullOrWhiteSpace(result.GroupCode))
            {
                ErrorMessage = "No se pudo crear el grupo.";
                return;
            }

            string route =
                $"//main/group-lobby?groupCode={Uri.EscapeDataString(result.GroupCode)}&isCurrentUserHost=true";

            await Shell.Current.GoToAsync(route);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error al crear el grupo: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task JoinGroupAsync()
    {
        if (string.IsNullOrWhiteSpace(JoinCode))
        {
            ErrorMessage = "Introduce un código válido.";
            return;
        }

        try
        {
            ErrorMessage = string.Empty;

            string normalizedCode = JoinCode.Trim().ToUpperInvariant();

            var lobby = await _groupService.JoinGroupAsync(normalizedCode);

            if (lobby is null || string.IsNullOrWhiteSpace(lobby.GroupCode))
            {
                ErrorMessage = "No se pudo unir al grupo.";
                return;
            }

            string route =
                $"//main/group-lobby?groupCode={Uri.EscapeDataString(lobby.GroupCode)}&isCurrentUserHost=false";

            await Shell.Current.GoToAsync(route);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error al unirse al grupo: {ex.Message}";
        }
    }
}