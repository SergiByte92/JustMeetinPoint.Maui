using JustMeetingPoint.Maui.NetUtils;
using JustMeetinPoint.Maui.Features.Auth.Services;
using JustMeetinPoint.Maui.Features.Profile.Dtos;

namespace JustMeetinPoint.Maui.Features.Profile.Services;

public sealed class ProfileService : IProfileService
{
    private readonly IAuthService _authService;

    private enum MainMenuOption
    {
        CreateGroup = 1,
        JoinGroup = 2,
        GetHomeData = 3,
        GetProfileData = 4
    }

    public ProfileService(IAuthService authService)
    {
        _authService = authService;
    }

    public async Task<UserProfileDto> GetProfileAsync()
    {
        return await Task.Run(() =>
        {
            var socket = _authService.CurrentSocket;

            if (socket is null || !socket.Connected)
                throw new InvalidOperationException("No hay socket autenticado activo.");

            SocketTools.sendInt(socket, (int)MainMenuOption.GetProfileData);

            var username = SocketTools.receiveString(socket);
            var email = SocketTools.receiveString(socket);
            var birthDate = SocketTools.receiveString(socket);

            return new UserProfileDto
            {
                Username = username,
                Email = email,
                BirthDateText = birthDate
            };
        });
    }
}