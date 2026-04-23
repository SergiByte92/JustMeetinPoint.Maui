using JustMeetinPoint.Maui.Features.Map.ViewModels;
using System.Globalization;
using System.Text;

namespace JustMeetinPoint.Maui.Features.Map.Views;

public partial class MapView : ContentPage
{
    private readonly MapViewModel _viewModel;

    private const double CollapsedTranslationY = 260;
    private const double ExpandedTranslationY = 0;

    private double _startTranslationY;

    public MapView(MapViewModel viewModel)
    {
        InitializeComponent();

        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    /// <summary>
    /// Se ejecuta cada vez que la pantalla aparece.
    /// Carga el resultado desde MeetingStateService y renderiza el mapa.
    /// </summary>
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await _viewModel.Load();

        LoadMapHtml();
    }

    /// <summary>
    /// Gestiona el gesto vertical del bottom sheet.
    /// Si el usuario arrastra hacia arriba, expande.
    /// Si arrastra hacia abajo, colapsa.
    /// </summary>
    private async void OnBottomSheetPanUpdated(object sender, PanUpdatedEventArgs e)
    {
        switch (e.StatusType)
        {
            case GestureStatus.Started:
                _startTranslationY = BottomSheet.TranslationY;
                break;

            case GestureStatus.Running:
                double newTranslation = _startTranslationY + e.TotalY;

                if (newTranslation < ExpandedTranslationY)
                    newTranslation = ExpandedTranslationY;

                if (newTranslation > CollapsedTranslationY)
                    newTranslation = CollapsedTranslationY;

                BottomSheet.TranslationY = newTranslation;
                break;

            case GestureStatus.Completed:
            case GestureStatus.Canceled:
                bool shouldExpand = BottomSheet.TranslationY < CollapsedTranslationY / 2;

                await SetBottomSheetExpandedAsync(shouldExpand);
                break;
        }
    }

    /// <summary>
    /// Expande o contrae el bottom sheet con animación.
    /// También activa/desactiva el contenido expandido.
    /// </summary>
    private async Task SetBottomSheetExpandedAsync(bool expanded)
    {
        _viewModel.IsSheetExpanded = expanded;

        double target = expanded
            ? ExpandedTranslationY
            : CollapsedTranslationY;

        ExpandedContent.IsVisible = expanded;

        await BottomSheet.TranslateTo(
            x: 0,
            y: target,
            length: 220,
            easing: Easing.CubicOut);
    }

    /// <summary>
    /// Genera el HTML del mapa y lo carga en el WebView.
    /// 
    /// Por ahora pinta:
    /// - origen del usuario
    /// - punto de encuentro
    /// - línea básica entre ambos
    /// 
    /// Más adelante puedes sustituir esa línea básica por la polyline real
    /// decodificada desde EncodedPolyline.
    /// </summary>
    private void LoadMapHtml()
    {
        string html = BuildMapHtml();
        MapWebView.Source = new HtmlWebViewSource
        {
            Html = html
        };
    }

    /// <summary>
    /// Construye un mapa Leaflet embebido.
    /// Usa OpenStreetMap como base.
    /// </summary>
    private string BuildMapHtml()
    {
        string destinationLat = FormatDouble(_viewModel.Latitude);
        string destinationLon = FormatDouble(_viewModel.Longitude);
        string originLat = FormatDouble(_viewModel.OriginLatitude);
        string originLon = FormatDouble(_viewModel.OriginLongitude);

        string meetingPointName = EscapeJs(_viewModel.MeetingPointName);
        string durationText = EscapeJs(_viewModel.DurationText);
        string summaryText = EscapeJs(_viewModel.SummaryText);

        bool hasOrigin =
            Math.Abs(_viewModel.OriginLatitude) > 0.000001 &&
            Math.Abs(_viewModel.OriginLongitude) > 0.000001;

        StringBuilder routeLineBuilder = new();

        if (hasOrigin)
        {
            routeLineBuilder.AppendLine($@"
                const origin = [{originLat}, {originLon}];

                L.marker(origin, {{
                    title: 'Tu ubicación'
                }}).addTo(map)
                  .bindPopup('Tu ubicación');

                L.polyline([origin, destination], {{
                    weight: 5,
                    opacity: 0.75
                }}).addTo(map);

                const bounds = L.latLngBounds([origin, destination]);
                map.fitBounds(bounds, {{ padding: [40, 40] }});
            ");
        }
        else
        {
            routeLineBuilder.AppendLine(@"
                map.setView(destination, 14);
            ");
        }

        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8' />
    <meta name='viewport' content='width=device-width, initial-scale=1.0' />

    <link
        rel='stylesheet'
        href='https://unpkg.com/leaflet@1.9.4/dist/leaflet.css' />

    <script src='https://unpkg.com/leaflet@1.9.4/dist/leaflet.js'></script>

    <style>
        html, body, #map {{
            height: 100%;
            width: 100%;
            margin: 0;
            padding: 0;
        }}

        .leaflet-control-attribution {{
            font-size: 10px;
        }}
    </style>
</head>

<body>
    <div id='map'></div>

    <script>
        const destination = [{destinationLat}, {destinationLon}];

        const map = L.map('map', {{
            zoomControl: false
        }});

        L.tileLayer('https://tile.openstreetmap.org/{{z}}/{{x}}/{{y}}.png', {{
            maxZoom: 19,
            attribution: '&copy; OpenStreetMap'
        }}).addTo(map);

        L.marker(destination, {{
            title: '{meetingPointName}'
        }}).addTo(map)
          .bindPopup('<b>{meetingPointName}</b><br>{durationText}<br>{summaryText}');

        {routeLineBuilder}
    </script>
</body>
</html>";
    }

    /// <summary>
    /// Formatea doubles con punto decimal, no coma.
    /// JavaScript necesita 41.38, no 41,38.
    /// </summary>
    private static string FormatDouble(double value)
    {
        return value.ToString(CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Escapa texto para evitar romper strings JavaScript.
    /// </summary>
    private static string EscapeJs(string? value)
    {
        return (value ?? string.Empty)
            .Replace("\\", "\\\\")
            .Replace("'", "\\'")
            .Replace("\r", "")
            .Replace("\n", " ");
    }
}