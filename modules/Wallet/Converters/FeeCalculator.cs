using System;
using System.Globalization;
using Xamarin.Forms;
namespace Wallet.Converters
{
    public class FeeCalculator:IValueConverter
    { 

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var maxfee = 200;
            var numericValue = System.Convert.ToDecimal(value);
            if (numericValue > maxfee)
                return string.Format("\u2248 {0:#,##0.00} USD", maxfee);
            

            return string.Format("\u2248 {0:#,##0.00} USD", numericValue);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
