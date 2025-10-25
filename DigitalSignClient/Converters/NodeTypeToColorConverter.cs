using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace DigitalSignClient.Converters
{
    public class NodeTypeToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string nodeType)
            {
                return nodeType switch
                {
                    "start" => new SolidColorBrush(Color.FromRgb(76, 175, 80)),    // #4CAF50
                    "sign" => new SolidColorBrush(Color.FromRgb(33, 150, 243)),    // #2196F3
                    "approval" => new SolidColorBrush(Color.FromRgb(255, 152, 0)), // #FF9800
                    "parallel" => new SolidColorBrush(Color.FromRgb(156, 39, 176)),// #9C27B0
                    "end" => new SolidColorBrush(Color.FromRgb(244, 67, 54)),      // #F44336
                    _ => new SolidColorBrush(Color.FromRgb(158, 158, 158))         // Gray
                };
            }
            return new SolidColorBrush(Colors.Gray);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}