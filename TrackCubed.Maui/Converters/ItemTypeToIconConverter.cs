using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrackCubed.Maui.Converters
{
    public class ItemTypeToIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string itemTypeName)
            {
                // Return a different emoji based on the item type string
                return itemTypeName.ToLower() switch
                {
                    "link" => "🔗",
                    "image" => "🖼️",
                    "song" => "🎵",
                    "video" => "▶️",
                    "journal entry" => "✍️",
                    "document" => "📄",
                    _ => "🧊" // A default "cubed" icon for custom or 'Other' types
                };
            }
            return "◾"; // Default icon if something goes wrong
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
