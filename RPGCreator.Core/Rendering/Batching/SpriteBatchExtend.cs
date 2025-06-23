#region LICENSE
//
// RPG Creator - Open-source RPG Engine.
// (c) 2025 Ward
// 
// This file is part of RPG Creator and is distributed under the MIT License.
// You are free to use, modify, and distribute this file under the terms of the MIT License.
// See LICENSE for details.
// 
// ---
// 
// Ce fichier fait partie de RPG Creator et est distribué sous licence MIT.
// Vous êtes libre de l'utiliser, de le modifier et de le distribuer sous les termes de la licence MIT.
// Voir LICENSE pour plus de détails.
// 
// Contact:
// => Mail: Ward727a@gmail.com
//    Please use this object: "RPG Creator [YourObject]"
// => Discord: ward727
// 
// For urgent inquiries, sending both an email and a message on Discord is highly recommended for a quicker response.
// 
// 
#endregion
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPGCreator.Core.Rendering.Batching
{
    public class SpriteBatchExtend : SpriteBatch
    {
        public bool IsBegin { get; private set; } = false;
        public float Opacity { get; private set; } = 1f;

        public struct SpriteBatchState
        {
            public SpriteBatchState(
                SpriteSortMode sortMode = SpriteSortMode.Deferred,
                BlendState? blendState = null,
                SamplerState? samplerState = null,
                DepthStencilState? depthStencilState = null,
                RasterizerState? rasterizerState = null,
                Effect? effect = null,
                Matrix? transformMatrix = null)
            {
                SortMode = sortMode;
                BlendState = blendState;
                SamplerState = samplerState;
                DepthStencilState = depthStencilState;
                RasterizerState = rasterizerState;
                Effect = effect;
                TransformMatrix = transformMatrix;
            }
            public SpriteSortMode SortMode { get; private set; } = SpriteSortMode.Deferred;
            public BlendState? BlendState { get; private set; } = null;
            public SamplerState? SamplerState { get; private set; } = null;
            public DepthStencilState? DepthStencilState { get; private set; } = null;
            public RasterizerState? RasterizerState { get; private set; } = null;
            public Effect? Effect { get; private set; } = null;
            public Matrix? TransformMatrix { get; private set; } = null;
        }

        Stack<SpriteBatchState> _states = new Stack<SpriteBatchState>();
        public SpriteBatchState CurrentState => _states.Peek();
        public SpriteBatchExtend(GraphicsDevice graphicsDevice) : base(graphicsDevice)
        {
        }

        public new void Begin(SpriteSortMode sortMode = SpriteSortMode.Deferred, BlendState blendState = null, SamplerState samplerState = null, DepthStencilState depthStencilState = null, RasterizerState rasterizerState = null, Effect effect = null, Matrix? transformMatrix = null)
        {
            if (IsBegin)
                return;

            SpriteBatchState spriteBatchState = new(
                sortMode: sortMode,
                blendState: blendState,
                samplerState: samplerState,
                depthStencilState: depthStencilState,
                rasterizerState: rasterizerState,
                effect: effect,
                transformMatrix: transformMatrix);

            _states.Push(spriteBatchState);

            base.Begin(sortMode, blendState, samplerState, depthStencilState, rasterizerState, effect, transformMatrix);
            IsBegin = true;
        }
        public new void End()
        {
            if (!IsBegin)
                return;

            _states.Pop();

            base.End();
            IsBegin = false;
        }

        protected void EndWithoutDispose()
        {
            if (!IsBegin)
                return;
            base.End();
            IsBegin = false;
        }

        public int ChildBegin(SpriteSortMode sortMode = SpriteSortMode.Deferred, BlendState blendState = null, SamplerState samplerState = null, DepthStencilState depthStencilState = null, RasterizerState rasterizerState = null, Effect effect = null, Matrix? transformMatrix = null)
        {
            if (IsBegin)
                EndWithoutDispose();
            Begin(sortMode, blendState, samplerState, depthStencilState, rasterizerState, effect, transformMatrix);

            return _states.Count;

        }

        public void ChildEnd()
        {
            if (!IsBegin)
                return;
            EndWithoutDispose();
            if (_states.Count > 0)
            {
                SpriteBatchState spriteBatchState = _states.Pop();
                Begin(spriteBatchState.SortMode, spriteBatchState.BlendState, spriteBatchState.SamplerState, spriteBatchState.DepthStencilState, spriteBatchState.RasterizerState, spriteBatchState.Effect, spriteBatchState.TransformMatrix);
            }
        }

        public void SetOpacity(float opacity)
        {
            if (opacity < 0f || opacity > 1f)
                throw new ArgumentOutOfRangeException(nameof(opacity), "Opacity must be between 0 and 1.");
            Opacity = opacity;
        }

        public void ResetOpacity()
        {
            Opacity = 1f;
        }

        public new void Draw(Texture2D texture, Vector2 position, Rectangle? sourceRectangle, Color color)
        {
            // Apply opacity to the color
            color *= Opacity;
            base.Draw(texture, position, sourceRectangle, color);
        }
    }
}
