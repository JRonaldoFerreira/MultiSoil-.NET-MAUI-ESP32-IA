// MauiProgram.cs
using System;
using CommunityToolkit.Maui;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;
using MultiSoil_EdgeAI.Data;
using MultiSoil_EdgeAI.Interfaces;
using MultiSoil_EdgeAI.Repositories;

using MultiSoil_EdgeAI.Services;
using MultiSoil_EdgeAI.Utils;
using MultiSoil_EdgeAI.ViewModels;
using MultiSoil_EdgeAI.Views;
using Microsoft.Extensions.DependencyInjection; // <- necessário para AddHttpClient



namespace MultiSoil_EdgeAI;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // ========= HTTP client para ler as medições do ESP32 =========
        builder.Services.AddHttpClient<ISensorReadingService, SensorReadingService>(client =>
        {
            // Usa o IP que aparece no monitor serial do ESP32
            client.BaseAddress = new Uri("http://192.168.100.38/");
            client.Timeout = TimeSpan.FromSeconds(3);
        });


        // ========= Infra e Serviços =========
        builder.Services.AddSingleton<LocalDb>();                                  // DB local (estado compartilhado)
        builder.Services.AddSingleton<ISessionService, SessionService>();          // sessão/autenticação
        builder.Services.AddSingleton<IAuthService, AuthService>();
        builder.Services.AddSingleton<ITalhaoSelectionService, TalhaoSelectionService>(); // seleção de talhão atual

        // ========= Repositórios (acesso a dados) =========
        builder.Services.AddTransient<IUserRepository, SqliteUserRepository>();
        builder.Services.AddTransient<ITalhaoRepository, SqliteTalhaoRepository>();
        builder.Services.AddTransient<IHistoricoRepository, SqliteHistoricoRepository>();

        // ========= ViewModels =========
        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<RegisterViewModel>();
        builder.Services.AddTransient<ReauthViewModel>();
        builder.Services.AddTransient<DashboardViewModel>();
        builder.Services.AddTransient<HistoricosViewModel>();
        builder.Services.AddTransient<HistoricoFormViewModel>();

        // ========= Views =========
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<RegisterPage>();
        builder.Services.AddTransient<ReauthPage>();
        builder.Services.AddTransient<DashboardPage>();
        builder.Services.AddTransient<HistoricosPage>();
        builder.Services.AddTransient<HistoricoFormPage>();
        builder.Services.AddTransient<TalhoesPage>();
        builder.Services.AddTransient<TalhaoFormPage>();
        builder.Services.AddTransient<RealtimePage>();

        var app = builder.Build();
        ServiceHelper.Initialize(app.Services);
        return app;
    }
}
