using System;
using System.Collections;
using System.Globalization;
using System.Windows.Data;

namespace FantasyLoveSimAssetTool.Views
{
    public class TrainingTargetOptionsConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            string scope = values?.Length > 0 ? values[0] as string : null;
            if (string.Equals(scope, "TrainingCategory", StringComparison.Ordinal))
                return values?.Length > 1 ? values[1] as IEnumerable : null;
            if (string.Equals(scope, "Training", StringComparison.Ordinal))
                return values?.Length > 2 ? values[2] as IEnumerable : null;
            return Array.Empty<string>();
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
