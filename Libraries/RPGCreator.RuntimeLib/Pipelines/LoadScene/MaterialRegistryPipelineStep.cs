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

using Arch.Core;
using CommunityToolkit.Diagnostics;
using Microsoft.Xna.Framework.Graphics;
using RPGCreator.Editor.ECS.Components;
using RPGCreator.Editor.ECS.Flags;
using RPGCreator.RuntimeLib.FNA;
using RPGCreator.SDK.Assets.AutoTiles;
using RPGCreator.SDK.Assets.Tileset;
using RPGCreator.SDK.Common.Logging;
using RPGCreator.SDK.PipelineManager;
using RPGCreator.SDK.Resources;
using RPGCreator.SDK.Scenes;
using RPGCreator.SDK.Services.Assets;
using RPGCreator.SDK.Services.Map;

namespace RPGCreator.RuntimeLib.Pipelines.LoadScene;

[PipelineStep(typeof(LoadSceneStepContext))]
public class MaterialRegistryPipelineStep(
    IMaterialRegistry materialRegistry,
    IAssetManager assetManager,
    IResourceManager resourceManager,
    GraphicsDevice graphicsDevice) : IPipelineStep<LoadSceneStepContext>
{
    private readonly ScopedLogger _logger = Logger.ForContext<MaterialRegistryPipelineStep>();

    private readonly QueryDescription _queryDescriptionForTilesetRegistry =
        new QueryDescription().WithAll<MapTilesetRegistryComponent, WorldEntityFlag>();

    public IEnumerable<Type> RunAfter { get; } = [typeof(MapTilesetRegistryPipelineStep)];
    public IEnumerable<Type> RunBefore { get; } = [];

    public ValueTask RunAsync(LoadSceneStepContext context)
    {
        var simulation = context.Simulation;
        var world = simulation.CurrentWorld;

        if (world == null)
        {
            _logger.Critical("World is null");
            return default;
        }

        MapTilesetRegistryComponent tilesetRegistry = default;

        world.Query(in _queryDescriptionForTilesetRegistry,
            (ref MapTilesetRegistryComponent tilesetRegistryComponent) =>
            {
                tilesetRegistry = tilesetRegistryComponent;
            });

        var materials = materialRegistry.GetMaterials();

        var registry = new GameMaterialRegistryComponent()
        {
            Rulesets = new Dictionary<MaterialId, AutoTileRulesetDefinition>(),
            Textures = new Dictionary<Ulid, Texture2D>(),
            Definitions = new Dictionary<Ulid, TilesetDefinition>()
        };

        Guard.IsNotNull(tilesetRegistry.TilesetIds);
        Guard.IsNotNull(tilesetRegistry.Definitions);
        Guard.IsNotNull(tilesetRegistry.Textures);
        
        foreach (var (_, materialId) in materials)
        {
            var rulesetId = materialRegistry.GetRuleset(materialId);

            var rulesetResult = assetManager.GetAsset<AutoTileRulesetDefinition>(rulesetId);

            if (rulesetResult.IsFailure)
            {
                _logger.Error("Error while trying to get ruleset for material {0}", args: materialId);
                _logger.Error("Error: {0}", args: rulesetResult.Error);
                continue;
            }

            var ruleset = rulesetResult.Value;
            registry.Rulesets.Add(materialId, ruleset);
            ResolveTileset(ruleset, materialId);
        }

        simulation.AttachWorldComponent(registry);

        return ValueTask.CompletedTask;

        void ResolveTileset(in AutoTileRulesetDefinition ruleset, MaterialId materialId)
        {
            var targetTileset = ruleset.TargetTileset;
            
            if(registry.Definitions.ContainsKey(targetTileset) && registry.Textures.ContainsKey(targetTileset))
            {
                _logger.Debug("Tileset {0} already resolved for material {1}", args: [targetTileset, materialId]);
                return;
            }

            if (tilesetRegistry.TilesetIds.TryGetValue(targetTileset, out var tilesetId))
            {
                if (tilesetRegistry.Textures.TryGetValue(tilesetId, out var texture) &&
                    tilesetRegistry.Definitions.TryGetValue(tilesetId, out var definition))
                {
                    registry.Definitions[targetTileset] = definition;
                    registry.Textures[targetTileset] = texture;
                    _logger.Debug("Tileset {0} resolved for material {1} by using already resolved tileset by MapTilesetRegistryPipelineStep.", args: [targetTileset, materialId]);
                    return;
                }

                _logger.Critical(
                    "Failed to resolve tileset {0} for material {1}. It was present inside the tilesetRegistry.TilesetIds but not in Definitions and Textures.",
                    args: [targetTileset, materialId]);
            }

            assetManager.GetAsset<TilesetDefinition>(targetTileset)
                .OnSuccess(definition =>
                {
                    var resourcePath = definition.TextureResourcePath;

                    var resData = resourceManager.GetResource(resourcePath).Match(data => data, err =>
                    {
                        _logger.Error("Failed to load tileset {0} for material {1} due to: {2}",
                            args: [targetTileset, materialId, err]);
                        return new ReadOnlyMemory<byte>();
                    });

                    if (resData.IsEmpty)
                        return;

                    var texture = Texture2D.FromMemory(graphicsDevice, resData);

                    registry.Definitions[targetTileset] = definition;
                    registry.Textures[targetTileset] = texture;
                    _logger.Debug("Tileset {0} resolved for material {1}", args: [targetTileset, materialId]);
                }).OnFailure(err =>
                {
                    _logger.Error("Failed to load tileset {0} for material {1} due to: {2}",
                        args: [targetTileset, materialId, err]);
                });
        }
    }
}