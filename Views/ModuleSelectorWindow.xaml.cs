using AplicacionDespacho.Modules.Common.Interfaces;
using AplicacionDespacho.Modules.Common.Models;
using AplicacionDespacho.Modules.Common.Views;
using AplicacionDespacho.Modules.Despacho;
using AplicacionDespacho.Modules.Trazabilidad;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace AplicacionDespacho.Views
{
    public partial class ModuleSelectorWindow : Window
    {
        private List<IModule> _availableModules;

        public ModuleSelectorWindow()
        {
            InitializeComponent();
            LoadAvailableModules();
        }

        private void LoadAvailableModules()
        {
            _availableModules = new List<IModule>
            {
                new DespachoModule(),
                new TrazabilidadModule()
            };

            // Filtrar solo módulos habilitados y ordenar      
            var enabledModules = _availableModules
                .Where(m => m.GetModuleInfo().IsEnabled)
                .OrderBy(m => m.GetModuleInfo().DisplayOrder)
                .Select(m => m.GetModuleInfo())
                .ToList();

            ModulesItemsControl.ItemsSource = enabledModules;
        }

    
        // Método para abrir módulo sin perfil específico    
        private void OpenModule_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is ModuleInfo moduleInfo)
            {
                var module = _availableModules.FirstOrDefault(m => m.GetModuleInfo().ModuleId == moduleInfo.ModuleId);
                if (module != null)
                {
                    LaunchModule(module, null);  // ✅ CORRECTO - pasar null como segundo parámetro  
                }
            }
        }

        // Método para hacer clic en la tarjeta del módulo  
        private void ModuleCard_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.Tag is string moduleId)
            {
                var module = _availableModules.FirstOrDefault(m => m.GetModuleInfo().ModuleId == moduleId);
                if (module != null && (module.GetModuleInfo().AvailableProfiles == null ||
                       module.GetModuleInfo().AvailableProfiles.Count == 0))
                {
                    LaunchModule(module, null);
                }
            }
        }

        // NUEVO: Método para abrir módulo con perfil específico  
        private void ProfileButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                // El Content del botón es el nombre del perfil (string)  
                string profile = button.Content.ToString();

                // Buscar el ModuleInfo desde el DataContext del ItemsControl  
                var moduleInfo = button.DataContext as string; // El perfil  

                // Necesitamos encontrar el módulo padre  
                // Buscar en el árbol visual para obtener el Border que contiene el ModuleInfo  
                DependencyObject parent = VisualTreeHelper.GetParent(button);
                while (parent != null && !(parent is Border))
                {
                    parent = VisualTreeHelper.GetParent(parent);
                }

                if (parent is Border border && border.DataContext is ModuleInfo moduleInfoObj)
                {
                    string moduleId = moduleInfoObj.ModuleId;

                    // Crear NUEVA instancia del módulo con el perfil específico  
                    IModule module = null;
                    if (moduleId == "Trazabilidad")
                    {
                        module = new TrazabilidadModule(profile);
                    }

                    if (module != null)
                    {
                        LaunchModule(module, null);
                    }
                }
            }
        }

        private void LaunchModule(IModule module, string profile)
        {
            try
            {
                // Si el módulo es TrazabilidadModule y se especificó un perfil, configurarlo    
                if (module is TrazabilidadModule trazaModule && !string.IsNullOrEmpty(profile))
                {
                    trazaModule.SwitchProfile(profile);
                }

                // Inicializar el módulo        
                var moduleWindow = module.InitializeModule();

                // Ocultar selector        
                this.Hide();

                // Mostrar ventana del módulo        
                moduleWindow.ShowDialog();

                // Al cerrar, volver a mostrar selector        
                this.Show();

                // Limpiar recursos        
                module.Cleanup();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error al iniciar el módulo {module.GetModuleInfo().DisplayName}:\n\n{ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void ConfigurarBaseDatos_Click(object sender, RoutedEventArgs e)
        {
            var ventanaConfig = new ConfiguracionBaseDatosWindow();
            ventanaConfig.Owner = this;
            ventanaConfig.ShowDialog();
        }

        private void ConfigurarSignalR_Click(object sender, RoutedEventArgs e)
        {
            var ventanaConfig = new ConfiguracionSignalRWindow();
            ventanaConfig.Owner = this;
            ventanaConfig.ShowDialog();
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "¿Está seguro que desea salir del sistema?",
                "Confirmar Salida",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                Application.Current.Shutdown();
            }
        }
    }

    // NUEVO: Convertidor para mostrar elemento cuando Count > 0  
    public class CountToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is List<string> list)
            {
                return list.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    // NUEVO: Convertidor inverso para mostrar elemento cuando Count == 0  
    public class InverseCountToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is List<string> list)
            {
                return list.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}