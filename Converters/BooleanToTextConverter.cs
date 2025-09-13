// Converters/BooleanToTextConverter.cs  
using System;
using System.Globalization;
using System.Windows.Data;

namespace AplicacionDespacho.Converters
{
    public class BooleanToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return boolValue ? "Sí" : "No";
            }
            return "No";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string stringValue)
            {
                return stringValue.Equals("Sí", StringComparison.OrdinalIgnoreCase);
            }
            return false;
        }
    }
}