// Services/DataAccess/AccesoDatosPallet.cs - VERSIÓN CON LOGGING ROBUSTO  
using System;  
using System.Data.SqlClient;  
using AplicacionDespacho.Models;  
using AplicacionDespacho.Configuration;  
using AplicacionDespacho.Services.Logging;




namespace AplicacionDespacho.Services.DataAccess  
{  
    public class AccesoDatosPallet : IAccesoDatosPallet  
    {  
        private readonly string _cadenaConexion;  
        private readonly ILoggingService _logger;
        private readonly AccesoDatosEmbalajeBicolor _accesoDatosEmbalajeBicolor;
        public AccesoDatosPallet()  
        {
            _cadenaConexion = AppConfig.PackingSJPConnectionStringDynamic;
            _logger = LoggingFactory.CreateLogger("AccesoDatosPallet");
            _accesoDatosEmbalajeBicolor = new AccesoDatosEmbalajeBicolor();
            _logger.LogInfo("AccesoDatosPallet inicializado con conexión a {Database}", "Packing_SJP");  
        }  
  
        // Constructor para inyección de dependencias (opcional)    
        public AccesoDatosPallet(string connectionString)  
        {  
            _cadenaConexion = connectionString ?? throw new ArgumentNullException(nameof(connectionString));  
            _logger = LoggingFactory.CreateLogger("AccesoDatosPallet");  
              
            _logger.LogInfo("AccesoDatosPallet inicializado con cadena de conexión personalizada");  
        }

        public InformacionPallet ObtenerDatosPallet(string numeroPallet)
        {
            _logger.LogDebug("Iniciando búsqueda de pallet: {NumeroPallet}", numeroPallet);

            InformacionPallet informacionPallet = null;
            string numeroPalletLimpio = numeroPallet.Trim();

            string consulta = @"        
        SELECT         
            p.NUMERO_DEL_PALLETS AS Pallet,        
            p.CANTIDAD_DE_CAJAS AS CantidadCajas,        
            t.DESCRIPCION AS Calibre,        
            e.DESCRIPCION AS Embalaje,        
            r.Texto_Royalty AS Variedad        
        FROM         
            PALLETIZADOR p        
        LEFT JOIN         
            TIPO t ON p.CALIBRE = t.CODIGO        
        LEFT JOIN         
            EMBALAJE e ON p.EMBALAJE = e.CODIGO        
        LEFT JOIN        
            Royalty r ON e.CODIGO_VARIEDAD = r.Cod_Variedad        
        WHERE         
            p.NUMERO_DEL_PALLETS = @NumeroPallet;";

            using (SqlConnection conexion = new SqlConnection(_cadenaConexion))
            {
                using (SqlCommand comando = new SqlCommand(consulta, conexion))
                {
                    comando.Parameters.AddWithValue("@NumeroPallet", numeroPalletLimpio);

                    try
                    {
                        _logger.LogDebug("Ejecutando consulta para pallet: {NumeroPallet}", numeroPalletLimpio);

                        conexion.Open();
                        SqlDataReader lector = comando.ExecuteReader();

                        if (lector.Read())
                        {
                            informacionPallet = new InformacionPallet
                            {
                                NumeroPallet = lector["Pallet"].ToString(),
                                Variedad = lector["Variedad"].ToString(),
                                Calibre = lector["Calibre"].ToString(),
                                Embalaje = lector["Embalaje"].ToString(),
                                NumeroDeCajas = lector.GetInt32(lector.GetOrdinal("CantidadCajas"))
                            };

                            // NUEVO: Detectar si es bicolor 
                            if (_accesoDatosEmbalajeBicolor.EsEmbalajeBicolor(informacionPallet.Embalaje))
                            {
                                _logger.LogInfo("🎯 Pallet bicolor detectado: {NumeroPallet} - Embalaje: {Embalaje}",
                                informacionPallet.NumeroPallet, informacionPallet.Embalaje);

                                informacionPallet.EsBicolor = true;
                                // Los campos SegundaVariedad y CajasSegundaVariedad se llenarán manualmente en la UI  
                                informacionPallet.SegundaVariedad = "";
                                informacionPallet.CajasSegundaVariedad = 0;
                            }

                            _logger.LogInfo("Pallet encontrado: {NumeroPallet} - {Variedad} - {Cajas} cajas - Bicolor: {EsBicolor}",
                                          informacionPallet.NumeroPallet,
                                          informacionPallet.Variedad,
                                          informacionPallet.NumeroDeCajas,
                                          informacionPallet.EsBicolor);
                        }
                        else
                        {
                            _logger.LogWarning("Pallet no encontrado: {NumeroPallet}", numeroPalletLimpio);
                        }

                        lector.Close();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error al obtener datos del pallet {NumeroPallet}: {ErrorMessage}",
                                       numeroPalletLimpio, ex.Message);
                        throw;
                    }
                }
            }

            return informacionPallet;
        }
    }  
}