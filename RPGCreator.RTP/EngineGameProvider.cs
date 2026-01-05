using CommunityToolkit.Diagnostics;
using Microsoft.Xna.Framework;
using RPGCreator.SDK;

namespace RPGCreator.RTP;

public class EngineGameProvider : IGameProvider
{
    private Game _game;

    public object GameInstance
    {
        get
        {
            Guard.IsAssignableToType<Game>(_game);
            return _game;
        }
    }

    public EngineGameProvider(Game game)
    {
        Guard.IsNotNull(game);
        _game = game;
    }
}