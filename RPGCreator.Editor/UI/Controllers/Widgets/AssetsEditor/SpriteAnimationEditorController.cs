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
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RPGCreator.Editor.ImGuiLib;
using RPGCreator.Editor.UI.Models.Widgets.AssetsEditor;
using RPGCreator.Editor.UI.Views;
using RPGCreator.RuntimeLib.FNA;
using RPGCreator.SDK.Assets.SpriteAnimation;
using RPGCreator.SDK.Common.Logging;
using RPGCreator.SDK.Modules.Editor.Ui;
using RPGCreator.SDK.Resources;
using RPGCreator.SDK.Services.Assets;
using RPGCreator.Shared.Attributes;
using Vector2 = System.Numerics.Vector2;


namespace RPGCreator.Editor.UI.Controllers.Widgets.AssetsEditor;

[EditorUiController]
public class SpriteAnimationEditorController(
    SpriteAnimationEditorModel model,
    IAssetManager assetManager,
    IResourceManager resourceManager,
    GraphicsDevice graphicsDevice,
    ImGuiRenderer imGuiRenderer)
{
    public readonly SpriteAnimationEditorModel Model = model;
    private IEditorUiBindableView? _resourceSelectorView = null;

    private ResourceSelector.ResourceSelectorArguments ResourceSelectorArguments => new()
    {
        AllowImportFromOs = true,
        ResourceTypes = [IResourceManager.ResourceType.Image],
        OnResourceSelected = (resource) =>
        {
            if (resource.ResourceId == Model.SpriteResourceId)
            {
                _resourceSelectorView = null;
                return;
            }

            LoadSpriteSheetTexture(resource.ResourceId);
            Model.IsDirty = true;
            _resourceSelectorView = null;
        }
    };

    public void Setup(SpriteAnimationDefinition animation)
    {
        Clear();

        Model.Animation = animation;
        Model.Name = animation.Name;
        Model.SpriteResourceId = animation.SpriteSheetResource;
        Model.FrameWidth = Math.Max(1, animation.FrameWidth);
        Model.FrameHeight = Math.Max(1, animation.FrameHeight);
        Model.Fps = animation.Fps;
        Model.PlaybackMode = animation.PlaybackMode;
        Model.Frames = [.. animation.Frames];
        Model.IsDirty = false;

        if (Model.SpriteResourceId != Ulid.Empty)
        {
            LoadSpriteSheetTexture(Model.SpriteResourceId);
        }
    }

    private void LoadSpriteSheetTexture(Ulid resourceId)
    {
        UnbindCurrentTexture();
        Model.SpriteResourceId = resourceId;

        resourceManager.GetResource(resourceId).OnSuccess(data =>
        {
            var texture2D = Texture2D.FromMemory(graphicsDevice, data);
            Model.TextureSize = new Point(texture2D.Width, texture2D.Height);
            Model.Texture = imGuiRenderer.BindTexture(texture2D);
        }).OnFailure(err =>
        {
            Logger.Error("Failed to load sprite sheet texture {0}: {1}", resourceId, err);
        });
    }

    public void OpenSpriteSheetSelector(IEditorUiRouterLocal router)
    {
        router.ActivateView("rpgc://views/resource_selector", ResourceSelectorArguments).OnSuccess(selector =>
        {
            _resourceSelectorView = selector;
            ImGui.OpenPopup((_resourceSelectorView as ResourceSelector)?.ModalId);
        });
    }

    public void TryDrawResourceSelector()
    {
        _resourceSelectorView?.Draw(EDrawAs.Modal);
    }

    /// <summary>
    /// Computes normalized UV coordinates (uv0, uv1) for a given tile index on the sprite sheet.
    /// </summary>
    public (Vector2 Uv0, Vector2 Uv1) GetTileUvs(ushort tileIndex)
    {
        var texW = Math.Max(1, Model.TextureSize.X);
        var texH = Math.Max(1, Model.TextureSize.Y);
        var frameW = Math.Clamp(Model.FrameWidth, 1, texW);
        var frameH = Math.Clamp(Model.FrameHeight, 1, texH);

        var columns = Math.Max(1, texW / frameW);
        var col = tileIndex % columns;
        var row = tileIndex / columns;

        var u0 = (float)(col * frameW) / texW;
        var v0 = (float)(row * frameH) / texH;
        var u1 = (float)((col + 1) * frameW) / texW;
        var v1 = (float)((row + 1) * frameH) / texH;

        return (new Vector2(u0, v0), new Vector2(u1, v1));
    }

    /// <summary>
    /// Advances the live preview animation timer based on delta time, Fps, DurationMultiplier, and PlaybackMode.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Throw if PlaybackMode is invalid.
    /// </exception>
    public void UpdatePreview(float deltaTime)
    {
        if (!Model.IsPlaying || Model.Frames.Count == 0 || Model.Fps <= 0f)
            return;

        Model.CurrentPreviewFrameIndex = Math.Clamp(Model.CurrentPreviewFrameIndex, 0, Model.Frames.Count - 1);
        var currentFrame = Model.Frames[Model.CurrentPreviewFrameIndex];
        var frameDuration = (1f / Model.Fps) * Math.Max(0.05f, currentFrame.DurationMultiplier);

        Model.PreviewTimer += deltaTime;
        if (Model.PreviewTimer < frameDuration)
            return;

        Model.PreviewTimer -= frameDuration;

        switch (Model.PlaybackMode)
        {
            case AnimationPlaybackMode.Loop:
                Model.CurrentPreviewFrameIndex = (Model.CurrentPreviewFrameIndex + 1) % Model.Frames.Count;
                break;

            case AnimationPlaybackMode.Once:
                if (Model.CurrentPreviewFrameIndex < Model.Frames.Count - 1)
                    Model.CurrentPreviewFrameIndex++;
                else
                    Model.IsPlaying = false;
                break;

            case AnimationPlaybackMode.PingPong:
                if (Model.Frames.Count == 1)
                {
                    Model.CurrentPreviewFrameIndex = 0;
                    break;
                }
                var next = Model.CurrentPreviewFrameIndex + Model.PingPongDirection;
                if (next >= Model.Frames.Count)
                {
                    Model.PingPongDirection = -1;
                    next = Model.Frames.Count - 2;
                }
                else if (next < 0)
                {
                    Model.PingPongDirection = 1;
                    next = 1;
                }
                Model.CurrentPreviewFrameIndex = Math.Clamp(next, 0, Model.Frames.Count - 1);
                break;
            default:
#pragma warning disable CA2208
                throw new ArgumentOutOfRangeException(nameof(Model.PlaybackMode), "Playback mode is invalid.");
#pragma warning restore CA2208
        }
    }

    public void RestartPreview()
    {
        Model.CurrentPreviewFrameIndex = 0;
        Model.PreviewTimer = 0f;
        Model.PingPongDirection = 1;
        Model.IsPlaying = true;
    }

    public void AddFrame(ushort tileIndex)
    {
        Model.Frames.Add(new AnimationFrame(tileIndex, 1f));
        Model.IsDirty = true;
    }

    public void RemoveFrameAt(int index)
    {
        if (index < 0 || index >= Model.Frames.Count)
            return;

        Model.Frames.RemoveAt(index);
        if (Model.CurrentPreviewFrameIndex >= Model.Frames.Count)
            Model.CurrentPreviewFrameIndex = Math.Max(0, Model.Frames.Count - 1);
        Model.IsDirty = true;
    }

    public void MoveFrame(int index, int direction)
    {
        var targetIndex = index + direction;
        if (index < 0 || index >= Model.Frames.Count || targetIndex < 0 || targetIndex >= Model.Frames.Count)
            return;

        (Model.Frames[index], Model.Frames[targetIndex]) = (Model.Frames[targetIndex], Model.Frames[index]);
        Model.IsDirty = true;
    }

    public void Save()
    {
        Model.Animation.Name = Model.Name;
        Model.Animation.SpriteSheetResource = Model.SpriteResourceId;
        Model.Animation.FrameWidth = Math.Max(1, Model.FrameWidth);
        Model.Animation.FrameHeight = Math.Max(1, Model.FrameHeight);
        Model.Animation.Fps = Math.Max(0.1f, Model.Fps);
        Model.Animation.PlaybackMode = Model.PlaybackMode;
        Model.Animation.Frames = [.. Model.Frames];

        assetManager.SaveAsset(Model.Animation);
        Model.IsDirty = false;
    }

    public void Clear()
    {
        UnbindCurrentTexture();
        Model.Frames.Clear();
        Model.CurrentPreviewFrameIndex = 0;
        Model.PreviewTimer = 0f;
        Model.IsDirty = false;
    }

    private void UnbindCurrentTexture()
    {
        if (!Model.Texture.HasValue)
            return;

        var textureInfo = imGuiRenderer.UnbindTexture(Model.Texture.Value);
        if (textureInfo is { IsManaged: false })
        {
            textureInfo.Texture.Dispose();
        }
        Model.Texture = null;
    }
}