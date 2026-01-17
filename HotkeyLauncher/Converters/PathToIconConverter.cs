using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using HotkeyLauncher.Services;

namespace HotkeyLauncher.Converters;

public class PathToIconConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string path && !string.IsNullOrWhiteSpace(path))
        {
            return IconExtractor.GetIconFromFile(path);
        }
        return null;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
