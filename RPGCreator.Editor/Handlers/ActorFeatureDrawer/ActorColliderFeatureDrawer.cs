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

using System;
using Hexa.NET.ImGui;
using RPGCreator.RuntimeLib.ActorFeaturesData;
using RPGCreator.SDK.ActorFeatures;
using RPGCreator.Shared.Attributes;

namespace RPGCreator.Editor.Handlers.ActorFeatureDrawer;

[ActorFeatureDrawer(typeof(ActorColliderFeatureData))]
public class ActorColliderFeatureDrawer : IActorFeatureDrawer<ActorColliderFeatureData>
{
    public bool Draw(ActorColliderFeatureData feature)
    {
        bool changed = false;
        int[] offset = [feature.OffsetX, feature.OffsetY];
        if (ImGui.DragInt2("Local Offset (X, Y)", ref offset[0]))
        {
            feature.OffsetX = offset[0];
            feature.OffsetY = offset[1];
            changed = true;
        }

        int[] size = [feature.Width, feature.Height];
        if (ImGui.DragInt2("Hitbox Size (W, H)", ref size[0], 1f, 1, 2048))
        {
            feature.Width = Math.Max(1, size[0]);
            feature.Height = Math.Max(1, size[1]);
            changed = true;
        }

        bool isSolid = feature.IsSolid;
        if (ImGui.Checkbox("Is Solid (Blocks Movement)", ref isSolid))
        {
            feature.IsSolid = isSolid;
            changed = true;
        }

        return changed;
    }
}