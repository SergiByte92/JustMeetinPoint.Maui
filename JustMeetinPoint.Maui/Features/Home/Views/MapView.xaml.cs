using JustMeetinPoint.Maui.Features.Home.Models;
using JustMeetinPoint.Maui.Features.Home.ViewModels;
using System.Globalization;
using System.Text;

namespace JustMeetinPoint.Maui.Features.Home.Views;

public partial class MapView : ContentPage
{
    private readonly MapViewModel _viewModel;

    public MapView(MapViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
        _viewModel = viewModel;
        _viewModel.PropertyChanged += ViewModel_PropertyChanged;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        _viewModel.Load();
        RenderMap();
        UpdateBottomSheet(false);
    }

    private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MapViewModel.IsSheetExpanded))
        {
            MainThread.BeginInvokeOnMainThread(async () => await UpdateBottomSheet(true));
        }
    }

    private void RenderMap()
    {
        string destinationLat = _viewModel.Latitude.ToString(CultureInfo.InvariantCulture);
        string destinationLon = _viewModel.Longitude.ToString(CultureInfo.InvariantCulture);
        string originLat = _viewModel.OriginLatitude.ToString(CultureInfo.InvariantCulture);
        string originLon = _viewModel.OriginLongitude.ToString(CultureInfo.InvariantCulture);

        string routeJsArray;

        if (_viewModel.RoutePoints != null && _viewModel.RoutePoints.Count > 1)
        {
            routeJsArray = BuildRoutePointsJsArray(_viewModel.RoutePoints);
        }
        else
        {
            routeJsArray = $"[[{originLat},{originLon}],[{destinationLat},{destinationLon}]]";
        }

        string html = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8' />
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <link rel='stylesheet' href='https://unpkg.com/leaflet@1.9.4/dist/leaflet.css' />
    <script src='https://unpkg.com/leaflet@1.9.4/dist/leaflet.js'></script>
    <style>
        html, body, #map {{
            height: 100%;
            margin: 0;
            padding: 0;
        }}
    </style>
</head>
<body>
    <div id='map'></div>

    <script>
        var destination = [{destinationLat}, {destinationLon}];
        var origin = [{originLat}, {originLon}];
        var routePoints = {routeJsArray};

        var map = L.map('map');

        L.tileLayer('https://{{s}}.tile.openstreetmap.org/{{z}}/{{x}}/{{y}}.png', {{
            maxZoom: 19,
            attribution: '&copy; OpenStreetMap contributors'
        }}).addTo(map);

        L.marker(origin).addTo(map).bindPopup('Origen');
        L.marker(destination).addTo(map).bindPopup('Punto de encuentro');

        var polyline = L.polyline(routePoints, {{
            color: '#0B2545',
            weight: 5
        }}).addTo(map);

        var bounds = L.latLngBounds(routePoints);
        map.fitBounds(bounds, {{ padding: [30, 30] }});
    </script>
</body>
</html>";

        MapWebView.Source = new HtmlWebViewSource
        {
            Html = html
        };
    }

    private static string BuildRoutePointsJsArray(List<RoutePointModel> points)
    {
        if (points == null || points.Count == 0)
            return "[]";

        var sb = new StringBuilder("[");
        for (int i = 0; i < points.Count; i++)
        {
            string lat = points[i].Latitude.ToString(CultureInfo.InvariantCulture);
            string lon = points[i].Longitude.ToString(CultureInfo.InvariantCulture);

            sb.Append($"[{lat},{lon}]");

            if (i < points.Count - 1)
                sb.Append(",");
        }

        sb.Append("]");
        return sb.ToString();
    }

    private async Task UpdateBottomSheet(bool animated)
    {
        ExpandedContent.IsVisible = _viewModel.IsSheetExpanded;

        double targetHeight = _viewModel.IsSheetExpanded ? 360 : 230;

        if (animated)
        {
            await BottomSheet.HeightRequestTo(targetHeight, 220, Easing.CubicOut);
        }
        else
        {
            BottomSheet.HeightRequest = targetHeight;
        }
    }
}

public static class VisualElementExtensions
{
    public static Task HeightRequestTo(this VisualElement element, double to, uint length, Easing easing)
    {
        var taskCompletionSource = new TaskCompletionSource<bool>();
        double from = element.HeightRequest < 0 ? element.Height : element.HeightRequest;

        var animation = new Animation(
            callback: value => element.HeightRequest = value,
            start: from,
            end: to);

        animation.Commit(
            owner: element,
            name: "HeightRequestTo",
            rate: 16,
            length: length,
            easing: easing,
            finished: (_, canceled) => taskCompletionSource.SetResult(!canceled));

        return taskCompletionSource.Task;
    }
}