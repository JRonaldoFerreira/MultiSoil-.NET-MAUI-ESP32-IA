// ViewModels/HistoricoRow.cs
using System;
using System.Linq;
using MultiSoil_EdgeAI.Models;

namespace MultiSoil_EdgeAI.ViewModels;

public class HistoricoRow
{
    public int Id { get; init; }
    public DateTime DataColeta { get; init; }
    public string TalhaoNome { get; init; } = string.Empty;
    public string IntervaloText { get; init; } = string.Empty;

    public double? Nitrogenio { get; init; }
    public double? Fosforo { get; init; }
    public double? Potassio { get; init; }
    public double? PH { get; init; }
    public double? CondutividadeEletrica { get; init; }
    public double? TemperaturaC { get; init; }
    public double? Umidade { get; init; }

    // Texto que aparece na lista de histórico
    public string ResumoText
    {
        get
        {
            string? Part(string label, double? v, string? suf = null)
                => v.HasValue ? $"{label}:{v.Value:0.##}{suf}" : null;

            var parts = new[]
            {
                Part("N",   Nitrogenio,            ""),
                Part("P",   Fosforo,               ""),
                Part("K",   Potassio,              ""),
                Part("pH",  PH,                    ""),
                Part("CE",  CondutividadeEletrica, ""),
                Part("T",   TemperaturaC,          "°C"),
                Part("Umid",Umidade,               "%")
            }.Where(p => p is not null);

            return parts.Any()
                ? string.Join("  ", parts!)
                : "Sem leituras registradas";
        }
    }

    public static HistoricoRow From(Historico h, string talhaoNome)
    {
        static string Fmt(int min) =>
            TimeSpan.FromMinutes(Math.Max(0, min))
                    .ToString(@"hh\:mm");

        return new HistoricoRow
        {
            Id = h.Id,
            DataColeta = h.DataColeta.Date,
            TalhaoNome = talhaoNome,
            IntervaloText = $"{Fmt(h.HoraInicioMin)} – {Fmt(h.HoraFimMin)}",

            Nitrogenio = h.Nitrogenio,
            Fosforo = h.Fosforo,
            Potassio = h.Potassio,
            PH = h.PH,
            CondutividadeEletrica = h.CondutividadeEletrica,
            TemperaturaC = h.TemperaturaC,
            Umidade = h.Umidade
        };
    }
}
