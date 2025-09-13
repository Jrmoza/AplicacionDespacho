// Views/AdminWindow.xaml.cs
using System;
using System.Collections.Generic;
using System.Windows;
using AplicacionDespacho.Models;
using AplicacionDespacho.Services.DataAccess;

namespace AplicacionDespacho
{
    public partial class AdminWindow : Window
    {
        private AccesoDatosViajes _accesoDatosViajes;

        public AdminWindow()
        {
            InitializeComponent();
            _accesoDatosViajes = new AccesoDatosViajes();
            CargarEmpresas();
        }

        private void CargarEmpresas()
        {
            var empresas = _accesoDatosViajes.ObtenerEmpresas();
            // Carga los datos en ambos ComboBoxes
            comboBoxEmpresasConductor.ItemsSource = empresas;
            comboBoxEmpresasConductor.DisplayMemberPath = "NombreEmpresa";
            comboBoxEmpresasConductor.SelectedValuePath = "EmpresaId";

            comboBoxEmpresasVehiculo.ItemsSource = empresas;
            comboBoxEmpresasVehiculo.DisplayMemberPath = "NombreEmpresa";
            comboBoxEmpresasVehiculo.SelectedValuePath = "EmpresaId";
        }

        private void btnGuardarEmpresa_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(textBoxEmpresa.Text) && !string.IsNullOrWhiteSpace(textBoxRUC.Text))
            {
                var nuevaEmpresa = new EmpresaTransporte
                {
                    NombreEmpresa = textBoxEmpresa.Text.ToUpper(),
                    RUC = textBoxRUC.Text
                };
                _accesoDatosViajes.GuardarEmpresa(nuevaEmpresa);
                MessageBox.Show("Empresa guardada con éxito.");
                textBoxEmpresa.Clear();
                textBoxRUC.Clear();
                CargarEmpresas(); // Vuelve a cargar los ComboBoxes para incluir la nueva empresa
            }
            else
            {
                MessageBox.Show("El nombre y el RUC de la empresa no pueden estar vacíos.");
            }
        }

        private void btnGuardarConductor_Click(object sender, RoutedEventArgs e)
        {
            // Usa el nombre correcto del ComboBox para conductores
            if (comboBoxEmpresasConductor.SelectedValue != null && !string.IsNullOrWhiteSpace(textBoxConductor.Text))
            {
                var nuevoConductor = new Conductor
                {
                    NombreConductor = textBoxConductor.Text.ToUpper(),
                    EmpresaId = (int)comboBoxEmpresasConductor.SelectedValue
                };
                _accesoDatosViajes.GuardarConductor(nuevoConductor);
                MessageBox.Show("Conductor guardado con éxito.");
                textBoxConductor.Clear();
            }
            else
            {
                MessageBox.Show("Selecciona una empresa y escribe el nombre del conductor.");
            }
        }

        private void btnGuardarVehiculo_Click(object sender, RoutedEventArgs e)
        {
            // Usa el nombre correcto del ComboBox para vehículos
            if (comboBoxEmpresasVehiculo.SelectedValue != null && !string.IsNullOrWhiteSpace(textBoxPlaca.Text))
            {
                var nuevoVehiculo = new Vehiculo
                {
                    Placa = textBoxPlaca.Text.ToUpper(),
                    EmpresaId = (int)comboBoxEmpresasVehiculo.SelectedValue
                };
                _accesoDatosViajes.GuardarVehiculo(nuevoVehiculo);
                MessageBox.Show("Vehículo guardado con éxito.");
                textBoxPlaca.Clear();
            }
            else
            {
                MessageBox.Show("Selecciona una empresa y escribe la placa del vehículo.");
            }
        }
    }
}