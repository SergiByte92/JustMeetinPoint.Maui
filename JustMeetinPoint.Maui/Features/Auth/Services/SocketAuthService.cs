using JustMeetingPoint.Maui.NetUtils;
using JustMeetinPoint.Maui.Features.Auth.Dtos;
using System.Net.Sockets;

namespace JustMeetinPoint.Maui.Features.Auth.Services;

public class SocketAuthService : IAuthService
{
    private readonly string _serverIp = "192.168.1.36";
    private readonly int _serverPort = 1001;

    public Socket? CurrentSocket { get; private set; }

    public bool IsAuthenticated => CurrentSocket != null && CurrentSocket.Connected;

    public async Task<RegisterResponseDto> RegisterAsync(RegisterRequestDto request)
    {
        return await Task.Run(() =>
        {
            Socket? socket = null;

            try
            {
                Console.WriteLine("Register: intentando conectar...");
                socket = SocketTools.CreateSocketConnection(_serverIp, _serverPort);
                Console.WriteLine("Register: conexión OK");

                SocketTools.sendInt(socket, 2); // MainUser.Register
                Console.WriteLine("Register: opción enviada");

                SocketTools.sendString(request.Username, socket);
                SocketTools.sendString(request.Email, socket);
                SocketTools.sendString(request.Password, socket);
                SocketTools.sendString(request.BirthDate.ToString("yyyy-MM-dd"), socket);
                Console.WriteLine("Register: datos enviados");

                bool success = SocketTools.receiveBool(socket);
                Console.WriteLine($"Register: respuesta recibida = {success}");

                return new RegisterResponseDto
                {
                    Success = success,
                    Message = success
                        ? "Registrado correctamente."
                        : "No se pudo registrar el usuario."
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Register: error -> {ex}");

                return new RegisterResponseDto
                {
                    Success = false,
                    Message = $"Error de conexión: {ex.Message}"
                };
            }
            finally
            {
                socket?.Close();
            }
        });
    }

    public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request)
    {
        return await Task.Run(() =>
        {
            Socket? socket = null;

            try
            {
                Console.WriteLine("Login: intentando conectar...");
                socket = SocketTools.CreateSocketConnection(_serverIp, _serverPort);
                Console.WriteLine("Login: conexión OK");

                SocketTools.sendInt(socket, 1); // MainUser.Login
                Console.WriteLine("Login: opción enviada");

                SocketTools.sendString(request.Email, socket);
                Console.WriteLine("Login: email enviado");

                SocketTools.sendString(request.Password, socket);
                Console.WriteLine("Login: password enviada");

                bool success = SocketTools.receiveBool(socket);
                Console.WriteLine($"Login: respuesta recibida = {success}");

                if (!success)
                {
                    socket.Close();

                    return new LoginResponseDto
                    {
                        Success = false,
                        Message = "Correo o contraseña incorrectos."
                    };
                }

                CurrentSocket = socket;

                Console.WriteLine("Login OK. Socket autenticado guardado.");
                Console.WriteLine($"Socket connected = {CurrentSocket?.Connected}");

                return new LoginResponseDto
                {
                    Success = true,
                    Message = "Inicio de sesión correcto."
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Login: error -> {ex}");

                try
                {
                    socket?.Close();
                }
                catch
                {
                }

                return new LoginResponseDto
                {
                    Success = false,
                    Message = $"Error de conexión: {ex.Message}"
                };
            }
        });
    }

    public void Logout()
    {
        try
        {
            CurrentSocket?.Shutdown(SocketShutdown.Both);
        }
        catch
        {
        }

        try
        {
            CurrentSocket?.Close();
        }
        catch
        {
        }

        CurrentSocket = null;
    }
}