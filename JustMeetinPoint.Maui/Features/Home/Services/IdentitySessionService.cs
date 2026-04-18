using System.Net.Sockets;

namespace JustMeetinPoint.Maui.Features.Auth.Services;

public class IdentitySessionService : IIdentitySessionService
{
    public Socket? Socket { get; private set; }

    public bool IsAuthenticated => Socket != null && Socket.Connected;

    public void SetAuthenticatedSocket(Socket socket)
    {
        Socket = socket;
    }

    public void Clear()
    {
        try
        {
            Socket?.Shutdown(SocketShutdown.Both);
        }
        catch
        {
        }

        try
        {
            Socket?.Close();
        }
        catch
        {
        }

        Socket = null;
    }
}