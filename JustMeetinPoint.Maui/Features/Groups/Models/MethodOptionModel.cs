using CommunityToolkit.Mvvm.ComponentModel;

namespace JustMeetinPoint.Maui.Features.Groups.Models;

public partial class MethodOptionModel : ObservableObject
{
    [ObservableProperty]
    private string name = string.Empty;

    [ObservableProperty]
    private string description = string.Empty;

    [ObservableProperty]
    private string value = string.Empty;

    [ObservableProperty]
    private bool isSelected;

    [ObservableProperty]
    private bool isEnabled = true;
}