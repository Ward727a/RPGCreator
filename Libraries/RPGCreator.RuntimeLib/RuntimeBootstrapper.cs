using Microsoft.Extensions.DependencyInjection;
using RPGCreator.RuntimeLib.Services;
using RPGCreator.SDK.Services.EngineService;
using RPGCreator.SDK.Services.RuntimeService;

namespace RPGCreator.RuntimeLib;

public static class RuntimeBootstrapper
{
    public static void AddServices(in IServiceCollection services)
    {
        services.AddSingleton<IEcsService, EcsService>();
        services.AddSingleton<IGameSimulationService, GameSimulationService>();
    }
}