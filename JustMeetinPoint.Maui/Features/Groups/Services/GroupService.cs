using JustMeetinPoint.Maui.Features.Auth.Services;
using JustMeetinPoint.Maui.Features.Groups.Models;
using JustMeetinPoint.Maui.Features.Map.Models;
using JustMeetingPoint.Maui.NetUtils;
using System.Net.Sockets;
using System.Text.Json;

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
    private const int MaxPollAttempts = 40;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

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
                /*
                 * El servidor, tras procesar Start, vuelve al inicio del bucle LobbyGroup
                 * y envía la cabecera estándar:
                 *
                 * bool sessionValid
                 * int memberCount
                 * bool hasStarted
                 *
                 * La consumimos aquí para dejar el socket alineado antes de SendLocation.
                 */
                bool sessionValid = SocketTools.receiveBool(socket);
                int memberCount = SocketTools.receiveInt(socket);
                bool hasStarted = SocketTools.receiveBool(socket);

                Console.WriteLine(
                    $"[GroupService] Start OK. SessionValid={sessionValid}, Members={memberCount}, HasStarted={hasStarted}");

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

            /*
             * IMPORTANTE:
             * A partir del nuevo servidor, la respuesta de SendLocation ya no es:
             *
             * double lat
             * double lon
             * int duration
             *
             * Ahora es:
             *
             * string json
             *
             * Ese JSON contiene:
             * - punto de encuentro
             * - duración
             * - distancia
             * - transbordos
             * - legs
             */
            SocketTools.sendInt(socket, LobbyOptionSendLocation);
            SocketTools.sendDouble(socket, latitude);
            SocketTools.sendDouble(socket, longitude);

            MeetingResultModel? immediateResult = ReceiveMeetingResultJson(socket);

            Console.WriteLine(
                $"[GroupService] Respuesta inicial => Duration={immediateResult?.DurationSeconds}, HasValidRoute={immediateResult?.HasValidRoute}");

            if (immediateResult is not null && immediateResult.DurationSeconds != -1)
                return NormalizeResult(immediateResult, latitude, longitude);

            /*
             * Si el servidor devuelve DurationSeconds == -1,
             * significa que todavía faltan ubicaciones.
             *
             * Entonces hacemos polling:
             * 1. leemos cabecera estándar del lobby
             * 2. enviamos PollResult
             * 3. recibimos JSON
             */
            for (int attempt = 1; attempt <= MaxPollAttempts; attempt++)
            {
                Thread.Sleep(PollDelayMilliseconds);

                Console.WriteLine($"[GroupService] Poll attempt {attempt}/{MaxPollAttempts}: leyendo cabecera...");

                bool sessionValid = SocketTools.receiveBool(socket);

                if (!sessionValid)
                    throw new InvalidOperationException("La sesión del grupo ha finalizado.");

                int memberCount = SocketTools.receiveInt(socket);
                bool hasStarted = SocketTools.receiveBool(socket);

                Console.WriteLine(
                    $"[GroupService] Poll header <- Members={memberCount}, HasStarted={hasStarted}");

                Console.WriteLine($"[GroupService] Poll attempt {attempt}/{MaxPollAttempts}: enviando PollResult...");

                SocketTools.sendInt(socket, LobbyOptionPollResult);

                MeetingResultModel? pollResult = ReceiveMeetingResultJson(socket);

                Console.WriteLine(
                    $"[GroupService] Poll result => Duration={pollResult?.DurationSeconds}, HasValidRoute={pollResult?.HasValidRoute}");

                if (pollResult is null)
                    continue;

                if (pollResult.DurationSeconds == -1)
                    continue;

                return NormalizeResult(pollResult, latitude, longitude);
            }

            throw new InvalidOperationException("El cálculo está tardando demasiado. Inténtalo de nuevo.");
        });
    }

    /// <summary>
    /// Lee el JSON enviado por el servidor y lo convierte a MeetingResultModel.
    /// </summary>
    private static MeetingResultModel? ReceiveMeetingResultJson(Socket socket)
    {
        string json = SocketTools.receiveString(socket);

        Console.WriteLine($"[GroupService] JSON recibido: {json}");

        MeetingResultModel? result = JsonSerializer.Deserialize<MeetingResultModel>(
            json,
            JsonOptions);

        if (result == null)
            throw new InvalidOperationException("No se pudo deserializar el resultado de ruta.");

        return result;
    }

    /// <summary>
    /// Normaliza el resultado recibido:
    /// - rellena origen si no viniera informado
    /// - transforma legs en itinerary
    /// - crea puntos básicos de ruta
    /// - gestiona errores funcionales
    /// </summary>
    private static MeetingResultModel NormalizeResult(
        MeetingResultModel result,
        double originLatitude,
        double originLongitude)
    {
        if (result.DurationSeconds == -2)
            throw new InvalidOperationException(result.AddressText);

        /*
         * Si el servidor no informa origen por cualquier motivo,
         * lo completamos desde la ubicación enviada por el cliente.
         */
        if (result.OriginLatitude == 0 && result.OriginLongitude == 0)
        {
            result.OriginLatitude = originLatitude;
            result.OriginLongitude = originLongitude;
        }

        /*
         * Si no hay RoutePoints, generamos una línea básica:
         * origen -> punto de encuentro.
         *
         * Más adelante puedes sustituir esto por la polyline real de OTP.
         */
        result.RoutePoints ??= BuildFallbackRoutePoints(result);

        /*
         * Convertimos Legs en TransitItineraryModel para que MapViewModel
         * pueda seguir usando su propiedad Itinerary.
         */
        result.Itinerary ??= BuildItinerary(result);

        /*
         * Ajustes de texto por seguridad.
         */
        if (string.IsNullOrWhiteSpace(result.MeetingPointName))
            result.MeetingPointName = "Punto de encuentro";

        if (string.IsNullOrWhiteSpace(result.AddressText))
            result.AddressText = result.HasValidRoute
                ? "Ruta calculada correctamente"
                : "No se encontró una ruta válida";

        if (string.IsNullOrWhiteSpace(result.DistanceText))
        {
            result.DistanceText = result.DistanceMeters > 0
                ? $"{result.DistanceMeters / 1000:0.0} km"
                : "Distancia no disponible";
        }

        if (string.IsNullOrWhiteSpace(result.FairnessText))
        {
            result.FairnessText = result.TransferCount == 0
                ? "Ruta directa sin transbordos"
                : $"Ruta con {result.TransferCount} transbordo{(result.TransferCount == 1 ? "" : "s")}";
        }

        return result;
    }

    /// <summary>
    /// Construye el itinerario que consume MapViewModel.
    /// </summary>
    private static TransitItineraryModel? BuildItinerary(MeetingResultModel result)
    {
        if (result.Legs == null || result.Legs.Count == 0)
            return null;

        return new TransitItineraryModel
        {
            DurationSeconds = result.DurationSeconds,
            DistanceMeters = result.DistanceMeters,
            TransfersCount = result.TransferCount,
            Legs = result.Legs
        };
    }

    /// <summary>
    /// Crea una ruta mínima de dos puntos:
    /// origen del usuario -> punto de encuentro.
    /// </summary>
    private static List<RoutePointModel> BuildFallbackRoutePoints(MeetingResultModel result)
    {
        return new List<RoutePointModel>
        {
            new RoutePointModel
            {
                Latitude = result.OriginLatitude,
                Longitude = result.OriginLongitude
            },
            new RoutePointModel
            {
                Latitude = result.Latitude,
                Longitude = result.Longitude
            }
        };
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