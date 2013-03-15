using System;
using System.Collections.ObjectModel;
using System.Text;
using Windows.UI.Xaml.Data;

namespace ContosoCookbook.Common
{
    public class ListConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var items = (ObservableCollection<string>) value;
            var bld = new StringBuilder();

            foreach (var item in items)
            {
                bld.Append(item);
                bld.Append("\r\n");
            }

            return bld.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}