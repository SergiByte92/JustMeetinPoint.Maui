using CommunityToolkit.Mvvm.ComponentModel;
using JustMeetinPoint.Maui.Features.Map.Models;
using JustMeetinPoint.Maui.Features.Shared.Services;
using Microsoft.Maui.Devices.Sensors;
using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace JustMeetinPoint.Maui.Features.Map.ViewModels;

/// <summary>
/// ViewModel de la pantalla de mapa.
/// 
/// Responsabilidad:
/// - Leer el resultado calculado desde IMeetingStateService.
/// - Exponer propiedades limpias para el XAML.
/// - Evitar bindings frágiles contra objetos null.
/// - Formatear duración, resumen y transbordos.
/// - Resolver la dirección humana del punto de encuentro mediante reverse geocoding.
/// </summary>
public partial class MapViewModel : ObservableObject
{
    private readonly IMeetingStateService _meetingStateService;

    public MapViewModel(IMeetingStateService meetingStateService)
    {
        _meetingStateService = meetingStateService;
    }

    [ObservableProperty]
    private double latitude;

    [ObservableProperty]
    private double longitude;

    [ObservableProperty]
    private int durationSeconds;

    [ObservableProperty]
    private bool isDefaultMap;

    [ObservableProperty]
    private double originLatitude;

    [ObservableProperty]
    private double originLongitude;

    [ObservableProperty]
    private string meetingPointName = "Punto de encuentro";

    [ObservableProperty]
    private string addressText = "Dirección no disponible";

    [ObservableProperty]
    private string distanceText = "Distancia no disponible";

    [ObservableProperty]
    private string fairnessText = "Equilibrio no disponible";

    [ObservableProperty]
    private bool isSheetExpanded;

    [ObservableProperty]
    private TransitItineraryModel? itinerary;

    [ObservableProperty]
    private bool hasValidRoute;

    /// <summary>
    /// Puntos usados para pintar una línea básica en el mapa.
    /// Por ahora suele ser:
    /// - origen del usuario
    /// - punto de encuentro
    /// 
    /// Más adelante se puede sustituir por la polyline real de OTP.
    /// </summary>
    public List<RoutePointModel> RoutePoints { get; private set; } = new();

    /// <summary>
    /// Indica si existe itinerario detallado con legs.
    /// </summary>
    public bool HasItinerary => Itinerary is not null && Itinerary.Legs.Count > 0;

    /// <summary>
    /// Propiedad segura para el CollectionView.
    /// 
    /// Evita usar ItemsSource="{Binding Itinerary.Legs}" en XAML,
    /// porque Itinerary puede ser null durante la construcción inicial de la vista.
    /// </summary>
    public List<RouteLegModel> Legs => Itinerary?.Legs ?? new List<RouteLegModel>();

    /// <summary>
    /// Muestra el mensaje contrario a HasItinerary.
    /// Lo usamos en XAML para evitar necesitar InvertedBoolConverter.
    /// </summary>
    public bool ShowNoItineraryMessage => !HasItinerary;

    /// <summary>
    /// Texto resumen del bottom sheet.
    /// </summary>
    public string SummaryText
    {
        get
        {
            if (IsDefaultMap)
                return "Sin datos de ruta";

            if (!HasValidRoute)
                return "Ruta no disponible";

            return $"{DurationText} · {DistanceText}";
        }
    }

    /// <summary>
    /// Duración formateada para UI.
    /// </summary>
    public string DurationText
    {
        get
        {
            if (IsDefaultMap)
                return "Sin datos de ruta";

            if (!HasValidRoute || DurationSeconds <= 0)
                return "Duración no disponible";

            if (DurationSeconds < 60)
                return $"{DurationSeconds} seg";

            int minutes = DurationSeconds / 60;
            int seconds = DurationSeconds % 60;

            if (seconds == 0)
                return $"{minutes} min";

            return $"{minutes} min {seconds} seg";
        }
    }

    /// <summary>
    /// Texto de transbordos.
    /// 
    /// Usa Itinerary porque ahí están los legs ya normalizados.
    /// </summary>
    public string TransfersText
    {
        get
        {
            if (!HasItinerary)
                return "Sin itinerario";

            int transfers = Itinerary!.TransfersCount;

            return transfers == 0
                ? "Sin transbordos"
                : $"{transfers} transbordo{(transfers == 1 ? "" : "s")}";
        }
    }

    partial void OnDurationSecondsChanged(int value)
    {
        OnPropertyChanged(nameof(DurationText));
        OnPropertyChanged(nameof(SummaryText));
    }

    partial void OnIsDefaultMapChanged(bool value)
    {
        OnPropertyChanged(nameof(DurationText));
        OnPropertyChanged(nameof(SummaryText));
    }

    partial void OnDistanceTextChanged(string value)
    {
        OnPropertyChanged(nameof(SummaryText));
    }

    partial void OnItineraryChanged(TransitItineraryModel? value)
    {
        OnPropertyChanged(nameof(HasItinerary));
        OnPropertyChanged(nameof(ShowNoItineraryMessage));
        OnPropertyChanged(nameof(TransfersText));
        OnPropertyChanged(nameof(Legs));
    }

    partial void OnHasValidRouteChanged(bool value)
    {
        OnPropertyChanged(nameof(DurationText));
        OnPropertyChanged(nameof(SummaryText));
        OnPropertyChanged(nameof(TransfersText));
    }

    /// <summary>
    /// Carga los datos del mapa.
    /// 
    /// Si existe CurrentResult, muestra el resultado real.
    /// Si no existe, carga un mapa por defecto centrado en Barcelona.
    /// </summary>
    public async Task Load()
    {
        Console.WriteLine($"[MapViewModel] CurrentResult null? {_meetingStateService.CurrentResult == null}");

        if (_meetingStateService.CurrentResult is not null)
        {
            var result = _meetingStateService.CurrentResult;

            Latitude = result.Latitude;
            Longitude = result.Longitude;
            DurationSeconds = result.DurationSeconds;

            OriginLatitude = result.OriginLatitude;
            OriginLongitude = result.OriginLongitude;

            /*
             * Valores base recibidos desde el resultado.
             * Después se intenta enriquecer MeetingPointName con calle + ciudad.
             */
            MeetingPointName = string.IsNullOrWhiteSpace(result.MeetingPointName)
                ? "Punto de encuentro"
                : result.MeetingPointName;

            AddressText = string.IsNullOrWhiteSpace(result.AddressText)
                ? "Dirección no disponible"
                : result.AddressText;

            DistanceText = string.IsNullOrWhiteSpace(result.DistanceText)
                ? "Distancia no disponible"
                : result.DistanceText;

            FairnessText = string.IsNullOrWhiteSpace(result.FairnessText)
                ? "Equilibrio no disponible"
                : result.FairnessText;

            RoutePoints = result.RoutePoints ?? new List<RoutePointModel>();
            Itinerary = result.Itinerary;
            HasValidRoute = result.HasValidRoute;

            IsDefaultMap = false;

            Console.WriteLine(
                $"[MapViewModel] Resultado mapa antes geocoding: " +
                $"Lat={Latitude.ToString(CultureInfo.InvariantCulture)}, " +
                $"Lon={Longitude.ToString(CultureInfo.InvariantCulture)}, " +
                $"Name='{MeetingPointName}', Address='{AddressText}'");

            /*
             * Reverse geocoding:
             * Coordenadas del punto de encuentro → calle + ciudad.
             * Si falla MAUI Geocoding, se intenta Nominatim/OpenStreetMap.
             * Si todo falla, se aplica fallback sin romper la vista.
             */
            await LoadMeetingPointAddressAsync(Latitude, Longitude);

            Console.WriteLine(
                $"[MapViewModel] Resultado mapa después geocoding: " +
                $"Name='{MeetingPointName}', Address='{AddressText}'");
        }
        else
        {
            Latitude = 41.3874;
            Longitude = 2.1686;
            DurationSeconds = 0;

            OriginLatitude = 41.3874;
            OriginLongitude = 2.1686;

            MeetingPointName = "Barcelona";
            AddressText = "Vista por defecto";
            DistanceText = "—";
            FairnessText = "Sin resultado";

            RoutePoints = new List<RoutePointModel>();
            Itinerary = null;
            HasValidRoute = false;

            IsDefaultMap = true;
        }

        /*
         * Forzamos refresco de propiedades calculadas.
         * Estas no son [ObservableProperty], así que hay que notificarlas manualmente.
         */
        OnPropertyChanged(nameof(DurationText));
        OnPropertyChanged(nameof(SummaryText));
        OnPropertyChanged(nameof(RoutePoints));
        OnPropertyChanged(nameof(HasItinerary));
        OnPropertyChanged(nameof(ShowNoItineraryMessage));
        OnPropertyChanged(nameof(TransfersText));
        OnPropertyChanged(nameof(Legs));
    }

    /// <summary>
    /// Convierte las coordenadas del punto de encuentro en una dirección legible.
    /// 
    /// Orden:
    /// 1. MAUI Geocoding nativo.
    /// 2. Nominatim/OpenStreetMap como fallback.
    /// 3. Fallback seguro si ambos fallan.
    /// </summary>
    private async Task LoadMeetingPointAddressAsync(double latitude, double longitude)
    {
        if (!IsValidCoordinate(latitude, longitude))
        {
            ApplyAddressFallback();
            return;
        }

        bool resolvedWithMaui =
            await TryResolveAddressWithMauiGeocodingAsync(latitude, longitude);

        if (resolvedWithMaui)
            return;

        bool resolvedWithNominatim =
            await TryResolveAddressWithNominatimAsync(latitude, longitude);

        if (resolvedWithNominatim)
            return;

        ApplyAddressFallback();
    }

    /// <summary>
    /// Intenta resolver la dirección usando el geocoder nativo de MAUI.
    /// En emuladores puede fallar o devolver información incompleta.
    /// </summary>
    private async Task<bool> TryResolveAddressWithMauiGeocodingAsync(
        double latitude,
        double longitude)
    {
        try
        {
            IEnumerable<Placemark> placemarks =
                await Geocoding.Default.GetPlacemarksAsync(latitude, longitude);

            Placemark? place = placemarks.FirstOrDefault();

            if (place is null)
                return false;

            string street = BuildStreetText(place);
            string city = BuildCityText(place);

            Console.WriteLine(
                $"[MapViewModel] MAUI geocoding. " +
                $"Street='{street}', City='{city}', " +
                $"Feature='{place.FeatureName}', " +
                $"Thoroughfare='{place.Thoroughfare}', " +
                $"SubThoroughfare='{place.SubThoroughfare}', " +
                $"Locality='{place.Locality}', " +
                $"SubAdminArea='{place.SubAdminArea}', " +
                $"AdminArea='{place.AdminArea}'");

            return ApplyResolvedAddress(street, city);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[MapViewModel] MAUI geocoding failed: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Intenta resolver la dirección usando Nominatim/OpenStreetMap.
    /// 
    /// Para demo local es útil porque suele ser más estable que el geocoder
    /// nativo del emulador.
    /// </summary>
    private async Task<bool> TryResolveAddressWithNominatimAsync(
        double latitude,
        double longitude)
    {
        try
        {
            using HttpClient client = new();

            /*
             * Nominatim exige User-Agent identificable.
             * Para demo local es suficiente.
             */
            client.DefaultRequestHeaders.UserAgent.ParseAdd(
                "JustMeetingPoint-MAUI/1.0");

            string url =
                $"https://nominatim.openstreetmap.org/reverse" +
                $"?format=jsonv2" +
                $"&lat={latitude.ToString(CultureInfo.InvariantCulture)}" +
                $"&lon={longitude.ToString(CultureInfo.InvariantCulture)}" +
                $"&zoom=18" +
                $"&addressdetails=1";

            NominatimReverseResponse? response =
                await client.GetFromJsonAsync<NominatimReverseResponse>(url);

            if (response is null)
                return false;

            string street = response.Address?.Road
                ?? response.Address?.Pedestrian
                ?? response.Address?.Footway
                ?? response.Address?.Cycleway
                ?? response.Address?.Neighbourhood
                ?? response.Address?.Suburb
                ?? response.Name
                ?? string.Empty;

            string city = response.Address?.City
                ?? response.Address?.Town
                ?? response.Address?.Village
                ?? response.Address?.Municipality
                ?? response.Address?.County
                ?? string.Empty;

            Console.WriteLine(
                $"[MapViewModel] Nominatim geocoding. " +
                $"Street='{street}', City='{city}', Display='{response.DisplayName}'");

            return ApplyResolvedAddress(street, city);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[MapViewModel] Nominatim geocoding failed: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Aplica la dirección resuelta al ViewModel.
    /// 
    /// Queremos que MeetingPointName sea lo más útil posible:
    /// - "Avinguda Diagonal, Barcelona"
    /// - "Carrer de Mallorca, Barcelona"
    /// - "Barcelona"
    /// </summary>
    private bool ApplyResolvedAddress(string? street, string? city)
    {
        street = street?.Trim() ?? string.Empty;
        city = city?.Trim() ?? string.Empty;

        if (!string.IsNullOrWhiteSpace(street) &&
            !string.IsNullOrWhiteSpace(city))
        {
            MeetingPointName = $"{street}, {city}";
            AddressText = city;
            return true;
        }

        if (!string.IsNullOrWhiteSpace(street))
        {
            MeetingPointName = street;

            if (string.IsNullOrWhiteSpace(AddressText) ||
                AddressText.Equals("Dirección no disponible", StringComparison.OrdinalIgnoreCase))
            {
                AddressText = "Ciudad no disponible";
            }

            return true;
        }

        if (!string.IsNullOrWhiteSpace(city))
        {
            MeetingPointName = city;
            AddressText = city;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Fallback seguro.
    /// 
    /// No debe volver a dejar una UI rota.
    /// Evita mantener "Punto de encuentro" como texto final si la dirección
    /// no se ha podido resolver.
    /// </summary>
    private void ApplyAddressFallback()
    {
        if (string.IsNullOrWhiteSpace(MeetingPointName) ||
            MeetingPointName.Equals("Punto de encuentro", StringComparison.OrdinalIgnoreCase))
        {
            MeetingPointName = "Ubicación aproximada";
        }

        if (string.IsNullOrWhiteSpace(AddressText))
            AddressText = "Dirección no disponible";
    }

    /// <summary>
    /// Construye la parte de calle desde MAUI Placemark.
    /// 
    /// Placemark puede devolver:
    /// - Thoroughfare: calle
    /// - SubThoroughfare: número
    /// - FeatureName: nombre de lugar si no hay calle exacta
    /// </summary>
    private static string BuildStreetText(Placemark place)
    {
        string? street = place.Thoroughfare;
        string? number = place.SubThoroughfare;
        string? featureName = place.FeatureName;

        if (!string.IsNullOrWhiteSpace(street) &&
            !string.IsNullOrWhiteSpace(number))
        {
            return $"{street} {number}";
        }

        if (!string.IsNullOrWhiteSpace(street))
            return street;

        /*
         * A veces FeatureName viene como número o dato poco útil.
         * Evitamos usarlo si parece solamente numérico.
         */
        if (!string.IsNullOrWhiteSpace(featureName) &&
            !double.TryParse(featureName, NumberStyles.Any, CultureInfo.InvariantCulture, out _))
        {
            return featureName;
        }

        return string.Empty;
    }

    /// <summary>
    /// Construye la parte de ciudad desde MAUI Placemark.
    /// 
    /// Según la zona, MAUI puede rellenar Locality, SubAdminArea o AdminArea.
    /// </summary>
    private static string BuildCityText(Placemark place)
    {
        if (!string.IsNullOrWhiteSpace(place.Locality))
            return place.Locality;

        if (!string.IsNullOrWhiteSpace(place.SubAdminArea))
            return place.SubAdminArea;

        if (!string.IsNullOrWhiteSpace(place.AdminArea))
            return place.AdminArea;

        return string.Empty;
    }

    /// <summary>
    /// Valida coordenadas antes de pedir reverse geocoding.
    /// </summary>
    private static bool IsValidCoordinate(double latitude, double longitude)
    {
        return latitude >= -90 &&
               latitude <= 90 &&
               longitude >= -180 &&
               longitude <= 180;
    }

    /// <summary>
    /// DTO mínimo para leer la respuesta de Nominatim.
    /// Se mantiene privado porque solo lo usa este ViewModel.
    /// </summary>
    private sealed class NominatimReverseResponse
    {
        [JsonPropertyName("display_name")]
        public string? DisplayName { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("address")]
        public NominatimAddress? Address { get; set; }
    }

    /// <summary>
    /// DTO mínimo para los campos de dirección de Nominatim.
    /// </summary>
    private sealed class NominatimAddress
    {
        [JsonPropertyName("road")]
        public string? Road { get; set; }

        [JsonPropertyName("pedestrian")]
        public string? Pedestrian { get; set; }

        [JsonPropertyName("footway")]
        public string? Footway { get; set; }

        [JsonPropertyName("cycleway")]
        public string? Cycleway { get; set; }

        [JsonPropertyName("neighbourhood")]
        public string? Neighbourhood { get; set; }

        [JsonPropertyName("suburb")]
        public string? Suburb { get; set; }

        [JsonPropertyName("city")]
        public string? City { get; set; }

        [JsonPropertyName("town")]
        public string? Town { get; set; }

        [JsonPropertyName("village")]
        public string? Village { get; set; }

        [JsonPropertyName("municipality")]
        public string? Municipality { get; set; }

        [JsonPropertyName("county")]
        public string? County { get; set; }
    }
}