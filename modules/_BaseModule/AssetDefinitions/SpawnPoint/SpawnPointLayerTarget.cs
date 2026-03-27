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

using System.Numerics;
using RPGCreator.SDK;
using RPGCreator.SDK.Assets.Definitions.Maps;
using RPGCreator.SDK.Editor;
using RPGCreator.SDK.Logging;

namespace _BaseModule.AssetDefinitions.SpawnPoint;

public class SpawnPointLayerTarget : IPaintTarget
{
    
    private static readonly ScopedLogger logger = Logger.ForContext<SpawnPointLayerTarget>();
    
    public List<Vector2> PreviewPosition { get; set; }
    public object? PreviewObject { get; set; }
    public int GridWidth { get; private set; }
    public int GridHeight { get; private set; }
    public IMapDef? MapDef { get; }
    private readonly SpawnPointLayer _layerDef;
    
    public SpawnPointLayerTarget(SpawnPointLayer layerDef, IMapDef map, int gridWidth, int gridHeight)
    {
        _layerDef = layerDef;
        MapDef = map;
        GridWidth = gridWidth;
        GridHeight = gridHeight;
    }
    
    public bool CanAcceptObject(object objectToPaint)
    {
        return objectToPaint is SpawnPointData;
    }

    public void PaintAt(Vector2 position, object objectToPaint)
    {
        if (objectToPaint is SpawnPointData spawnPointData)
        {
            _layerDef.ClearElements();
            _layerDef.AddElement(spawnPointData, position);
        }
    }

    public void EraseAt(Vector2 position)
    {
        _layerDef.RemoveElement(position);
    }

    public void PreviewAt(Vector2 position, object objectToPreview)
    {
        logger.Warning("Previewing is not yet implemented.");
    }

    public void PreviewAt(List<Vector2> positions, object objectToPreview)
    {
        logger.Warning("Previewing is not yet implemented.");
    }

    public void ClearPreview()
    {
        if (RuntimeServices.RenderService.CurrentPreviewTarget == this)
        {
            RuntimeServices.RenderService.CurrentPreviewTarget = null;
        }
        
        PreviewObject = null;
    }
}