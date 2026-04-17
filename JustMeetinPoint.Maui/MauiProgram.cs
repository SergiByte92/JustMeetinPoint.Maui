using Microsoft.Extensions.Logging;
using JustMeetinPoint.Maui.Features.Auth.Services;
using JustMeetinPoint.Maui.Features.Auth.ViewModels;
using JustMeetinPoint.Maui.Features.Auth.Views;

namespace JustMeetinPoint.Maui;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

#if DEBUG
        builder.Logging.AddDebug();
#endif

        builder.Services.AddSingleton<IAuthService, SocketAuthService>();

        builder.Services.AddTransient<RegisterViewModel>();
        builder.Services.AddTransient<RegisterView>();

        return builder.Build();
    }
}