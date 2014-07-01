using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows;

namespace AutoDiagrammer
{
    public class HasDataAndShouldShowMultiConverter : IMultiValueConverter
    {
        #region IMultiValueConverter Members

        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool hasValue = false;
            bool shouldShow = false;
            bool parsedOk1 = false;
            bool parsedOk2 = false;
            parsedOk1 =  Boolean.TryParse(values[0].ToString(), out hasValue);
            parsedOk2 = Boolean.TryParse(values[1].ToString(), out shouldShow);

            if (parsedOk1 && parsedOk2)
                return hasValue && shouldShow ? Visibility.Visible : Visibility.Collapsed;
            else
                return Visibility.Collapsed; ;

        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
