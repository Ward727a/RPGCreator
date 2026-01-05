using RPGCreator.Core.Types.Assets.Entities;
using RPGCreator.Core.Types.Internal;

namespace RPGCreator.Core.Types.Assets.Actors;

public interface IMoveableActor
{
    public event EventHandler<Point>? PositionChanged;
    public Point Position { get; set; }
    public void GoTo(Point position);
    public void GoTo(int x, int y);
}