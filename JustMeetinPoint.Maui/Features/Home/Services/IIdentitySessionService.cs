using System.Net.Sockets;

namespace JustMeetinPoint.Maui.Features.Auth.Services;

public interface IIdentitySessionService
{
    Socket? Socket { get; }
    bool IsAuthenticated { get; }

    void SetAuthenticatedSocket(Socket socket);
    void Clear();
}