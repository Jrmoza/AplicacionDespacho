// Models/Reports/ResumenPorVariedad.cs  
using System.Collections.Generic;

namespace AplicacionDespacho.Models.Reports
{
    public class ResumenPorVariedad
    {
        public string Variedad { get; set; }
        public int TotalCajas { get; set; }
        public decimal TotalKilos { get; set; }
        public int TotalPallets { get; set; }
        public List<ResumenVariedadEmbalaje> DetallesPorEmbalaje { get; set; }

        public ResumenPorVariedad()
        {
            DetallesPorEmbalaje = new List<ResumenVariedadEmbalaje>();
        }
    }
}