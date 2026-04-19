using JustMeetinPoint.Maui.Features.Auth.Services;
using JustMeetinPoint.Maui.Features.Home.Models;
using JustMeetingPoint.Maui.NetUtils;
using System.Net.Sockets;

namespace JustMeetinPoint.Maui.Features.Home.Services;

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

    public async Task<GroupLobbyModel> CreateGroupAsync()
    {
        return await Task.Run(() =>
        {
            Socket socket = GetAuthenticatedSocket();

            string groupName = "Grupo";
            string groupLabel = "General";
            string groupDescription = "Grupo creado desde la app";
            string groupMethod = "centroid";

            SocketTools.sendInt(socket, MainGroupCreateGroup);
            SocketTools.sendString(groupName, socket);
            SocketTools.sendString(groupLabel, socket);
            SocketTools.sendString(groupDescription, socket);
            SocketTools.sendString(groupMethod, socket);

            bool success = SocketTools.receiveBool(socket);
            if (!success)
                throw new InvalidOperationException("No se pudo crear el grupo en el servidor.");

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

            Console.WriteLine("[GroupService] Enviando LobbyOption.Refresh...");
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

            Console.WriteLine("[GroupService] Enviando LobbyOption.Exit...");
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

            Console.WriteLine("[GroupService] Enviando LobbyOption.Start...");
            SocketTools.sendInt(socket, LobbyOptionStart);

            bool started = SocketTools.receiveBool(socket);
            Console.WriteLine($"[GroupService] StartGroupAsync => {started}");

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

            // Primera respuesta del servidor tras recibir la ubicación.
            // Si ya están todas las ubicaciones, devuelve el resultado directamente.
            // Si aún faltan usuarios, devuelve 0, 0, -1.
            double resultLat = SocketTools.receiveDouble(socket);
            double resultLon = SocketTools.receiveDouble(socket);
            int duration = SocketTools.receiveInt(socket);

            Console.WriteLine($"[GroupService] Respuesta inicial => lat:{resultLat}, lon:{resultLon}, duration:{duration}");

            // Resultado inmediato: todos los usuarios ya habían enviado su ubicación.
            if (duration >= 0)
            {
                return new MeetingResultModel
                {
                    Latitude = resultLat,
                    Longitude = resultLon,
                    DurationSeconds = duration
                };
            }

            if (duration == -2)
                throw new InvalidOperationException("Error calculando la ruta en el servidor.");

            // duration == -1: el servidor está esperando las ubicaciones del resto
            // del grupo. Entramos en bucle de polling.
            //
            // ── BUG CORREGIDO ────────────────────────────────────────────────────
            // El servidor ejecuta su bucle LobbyGroup así:
            //
            //   while (true) {
            //     sendBool  (sessionValid)   ← 1 byte
            //     sendInt   (memberCount)    ← 4 bytes
            //     sendBool  (hasStarted)     ← 1 byte
            //     option = receiveInt()      ← lee la opción del cliente
            //     switch(option) { ... }
            //   }
            //
            // Antes del fix, el bucle de polling hacía:
            //   sendInt(PollResult)          ← ok, el servidor lo lee como opción ✓
            //   receiveDouble()              ← ❌ leía el bool+int+bool del siguiente
            //   receiveDouble()              ←    ciclo del servidor como si fueran
            //   receiveInt()                 ←    doubles/int → datos basura
            //
            // El primer usuario que enviaba su ubicación (sin ser el último del grupo)
            // nunca recibía el resultado correcto. Solo el último usuario del grupo
            // (que no necesita polling) funcionaba bien.
            //
            // Fix: leer los 3 valores de cabecera que el servidor envía al inicio
            // de cada iteración de LobbyGroup ANTES de enviar PollResult.
            // ─────────────────────────────────────────────────────────────────────
            while (true)
            {
                Thread.Sleep(1500);

                Console.WriteLine("[GroupService] PollResult: leyendo cabecera de sesión...");

                // Leer la cabecera que el servidor envía al inicio de cada iteración.
                bool sessionValid = SocketTools.receiveBool(socket);

                if (!sessionValid)
                    throw new InvalidOperationException("La sesión del grupo ha finalizado.");

                // memberCount y hasStarted no son necesarios aquí,
                // pero el servidor los envía siempre → hay que consumirlos.
                SocketTools.receiveInt(socket);  // memberCount  (descartado)
                SocketTools.receiveBool(socket); // hasStarted   (descartado)

                // Ahora sí enviamos la opción: el servidor está esperando este int.
                Console.WriteLine("[GroupService] Enviando PollResult...");
                SocketTools.sendInt(socket, LobbyOptionPollResult);

                // Leemos el resultado del poll.
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
                        DurationSeconds = duration
                    };
                }

                if (duration == -2)
                    throw new InvalidOperationException("Error calculando la ruta en el servidor.");

                // duration == -1: seguimos esperando.
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