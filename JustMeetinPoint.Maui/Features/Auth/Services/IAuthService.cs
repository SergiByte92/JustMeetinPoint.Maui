using JustMeetinPoint.Maui.Features.Auth.Dtos;

namespace JustMeetinPoint.Maui.Features.Auth.Services;

public interface IAuthService
{
    Task<RegisterResponseDto> RegisterAsync(RegisterRequestDto request);
    Task<LoginResponseDto> LoginAsync(LoginRequestDto request);
}