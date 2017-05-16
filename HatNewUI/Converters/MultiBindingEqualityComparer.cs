using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using HatNewUI.Helpers;

namespace HatNewUI.Converters
{
    public class MultiBindingEqualityComparer : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (values.IsNullOrEmpty()) return false;
            var first = values[0];
            var retval = values.Skip(1).All(x => x != null && x.Equals(first));
            return retval;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class MultiBindingReferenceEqualityComparer : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (values.IsNullOrEmpty()) return false;
            var first = values[0];
            var retval = values.Skip(1).All(x => x != null && x == first);
            return retval;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
