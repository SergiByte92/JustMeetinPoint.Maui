using JustMeetinPoint.Maui.Features.Profile.Dtos;

namespace JustMeetinPoint.Maui.Features.Profile.Services;

public interface IProfileService
{
    Task<UserProfileDto> GetProfileAsync();
}