using JustMeetinPoint.Maui.Features.Auth.Dtos;
using System.Net.Sockets;

namespace JustMeetinPoint.Maui.Features.Auth.Services;

public interface IAuthService
{
    Socket? CurrentSocket { get; }
    bool IsAuthenticated { get; }

    Task<LoginResponseDto> LoginAsync(LoginRequestDto request);
    Task<RegisterResponseDto> RegisterAsync(RegisterRequestDto request);
    void Logout();
}