using System;
using System.Globalization;
using System.Windows.Data;

namespace FantasyLoveSimAssetTool.Views
{
    public class EmptyStringToIntConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value?.ToString() ?? "0";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string text = value?.ToString();
            if (string.IsNullOrWhiteSpace(text))
            {
                return 0;
            }

            if (int.TryParse(text.Trim(), NumberStyles.Integer, culture, out int result))
            {
                return result;
            }

            return 0;
        }
    }
}
