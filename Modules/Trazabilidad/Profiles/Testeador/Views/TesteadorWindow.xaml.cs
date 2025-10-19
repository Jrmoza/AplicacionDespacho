using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Collections.Generic;
using AplicacionDespacho.Modules.Trazabilidad.Profiles.Testeador.ViewModels;
using System.Windows.Media;


namespace AplicacionDespacho.Modules.Trazabilidad.Profiles.Testeador.Views
{
    public partial class TesteadorWindow : Window
    {
        public TesteadorWindow()
        {
            InitializeComponent();
            this.DataContext = new TesteadorViewModel();
        }

        private void BtnCerrar_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void BtnLimpiar_Click(object sender, RoutedEventArgs e)
        {
            // Limpiar el campo de búsqueda  
            if (this.DataContext is TesteadorViewModel viewModel)
            {
                viewModel.NumeroPallet = string.Empty;
                viewModel.Lotes.Clear();
                viewModel.PalletInfo = string.Empty;
                viewModel.EstadoValidacion = string.Empty;
            }
        }
    }

    // Convertidor para mostrar elemento cuando Count > 0  
    public class CountToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int count)
            {
                return count > 0 ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    // Convertidor inverso para mostrar elemento cuando Count == 0  
    public class InverseCountToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int count)
            {
                return count == 0 ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class BoolToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool tieneDiscrepancia && tieneDiscrepancia)
                return Brushes.Red;
            return Brushes.Green;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class BoolToFontWeightConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool tieneDiscrepancia && tieneDiscrepancia)
                return FontWeights.Bold;
            return FontWeights.Normal;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    // Convertidor para mostrar elemento cuando el valor es null  
    public class NullToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value == null ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}