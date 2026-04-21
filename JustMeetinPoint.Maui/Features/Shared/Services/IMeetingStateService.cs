using JustMeetinPoint.Maui.Features.Map.Models;

namespace JustMeetinPoint.Maui.Features.Shared.Services;

public interface IMeetingStateService
{
    MeetingResultModel? CurrentResult { get; set; }
    void Clear();
}