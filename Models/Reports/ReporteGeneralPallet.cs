// Models/Reports/ReporteGeneralPallet.cs  
using System;

namespace AplicacionDespacho.Models.Reports
{
    public class ReporteGeneralPallet
    {
        // Información del Pallet  
        public int PalletId { get; set; }
        public string NumeroPallet { get; set; }
        public string Variedad { get; set; }
        public string Calibre { get; set; }
        public string Embalaje { get; set; }
        public int NumeroDeCajas { get; set; }
        public decimal PesoUnitario { get; set; }
        public decimal PesoTotal { get; set; }
        public DateTime FechaEscaneo { get; set; }
        public bool Modificado { get; set; }

        // Información de Envío  
        public string EstadoEnvio { get; set; }
        public DateTime? FechaEnvio { get; set; }
        public string UsuarioEnvio { get; set; }

        // Información Completa del Viaje  
        public int ViajeId { get; set; }
        public DateTime FechaViaje { get; set; }
        public int NumeroViaje { get; set; }
        public string NumeroGuia { get; set; }
        public string Responsable { get; set; }
        public string PuntoPartida { get; set; }
        public string PuntoLlegada { get; set; }
        public string EstadoViaje { get; set; }
        public DateTime FechaCreacionViaje { get; set; }
        public DateTime? FechaModificacionViaje { get; set; }
        public string UsuarioCreacionViaje { get; set; }
        public string UsuarioModificacionViaje { get; set; }

        // Información de Transporte  
        public string NombreEmpresa { get; set; }
        public string RUCEmpresa { get; set; }
        public string NombreConductor { get; set; }
        public string PlacaVehiculo { get; set; }
    }
}