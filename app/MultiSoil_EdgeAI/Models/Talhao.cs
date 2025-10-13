using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;

namespace MultiSoil_EdgeAI.Models
{
  

    [Table("Talhoes")]
    public class Talhao
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }


        [NotNull, MaxLength(120)]
        public string Nome { get; set; } = string.Empty;


        [NotNull, MaxLength(80)]
        public string Cultura { get; set; } = string.Empty; // ex.: Milho, Soja


        // Área em hectares
        [NotNull]
        public double AreaHa { get; set; }


        public double Latitude { get; set; }
        public double Longitude { get; set; }


        public DateTime? DataPlantio { get; set; }


        [NotNull, MaxLength(24)]
        public string Status { get; set; } = "Ativo"; // Ativo/Inativo


        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAtUtc { get; set; }
    }
}
