using RPGCreator.SDK.Types;

namespace RPGCreator.SDK.Editor.Brushes;

public interface IBrushInfo
{
    URN UniqueName { get; }
    string Name { get; }
    string Description { get; }
}