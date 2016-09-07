using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace TPDIAProject.Converters
{
    class StringFormatConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string format = (string)parameter;
            if (string.IsNullOrWhiteSpace(format))
            {
                return value;
            }

            string result = value.ToString();
            try
            {
                result = string.Format(format, value);
            }
            catch (Exception)
            {
            }

            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
