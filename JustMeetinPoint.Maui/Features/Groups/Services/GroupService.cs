using JustMeetinPoint.Maui.Features.Auth.Services;
using JustMeetinPoint.Maui.Features.Groups.Models;
using JustMeetinPoint.Maui.Features.Map.Models;
using JustMeetingPoint.Maui.NetUtils;
using System.Net.Sockets;
using System.Threading;

namespace JustMeetinPoint.Maui.Features.Groups.Services;

public class GroupService : IGroupService
{
    private readonly IAuthService _authService;

    private const int MainGroupCreateGroup = 1;
    private const int MainGroupJoinGroup = 2;

    private const int LobbyOptionRefresh = 1;
    private const int LobbyOptionExit = 2;
    private const int LobbyOptionStart = 3;
    private const int LobbyOptionSendLocation = 4;
    private const int LobbyOptionPollResult = 5;

    private const int PollDelayMilliseconds = 1500;
    private const int MaxPollAttempts = 40; // ~60 segundos

    public GroupService(IAuthService authService)
    {
        _authService = authService;
    }

    public async Task<GroupLobbyModel> CreateGroupAsync(
        string name,
        string description,
        string method,
        string category)
    {
        return await Task.Run(() =>
        {
            Socket socket = GetAuthenticatedSocket();

            SocketTools.sendInt(socket, MainGroupCreateGroup);
            SocketTools.sendString(name, socket);
            SocketTools.sendString(category, socket);
            SocketTools.sendString(description, socket);
            SocketTools.sendString(method, socket);

            bool success = SocketTools.receiveBool(socket);
            if (!success)
                throw new InvalidOperationException("No se pudo crear el grupo.");

            string groupCode = SocketTools.receiveString(socket);
            bool sessionValid = SocketTools.receiveBool(socket);

            if (!sessionValid)
                throw new InvalidOperationException("La sesión de lobby no es válida.");

            int memberCount = SocketTools.receiveInt(socket);
            bool hasStarted = SocketTools.receiveBool(socket);

            return new GroupLobbyModel
            {
                GroupCode = groupCode,
                MemberCount = memberCount,
                HasStarted = hasStarted,
                IsCurrentUserHost = true
            };
        });
    }

    public async Task<GroupLobbyModel> JoinGroupAsync(string groupCode)
    {
        return await Task.Run(() =>
        {
            Socket socket = GetAuthenticatedSocket();

            SocketTools.sendInt(socket, MainGroupJoinGroup);
            SocketTools.sendString(groupCode, socket);

            bool success = SocketTools.receiveBool(socket);
            if (!success)
                throw new InvalidOperationException("No se pudo unir al grupo.");

            bool sessionValid = SocketTools.receiveBool(socket);
            if (!sessionValid)
                throw new InvalidOperationException("La sesión de lobby no es válida.");

            int memberCount = SocketTools.receiveInt(socket);
            bool hasStarted = SocketTools.receiveBool(socket);

            return new GroupLobbyModel
            {
                GroupCode = groupCode,
                MemberCount = memberCount,
                HasStarted = hasStarted,
                IsCurrentUserHost = false
            };
        });
    }

    public async Task<GroupLobbyModel> RefreshLobbyAsync(string groupCode, bool isCurrentUserHost)
    {
        return await Task.Run(() =>
        {
            Socket socket = GetAuthenticatedSocket();

            Console.WriteLine($"[GroupService] RefreshLobbyAsync -> Group={groupCode}, Host={isCurrentUserHost}");

            SocketTools.sendInt(socket, LobbyOptionRefresh);

            bool sessionValid = SocketTools.receiveBool(socket);
            if (!sessionValid)
                throw new InvalidOperationException("La sesión del grupo ya no existe.");

            int memberCount = SocketTools.receiveInt(socket);
            bool hasStarted = SocketTools.receiveBool(socket);

            Console.WriteLine($"[GroupService] RefreshLobbyAsync <- MemberCount={memberCount}, HasStarted={hasStarted}");

            return new GroupLobbyModel
            {
                GroupCode = groupCode,
                MemberCount = memberCount,
                HasStarted = hasStarted,
                IsCurrentUserHost = isCurrentUserHost
            };
        });
    }

    public async Task LeaveGroupAsync(string groupCode)
    {
        await Task.Run(() =>
        {
            Socket socket = GetAuthenticatedSocket();
            SocketTools.sendInt(socket, LobbyOptionExit);
        });
    }

    public async Task<bool> StartGroupAsync(string groupCode, bool isCurrentUserHost)
    {
        return await Task.Run(() =>
        {
            if (!isCurrentUserHost)
                return false;

            Socket socket = GetAuthenticatedSocket();

            Console.WriteLine($"[GroupService] StartGroupAsync -> Group={groupCode}, Host={isCurrentUserHost}");

            SocketTools.sendInt(socket, LobbyOptionStart);
            bool started = SocketTools.receiveBool(socket);

            if (started)
            {
                bool sessionValid = SocketTools.receiveBool(socket);
                SocketTools.receiveInt(socket);   // memberCount
                SocketTools.receiveBool(socket);  // hasStarted

                Console.WriteLine($"[GroupService] Start OK. SessionValid={sessionValid}");

                if (!sessionValid)
                    throw new InvalidOperationException("Sesión inválida tras iniciar el grupo.");
            }

            return started;
        });
    }

    public async Task<MeetingResultModel?> SendLocationAndWaitResultAsync(
        string groupCode,
        double latitude,
        double longitude)
    {
        return await Task.Run(() =>
        {
            Socket socket = GetAuthenticatedSocket();

            Console.WriteLine($"[GroupService] Enviando ubicación: {latitude}, {longitude}");

            SocketTools.sendInt(socket, LobbyOptionSendLocation);
            SocketTools.sendDouble(socket, latitude);
            SocketTools.sendDouble(socket, longitude);

            double resultLat = SocketTools.receiveDouble(socket);
            double resultLon = SocketTools.receiveDouble(socket);
            int duration = SocketTools.receiveInt(socket);

            Console.WriteLine($"[GroupService] Respuesta inicial => lat:{resultLat}, lon:{resultLon}, duration:{duration}");

            MeetingResultModel? immediateResult = BuildMeetingResultFromServerResponse(
                resultLat,
                resultLon,
                duration,
                latitude,
                longitude);

            if (immediateResult is not null)
                return immediateResult;

            for (int attempt = 1; attempt <= MaxPollAttempts; attempt++)
            {
                Thread.Sleep(PollDelayMilliseconds);

                Console.WriteLine($"[GroupService] Poll attempt {attempt}/{MaxPollAttempts}: leyendo cabecera...");

                bool sessionValid = SocketTools.receiveBool(socket);
                if (!sessionValid)
                    throw new InvalidOperationException("La sesión del grupo ha finalizado.");

                SocketTools.receiveInt(socket);   // memberCount
                SocketTools.receiveBool(socket);  // hasStarted

                Console.WriteLine($"[GroupService] Poll attempt {attempt}/{MaxPollAttempts}: enviando PollResult...");
                SocketTools.sendInt(socket, LobbyOptionPollResult);

                resultLat = SocketTools.receiveDouble(socket);
                resultLon = SocketTools.receiveDouble(socket);
                duration = SocketTools.receiveInt(socket);

                Console.WriteLine($"[GroupService] Poll => lat:{resultLat}, lon:{resultLon}, duration:{duration}");

                MeetingResultModel? pollResult = BuildMeetingResultFromServerResponse(
                    resultLat,
                    resultLon,
                    duration,
                    latitude,
                    longitude);

                if (pollResult is not null)
                    return pollResult;

                if (duration == -1)
                    continue;
            }

            throw new InvalidOperationException("El cálculo está tardando demasiado. Inténtalo de nuevo.");
        });
    }

    private static MeetingResultModel? BuildMeetingResultFromServerResponse(
        double resultLat,
        double resultLon,
        int duration,
        double originLatitude,
        double originLongitude)
    {
        if (duration >= 0)
        {
            return new MeetingResultModel
            {
                Latitude = resultLat,
                Longitude = resultLon,
                DurationSeconds = duration,
                OriginLatitude = originLatitude,
                OriginLongitude = originLongitude,
                MeetingPointName = "Punto de encuentro",
                AddressText = "Dirección no disponible",
                DistanceText = "Distancia no disponible",
                FairnessText = "Resultado calculado correctamente"
            };
        }

        if (duration == -2)
            throw new InvalidOperationException("Error calculando la ruta en el servidor.");

        if (duration == -3)
        {
            return new MeetingResultModel
            {
                Latitude = resultLat,
                Longitude = resultLon,
                DurationSeconds = 0,
                OriginLatitude = originLatitude,
                OriginLongitude = originLongitude,
                MeetingPointName = "Punto de encuentro",
                AddressText = "No se encontró una ruta válida",
                DistanceText = "Distancia no disponible",
                FairnessText = "Centroide calculado, pero sin ruta disponible"
            };
        }

        // duration == -1 => aún faltan ubicaciones / aún no hay resultado
        return null;
    }

    private Socket GetAuthenticatedSocket()
    {
        Socket? socket = _authService.CurrentSocket;

        Console.WriteLine($"[GroupService] Socket null? {socket == null}");
        Console.WriteLine($"[GroupService] Socket connected? {socket?.Connected}");

        if (socket == null || !socket.Connected)
            throw new InvalidOperationException("No hay una sesión autenticada activa.");

        return socket;
    }
}