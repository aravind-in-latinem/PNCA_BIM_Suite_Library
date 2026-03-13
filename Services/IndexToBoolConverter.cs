using System.Globalization;
using System.Windows.Data;
using System;
namespace PNCA_BIM_Suite_Library.Services
{
    public class IndexToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => (int)value == System.Convert.ToInt32(parameter);

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => (bool)value ? System.Convert.ToInt32(parameter) : Binding.DoNothing;
    }
}
