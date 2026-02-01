using CommunityToolkit.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Xna.Framework;
using RPGCreator.SDK;

namespace RPGCreator.RTP;

public partial class EngineGameProvider : ObservableObject, IGameProvider
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
        OnPropertyChanging(nameof(GameInstance));
        _game = game;
        OnPropertyChanged(nameof(GameInstance));
    }
}