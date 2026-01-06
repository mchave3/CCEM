using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace CCEM.Common.Converters;

/// <summary>
/// Converts a null value to Collapsed and non-null to Visible.
/// </summary>
public sealed partial class NullToVisibilityConverter : IValueConverter
{
    /// <summary>
    /// Gets or sets whether the conversion should be inverted.
    /// </summary>
    public bool IsInverted { get; set; }

    public object Convert(object? value, Type targetType, object parameter, string language)
    {
        bool isNull = value is null;

        if (IsInverted)
        {
            isNull = !isNull;
        }

        return isNull ? Visibility.Collapsed : Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
