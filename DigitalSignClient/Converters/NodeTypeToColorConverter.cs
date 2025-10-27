using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace DigitalSignClient.Converters
{
    // Converter cho màu node theo NodeType
    public class NodeTypeToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string nodeType)
            {
                return nodeType.ToLower() switch
                {
                    "start" => new SolidColorBrush(Color.FromRgb(40, 167, 69)), // Green
                    "end" => new SolidColorBrush(Color.FromRgb(220, 53, 69)),   // Red
                    "step" => new SolidColorBrush(Color.FromRgb(0, 122, 204)),  // Blue
                    _ => new SolidColorBrush(Color.FromRgb(108, 117, 125))      // Gray
                };
            }
            return new SolidColorBrush(Color.FromRgb(108, 117, 125));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    // Converter cho border khi node được chọn
    public class SelectedToBorderConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isSelected && isSelected)
            {
                return new SolidColorBrush(Color.FromRgb(255, 193, 7)); // Yellow
            }
            return new SolidColorBrush(Colors.Transparent);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    // Converter để chuyển text thành chữ hoa
    public class UpperCaseConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string text)
            {
                return text.ToUpper();
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}