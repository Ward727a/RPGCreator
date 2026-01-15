using RPGCreator.SDK.RuntimeService;

namespace RPGCreator.SDK;

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
    public static IMapService? MapService = null!;
    public static ILayerService? LayerService = null!;
}