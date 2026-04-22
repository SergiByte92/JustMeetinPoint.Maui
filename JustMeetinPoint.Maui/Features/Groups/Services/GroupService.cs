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

            SocketTools.sendInt(socket, LobbyOptionRefresh);

            bool sessionValid = SocketTools.receiveBool(socket);
            if (!sessionValid)
                throw new InvalidOperationException("La sesión del grupo ya no existe.");

            int memberCount = SocketTools.receiveInt(socket);
            bool hasStarted = SocketTools.receiveBool(socket);

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

            if (duration >= 0)
            {
                return new MeetingResultModel
                {
                    Latitude = resultLat,
                    Longitude = resultLon,
                    DurationSeconds = duration,
                    OriginLatitude = latitude,
                    OriginLongitude = longitude,
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
                    OriginLatitude = latitude,
                    OriginLongitude = longitude,
                    MeetingPointName = "Punto de encuentro",
                    AddressText = "No se encontró una ruta válida",
                    DistanceText = "Distancia no disponible",
                    FairnessText = "Centroide calculado, pero sin ruta disponible"
                };
            }

            while (true)
            {
                Thread.Sleep(1500);

                Console.WriteLine("[GroupService] PollResult: leyendo cabecera...");

                bool sessionValid = SocketTools.receiveBool(socket);
                if (!sessionValid)
                    throw new InvalidOperationException("La sesión del grupo ha finalizado.");

                SocketTools.receiveInt(socket);   // memberCount
                SocketTools.receiveBool(socket);  // hasStarted

                Console.WriteLine("[GroupService] Enviando PollResult...");
                SocketTools.sendInt(socket, LobbyOptionPollResult);

                resultLat = SocketTools.receiveDouble(socket);
                resultLon = SocketTools.receiveDouble(socket);
                duration = SocketTools.receiveInt(socket);

                Console.WriteLine($"[GroupService] Poll => lat:{resultLat}, lon:{resultLon}, duration:{duration}");

                if (duration >= 0)
                {
                    return new MeetingResultModel
                    {
                        Latitude = resultLat,
                        Longitude = resultLon,
                        DurationSeconds = duration,
                        OriginLatitude = latitude,
                        OriginLongitude = longitude,
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
                        OriginLatitude = latitude,
                        OriginLongitude = longitude,
                        MeetingPointName = "Punto de encuentro",
                        AddressText = "No se encontró una ruta válida",
                        DistanceText = "Distancia no disponible",
                        FairnessText = "Centroide calculado, pero sin ruta disponible"
                    };
                }
            }
        });
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