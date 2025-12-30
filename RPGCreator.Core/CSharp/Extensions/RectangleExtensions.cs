using Microsoft.Xna.Framework;
using RPGCreator.SDK.Types;

namespace RPGCreator.Core.CSharp.Extensions;

public static class RectangleExtensions
{
    public static Rectangle ToRectangle(this Rect rect)
    {
        return new Rectangle(
            (int)rect.X,
            (int)rect.Y,
            (int)rect.Width,
            (int)rect.Height
        );
    }
}