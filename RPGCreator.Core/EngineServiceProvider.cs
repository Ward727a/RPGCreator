using RPGCreator.Core.Runtimes.Services;
using Serilog;
using IServiceProvider = RPGCreator.Core.Runtimes.Services.Core.IServiceProvider;

namespace RPGCreator.Core;

public class EngineServiceProvider : IServiceProvider
{
    private readonly Dictionary<System.Type, IRuntimeService> _services;

    public TService GetService<TService>() where TService : IRuntimeService
    {
        var serviceType = typeof(TService);
        
        if (_services.TryGetValue(serviceType, out var service))
        {
            return (TService)service;
        }
        
        Log.Fatal("Service of type {ServiceType} not found.", serviceType.Name);
        // This is a fatal error, we cannot continue without the service.
        throw new InvalidOperationException($"Service of type {serviceType.Name} not found.");
    }

    public IRuntimeService GetService(System.Type serviceType)
    {
        _services.TryGetValue(serviceType, out var service);
        
        if (service is not null) return service;
        
        Log.Fatal("Service of type {ServiceType} not found.", serviceType.Name);
        // This is a fatal error, we cannot continue without the service.
        throw new InvalidOperationException($"Service of type {serviceType.Name} not found.");
    }

    public bool RegisterService<TService>(TService service) where TService : IRuntimeService
    {
        if (service is null)
        {
            Log.Error("Cannot register a null service.");
            return false;
        }

        var serviceType = typeof(TService);
        
        if (_services.ContainsKey(serviceType))
        {
            Log.Warning("Service of type {ServiceType} is already registered.", serviceType.Name);
            return false;
        }

        _services[serviceType] = service;
        Log.Information("Service of type {ServiceType} registered successfully.", serviceType.Name);
        return true;
    }

    public bool HasService<TService>() where TService : IRuntimeService
    {
        var serviceType = typeof(TService);
        var hasService = _services.ContainsKey(serviceType);
        
        #if DEBUG
        if (hasService)
        {
            Log.Information("Service of type {ServiceType} is available.", serviceType.Name);
        }
        else
        {
            Log.Warning("Service of type {ServiceType} is not available.", serviceType.Name);
        }
        #endif
        
        return hasService;
    }

    public bool UnregisterService<TService>() where TService : IRuntimeService
    {
        var serviceType = typeof(TService);
        
        if (!_services.Remove(serviceType))
        {
            Log.Warning("Service of type {ServiceType} is not registered.", serviceType.Name);
            return false;
        }

        Log.Information("Service of type {ServiceType} unregistered successfully.", serviceType.Name);
        return true;
    }
}