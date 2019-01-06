using ID3TagEditLib;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace WpfId3TagEdit
{
    class FontWeightConverter : IValueConverter
    {
        private static readonly FontWeight syncValueWeight = FontWeights.Normal, notSyncValuesWeight = FontWeights.Bold;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? syncValueWeight : notSyncValuesWeight;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
