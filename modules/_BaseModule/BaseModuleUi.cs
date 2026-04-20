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

using System.Runtime.CompilerServices;
using _BaseModule.AssetDefinitions.SpawnPoint;
using _BaseModule.UI.StatsFeature;
using _BaseModule.UI.StatsModifier;
using RPGCreator.SDK;
using RPGCreator.SDK.Assets.Definitions.Maps.Layers;
using RPGCreator.SDK.EditorUiService;
using RPGCreator.UI.Extensions;

namespace _BaseModule;

internal static class BaseModuleUi
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void Register(IUiExtensionManager extensionManager)
    {
        extensionManager.AssetsManager().Menu((o, context) =>
        {
            context.RegisterAssetsMenuOption("Stats", () =>
            {
                return new StatsManagement(context);
            });
            context.RegisterAssetsMenuOption("Stat Modifiers", () =>
            {
                return new StatsModifierManagement(context);
            });
        });
        extensionManager.EditorLeftPanel().LayerPanel_LayerCreator((o, context) =>
        {
            context.AddType("spawn_point_layer", "Spawn Points Layer", false);
            context.LayerCreated += args =>
            {
                var layerName = args.Name;
                var layerType = args.Key;
                
                BaseLayerDef newLayer;
                
                switch (layerType)
                {
                    case "spawn_point_layer": // Spawn Point Layer
                        newLayer = EngineServices.AssetsManager.CreateAsset<SpawnPointLayer>();
                        break;
                    default: // Not supported by default
                        return;
                }
                
                newLayer.Name = layerName;
                newLayer.ZIndex = RuntimeServices.MapService.CurrentLoadedMapDefinition!.TileLayers.Count; // Set ZIndex to the last index
                newLayer.LayerIndex = RuntimeServices.MapService.GetLastLayerIndex() + 1; // Set LayerIndex to the next available index
                if(!RuntimeServices.MapService.HasLoadedMap)
                {
                    return;
                }

                if (RuntimeServices.MapService.TryAddLayer(newLayer))
                {
                    newLayer.LayerIndex = RuntimeServices.MapService.GetLastLayerIndex();
                    RuntimeServices.MapService.SelectLayer(newLayer.LayerIndex);
                    return;
                }
                
                EditorUiServices.NotificationService.Error("Error Adding Layer", "Could not add the new layer. It may already exist?");
            };
        });
    }
}