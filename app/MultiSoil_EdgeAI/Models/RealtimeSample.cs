using SQLite;

namespace MultiSoil_EdgeAI.Models;

[Table("RealtimeSamples")]
public class RealtimeSample
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Indexed, NotNull]
    public int UserId { get; set; }

    [Indexed, NotNull]
    public int TalhaoId { get; set; }

    [Indexed, NotNull]
    public DateTime Timestamp { get; set; }  // data/hora local da coleta

    public double? Nitrogenio { get; set; }
    public double? Fosforo { get; set; }
    public double? Potassio { get; set; }
    public double? PH { get; set; }
    public double? CondutividadeEletrica { get; set; }
    public double? TemperaturaC { get; set; }
    public double? Umidade { get; set; }
}
