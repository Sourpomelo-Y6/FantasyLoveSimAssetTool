using System;
using System.Collections;
using System.Globalization;
using System.Windows.Data;

namespace FantasyLoveSimAssetTool.Views
{
    public class SkillConditionScopeOptionsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string type = value as string;
            if (type == "TrainingProficiency") return new[] { "Training" };
            if (type == "MonsterDefeatCount") return new[] { "Total", "Enemy" };
            if (type == "Affection" || type == "Day") return new[] { "Total" };
            return new[] { "Total", "Training", "TrainingCategory" };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    public class SkillConditionTargetOptionsConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            string scope = values?.Length > 0 ? values[0] as string : null;
            if (scope == "Training") return values?.Length > 1 ? values[1] as IEnumerable : null;
            if (scope == "TrainingCategory") return values?.Length > 2 ? values[2] as IEnumerable : null;
            if (scope == "Enemy") return values?.Length > 3 ? values[3] as IEnumerable : null;
            return Array.Empty<string>();
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
