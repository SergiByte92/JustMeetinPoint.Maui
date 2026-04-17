using System.Net.Http.Json;
using JustMeetinPoint.Maui.Features.Auth.Dtos;

namespace JustMeetinPoint.Maui.Features.Auth.Services;

public class AuthService : IAuthService
{
    private readonly HttpClient _httpClient;

    public AuthService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<RegisterResponseDto> RegisterAsync(RegisterRequestDto request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/auth/register", request);

            if (!response.IsSuccessStatusCode)
            {
                return new RegisterResponseDto
                {
                    Success = false,
                    Message = $"Error HTTP: {(int)response.StatusCode}"
                };
            }

            var result = await response.Content.ReadFromJsonAsync<RegisterResponseDto>();

            return result ?? new RegisterResponseDto
            {
                Success = false,
                Message = "Respuesta vacía del servidor."
            };
        }
        catch (Exception ex)
        {
            return new RegisterResponseDto
            {
                Success = false,
                Message = $"Error de conexión: {ex.Message}"
            };
        }
    }
}