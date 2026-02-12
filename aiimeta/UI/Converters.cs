using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace aiimeta.UI
{
    /// <summary>Set of several value converters.</summary>
    /// <remarks>
    /// This class defines several value converters as its inner class.
    /// It also provides stock objects of value converters for use in XAML.
    /// </remarks>
    public static class Converters
    {
        #region Boxing

        // Static objects to avoid frequent boxing.
        // Perhaps this is an overkill for aiimeta. :)

        private static readonly object True = true;

        private static readonly object False = false;

        private static readonly object Visible = Visibility.Visible;

        private static readonly object Collapsed = Visibility.Collapsed;

        /// <summary>Boxes a bool value, optionally converting to Visibility.</summary>
        /// <param name="target_type">Either typeof(<see cref="bool"/>) or typeof(<see cref="Visibility"/>).</param>
        /// <param name="value">bool value to convert.</param>
        /// <returns>Boxed object, or <see cref="UnsetValue"/> if <paramref name="target_type"/> is unsupported.</returns>
        private static object Box(Type target_type, bool value)
        {
            if (target_type == typeof(bool)) return value ? True : False;
            if (target_type == typeof(Visibility)) return value ? Visible : Collapsed;
            return DependencyProperty.UnsetValue;
        }

        #endregion

        public static readonly BooleanToVisibilityConverter BoolToVisibility = new BooleanToVisibilityConverter();

        public static readonly PositiveNumberConverter IsPositive = new PositiveNumberConverter();

        /// <summary>Number to bool or Visibility converter based on its positiveness.</summary>
        /// <remarks>
        /// The source may be either double, int, or long.
        /// The target may be either bool or <see cref="Visibility"/>.
        /// A positive value is converted to true, and a zero or negative number to false.
        /// This converter can't convert a bool or Visibility value back to a number, so
        /// it is for <see cref="BindingMode.OneWay"/> and <see cref="BindingMode.OneTime"/> bindings only.
        /// </remarks>
        [ValueConversion(typeof(double), typeof(bool))]
        [ValueConversion(typeof(int),    typeof(bool))]
        [ValueConversion(typeof(long),   typeof(bool))]
        [ValueConversion(typeof(double), typeof(Visibility))]
        [ValueConversion(typeof(int),    typeof(Visibility))]
        [ValueConversion(typeof(long),   typeof(Visibility))]
        public class PositiveNumberConverter : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                switch (value)
                {
                    case double d: return Box(targetType, d > 0);
                    case int i:    return Box(targetType, i > 0);
                    case long l:   return Box(targetType, l > 0);
                    default: return DependencyProperty.UnsetValue; 
                }
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                throw new NotImplementedException();
            }
        }

        public static readonly ZeroNumberConverter IsZero = new ZeroNumberConverter();

        /// <summary>Number to bool or Visibility converter based on its zeroness.</summary>
        /// <remarks>
        /// The source may be either double, int, or long.
        /// The target may be either bool or <see cref="Visibility"/>.
        /// A value of zero is converted to true <see cref="Visibility.Visible"/>, 
        /// and any other vlaues to false or <see cref="Visibility.Collapsed"/>.
        /// This converter can't convert a bool or <see cref="Visibility"/> value back to a number, so
        /// it is for <see cref="BindingMode.OneWay"/> and <see cref="BindingMode.OneTime"/> bindings only.
        /// </remarks>
        [ValueConversion(typeof(double), typeof(bool))]
        [ValueConversion(typeof(int),    typeof(bool))]
        [ValueConversion(typeof(long),   typeof(bool))]
        [ValueConversion(typeof(double), typeof(Visibility))]
        [ValueConversion(typeof(int),    typeof(Visibility))]
        [ValueConversion(typeof(long),   typeof(Visibility))]
        public class ZeroNumberConverter : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                switch (value)
                {
                    case double d: return Box(targetType, d == 0);
                    case int i:    return Box(targetType, i == 0);
                    case long l:   return Box(targetType, l == 0);
                    default: return DependencyProperty.UnsetValue; 
                }
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                throw new NotImplementedException();
            }
        }

        public static readonly BooleanOrConverter Or = new BooleanOrConverter();

        /// <summary>Calculates logical OR of bool values and optionally converts the result to Visibility.</summary>
        [ValueConversion(typeof(bool), typeof(Visibility))]
        [ValueConversion (typeof(bool), typeof(bool))]
        public class BooleanOrConverter : IMultiValueConverter
        {
            public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
            {
                return Box(targetType, CalculateOr(values));
            }

            private static bool CalculateOr(object[] values)
            {
                foreach (var obj in values)
                {
                    if (obj is bool b && b) return true;
                }
                return false;
            }

            public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            {
                throw new NotImplementedException();
            }
        }
    }
}
