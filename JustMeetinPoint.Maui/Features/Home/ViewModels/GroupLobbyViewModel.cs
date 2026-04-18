using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JustMeetinPoint.Maui.Features.Home.Services;

namespace JustMeetinPoint.Maui.Features.Home.ViewModels;

[QueryProperty(nameof(GroupCode), "groupCode")]
[QueryProperty(nameof(IsCurrentUserHostRaw), "isCurrentUserHost")]
public partial class GroupLobbyViewModel : ObservableObject
{
    private readonly IGroupService _groupService;

    public GroupLobbyViewModel(IGroupService groupService)
    {
        _groupService = groupService;
    }

    [ObservableProperty]
    private string groupCode = string.Empty;

    [ObservableProperty]
    private int memberCount;

    [ObservableProperty]
    private bool hasStarted;

    [ObservableProperty]
    private bool isCurrentUserHost;

    [ObservableProperty]
    private bool isBusy;

    public string IsCurrentUserHostRaw
    {
        set
        {
            if (bool.TryParse(value, out bool parsed))
            {
                IsCurrentUserHost = parsed;
            }
        }
    }

    public string StatusText => HasStarted
        ? "El grupo ya ha iniciado."
        : "Esperando a más participantes...";

    public string ParticipantsText => $"{MemberCount} participante{(MemberCount == 1 ? string.Empty : "s")} conectado{(MemberCount == 1 ? string.Empty : "s")}";

    partial void OnGroupCodeChanged(string value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            MainThread.BeginInvokeOnMainThread(async () => await LoadLobbyAsync());
        }
    }

    partial void OnMemberCountChanged(int value)
    {
        OnPropertyChanged(nameof(ParticipantsText));
    }

    partial void OnHasStartedChanged(bool value)
    {
        OnPropertyChanged(nameof(StatusText));
    }

    [RelayCommand]
    private async Task LoadLobbyAsync()
    {
        if (IsBusy || string.IsNullOrWhiteSpace(GroupCode))
            return;

        try
        {
            IsBusy = true;

            var lobby = await _groupService.RefreshLobbyAsync(GroupCode, IsCurrentUserHost);

            MemberCount = lobby.MemberCount;
            HasStarted = lobby.HasStarted;
        }
        catch
        {
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        await LoadLobbyAsync();
    }

    [RelayCommand]
    private async Task LeaveGroupAsync()
    {
        if (IsBusy || string.IsNullOrWhiteSpace(GroupCode))
            return;

        try
        {
            IsBusy = true;
            await _groupService.LeaveGroupAsync(GroupCode);
            await Shell.Current.GoToAsync("//main/groups");
        }
        finally
        {
            IsBusy = false;
        }
    }
}