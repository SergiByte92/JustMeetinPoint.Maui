using JustMeetinPoint.Maui.Features.Map.Models;

namespace JustMeetinPoint.Maui.Features.Shared.Services;

public class MeetingStateService : IMeetingStateService
{
    public MeetingResultModel? CurrentResult { get; set; }

    public void Clear()
    {
        CurrentResult = null;
    }
}