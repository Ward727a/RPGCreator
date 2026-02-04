namespace RPGCreator.SDK.RuntimeService;

public interface IGameRunner : IService
{
    event Action? OnInitialize;
    event Action? OnLoad;
    event Action<TimeSpan>? OnUpdate;
    event Action<TimeSpan>? OnDraw;
}