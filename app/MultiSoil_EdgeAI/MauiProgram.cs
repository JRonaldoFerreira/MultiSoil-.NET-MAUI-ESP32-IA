using CommunityToolkit.Mvvm;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
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

#if DEBUG
        builder.Logging.AddDebug();
#endif

        // -----------------------
        // DI: Data / Repos / Services
        // -----------------------
        builder.Services.AddSingleton<LocalDb>();

        // Repositories
        builder.Services.AddSingleton<IUserRepository, SqliteUserRepository>();
        builder.Services.AddSingleton<ITalhaoRepository, SqliteTalhaoRepository>();

        // Domain Services
        builder.Services.AddSingleton<ISessionService, SessionService>();
        builder.Services.AddSingleton<IAuthService, AuthService>();
        builder.Services.AddSingleton<ITalhaoService, TalhaoService>();

        // -----------------------
        // ViewModels
        // -----------------------
        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<RegisterViewModel>();
        builder.Services.AddTransient<ReauthViewModel>();
        builder.Services.AddTransient<TalhoesViewModel>();
        builder.Services.AddTransient<TalhaoFormViewModel>();

        // -----------------------
        // Views
        // -----------------------
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<RegisterPage>();
        builder.Services.AddTransient<ReauthPage>();
        builder.Services.AddTransient<DashboardPage>();
        builder.Services.AddTransient<TalhoesPage>();
        builder.Services.AddTransient<TalhaoFormPage>();

        var app = builder.Build();

        // (Opcional) Service locator para resolver serviços fora do DI-constructor
        ServiceHelper.Initialize(app.Services);

        // Inicializa o banco (cria tabelas/seed)
        var db = app.Services.GetRequiredService<LocalDb>();
        db.InitializeAsync().GetAwaiter().GetResult();

        return app;
    }
}
