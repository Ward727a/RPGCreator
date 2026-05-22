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

using System.Collections.Immutable;
using System.Numerics;
using Fluxor;
using RPGCreator.SDK.GlobalState;

namespace RPGCreator.SDK.States;

public record ToolState(
    ToolLogic? ActiveTool,
    object? Payload,
    IImmutableDictionary<string, object?> ParameterValue,
    Vector2 LastDrawAt
)
{
    public bool HasActiveTool => ActiveTool != null;
    public bool HasPayload => Payload != null;
    
    public Type PayloadType => Payload?.GetType() ?? typeof(void);
    public EPayloadType ToolPayloadType => ActiveTool?.PayloadType ?? EPayloadType.None;
}

public class FeatureToolState : Feature<ToolState>
{
    public override string GetName() => "ToolState";

    protected override ToolState GetInitialState() => new(null, null, ImmutableDictionary<string, object?>.Empty, Vector2.Zero);
}