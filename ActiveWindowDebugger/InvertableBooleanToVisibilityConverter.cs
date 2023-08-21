using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ActiveWindowDebugger;

[ValueConversion(typeof(bool), typeof(Visibility))]
public class InvertableBooleanToVisibilityConverter : IValueConverter
{
    private enum Parameters
    {
        Normal, Inverted
    }

    public object Convert(object value, Type targetType,
                          object parameter, CultureInfo culture)
    {
        bool boolValue = (bool)value;
        var direction = (Parameters)Enum.Parse(typeof(Parameters), (string)parameter);

        return direction == Parameters.Inverted
            ? !boolValue ? Visibility.Visible : Visibility.Collapsed
            : (object)(boolValue ? Visibility.Visible : Visibility.Collapsed);
    }

    public object ConvertBack(object value, Type targetType,
        object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
