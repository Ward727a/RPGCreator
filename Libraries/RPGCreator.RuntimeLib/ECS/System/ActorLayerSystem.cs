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

using Arch.Buffer;
using Arch.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RPGCreator.Editor.ECS.Flags;
using RPGCreator.RuntimeLib.ECS.Components.Actor;
using RPGCreator.RuntimeLib.FNA;
using RPGCreator.SDK.Assets.Actors;
using RPGCreator.SDK.Assets.SpriteAnimation;
using RPGCreator.SDK.Common.Logging;
using RPGCreator.SDK.ECS;
using RPGCreator.SDK.ECS.Components;
using RPGCreator.SDK.Resources;
using RPGCreator.SDK.Scenes;
using RPGCreator.SDK.Services.Assets;
using RPGCreator.Shared.Types;
using Color = Microsoft.Xna.Framework.Color;
using SpriteEffects = Microsoft.Xna.Framework.Graphics.SpriteEffects;

namespace RPGCreator.RuntimeLib.ECS.System;

public class ActorLayerSystem(
    World world,
    CommandBuffer commandBuffer,
    ISceneRender sceneRender,
    GraphicsDevice graphicsDevice,
    IAssetManager assetManager,
    IResourceManager resourceManager) : RpgcBaseSystem(world, commandBuffer, sceneRender)
{
    private static readonly URN IdleSlotUrn = new("rpgc", "anim_slots", "idle");

    private readonly QueryDescription _actorQuery =
        new QueryDescription().WithAll<ActorInstanceComponent, TransformComponent, LayerComponent>();

    private readonly Dictionary<Ulid, Texture2D?> _textureCache = [];
    private Texture2D _fallbackTexture = null!;
    private double _totalTime;

    public override void Initialize()
    {
        _fallbackTexture = new Texture2D(graphicsDevice, 1, 1);
        _fallbackTexture.SetData([Color.White]);
        _fallbackTexture.Name = "ActorFallbackPixel";
    }

    public override void Update(in double t)
    {
        _totalTime += t;

        World.Query(in _actorQuery,
            (Entity entity, ref ActorInstanceComponent actorInstance, ref TransformComponent transform, ref LayerComponent layerComponent) =>
            {
                if (!layerComponent.Visible)
                    return;

                var drawColor = Color.White;
                var parentLayerEntity = actorInstance.ParentLayer.LayerEntity;

                if (actorInstance.ParentLayer.IsPhantom ||
                    (World.IsAlive(parentLayerEntity) && World.Has<PhantomLayerFlag>(parentLayerEntity)))
                {
                    drawColor *= 0.5f + 0.3f * (float)Math.Sin(_totalTime * Math.PI);
                }

                var destPos = new Vector2(transform.Position.X, transform.Position.Y);
                var sortY = destPos.Y + transform.Size.Y;

                var actorResult = assetManager.GetAsset<ActorDefinition>(actorInstance.ActorDefinitionId);
                if (actorResult.IsFailure)
                {
                    DrawFallbackBox(destPos, transform.Size, Color.Magenta * (drawColor.A / 255f), sortY, layerComponent.ZIndex);
                    return;
                }

                var actorDef = actorResult.Value;

                // Determine facing direction (from ActorMovementComponent if present, otherwise Down)
                var facing = ActorDirection.Down;
                if (World.Has<ActorMovementComponent>(entity))
                {
                    facing = World.Get<ActorMovementComponent>(entity).FacingDirection;
                }

                // Try rendering the directional SpriteAnimation (Idle or first available slot)
                if (TryDrawActorAnimation(actorDef, facing, destPos, drawColor, sortY, layerComponent.ZIndex))
                    return;

                // Fallback: Try rendering the Actor's IconResource / Portrait
                if (actorDef.IconResource != Ulid.Empty)
                {
                    var iconTex = GetOrLoadTexture(actorDef.IconResource);
                    if (iconTex != null)
                    {
                        var srcRect = new Rectangle(0, 0, iconTex.Width, iconTex.Height);
                        SceneRender.SubmitSprite(iconTex, destPos, srcRect, drawColor, sortY, layerComponent.ZIndex);
                        return;
                    }
                }

                // Ultimate Fallback: Colored placeholder box
                DrawFallbackBox(destPos, transform.Size, Color.CornflowerBlue * (drawColor.A / 255f), sortY, layerComponent.ZIndex);
            });
    }

    private bool TryDrawActorAnimation(
        ActorDefinition actorDef,
        ActorDirection facing,
        Vector2 destPos,
        Color color,
        float sortY,
        int zIndex)
    {
        if (actorDef.Animations.Count == 0)
            return false;

        if (!actorDef.Animations.TryGetValue(IdleSlotUrn, out var animSet))
        {
            animSet = actorDef.Animations.Values.FirstOrDefault();
        }

        if (animSet == null)
            return false;

        var (animAssetId, flipHorizontally) = animSet.Resolve(facing);
        if (animAssetId == Ulid.Empty)
            return false;

        var animRes = assetManager.GetAsset<SpriteAnimationDefinition>(animAssetId);
        if (animRes.IsFailure)
            return false;

        var animDef = animRes.Value;
        if (animDef.SpriteSheetResource == Ulid.Empty || animDef.Frames.Count == 0)
            return false;

        var sheetTexture = GetOrLoadTexture(animDef.SpriteSheetResource);
        if (sheetTexture == null)
            return false;

        // Calculate current animation frame from _totalTime and Fps
        var frameCount = animDef.Frames.Count;
        var currentFrameIdx = 0;
        if (animDef.Fps > 0f && frameCount > 1)
        {
            var totalTicks = (int)(_totalTime * animDef.Fps);
            currentFrameIdx = animDef.PlaybackMode switch
            {
                AnimationPlaybackMode.Once => Math.Min(totalTicks, frameCount - 1),
                AnimationPlaybackMode.PingPong => PingPong(totalTicks, frameCount - 1),
                _ => totalTicks % frameCount
            };
        }

        var tileIndex = animDef.Frames[currentFrameIdx].TileIndex;
        var frameW = Math.Max(1, animDef.FrameWidth);
        var frameH = Math.Max(1, animDef.FrameHeight);
        var cols = Math.Max(1, sheetTexture.Width / frameW);

        var srcX = (tileIndex % cols) * frameW;
        var srcY = (tileIndex / cols) * frameH;

        var sourceRect = new Rectangle(srcX, srcY, frameW, frameH);
        var effects = flipHorizontally ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

        SceneRender.SubmitSprite(sheetTexture, destPos, sourceRect, color, sortY, zIndex, effects);
        return true;
    }

    private void DrawFallbackBox(Vector2 destPos, Point size, Color color, float sortY, int zIndex)
    {
        SceneRender.SubmitSprite(_fallbackTexture, destPos, new Rectangle(0, 0, size.X, size.Y), color, sortY, zIndex);
    }

    private Texture2D? GetOrLoadTexture(Ulid resourceId)
    {
        if (resourceId == Ulid.Empty)
            return null;

        if (_textureCache.TryGetValue(resourceId, out var cached))
            return cached;

        Texture2D? loaded = null;
        resourceManager.GetResource(resourceId)
            .OnSuccess(data =>
            {
                loaded = Texture2D.FromMemory(graphicsDevice, data);
                _textureCache[resourceId] = loaded;
            })
            .OnFailure(err =>
            {
                Logger.Once.Error("ActorLayerSystem failed to load texture {0}: {1}", resourceId, err);
                _textureCache[resourceId] = null;
            });

        return loaded;
    }

    public override void Dispose()
    {
        _fallbackTexture.Dispose();
        foreach (var tex in _textureCache.Values)
        {
            tex?.Dispose();
        }
        _textureCache.Clear();
        GC.SuppressFinalize(this);
    }

    private static int PingPong(int t, int length)
    {
        if (length <= 0) return 0;
        var period = length * 2;
        var mod = t % period;
        return mod <= length ? mod : period - mod;
    }
}
