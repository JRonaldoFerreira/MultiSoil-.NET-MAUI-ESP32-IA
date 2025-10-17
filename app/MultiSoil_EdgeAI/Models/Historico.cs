using SQLite;

namespace MultiSoil_EdgeAI.Models;

[Table("Historicos")]
public class Historico
{
    [PrimaryKey, AutoIncrement] public int Id { get; set; }

    [Indexed, NotNull] public int UserId { get; set; }
    [Indexed, NotNull] public int TalhaoId { get; set; }

    [NotNull] public DateTime DataColeta { get; set; }
    public int HoraInicioMin { get; set; }
    public int HoraFimMin { get; set; }

    // >>> TODAS AS LEITURAS COMO NULLABLE <<<
    public double? Nitrogenio { get; set; }
    public double? Fosforo { get; set; }
    public double? Potassio { get; set; }
    public double? PH { get; set; }
    public double? CondutividadeEletrica { get; set; }
    public double? TemperaturaC { get; set; }
    public double? Umidade { get; set; }

    public string? Observacoes { get; set; }

    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedUtc { get; set; }
}
