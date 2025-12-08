// Models/Talhao.cs
using SQLite;

namespace MultiSoil_EdgeAI.Models;

[Table("Talhoes")]
public class Talhao
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Indexed, NotNull]
    public int UserId { get; set; }

    [NotNull]
    public string Nome { get; set; } = string.Empty;

    [NotNull]
    public string Cultura { get; set; } = string.Empty;

    public double Latitude { get; set; }
    public double Longitude { get; set; }

    public string Microcontrolador { get; set; } = string.Empty;

    // NOVO: URL do servidor do ESP32 para este talhão
    [NotNull]
    public string ServidorUrl { get; set; } = string.Empty;

    public double AreaHa { get; set; } = 0d;

    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedUtc { get; set; }
}
