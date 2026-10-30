// RPG Creator - Open-source RPG Engine.
// (c) 2026 Ward
// 
// This file is part of RPG Creator and is distributed under the Apache 2.0 License.
// You are free to use, modify, and distribute this file under the terms of the Apache 2.0 License.
// See LICENSE for details.
// 
// ---
// 
// Ce fichier fait partie de RPG Creator et est distribué sous licence Apache 2.0.
// Vous êtes libre de l'utiliser, de le modifier et de le distribuer sous les termes de la licence Apache 2.0.
// Voir LICENSE pour plus de détails.
// 
// Contact:
// => Mail: Ward727a@gmail.com
//    Please use this object: "RPG Creator [YourObject]"
// => Discord: ward727
// 
// For urgent inquiries, sending both an email and a message on Discord is highly recommended for a quicker response.

using Microsoft.Extensions.DependencyInjection;
using RPGCreator.SDK.Common.Logging;
using RPGCreator.SDK.Services.Map;
using RPGCreator.Shared.Attributes;
using RPGCreator.Shared.Types;

namespace RPGCreator.EngineLib.Registry;

[EngineService(typeof(ILayerRegistry))]
public class LayerRegistry(IServiceProvider provider) : ILayerRegistry
{
    private static readonly ScopedLogger Logger = SDK.Common.Logging.Logger.ForContext<LayerRegistry>();
    private readonly Dictionary<URN, (string displayName, Type type)> _layerTypes = new();
    
    public void RegisterLayerType(Type layerType, URN layerUrn, string displayName, bool overrideIfExist = false)
    {
        if (!typeof(IMapLayer).IsAssignableFrom(layerType))
        {
            Logger.Error("Layer type {type} with URN {urn} is not assignable from IMapLayer", args: [layerType, layerUrn]);
            return; 
        }
        
        if (_layerTypes.ContainsKey(layerUrn) && !overrideIfExist)
            return;
        
        _layerTypes[layerUrn] = (displayName, layerType);
    }

    public void UnregisterLayerType(URN layerUrn)
    {
        _layerTypes.Remove(layerUrn);
    }

    public Type? GetLayerType(URN layerUrn)
    {
        return _layerTypes.GetValueOrDefault(layerUrn).type;
    }

    public Result<IMapLayer> CreateLayerInstance(URN layerUrn)
    {
        var layerType = GetLayerType(layerUrn);
        if (layerType == null)
            return Result<IMapLayer>.Fail("Layer not found");

        if (ActivatorUtilities.CreateInstance(provider, layerType) is not IMapLayer layerInstance)
            return Result<IMapLayer>.Fail("Failed to create layer instance");
        
        return Result<IMapLayer>.Ok(layerInstance);
    }

    public bool HasLayerType(URN layerUrn)
    {
        return _layerTypes.ContainsKey(layerUrn);
    }

    public IReadOnlyDictionary<URN, (string displayName, Type type)> GetLayerTypes() => _layerTypes;
}