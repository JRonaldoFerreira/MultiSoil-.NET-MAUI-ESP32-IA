using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiSoil_EdgeAI.Models
{
    /// <summary>
    /// Representa a leitura atual vinda do ESP32 NPK.
    /// Os nomes das propriedades batem com o JSON: N, P, K, PH, CE, Temp, Umid.
    /// </summary>
    public sealed class Reading
    {
        public double? N { get; set; }
        public double? P { get; set; }
        public double? K { get; set; }
        public double? PH { get; set; }
        public double? CE { get; set; }
        public double? Temp { get; set; }
        public double? Umid { get; set; }
    }
}
