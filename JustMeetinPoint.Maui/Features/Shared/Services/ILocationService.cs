using JustMeetinPoint.Maui.Features.Map.Models;

namespace JustMeetinPoint.Maui.Features.Shared.Services;

public interface ILocationService
{
    Task<LocationResultModel?> SearchAsync(string query, CancellationToken cancellationToken);

    Task<IEnumerable<LocationResultModel>> GetByCategoryAsync(
        string category,
        double lat,
        double lon,
        CancellationToken cancellationToken);
}