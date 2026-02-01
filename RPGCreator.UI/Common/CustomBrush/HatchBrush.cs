using System.Globalization;
using Avalonia;
using Avalonia.Media;

namespace RPGCreator.UI.Common.CustomBrush;

public class HatchBrush
{
    public static IBrush CreateHatchBrush(Color color, double spacing = 10, double thickness = 1, double angle = 45)
    {
        
        double midY = spacing / 2.0;

        // M 0,5 L 10,5 (Move to 0,Mid -> Line to Spacing,Mid)
        string pathData = string.Format(CultureInfo.InvariantCulture, 
            "M 0,{0} L {1},{0}", midY, spacing);

        var geometry = StreamGeometry.Parse(pathData);
        
        var drawing = new GeometryDrawing
        {
            Geometry = geometry,
            Pen = new Pen(new SolidColorBrush(color), thickness)
        };
    
        var brush = new DrawingBrush
        {
            Drawing = drawing,
            TileMode = TileMode.Tile,
    
            DestinationRect = new RelativeRect(0, 0, spacing, spacing, RelativeUnit.Absolute),
    
            Stretch = Stretch.None,
            Transform = new RotateTransform(angle)
        };

        return brush;
    }
}