using JustMeetinPoint.Maui.Features.Map.Models;
using JustMeetinPoint.Maui.Features.Map.ViewModels;
using System.ComponentModel;
using System.Globalization;
using System.Text;

namespace JustMeetinPoint.Maui.Features.Map.Views;

public partial class MapView : ContentPage
{
    private readonly MapViewModel _viewModel;

    private bool _isLoaded;
    private bool _eventsSubscribed;
    private bool _isAnimating;
    private double _panStartTranslationY;

    private const double ExpandedTranslationY = 0;
    private const double CollapsedTranslationY = 332;
    private const uint SheetAnimationDuration = 220;

    public MapView(MapViewModel viewModel)
    {
        InitializeComponent();

        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        SubscribeEvents();

        if (!_isLoaded)
        {
            await _viewModel.Load();
            RenderMap();
            await UpdateBottomSheet(animated: false);
            _isLoaded = true;
            return;
        }

        await UpdateBottomSheet(animated: false);
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        UnsubscribeEvents();
    }

    private void SubscribeEvents()
    {
        if (_eventsSubscribed)
            return;

        _viewModel.PropertyChanged += ViewModel_PropertyChanged;
        _eventsSubscribed = true;
    }

    private void UnsubscribeEvents()
    {
        if (!_eventsSubscribed)
            return;

        _viewModel.PropertyChanged -= ViewModel_PropertyChanged;
        _eventsSubscribed = false;
    }

    private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MapViewModel.IsSheetExpanded))
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await UpdateBottomSheet(animated: true);
            });
        }
    }

    private void RenderMap()
    {
        string destinationLat = _viewModel.Latitude.ToString(CultureInfo.InvariantCulture);
        string destinationLon = _viewModel.Longitude.ToString(CultureInfo.InvariantCulture);
        string originLat = _viewModel.OriginLatitude.ToString(CultureInfo.InvariantCulture);
        string originLon = _viewModel.OriginLongitude.ToString(CultureInfo.InvariantCulture);

        string routeJsArray = _viewModel.RoutePoints is not null && _viewModel.RoutePoints.Count > 1
            ? BuildRoutePointsJsArray(_viewModel.RoutePoints)
            : $"[[{originLat},{originLon}],[{destinationLat},{destinationLon}]]";

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

        var map = L.map('map', {{
            zoomControl: false
        }});

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
        if (points is null || points.Count == 0)
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
        if (_isAnimating)
            return;

        try
        {
            _isAnimating = true;

            double targetTranslation = _viewModel.IsSheetExpanded
                ? ExpandedTranslationY
                : CollapsedTranslationY;

            if (_viewModel.IsSheetExpanded)
            {
                ExpandedContent.IsVisible = true;
            }

            if (animated)
            {
                await BottomSheet.TranslateTo(0, targetTranslation, SheetAnimationDuration, Easing.CubicOut);
            }
            else
            {
                BottomSheet.TranslationY = targetTranslation;
            }

            if (!_viewModel.IsSheetExpanded)
            {
                ExpandedContent.IsVisible = false;
            }
        }
        finally
        {
            _isAnimating = false;
        }
    }

    private async void OnBottomSheetPanUpdated(object? sender, PanUpdatedEventArgs e)
    {
        if (_isAnimating)
            return;

        switch (e.StatusType)
        {
            case GestureStatus.Started:
                _panStartTranslationY = BottomSheet.TranslationY;
                break;

            case GestureStatus.Running:
                double nextTranslation = _panStartTranslationY + e.TotalY;

                nextTranslation = Math.Max(ExpandedTranslationY, nextTranslation);
                nextTranslation = Math.Min(CollapsedTranslationY, nextTranslation);

                if (nextTranslation < CollapsedTranslationY)
                {
                    ExpandedContent.IsVisible = true;
                }

                BottomSheet.TranslationY = nextTranslation;
                break;

            case GestureStatus.Completed:
            case GestureStatus.Canceled:
                bool shouldExpand = BottomSheet.TranslationY < (CollapsedTranslationY / 2);

                _viewModel.IsSheetExpanded = shouldExpand;
                await UpdateBottomSheet(animated: true);
                break;
        }
    }
}