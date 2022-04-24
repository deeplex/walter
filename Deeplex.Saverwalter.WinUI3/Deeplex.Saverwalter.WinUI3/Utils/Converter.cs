using Deeplex.Saverwalter.Model;
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

    public class EuroStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
            => ((double)value).ToString() + "€";

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class IntToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
            => (int)value > 0;

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class DateStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is DateTime d)
            {
                return d.ToString("dd.MM.yyyy");
            }
            else if (value is DateTimeOffset t)
            {
                return t.ToString("dd.MM.yyyy");
            }
            else
            {
                return "Offen";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class ToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
            => value.ToString();

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class GetBriefAnredeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
            => App.WalterService.ctx.FindPerson((Guid)value).GetBriefAnrede();

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class ErstelltAmConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
            => "Erstellt am: " + ((DateTime)value).ToString("dd.MM.yyyy HH:mm:ss");

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class PathConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
            => "Dateipfad: " + value;

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class FileSizeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
            => "Dateigröße: " + Math.Round((double)value / 1000).ToString() + "kb";

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class ToDescriptionStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is Betriebskostentyp t)
            {
                return t.ToDescriptionString();
            }
            throw new Exception();
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
