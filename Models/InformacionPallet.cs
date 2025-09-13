using AplicacionDespacho.utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


// Models/InformacionPallet.cs
namespace AplicacionDespacho.Models
{
    public class InformacionPallet
    {
        public string NumeroPallet { get; set; }
        public string Variedad { get; set; }
        public string Calibre { get; set; }
        public string Embalaje { get; set; }
        public int NumeroDeCajas { get; set; }

        // Nuevos campos para peso  
        public decimal PesoUnitario { get; set; }
        public decimal PesoTotal { get; set; }

        // Campos para rastrear modificaciones  
        public string VariedadOriginal { get; set; }
        public string CalibreOriginal { get; set; }
        public string EmbalajeOriginal { get; set; }
        public int NumeroDeCajasOriginal { get; set; }
        public bool Modificado { get; set; } = false;
        public DateTime FechaEscaneo { get; set; } = FechaOperacionalHelper.ObtenerFechaOperacionalActual();
        public DateTime? FechaModificacion { get; set; }
    }
}