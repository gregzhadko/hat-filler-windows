using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace HatNewUI.Converters
{
    public class GenericObjectToVisibilityConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value != null ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new Exception("This converter is only meant to be used in OneWay conversions");
        }
    }

    public class GenericInvertedObjectToVisibilityConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value != null ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new Exception("This converter is only meant to be used in OneWay conversions");
        }
    }

    public class InvertedVisibilityToVisibilityConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is Visibility)) return value;
            var realValue = (Visibility)value;
            return realValue == Visibility.Collapsed || realValue == Visibility.Hidden ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new Exception("This converter is only meant to be used in OneWay conversions");
        }
    }

    public class IsNullReferenceConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value == null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new Exception("This converter is only meant to be used in OneWay conversions");
        }
    }

    public class InvertedIsNullReferenceConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value != null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new Exception("This converter is only meant to be used in OneWay conversions");
        }
    }


    public class GenericBooleanToVisibilityConverter : BooleanConverter<Visibility>
    {
        public GenericBooleanToVisibilityConverter()
            : base(Visibility.Visible, Visibility.Collapsed)
        {
        }
    }

    public class BooleanImplementConverter : BooleanConverter<Boolean>
    {
        public BooleanImplementConverter()
            : base(true, false)
        {
        }
    }

    public class BooleanToBrushConverter : BooleanConverter<Brush>
    {
        public BooleanToBrushConverter()
            : base(Brushes.Transparent, Brushes.Transparent)
        {
        }
    }

    public class BooleanConverter<T> : IValueConverter
    {
        public BooleanConverter(T trueValue, T falseValue)
        {
            True = trueValue;
            False = falseValue;
        }

        public T True { get; set; }
        public T False { get; set; }

        public virtual object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is bool && ((bool)value) ? True : False;
        }

        public virtual object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is T && EqualityComparer<T>.Default.Equals((T)value, True);
        }
    }

    public class MultiBooleanToVisibilityConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var castedValues = values.OfType<bool>().ToArray();

            if (castedValues.Length == 0)
            {
                return null;
            }

            return castedValues.Any(val => val) ? Visibility.Visible : Visibility.Collapsed;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class StringToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var s = value as string;

            return String.IsNullOrWhiteSpace(s) ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new Exception("This converter is only meant to be used in OneWay conversions");
        }
    }
}
