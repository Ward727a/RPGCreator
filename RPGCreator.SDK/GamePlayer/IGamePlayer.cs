namespace RPGCreator.SDK.GamePlayer;

public interface IGamePlayer
{
    event Action? OnInitialize;
    event Action? OnLoad;
    event Action<TimeSpan>? OnUpdate;
    event Action<TimeSpan>? OnDraw;
}