using JustMeetinPoint.Maui.Features.Home.Models;

namespace JustMeetinPoint.Maui.Features.Home.Services;

public interface ILocationService
{
    Task<LocationResultModel?> SearchAsync(string query, CancellationToken cancellationToken);

    Task<IEnumerable<LocationResultModel>> GetByCategoryAsync(
        string category,
        double lat,
        double lon,
        CancellationToken cancellationToken);
}