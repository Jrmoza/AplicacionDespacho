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
                // Limpiar datos anteriores  
                Lotes.Clear();
                PalletInfo = "";
                EstadoValidacion = "";

                // TODO: Implementar consulta con la nueva query que incluye lotes  
                // Por ahora usamos la consulta simple existente  
                var pallet = _accesoDatosPallet.ObtenerDatosPallet(NumeroPallet);

                if (pallet != null)
                {
                    PalletInfo = $"Pallet: {pallet.NumeroPallet}\n" +
                                $"Variedad: {pallet.Variedad}\n" +
                                $"Calibre: {pallet.Calibre}\n" +
                                $"Embalaje: {pallet.Embalaje}\n" +
                                $"Total Cajas: {pallet.NumeroDeCajas}";

                    // TODO: Cargar lotes desde la nueva consulta  
                    // Por ahora mostramos un lote único con el total  
                    Lotes.Add(new LoteInfo
                    {
                        CodigoCuartel = "N/A",
                        CSGPredio = "N/A",
                        NombrePredio = "N/A",
                        NombreProductor = "N/A",
                        CantidadCajas = pallet.NumeroDeCajas
                    });

                    EstadoValidacion = "Pallet encontrado";
                    ColorValidacion = Brushes.Green;
                }
                else
                {
                    PalletInfo = "Pallet no encontrado";
                    EstadoValidacion = "No encontrado";
                    ColorValidacion = Brushes.Red;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al buscar pallet:\n{ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                EstadoValidacion = "Error en búsqueda";
                ColorValidacion = Brushes.Red;
            }
        }

        private bool CanEliminarPallet(object parameter)
        {
            return !string.IsNullOrWhiteSpace(NumeroPallet) && Lotes.Count > 0;
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
                    // TODO: Implementar eliminación en AccesoDatosPallet  
                    // Por ahora mostramos mensaje de no implementado  
                    MessageBox.Show(
                        "Funcionalidad de eliminación pendiente de implementar.\n\n" +
                        "Se requiere crear método EliminarPallet en AccesoDatosPallet " +
                        "que ejecute las 5 consultas DELETE en orden correcto.",
                        "Pendiente",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);

                    // Limpiar después de eliminar  
                    // Lotes.Clear();  
                    // PalletInfo = "Pallet eliminado exitosamente";  
                    // EstadoValidacion = "Eliminado";  
                    // ColorValidacion = Brushes.Orange;  
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al eliminar pallet:\n{ex.Message}",
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
        public string CodigoCuartel { get; set; }
        public string CSGPredio { get; set; }
        public string NombrePredio { get; set; }
        public string NombreProductor { get; set; }
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