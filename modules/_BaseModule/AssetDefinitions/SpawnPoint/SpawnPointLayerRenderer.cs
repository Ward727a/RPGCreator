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

using RPGCreator.SDK;
using RPGCreator.SDK.Editor;

namespace _BaseModule.AssetDefinitions.SpawnPoint;

public class SpawnPointLayerRenderer : BaseLayerRenderer<SpawnPointLayer>
{
    private readonly string _spawnPointTexturePath;
    
    public SpawnPointLayerRenderer()
    {
        _spawnPointTexturePath = Path.Combine(RpgEnv.Path.ExeAssetsFolder, "Icons", "GameIconsNet", "spawn-node-32x.png");
    }
    
    public override void Render(SpawnPointLayer layer, long chunkId)
    {
        var chunkElements = layer.GetElements(chunkId);
        if (chunkElements == null)
            return;
        
        if(chunkElements.IsEmpty)
            return;
        
        for (int i = 0; i < chunkElements.Length; i++)
        {
            var spawnData  = chunkElements[i]; 
            if(spawnData == null)
                continue;
            
            var position = layer.GetElementWorldPosition(chunkId, i);
            
            
            RuntimeServices.RenderService.DirectDraw(_spawnPointTexturePath, position);
        }
    }
}