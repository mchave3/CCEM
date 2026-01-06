using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace CCEM.Common.Converters;

/// <summary>
/// Converts a boolean value to a Visibility value.
/// </summary>
public partial class BoolToVisibilityConverter : IValueConverter
{
    /// <summary>
    /// Gets or sets whether the conversion should be inverted.
    /// </summary>
    public bool IsInverted { get; set; }

    public object Convert(object value, Type targetType, object parameter, string language)
    {
        bool boolValue = value is bool b && b;
        
        if (IsInverted)
        {
            boolValue = !boolValue;
        }
        
        return boolValue ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        bool result = value is Visibility visibility && visibility == Visibility.Visible;
        
        return IsInverted ? !result : result;
    }
}
