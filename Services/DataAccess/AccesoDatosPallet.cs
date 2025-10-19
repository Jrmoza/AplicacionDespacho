// Services/DataAccess/AccesoDatosPallet.cs - VERSIÓN CON LOGGING ROBUSTO  
using AplicacionDespacho.Configuration;
using AplicacionDespacho.Models;
using AplicacionDespacho.Modules.Trazabilidad.Profiles.Testeador.ViewModels;
using AplicacionDespacho.Services.Logging;
using System;
using System.Collections.Generic;  // ⭐ AGREGAR ESTE USING  
using System.Data.SqlClient;



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

        public (InformacionPallet pallet, List<LoteInfo> lotes, string estadoValidacion) ObtenerPalletConLotes(string numeroPallet)
        {
            string consulta = @"    
        SELECT               
            CAST(GETDATE() AS DATE) AS Fecha,  
            p.NUMERO_DEL_PALLETS,              
            p.CANTIDAD_DE_CAJAS AS TotalCajas,              
            t.DESCRIPCION AS CalibreDescripcion,              
            e.DESCRIPCION AS EmbalajeDescripcion,              
            r.Texto_Royalty AS VariedadNombre,              
            ps.CUARTEL AS CodigoCuartel,              
            prod.DESCRIPCION AS NombreProductor,              
            pr.CSG AS CSGPredio,              
            pr.DESCRIPCION AS NombrePredio,           
            COUNT(dp.NUMERO_UNICO) AS CajasPorCuartel,      
            t_detalle.DESCRIPCION AS CalibreLote,      
            e_detalle.DESCRIPCION AS EmbalajeDetalle,  
            r_detalle.Texto_Royalty AS VariedadLote      
        FROM PALLETIZADOR p              
        LEFT JOIN TIPO t ON p.CALIBRE = t.CODIGO              
        LEFT JOIN EMBALAJE e ON p.EMBALAJE = e.CODIGO              
        LEFT JOIN Royalty r ON e.CODIGO_VARIEDAD = r.Cod_Variedad              
        INNER JOIN DETALLE_PALLETIZADOR dp ON p.NUMERO_DEL_PALLETS = dp.NUMERO_DEL_PALLETS              
        INNER JOIN PROGRAMA_SELECCION ps ON dp.PROGRAMA = ps.CORRELATIVO              
        LEFT JOIN PRODUCTOR prod ON ps.PRODUCTOR = prod.CODIGO              
        LEFT JOIN PREDIO pr ON ps.PREDIO = pr.CODIGO_PREDIO AND ps.PRODUCTOR = pr.CODIGO_PRODUCTOR      
        LEFT JOIN TIPO t_detalle ON dp.CALIBRE = t_detalle.CODIGO      
        LEFT JOIN EMBALAJE e_detalle ON dp.EMBALAJE = e_detalle.CODIGO      
        LEFT JOIN Royalty r_detalle ON e_detalle.CODIGO_VARIEDAD = r_detalle.Cod_Variedad      
        WHERE p.NUMERO_DEL_PALLETS = @NumeroPallet              
        GROUP BY p.NUMERO_DEL_PALLETS, p.CANTIDAD_DE_CAJAS, t.DESCRIPCION, e.DESCRIPCION,         
                 r.Texto_Royalty, ps.CUARTEL, prod.DESCRIPCION, pr.CSG, pr.DESCRIPCION,      
                 t_detalle.DESCRIPCION, e_detalle.DESCRIPCION, r_detalle.Texto_Royalty  
        ORDER BY ps.CUARTEL, t_detalle.DESCRIPCION";

            string consultaValidacion = @"    
        SELECT p.NUMERO_DEL_PALLETS, p.CANTIDAD_DE_CAJAS AS TotalDeclarado,      
               COUNT(dp.NUMERO_UNICO) AS TotalContado,      
               CASE WHEN p.CANTIDAD_DE_CAJAS = COUNT(dp.NUMERO_UNICO) THEN 'OK' ELSE 'DISCREPANCIA' END AS EstadoValidacion      
        FROM PALLETIZADOR p      
        INNER JOIN DETALLE_PALLETIZADOR dp ON p.NUMERO_DEL_PALLETS = dp.NUMERO_DEL_PALLETS      
        WHERE p.NUMERO_DEL_PALLETS = @NumeroPallet      
        GROUP BY p.NUMERO_DEL_PALLETS, p.CANTIDAD_DE_CAJAS";

            using (SqlConnection conexion = new SqlConnection(_cadenaConexion))
            {
                conexion.Open();

                InformacionPallet pallet = null;
                List<LoteInfo> lotes = new List<LoteInfo>();
                string estadoValidacion = "";

                // Ejecutar consulta principal    
                using (SqlCommand comando = new SqlCommand(consulta, conexion))
                {
                    comando.Parameters.AddWithValue("@NumeroPallet", numeroPallet.Trim());

                    using (SqlDataReader reader = comando.ExecuteReader())
                    {
                        bool primeraFila = true;
                        while (reader.Read())
                        {
                            if (primeraFila)
                            {
                                pallet = new InformacionPallet
                                {
                                    NumeroPallet = reader["NUMERO_DEL_PALLETS"].ToString(),
                                    NumeroDeCajas = Convert.ToInt32(reader["TotalCajas"]),
                                    CalibreCaja = reader["CalibreLote"].ToString(),
                                    EmbalajeCaja = reader["EmbalajeDetalle"].ToString(),
                                    VariedadCaja = reader["VariedadLote"].ToString()
                                };
                                primeraFila = false;
                            }

                            lotes.Add(new LoteInfo
                            {
                                Fecha = reader.GetDateTime(reader.GetOrdinal("Fecha")),
                                CodigoCuartel = reader["CodigoCuartel"].ToString(),
                                CSGPredio = reader["CSGPredio"].ToString(),
                                NombrePredio = reader["NombrePredio"].ToString(),
                                NombreProductor = reader["NombreProductor"].ToString(),
                                CantidadCajas = Convert.ToInt32(reader["CajasPorCuartel"]),
                                CalibreCaja = reader["CalibreLote"].ToString(),
                                EmbalajeCaja = reader["EmbalajeDetalle"].ToString(),
                                VariedadCaja = reader["VariedadLote"].ToString()
                            });
                        }
                    }
                }

                // Ejecutar consulta de validación    
                if (pallet != null)
                {
                    using (SqlCommand comandoValidacion = new SqlCommand(consultaValidacion, conexion))
                    {
                        comandoValidacion.Parameters.AddWithValue("@NumeroPallet", numeroPallet.Trim());

                        using (SqlDataReader reader = comandoValidacion.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                estadoValidacion = reader["EstadoValidacion"].ToString();
                            }
                        }
                    }
                }

                return (pallet, lotes, estadoValidacion);
            }
        }

        public (bool encontrado, bool completo, List<string> tablasConRegistros, string mensaje) VerificarEstadoPallet(string numeroPallet)
        {
            _logger.LogInfo("Verificando estado del pallet: {NumeroPallet}", numeroPallet);

            var tablasConRegistros = new List<string>();
            string numeroPalletLimpio = numeroPallet.Trim();

            using (SqlConnection conexion = new SqlConnection(_cadenaConexion))
            {
                conexion.Open();

                // Verificar en cada tabla  
                string[] consultas = new string[]
                {
            "SELECT COUNT(*) FROM Palet_Listos WHERE palet = @NumeroPallet",
            "SELECT COUNT(*) FROM Cabecera_Palet WHERE n_pallet = @NumeroPallet",
            "SELECT COUNT(*) FROM Detalles_Lecturas WHERE n_palet = @NumeroPallet",
            "SELECT COUNT(*) FROM DETALLE_PALLETIZADOR WHERE NUMERO_DEL_PALLETS = @NumeroPallet",
            "SELECT COUNT(*) FROM PALLETIZADOR WHERE NUMERO_DEL_PALLETS = @NumeroPallet"
                };

                string[] nombresTablas = new string[]
                {
            "Palet_Listos",
            "Cabecera_Palet",
            "Detalles_Lecturas",
            "DETALLE_PALLETIZADOR",
            "PALLETIZADOR"
                };

                for (int i = 0; i < consultas.Length; i++)
                {
                    using (SqlCommand comando = new SqlCommand(consultas[i], conexion))
                    {
                        comando.Parameters.AddWithValue("@NumeroPallet", numeroPalletLimpio);
                        int count = (int)comando.ExecuteScalar();

                        if (count > 0)
                        {
                            tablasConRegistros.Add(nombresTablas[i]);
                            _logger.LogDebug("Pallet {NumeroPallet} encontrado en {Tabla}: {Count} registros",
                                numeroPalletLimpio, nombresTablas[i], count);
                        }
                    }
                }
            }

            bool encontrado = tablasConRegistros.Count > 0;
            bool completo = tablasConRegistros.Count == 5; // Debe estar en las 5 tablas  

            string mensaje;
            if (!encontrado)
            {
                mensaje = "Pallet no encontrado en ninguna tabla";
            }
            else if (completo)
            {
                mensaje = "Pallet completo - registrado en todas las tablas";
            }
            else
            {
                mensaje = $"⚠️ PALLET INCOMPLETO - Solo encontrado en: {string.Join(", ", tablasConRegistros)}\n" +
                          $"Falta en: {string.Join(", ", new[] { "Palet_Listos", "Cabecera_Palet", "Detalles_Lecturas", "DETALLE_PALLETIZADOR", "PALLETIZADOR" }.Except(tablasConRegistros))}";
            }

            _logger.LogInfo("Estado del pallet {NumeroPallet}: Encontrado={Encontrado}, Completo={Completo}, Tablas={Tablas}",
                numeroPalletLimpio, encontrado, completo, string.Join(", ", tablasConRegistros));

            return (encontrado, completo, tablasConRegistros, mensaje);
        }

        public bool EliminarPallet(string numeroPallet)
        {
            _logger.LogInfo("Iniciando eliminación de pallet: {NumeroPallet}", numeroPallet);

            using (SqlConnection conexion = new SqlConnection(_cadenaConexion))
            {
                conexion.Open();
                using (SqlTransaction transaccion = conexion.BeginTransaction())
                {
                    try
                    {
                        // Orden crítico: eliminar de tablas dependientes primero  
                        string[] consultasDelete = new string[]
                        {
                    "DELETE FROM Palet_Listos WHERE palet = @NumeroPallet",
                    "DELETE FROM Cabecera_Palet WHERE n_pallet = @NumeroPallet",
                    "DELETE FROM Detalles_Lecturas WHERE n_palet = @NumeroPallet",
                    "DELETE FROM DETALLE_PALLETIZADOR WHERE NUMERO_DEL_PALLETS = @NumeroPallet",
                    "DELETE FROM PALLETIZADOR WHERE NUMERO_DEL_PALLETS = @NumeroPallet"
                        };

                        foreach (string consulta in consultasDelete)
                        {
                            using (SqlCommand comando = new SqlCommand(consulta, conexion, transaccion))
                            {
                                comando.Parameters.AddWithValue("@NumeroPallet", numeroPallet.Trim());
                                int filasAfectadas = comando.ExecuteNonQuery();
                                _logger.LogDebug("Consulta: {Consulta}, Filas afectadas: {Filas}", consulta, filasAfectadas);
                            }
                        }

                        transaccion.Commit();
                        _logger.LogInfo("Pallet {NumeroPallet} eliminado exitosamente", numeroPallet);
                        return true;
                    }
                    catch (Exception ex)
                    {
                        transaccion.Rollback();
                        _logger.LogError(ex, "Error al eliminar pallet {NumeroPallet}", numeroPallet);
                        throw;
                    }
                }
            }
        }
        public (bool encontrado, List<string> tablasConRegistros) VerificarExistenciaPallet(string numeroPallet)
        {
            var tablasConRegistros = new List<string>();

            using (SqlConnection conexion = new SqlConnection(_cadenaConexion))
            {
                conexion.Open();

                // Verificar cada tabla  
                string[] tablas = new string[]
                {
            "SELECT COUNT(*) FROM PALLETIZADOR WHERE NUMERO_DEL_PALLETS = @NumeroPallet",
            "SELECT COUNT(*) FROM DETALLE_PALLETIZADOR WHERE NUMERO_DEL_PALLETS = @NumeroPallet",
            "SELECT COUNT(*) FROM Palet_Listos WHERE palet = @NumeroPallet",
            "SELECT COUNT(*) FROM Cabecera_Palet WHERE n_pallet = @NumeroPallet",
            "SELECT COUNT(*) FROM Detalles_Lecturas WHERE n_palet = @NumeroPallet"
                };

                string[] nombresTablas = { "PALLETIZADOR", "DETALLE_PALLETIZADOR", "Palet_Listos", "Cabecera_Palet", "Detalles_Lecturas" };

                for (int i = 0; i < tablas.Length; i++)
                {
                    using (SqlCommand comando = new SqlCommand(tablas[i], conexion))
                    {
                        comando.Parameters.AddWithValue("@NumeroPallet", numeroPallet.Trim());
                        int count = (int)comando.ExecuteScalar();

                        if (count > 0)
                        {
                            tablasConRegistros.Add(nombresTablas[i]);
                        }
                    }
                }
            }

            return (tablasConRegistros.Count > 0, tablasConRegistros);
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