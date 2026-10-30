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
using RPGCreator.SDK.Assets.Actors;
using RPGCreator.Shared.Attributes;

namespace RPGCreator.Editor.Handlers.ActorFeatureDrawer;

[ActorFeatureDrawer(typeof(ActorMovementFeatureData))]
public class ActorMovementFeatureDrawer : IActorFeatureDrawer<ActorMovementFeatureData>
{
    public bool Draw(ActorMovementFeatureData feature)
    {
        bool changed = false;
        float walkSpeed = feature.WalkSpeed;
        if (ImGui.DragFloat("Walk Speed (px/s)", ref walkSpeed, 1f, 0f, 2000f))
        {
            feature.WalkSpeed = walkSpeed;
            changed = true;
        }

        bool canRun = feature.CanRun;
        if (ImGui.Checkbox("Can Run", ref canRun))
        {
            feature.CanRun = canRun;
            changed = true;
        }

        if (feature.CanRun)
        {
            float runSpeed = feature.RunSpeed;
            if (ImGui.DragFloat("Run Speed (px/s)", ref runSpeed, 1f, 0f, 2000f))
            {
                feature.RunSpeed = runSpeed;
                changed = true;
            }
        }

        bool allowDiag = feature.AllowDiagonalMovement;
        if (ImGui.Checkbox("Allow Diagonal Movement", ref allowDiag))
        {
            feature.AllowDiagonalMovement = allowDiag;
            changed = true;
        }

        if (ImGui.BeginCombo("Initial Facing Direction", feature.InitialFacingDirection.ToString()))
        {
            foreach (var dir in Enum.GetValues<ActorDirection>())
            {
                if (ImGui.Selectable(dir.ToString(), dir == feature.InitialFacingDirection))
                {
                    feature.InitialFacingDirection = dir;
                    changed = true;
                }
            }
            ImGui.EndCombo();
        }

        return changed;
    }
}