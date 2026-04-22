using CommunityToolkit.Mvvm.ComponentModel;

namespace JustMeetinPoint.Maui.Features.Groups.Models;

public partial class CategoryOptionModel : ObservableObject
{
    [ObservableProperty]
    private string name = string.Empty;

    [ObservableProperty]
    private bool isSelected;
}