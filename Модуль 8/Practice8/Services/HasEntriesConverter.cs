using System;
using System.Collections;
using System.Globalization;
using System.Windows.Data;

namespace Practice8
{
    public class HasEntriesConverter : IMultiValueConverter
    {
        public object Convert(
            object[] values,
            Type targetType,
            object parameter,
            CultureInfo culture)
        {
            if (values == null || values.Length < 2)
            {
                return false;
            }

            if (!(values[0] is DateTime))
            {
                return false;
            }

            DateTime date = ((DateTime)values[0]).Date;

            IEnumerable dates = values[1] as IEnumerable;

            if (dates == null)
            {
                return false;
            }

            foreach (object value in dates)
            {
                if (value is DateTime)
                {
                    DateTime markedDate = ((DateTime)value).Date;

                    if (markedDate == date)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public object[] ConvertBack(
            object value,
            Type[] targetTypes,
            object parameter,
            CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}