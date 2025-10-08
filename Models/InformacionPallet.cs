// Models/InformacionPallet.cs - Agregar campos para bicolor E50G6CB    
using AplicacionDespacho.utilities;

namespace AplicacionDespacho.Models
{
    public class InformacionPallet
    {
        // Campos existentes (mantener todos)    
        public string NumeroPallet { get; set; }
        public string Variedad { get; set; }
        public string Calibre { get; set; }
        public string Embalaje { get; set; }
        public int NumeroDeCajas { get; set; }
        public decimal PesoUnitario { get; set; }
        public decimal PesoTotal { get; set; }
        public bool TienePesoInconsistente { get; set; } = false;
        // Campos para rastrear modificaciones (mantener todos)    
        public string VariedadOriginal { get; set; }
        public string CalibreOriginal { get; set; }
        public string EmbalajeOriginal { get; set; }
        public int NumeroDeCajasOriginal { get; set; }
        public bool Modificado { get; set; } = false;
        public DateTime FechaEscaneo { get; set; } = FechaOperacionalHelper.ObtenerFechaOperacionalActual();
        public DateTime? FechaModificacion { get; set; }

        // NUEVOS CAMPOS PARA PALLETS BICOLOR E50G6CB  
        public bool EsBicolor { get; set; } = false;
        public string SegundaVariedad { get; set; }
        public int CajasSegundaVariedad { get; set; } = 0;

        // Campos para rastrear modificaciones bicolor  
        public string SegundaVariedadOriginal { get; set; }
        public int CajasSegundaVariedadOriginal { get; set; } = 0;

        // PROPIEDADES CALCULADAS PARA PALLETS BICOLOR  
        public int TotalCajasBicolor => EsBicolor ? (NumeroDeCajas + CajasSegundaVariedad) : NumeroDeCajas;
        public decimal PesoTotalBicolor => EsBicolor ? (NumeroDeCajas + CajasSegundaVariedad) * PesoUnitario : PesoTotal;

        // NUEVAS PROPIEDADES PARA CLASIFICACIÓN PC/PH/CT (TRES CATEGORÍAS) 
        public string TipoPallet => DeterminarTipoPallet();
        public bool EsPC => TipoPallet == "PC";
        public bool EsPH => TipoPallet == "PH";
        public bool EsCT => TipoPallet == "CT";
        public bool EsEN => TipoPallet == "EN";

        // Propiedades para reportería bicolor    
        public string VariedadParaReporte => EsBicolor ? $"{Variedad} + {SegundaVariedad}" : Variedad;
        public int CajasParaReporte => EsBicolor ? (NumeroDeCajas + CajasSegundaVariedad) : NumeroDeCajas;

        // Propiedades para mostrar información completa    
        public string DescripcionCompleta => EsBicolor ?
            $"BICOLOR: {Variedad} ({NumeroDeCajas}) + {SegundaVariedad} ({CajasSegundaVariedad}) = {TotalCajasBicolor} cajas" :
            $"{Variedad} - {NumeroDeCajas} cajas";

        public string VariedadDisplay => EsBicolor ?
            $"{Variedad} + {SegundaVariedad}" :
            Variedad;

        public string TotalCajasDisplay => EsBicolor ?
            $"{TotalCajasBicolor} ({NumeroDeCajas}+{CajasSegundaVariedad})" :
            NumeroDeCajas.ToString();

        // Método privado para determinar tipo de pallet (SOLO PC O PH)  
        private string DeterminarTipoPallet()
        {
            if (NumeroPallet.ToUpper().EndsWith("PC") || NumeroPallet.ToUpper().Contains("PC"))
                return "PC";
            else if (NumeroPallet.ToUpper().EndsWith("PH") || NumeroPallet.ToUpper().Contains("PH"))
                return "PH";
            else if (NumeroPallet.ToUpper().EndsWith("CT") || NumeroPallet.ToUpper().Contains("CT"))
                return "CT";
            else if (NumeroPallet.ToUpper().EndsWith("EN") || NumeroPallet.ToUpper().Contains("EN"))
                return "EN";
            else
                return "PC"; // Por defecto PC si no se puede determinar    
        }
    }
}