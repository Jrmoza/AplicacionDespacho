using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using AplicacionDespacho.Models;
using AplicacionDespacho.Services.DataAccess;

namespace AplicacionDespacho.Modules.Trazabilidad.Profiles.Testeador.ViewModels
{
    /// <summary>    
    /// ViewModel para el perfil Testeador del módulo de Trazabilidad    
    /// Permite consultar y eliminar pallets de la base Packing_SJP    
    /// </summary>    
    public class TesteadorViewModel : INotifyPropertyChanged
    {
        private readonly AccesoDatosPallet _accesoDatosPallet;

        // Propiedades para búsqueda    
        private string _numeroPallet;
        public string NumeroPallet
        {
            get => _numeroPallet;
            set
            {
                _numeroPallet = value;
                OnPropertyChanged(nameof(NumeroPallet));
            }
        }

        // Propiedades para mostrar información del pallet    
        private string _palletInfo;
        public string PalletInfo
        {
            get => _palletInfo;
            set
            {
                _palletInfo = value;
                OnPropertyChanged(nameof(PalletInfo));
            }
        }

        // Lista de lotes/cuarteles del pallet    
        private ObservableCollection<LoteInfo> _lotes;
        public ObservableCollection<LoteInfo> Lotes
        {
            get => _lotes;
            set
            {
                _lotes = value;
                OnPropertyChanged(nameof(Lotes));
            }
        }

        // Estado de validación    
        private string _estadoValidacion;
        public string EstadoValidacion
        {
            get => _estadoValidacion;
            set
            {
                _estadoValidacion = value;
                OnPropertyChanged(nameof(EstadoValidacion));
            }
        }

        private Brush _colorValidacion;
        public Brush ColorValidacion
        {
            get => _colorValidacion;
            set
            {
                _colorValidacion = value;
                OnPropertyChanged(nameof(ColorValidacion));
            }
        }

        // Comandos    
        public ICommand BuscarPalletCommand { get; }
        public ICommand EliminarPalletCommand { get; }

        public TesteadorViewModel()
        {
            _accesoDatosPallet = new AccesoDatosPallet();
            Lotes = new ObservableCollection<LoteInfo>();

            BuscarPalletCommand = new RelayCommand(BuscarPallet, CanBuscarPallet);
            EliminarPalletCommand = new RelayCommand(EliminarPallet, CanEliminarPallet);

            PalletInfo = "Ingrese un número de pallet para consultar";
            EstadoValidacion = "";
            ColorValidacion = Brushes.Gray;
        }

        private bool CanBuscarPallet(object parameter)
        {
            return !string.IsNullOrWhiteSpace(NumeroPallet);
        }

        private void BuscarPallet(object parameter)
        {
            try
            {
                // ⭐ LOG 1: Inicio del método  
                Console.WriteLine("=== INICIO BuscarPallet ===");
                Console.WriteLine($"NumeroPallet ingresado: '{NumeroPallet}'");

                // Limpiar datos anteriores  
                Lotes.Clear();
                PalletInfo = "";
                EstadoValidacion = "";

                Console.WriteLine("Datos anteriores limpiados");

                // ⭐ LOG 2: Antes de llamar a ObtenerPalletConLotes  
                Console.WriteLine($"Llamando a _accesoDatosPallet.ObtenerPalletConLotes('{NumeroPallet}')...");

                // Intentar obtener información completa directamente  
                var (pallet, lotes, estadoValidacion) = _accesoDatosPallet.ObtenerPalletConLotes(NumeroPallet);

                // ⭐ LOG 3: Después de llamar a ObtenerPalletConLotes  
                Console.WriteLine($"Resultado de ObtenerPalletConLotes:");
                Console.WriteLine($"  - pallet: {(pallet != null ? "NO NULL" : "NULL")}");
                Console.WriteLine($"  - lotes: {(lotes != null ? $"Count={lotes.Count}" : "NULL")}");
                Console.WriteLine($"  - estadoValidacion: '{estadoValidacion}'");

                if (pallet != null && lotes != null && lotes.Count > 0)
                {
                    // ⭐ LOG 4: Pallet encontrado correctamente  
                    Console.WriteLine("✓ Pallet encontrado correctamente");
                    Console.WriteLine($"  - NumeroPallet: {pallet.NumeroPallet}");
                    Console.WriteLine($"  - Variedad: {pallet.Variedad}");
                    Console.WriteLine($"  - Calibre: {pallet.Calibre}");
                    Console.WriteLine($"  - Embalaje: {pallet.Embalaje}");
                    Console.WriteLine($"  - NumeroDeCajas: {pallet.NumeroDeCajas}");

                    // Pallet completo - mostrar normalmente  
                    PalletInfo = $"Pallet: {pallet.NumeroPallet}\n" +
                                $"Variedad: {pallet.Variedad}\n" +
                                $"Calibre: {pallet.Calibre}\n" +
                                $"Embalaje: {pallet.Embalaje}\n" +
                                $"Total Cajas: {pallet.NumeroDeCajas}";

                    // ⭐ LOG 5: Procesando lotes  
                    Console.WriteLine($"Procesando {lotes.Count} lotes...");
                    int loteIndex = 0;
                    foreach (var lote in lotes)
                    {
                        Console.WriteLine($"  Lote {loteIndex++}:");
                        Console.WriteLine($"    - CodigoCuartel: {lote.CodigoCuartel}");
                        Console.WriteLine($"    - NombreProductor: {lote.NombreProductor}");
                        Console.WriteLine($"    - CantidadCajas: {lote.CantidadCajas}");

                        // Verificar si tiene propiedades de variedad/calibre  
                        if (lote.GetType().GetProperty("VariedadCaja") != null)
                        {
                            var variedadCaja = lote.GetType().GetProperty("VariedadCaja").GetValue(lote);
                            Console.WriteLine($"    - VariedadCaja: {variedadCaja}");
                        }
                        if (lote.GetType().GetProperty("CalibreCaja") != null)
                        {
                            var calibreCaja = lote.GetType().GetProperty("CalibreCaja").GetValue(lote);
                            Console.WriteLine($"    - CalibreCaja: {calibreCaja}");
                        }
                        if (lote.GetType().GetProperty("EmbalajeCaja") != null)
                        {
                            var embalajeCaja = lote.GetType().GetProperty("EmbalajeCaja").GetValue(lote);
                            Console.WriteLine($"    - EmbalajeCaja: {embalajeCaja}");
                        }

                        Lotes.Add(lote);
                    }

                    // ⭐ LOG 6: Validación de suma de cajas  
                    int sumaCajas = Lotes.Sum(l => l.CantidadCajas);
                    Console.WriteLine($"Validación de cajas:");
                    Console.WriteLine($"  - Total declarado: {pallet.NumeroDeCajas}");
                    Console.WriteLine($"  - Suma de lotes: {sumaCajas}");
                    Console.WriteLine($"  - Coincide: {sumaCajas == pallet.NumeroDeCajas}");

                    if (sumaCajas == pallet.NumeroDeCajas)
                    {
                        EstadoValidacion = $"OK - {Lotes.Count} lote(s) encontrado(s)";
                        ColorValidacion = Brushes.Green;
                        Console.WriteLine("✓ Estado: OK");

                        /* // NUEVO: Mostrar mensaje de éxito  
                        MessageBox.Show(  
                            $"Pallet encontrado correctamente:\n\n" +  
                            $"• Número: {pallet.NumeroPallet}\n" +  
                            $"• Variedad: {pallet.Variedad}\n" +  
                            $"• Total cajas: {pallet.NumeroDeCajas}\n" +  
                            $"• Lotes: {Lotes.Count}\n" +  
                            $"• Estado: OK",  
                            "Pallet Correcto",  
                            MessageBoxButton.OK,  
                            MessageBoxImage.Information);*/
                    }
                    else
                    {
                        EstadoValidacion = $"DISCREPANCIA - Total: {pallet.NumeroDeCajas}, Suma lotes: {sumaCajas}";
                        ColorValidacion = Brushes.Orange;
                        Console.WriteLine("⚠ Estado: DISCREPANCIA");

                        // NUEVO: Mostrar advertencia de discrepancia  
                        MessageBox.Show(
                            $"⚠️ DISCREPANCIA DETECTADA:\n\n" +
                            $"Total declarado: {pallet.NumeroDeCajas} cajas\n" +
                            $"Suma de lotes: {sumaCajas} cajas\n\n" +
                            $"Verifique la información del pallet.",
                            "Advertencia",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);
                    }
                }
                else
                {
                    // ⭐ LOG 7: Pallet no encontrado o incompleto  
                    Console.WriteLine("✗ Pallet no encontrado con consulta principal");
                    Console.WriteLine("Verificando estado detallado del pallet...");

                    // Pallet no encontrado o incompleto - verificar estado detallado  
                    var (encontrado, completo, tablasConRegistros, mensaje) =
                        _accesoDatosPallet.VerificarEstadoPallet(NumeroPallet);

                    // ⭐ LOG 8: Resultado de VerificarEstadoPallet  
                    Console.WriteLine($"Resultado de VerificarEstadoPallet:");
                    Console.WriteLine($"  - encontrado: {encontrado}");
                    Console.WriteLine($"  - completo: {completo}");
                    Console.WriteLine($"  - tablasConRegistros: {tablasConRegistros}");
                    Console.WriteLine($"  - mensaje: {mensaje}");

                    if (!encontrado)
                    {
                        Console.WriteLine("✗ Pallet NO ENCONTRADO en ninguna tabla");

                        PalletInfo = "Pallet no encontrado en ninguna tabla";
                        EstadoValidacion = "No encontrado";
                        ColorValidacion = Brushes.Red;

                        // NUEVO: Mostrar mensaje de no encontrado  
                        MessageBox.Show(
                            $"El pallet '{NumeroPallet}' no existe en la base de datos Packing_SJP.",
                            "Pallet No Encontrado",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                    }
                    else
                    {
                        Console.WriteLine("⚠ Pallet INCOMPLETO (encontrado en algunas tablas)");

                        // Pallet incompleto  
                        PalletInfo = mensaje;
                        EstadoValidacion = "INCOMPLETO - Puede eliminar";
                        ColorValidacion = Brushes.Orange;

                        // NUEVO: Mostrar mensaje de pallet incompleto  
                        MessageBox.Show(
                            mensaje + "\n\n" +
                            "Puede eliminar este pallet usando el botón 'Eliminar Pallet'.",
                            "Pallet Incompleto",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);
                    }
                }

                // ⭐ LOG 9: Fin exitoso  
                Console.WriteLine("=== FIN BuscarPallet (EXITOSO) ===\n");
            }
            catch (Exception ex)
            {
                // ⭐ LOG 10: Error capturado  
                Console.WriteLine("=== ERROR EN BuscarPallet ===");
                Console.WriteLine($"Tipo de excepción: {ex.GetType().Name}");
                Console.WriteLine($"Mensaje: {ex.Message}");
                Console.WriteLine($"StackTrace:\n{ex.StackTrace}");

                if (ex.InnerException != null)
                {
                    Console.WriteLine($"InnerException: {ex.InnerException.Message}");
                    Console.WriteLine($"InnerException StackTrace:\n{ex.InnerException.StackTrace}");
                }
                Console.WriteLine("=== FIN ERROR ===\n");

                MessageBox.Show($"Error al buscar pallet:\n{ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                EstadoValidacion = "Error en búsqueda";
                ColorValidacion = Brushes.Red;
            }
        }

        private bool CanEliminarPallet(object parameter)
        {
            // Permitir eliminación si:  
            // 1. Hay lotes cargados (pallet completo), O  
            // 2. El estado es "INCOMPLETO" (pallet parcial detectado)  
            return !string.IsNullOrWhiteSpace(NumeroPallet) &&
                   (Lotes.Count > 0 || EstadoValidacion.Contains("INCOMPLETO"));
        }

        private void EliminarPallet(object parameter)
        {
            var result = MessageBox.Show(
                $"¿Está seguro que desea eliminar el pallet {NumeroPallet}?\n\n" +
                "Esta acción eliminará el pallet de las siguientes tablas:\n" +
                "- Palet_Listos\n" +
                "- Cabecera_Palet\n" +
                "- Detalles_Lecturas\n" +
                "- DETALLE_PALLETIZADOR\n" +
                "- PALLETIZADOR\n\n" +
                "Esta acción NO se puede deshacer.",
                "Confirmar Eliminación",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    // Ejecutar eliminación usando el nuevo método  
                    bool eliminado = _accesoDatosPallet.EliminarPallet(NumeroPallet);

                    if (eliminado)
                    {
                        MessageBox.Show(
                            $"El pallet {NumeroPallet} ha sido eliminado exitosamente de todas las tablas.",
                            "Eliminación Exitosa",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);

                        // Limpiar la interfaz después de eliminar  
                        Lotes.Clear();
                        PalletInfo = "Pallet eliminado exitosamente";
                        EstadoValidacion = "Eliminado";
                        ColorValidacion = Brushes.Orange;
                        NumeroPallet = string.Empty;
                    }
                    else
                    {
                        MessageBox.Show(
                            $"No se pudo eliminar el pallet {NumeroPallet}. Verifique que exista en la base de datos.",
                            "Error de Eliminación",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        $"Error al eliminar pallet:\n{ex.Message}\n\n" +
                        "La transacción ha sido revertida. No se realizaron cambios en la base de datos.",
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    /// <summary>    
    /// Clase auxiliar para mostrar información de lotes en el DataGrid    
    /// </summary>    
    public class LoteInfo
    {
        public DateTime Fecha { get; set; }                    // ⭐ NUEVO  
        public string CodigoCuartel { get; set; }
        public string CSGPredio { get; set; }
        public string NombrePredio { get; set; }
        public string NombreProductor { get; set; }
        public string EmbalajeCaja { get; set; }               // ⭐ NUEVO  
        public string VariedadCaja { get; set; }               // ⭐ NUEVO  
        public string CalibreCaja { get; set; }                // ⭐ NUEVO  
        public int CantidadCajas { get; set; }
    }
    /// <summary>    
    /// Implementación simple de ICommand para los botones    
    /// </summary>    
    public class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Predicate<object> _canExecute;

        public RelayCommand(Action<object> execute, Predicate<object> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute(parameter);
        }

        public void Execute(object parameter)
        {
            _execute(parameter);
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }
}