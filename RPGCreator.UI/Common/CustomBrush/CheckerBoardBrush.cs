using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;

namespace RPGCreator.UI.Common.CustomBrush;

public class CheckerBoardBrush
{
    public static IBrush CreateCheckerBoardBrush(Color c1, Color c2, double size = 20)
    {
        var patternCanvas = new Canvas
        {
            Width = size,
            Height = size
        };

        double half = size / 2;

        // Carré Haut-Gauche
        patternCanvas.Children.Add(new Rectangle
        {
            Width = half, Height = half,
            Fill = new SolidColorBrush(c1),
            [Canvas.LeftProperty] = 0,
            [Canvas.TopProperty] = 0
        });

        // Carré Bas-Droit
        patternCanvas.Children.Add(new Rectangle
        {
            Width = half, Height = half,
            Fill = new SolidColorBrush(c1),
            [Canvas.LeftProperty] = half,
            [Canvas.TopProperty] = half
        });
    
        // Le fond global sera la couleur 2 (c2)
        // Astuce : On peut mettre c2 en Background du canvas, ou dessiner 2 autres carrés.
        patternCanvas.Background = new SolidColorBrush(c2);

        // 2. On crée le Brush
        var brush = new VisualBrush
        {
            Visual = patternCanvas,
            TileMode = TileMode.Tile,
            Stretch = Stretch.None,
            SourceRect = new RelativeRect(0, 0, size, size, RelativeUnit.Absolute),
            DestinationRect = new RelativeRect(0, 0, size, size, RelativeUnit.Absolute)
        };

        return brush;
    }
}