using CommunityToolkit.Mvvm;
using MultiSoil_EdgeAI.Data;
using MultiSoil_EdgeAI.Interfaces;
using MultiSoil_EdgeAI.Repositories;
using MultiSoil_EdgeAI.Services;
using MultiSoil_EdgeAI.Utils;
using MultiSoil_EdgeAI.ViewModels;
using MultiSoil_EdgeAI.Views;

namespace MultiSoil_EdgeAI;

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

        // DI: Database + Repos + Services
        builder.Services.AddSingleton<LocalDb>();
        builder.Services.AddSingleton<IUserRepository, SqliteUserRepository>();
        builder.Services.AddSingleton<ISessionService, SessionService>();
        builder.Services.AddSingleton<IAuthService, AuthService>();

        // ViewModels
        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<RegisterViewModel>();
        builder.Services.AddTransient<ReauthViewModel>();

        // Views
        

        // Views
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<ReauthPage>();
        builder.Services.AddTransient<RegisterPage>();
        builder.Services.AddTransient<DashboardPage>();
        
        var app = builder.Build();
        ServiceHelper.Initialize(app.Services);
        return app;
    }
}
