using AplicacionDespacho.Modules.Common.Interfaces;
using AplicacionDespacho.Modules.Common.Views;
using AplicacionDespacho.Modules.Despacho;
using AplicacionDespacho.Modules.Trazabilidad;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

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

        /// <summary>  
        /// Carga todos los módulos disponibles del sistema  
        /// </summary>  
        private void LoadAvailableModules()
        {
            _availableModules = new List<IModule>
            {
                new DespachoModule(),
                new TrazabilidadModule()  
                // FUTURO: Agregar más módulos aquí  
                // new RecepcionModule(),  
                // new AbocadoModule(),  
                // etc.  
            };

            // Filtrar solo módulos habilitados y ordenar por DisplayOrder  
            var enabledModules = _availableModules
                .Where(m => m.ModuleInfo.IsEnabled)
                .OrderBy(m => m.ModuleInfo.DisplayOrder)
                .Select(m => m.ModuleInfo)
                .ToList();

            ModulesItemsControl.ItemsSource = enabledModules;
        }

        /// <summary>  
        /// Maneja el clic en una tarjeta de módulo (solo para módulos sin perfiles)  
        /// </summary>  
        private void ModuleCard_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.Tag is string moduleId)
            {
                var module = _availableModules.FirstOrDefault(m => m.ModuleInfo.ModuleId == moduleId);
                if (module != null && !module.ModuleInfo.HasProfiles)
                {
                    LaunchModule(module, null);
                }
            }
        }

        /// <summary>  
        /// Maneja el clic en un botón de perfil  
        /// </summary>  
        private void ProfileButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                string profile = button.Content.ToString();
                string moduleId = button.Tag.ToString();

                var module = _availableModules.FirstOrDefault(m => m.ModuleInfo.ModuleId == moduleId);
                if (module != null)
                {
                    LaunchModule(module, profile);
                }
            }
        }

        /// <summary>  
        /// Maneja el clic en el botón de acceso directo (módulos sin perfiles)  
        /// </summary>  
        private void AccessButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string moduleId)
            {
                var module = _availableModules.FirstOrDefault(m => m.ModuleInfo.ModuleId == moduleId);
                if (module != null)
                {
                    LaunchModule(module, null);
                }
            }
        }

        /// <summary>  
        /// Lanza el módulo seleccionado con el perfil especificado  
        /// </summary>  
        private void LaunchModule(IModule module, string profile)
        {
            try
            {
                // Inicializar el módulo  
                module.Initialize();

                // Crear la ventana del módulo  
                Window moduleWindow;
                if (module.ModuleInfo.HasProfiles && !string.IsNullOrEmpty(profile))
                {
                    moduleWindow = module.CreateModuleWindow(profile);
                }
                else
                {
                    moduleWindow = module.CreateModuleWindow();
                }

                // Ocultar la ventana de selección  
                this.Hide();

                // Mostrar la ventana del módulo  
                moduleWindow.ShowDialog();

                // Al cerrar el módulo, volver a mostrar el selector  
                this.Show();

                // Limpiar recursos del módulo  
                module.Cleanup();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error al iniciar el módulo {module.ModuleInfo.DisplayName}:\n\n{ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
        private void BtnConfigBD_Click(object sender, RoutedEventArgs e)
        {
            var ventanaConfig = new ConfiguracionBaseDatosWindow();
            ventanaConfig.Owner = this;
            ventanaConfig.ShowDialog();
        }

        private void BtnConfigSignalR_Click(object sender, RoutedEventArgs e)
        {
            var ventanaConfig = new ConfiguracionSignalRWindow();
            ventanaConfig.Owner = this;
            ventanaConfig.ShowDialog();
        }
        /// <summary>  
        /// Maneja el clic en el botón Salir  
        /// </summary>  
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
}