using System;
using System.Windows.Data;

namespace WordReview.Converters
{
    public class TextSearchMultiConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (values[0] != null)
                return values[0] as string;
              else if (values[1] != null)
                return values[1] as string;
            return "";
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
