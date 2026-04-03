using System.Drawing;
using RPGCreator.Core.Types.Blueprint;

namespace RPGCreator.SDK.Graph.Ports;

public static class PortKindExtensions
{
    public static string ToDisplayString(this PortKind kind) => kind switch
    {
        PortKind.Exec => "Execution",
        PortKind.Value => "Value",
        _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null)
    };
    public static Color GetColor(this PortKind kind) => kind switch
    {
        PortKind.Exec => Color.FromArgb(255, 165, 0), // Orange for Exec
        PortKind.Value => Color.FromArgb(135, 206, 235), // SkyBlue for Value
        PortKind.Events => Color.FromArgb(255, 192, 203), // Pink for Events
        PortKind.Data => Color.FromArgb(144, 238, 144), // LightGreen for Data
        PortKind.String => Color.FromArgb(255, 255, 224), // LightYellow for String
        PortKind.Number => Color.FromArgb(255, 222, 173), // LightSalmon for Number
        PortKind.Boolean => Color.FromArgb(255, 160, 122), // LightCoral for Boolean
        PortKind.Object => Color.FromArgb(221, 160, 221), // Plum for Object
        PortKind.Enum => Color.FromArgb(100, 149, 237), // CornflowerBlue for Enum
        _ => Color.FromArgb(120, 120, 120) // Default gray for unknown kinds
    };
}