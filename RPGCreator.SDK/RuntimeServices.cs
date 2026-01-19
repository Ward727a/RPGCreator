using System.Diagnostics.CodeAnalysis;
using RPGCreator.SDK.RuntimeService;

namespace RPGCreator.SDK;


public class RuntimeServicesProvider : IServiceProvider
{
    private readonly Dictionary<Type, IService> _services = new();
    
    public T GetService<T>() where T : class, IService
    {
        return (T)_services[typeof(T)];
    }

    public bool TryGetService<T>([NotNullWhen(true)] out T? service) where T : class, IService
    {
        if (_services.TryGetValue(typeof(T), out var svc))
        {
            service = (T)svc;
            return true;
        }
        
        service = null;
        return false;
    }

    public void RegisterService<T>(T service) where T : class, IService
    {
        _services[typeof(T)] = service;
    }
}

/// <summary>
/// Every services related to the runtime environment.<br/>
/// ex: game loop, time management, map editing, etc...<br/>
/// <br/>
/// For a better understanding of runtime services, you need to think like that:<br/>
/// - Where is the service being initialized?<br/>
/// >>> In the Core? Then it's a EngineServices.<br/>
/// >>> In the RTP? Then it's a RuntimeServices.<br/>
/// - What 'parts' the service is trying to 'connect'?
/// >>> Core to UI? UI to Core? Then it's a EngineServices.<br/>
/// >>> RTP to UI? UI to RTP? Then it's a RuntimeServices.<br/>
/// >>> RTP to Core? Core to RTP? Then it depends on what the service need to do.<br/>
/// - Is the service need to <b>mainly</b> interact with or use MonoGame/XNA or any other game framework?<br/>
/// >>> Yes? Then it's probably a RuntimeServices.<br/>
/// >>> No? Then it's a EngineServices.<br/>
/// - In the condition where the service need to interact with both Core and RTP, then it's a EngineServices.<br/>
/// <br/>
/// The core need to be as decoupled as possible from any other parts of the engine. Meaning that even if the RTP or UI are not present, the core should still be able to function properly.<br/>
/// But not the other way around, the RTP and UI can depend on the core to function properly as the core will always be present, and if not, then the engine is not supposed to work at all.<br/>
/// </summary>
public static class RuntimeServices
{
    private static readonly RuntimeServicesProvider ServiceProvider = new();
    
    // ReSharper disable MemberCanBePrivate.Global
    public static void RegisterService<T>(T service) where T : class, IService
    {
        ServiceProvider.RegisterService(service);
    }
    
    public static T GetService<T>() where T : class, IService
    {
        if (ServiceProvider.TryGetService<T>(out var service))
        {
            return service;
        }

        throw new InvalidOperationException($"[Runtime] Critical Service Missing: {typeof(T).Name}. Make sure it's registered during runtime initialization.");
    }
    
    public static IMapService MapService
    {
        get => GetService<IMapService>();
        set => RegisterService(value);
    }
    public static ILayerService LayerService
    {
        get => GetService<ILayerService>();
        set => RegisterService(value);
    }
    public static ICameraService CameraService
    {
        get => GetService<ICameraService>();
        set => RegisterService(value);
    }
    public static IRenderService RenderService
    {
        get => GetService<IRenderService>();
        set => RegisterService(value);
    }
}