using JustMeetinPoint.Maui.Features.Home.ViewModels;
using System.Globalization;

namespace JustMeetinPoint.Maui.Features.Home.Views;

public partial class MapView : ContentPage
{
    private readonly MapViewModel _viewModel;

    public MapView(MapViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
        _viewModel = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        _viewModel.Load();

        string lat = _viewModel.Latitude.ToString(CultureInfo.InvariantCulture);
        string lon = _viewModel.Longitude.ToString(CultureInfo.InvariantCulture);

        string markerText = _viewModel.IsDefaultMap
            ? "Barcelona (vista por defecto)"
            : "Punto de encuentro";

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
        var lat = {lat};
        var lon = {lon};

        var map = L.map('map').setView([lat, lon], 13);

        L.tileLayer('https://{{s}}.tile.openstreetmap.org/{{z}}/{{x}}/{{y}}.png', {{
            maxZoom: 19,
            attribution: '&copy; OpenStreetMap contributors'
        }}).addTo(map);

        L.marker([lat, lon]).addTo(map)
            .bindPopup('{markerText}')
            .openPopup();
    </script>
</body>
</html>";

        MapWebView.Source = new HtmlWebViewSource
        {
            Html = html
        };
    }
}