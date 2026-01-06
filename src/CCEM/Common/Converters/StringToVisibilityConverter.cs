using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace CCEM.Common.Converters;

/// <summary>
/// Converts a string value to Visibility. Empty/null strings become Collapsed.
/// </summary>
public sealed partial class StringToVisibilityConverter : IValueConverter
{
    /// <summary>
    /// Gets or sets whether the conversion should be inverted.
    /// </summary>
    public bool IsInverted { get; set; }

    public object Convert(object? value, Type targetType, object parameter, string language)
    {
        bool hasValue = !string.IsNullOrWhiteSpace(value as string);

        if (IsInverted)
        {
            hasValue = !hasValue;
        }

        return hasValue ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
