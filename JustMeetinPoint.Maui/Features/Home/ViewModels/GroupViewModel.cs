using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JustMeetinPoint.Maui.Features.Home.Services;
using JustMeetinPoint.Maui.Features.Home.Views;

namespace JustMeetinPoint.Maui.Features.Home.ViewModels;

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

    [ObservableProperty]
    private bool isBusy;

    [RelayCommand]
    private async Task CreateGroupAsync()
    {
        if (IsBusy)
            return;

        try
        {
            IsBusy = true;
            ErrorMessage = string.Empty;

            var lobby = await _groupService.CreateGroupAsync();

            await Shell.Current.GoToAsync(
                $"{nameof(GroupLobbyView)}?groupCode={lobby.GroupCode}&isCurrentUserHost={lobby.IsCurrentUserHost}");
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task JoinGroupAsync()
    {
        if (IsBusy)
            return;

        var normalizedCode = JoinCode?.Trim().ToUpperInvariant();

        if (string.IsNullOrWhiteSpace(normalizedCode))
        {
            ErrorMessage = "Introduce un código válido.";
            return;
        }

        try
        {
            IsBusy = true;
            ErrorMessage = string.Empty;

            var lobby = await _groupService.JoinGroupAsync(normalizedCode);

            await Shell.Current.GoToAsync(
                $"{nameof(GroupLobbyView)}?groupCode={lobby.GroupCode}&isCurrentUserHost={lobby.IsCurrentUserHost}");
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }
}