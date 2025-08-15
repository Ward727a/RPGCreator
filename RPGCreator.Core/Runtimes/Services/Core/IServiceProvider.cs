namespace RPGCreator.Core.Runtimes.Services.Core;

public interface IServiceProvider
{
    public TService GetService<TService>() where TService : IRuntimeService;
    public IRuntimeService GetService(System.Type serviceType);
    public bool RegisterService<TService>(TService service) where TService : IRuntimeService;
    public bool HasService<TService>() where TService : IRuntimeService;
    public bool UnregisterService<TService>() where TService : IRuntimeService;
}