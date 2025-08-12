using RPGCreator.Core.Type.Internal;

namespace RPGCreator.Core.Type.Assets.Actors;

public interface IMoveableActor : IActor
{
    public event EventHandler<Point>? PositionChanged;
    
    public Point Position { get; set; }
    
    public void GoTo(Point position);
    public void GoTo(int x, int y);
}