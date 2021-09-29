using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using System;

namespace Deeplex.Saverwalter.WinUI3.Utils
{
    public class IsEnabledConverter : IValueConverter
    {
        public SolidColorBrush enabled => Application.Current.Resources["enabled"] as SolidColorBrush;
        public SolidColorBrush disabled => Application.Current.Resources["disabled"] as SolidColorBrush;

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            bool? isEnabled = (bool)value;
            if (isEnabled.HasValue && isEnabled.Value == true)
            {
                return enabled;
            }
            return disabled;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return enabled;
        }
    }
}
