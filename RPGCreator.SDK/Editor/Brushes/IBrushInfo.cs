using RPGCreator.SDK.Types;
using RPGCreator.Shared.Types;

namespace RPGCreator.SDK.Editor.Brushes;

public interface IBrushInfo
{
    URN UniqueName { get; }
    string Name { get; }
    string Description { get; }
}