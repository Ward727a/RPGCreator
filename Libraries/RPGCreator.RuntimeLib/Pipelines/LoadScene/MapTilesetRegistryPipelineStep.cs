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

using Microsoft.Xna.Framework.Graphics;
using RPGCreator.Editor.ECS.Components;
using RPGCreator.RuntimeLib.FNA;
using RPGCreator.SDK.Assets.Tileset;
using RPGCreator.SDK.PipelineManager;
using RPGCreator.SDK.Resources;
using RPGCreator.SDK.Scenes;
using RPGCreator.SDK.Services.Assets;

namespace RPGCreator.RuntimeLib.Pipelines.LoadScene;

[PipelineStep(typeof(LoadSceneStepContext))]
public class MapTilesetRegistryPipelineStep(IAssetManager assetManager, IResourceManager resourceManager, GraphicsDevice graphicsDevice) : IPipelineStep<LoadSceneStepContext>
{
    public IEnumerable<Type> RunAfter { get; } = [];
    public IEnumerable<Type> RunBefore { get; } = [];
    public ValueTask RunAsync(LoadSceneStepContext context)
    {
        var map = context.MapDefinition;
        var simulation = context.Simulation;
        
        var registry = new MapTilesetRegistryComponent
        {
            Textures = new Dictionary<byte, Texture2D>(),
            Definitions = new Dictionary<byte, TilesetDefinition>()
        };

        foreach (var (tilesetIndex, tilesetId) in map.Header.TilesetMappings)
        {
            assetManager.GetAsset<TilesetDefinition>(tilesetId).OnSuccess(tileset =>
            {
                resourceManager.GetResource(tileset.TextureResourcePath).OnSuccess(textureData =>
                {
                    var texture2D = Texture2D.FromMemory(graphicsDevice, textureData);
                    registry.Textures.Add(tilesetIndex, texture2D);
                    registry.Definitions.Add(tilesetIndex, tileset);
                });
            });
        }
        
        registry.TilesetIds = (map.Header.TilesetMappings.ToDictionary(x => x.Value, x => x.Key));
        
        simulation.AttachWorldComponent(registry);
        
        return ValueTask.CompletedTask;
    }
}