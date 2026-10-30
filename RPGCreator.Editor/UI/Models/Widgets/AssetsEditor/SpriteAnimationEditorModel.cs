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
using System.Collections.Generic;
using Hexa.NET.ImGui;
using Microsoft.Xna.Framework;
using RPGCreator.SDK.Assets.SpriteAnimation;
using RPGCreator.Shared.Attributes;

namespace RPGCreator.Editor.UI.Models.Widgets.AssetsEditor;

[EditorUiModel]
public class SpriteAnimationEditorModel
{
    public SpriteAnimationDefinition Animation { get; set; } = null!;
    public bool IsDirty { get; set; }

    public string Name = string.Empty;
    public Ulid SpriteResourceId = Ulid.Empty;
    public int FrameWidth = 32;
    public int FrameHeight = 32;
    public float Fps = 8f;
    public AnimationPlaybackMode PlaybackMode = AnimationPlaybackMode.Loop;
    public List<AnimationFrame> Frames = [];

    public ImTextureRef? Texture = null;
    public Point TextureSize = new(32, 32);

    public bool IsPlaying = true;
    public int CurrentPreviewFrameIndex = 0;
    public float PreviewTimer = 0f;
    public int PingPongDirection = 1;
}