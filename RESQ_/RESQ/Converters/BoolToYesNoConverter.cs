using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RESQ.Converters
{
    public class BoolToYesNoConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
                return boolValue ? "Yes" : "No";

            return "No";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string strValue)
                return strValue.Equals("Yes", StringComparison.OrdinalIgnoreCase);

            return false;
        }
    }
}
