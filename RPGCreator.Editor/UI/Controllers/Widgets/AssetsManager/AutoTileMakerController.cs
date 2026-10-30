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
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RPGCreator.Editor.ImGuiLib;
using RPGCreator.Editor.UI.Models.Widgets.AssetsManager;
using RPGCreator.Editor.UI.Views.EditorTools.Payload;
using RPGCreator.RuntimeLib.FNA;
using RPGCreator.SDK.Assets.AutoTiles;
using RPGCreator.SDK.Assets.Tileset;
using RPGCreator.SDK.Common.Logging;
using RPGCreator.SDK.Modules.Editor.Ui;
using RPGCreator.SDK.Resources;
using RPGCreator.SDK.Services.Assets;
using RPGCreator.SDK.Services.Map;
using RPGCreator.Shared.Attributes;
using RPGCreator.Shared.Types;
using Vector2 = System.Numerics.Vector2;

namespace RPGCreator.Editor.UI.Controllers.Widgets.AssetsManager;


public partial class AutoTileAssetManagerController
{
    public record AutoTileMakerModalArgs(Action RefreshList);
    [EditorUiController]
    public class AutoTileMakerModalController(AutoTileAssetManagerModel.AutoTileMakerModalModel model, IAssetManager assetManager, IMaterialRegistry materialRegistry, IResourceManager resourceManager, ImGuiRenderer imGuiRenderer, Game game)
    {
        private readonly GraphicsDevice _graphicsDevice = game.GraphicsDevice;
        private IEditorUiBindableView? _tilePickerWidget;
        
        public Action RefreshList { get; private set; } = () => { };
        private List<Ulid>? UlidsTilesets;
        public AutoTileAssetManagerModel.AutoTileMakerModalModel State { get; } = model;

        public void Init(AutoTileMakerModalArgs args)
        {
            RefreshList = args.RefreshList;
        }
        
        public void SetTilePickerWidget(IEditorUiBindableView tilePickerWidget)
        {
            _tilePickerWidget = tilePickerWidget;
        }
        
        public TilePickerArgs MakeAutoTilePickerArg()
        {
            return new TilePickerArgs(ConfirmTileIndexSelection, _tilePickerWidget);
        }
        
        public List<Ulid> GetTilesets()
        {
            if (UlidsTilesets == null)
                UlidsTilesets = assetManager.ListAssets<TilesetDefinition>().ToList();
            return UlidsTilesets;
        }
        public TilesetDefinition? GetTilesetDefinition(Ulid id)
        {
            return assetManager.GetAsset<TilesetDefinition>(id).Match(tileset => tileset, err => null);
        }
        public void SelectTileset(TilesetDefinition tileset)
        {
            if (State.SelectedTileset != null)
            {
                assetManager.ClearAssetFromCache(State.SelectedTileset.AssetId);
            }

            State.SelectedTileset = tileset;

            if (State.TilesetTextureRef.HasValue)
            {
                imGuiRenderer.UnbindTexture(State.TilesetTextureRef.Value);
                State.TilesetTexture?.Dispose();
                State.TilesetTextureRef = null;
                State.TilesetTexture = null;
            }
            State.TileUv0 = Vector2.Zero;
            State.TileUv1 = Vector2.Zero;
            State.DefaultTileIndex = TileIndex.Zero;
            uv0 = Vector2.Zero;
            uv1 = Vector2.Zero;
            defaultTile = TileIndex.Zero;

            resourceManager.GetResource(tileset.TextureResourcePath).OnSuccess((data =>
            {
                State.TilesetTexture = Texture2D.FromMemory(_graphicsDevice, data);
                State.TilesetTexture.Name = "AutoTileManager.TileSelector";
                State.TilesetTextureRef = imGuiRenderer.BindTexture(State.TilesetTexture);
                State.TilesetImageSize = new(State.TilesetTexture.Width, State.TilesetTexture.Height);
            })).OnFailure(err =>
            {
                State.SelectedTileset = null;
                Logger.Error(err);
            });
        }

        private TilesetTilePickerArgs _tilePickerArgs = new(default, Point.Zero, Point.Zero, (_, _) => { });

        public TilesetTilePickerArgs UpdateArgs(out bool success, out bool fromCache)
        {
            success = false;
            fromCache = true;
            if (State.SelectedTileset == null || !State.TilesetTextureRef.HasValue || State.TilesetTexture == null) return default;

            if (State.DefaultTileIndex != TileIndex.Zero)
            {
                _tilePickerArgs.StartIndex = new(State.DefaultTileIndex.X, State.DefaultTileIndex.Y);
                _tilePickerArgs.EndIndex = _tilePickerArgs.StartIndex;
            }

            success = true;
            if (_tilePickerArgs.Image.TexID == State.TilesetTextureRef.Value.TexID) return _tilePickerArgs;

            _tilePickerArgs.OnTileSelected = SelectTileIndex;
            _tilePickerArgs.Image = State.TilesetTextureRef.Value;
            _tilePickerArgs.TileSize = new Point(State.SelectedTileset.TileWidth, State.SelectedTileset.TileHeight);
            _tilePickerArgs.ImageSize = new Point(State.TilesetTexture.Width, State.TilesetTexture.Height);

            fromCache = false;
            return _tilePickerArgs;
        }

        private Vector2 uv0 = Vector2.Zero;
        private Vector2 uv1 = Vector2.Zero;
        private TileIndex defaultTile = TileIndex.Zero;

        private void SelectTileIndex(TileIndex start, TileIndex end)
        {
            if (State.SelectedTileset == null)
            {
                return;
            }
            
            var boundsTile = new Rectangle(start.X * State.SelectedTileset.TileWidth, start.Y * State.SelectedTileset.TileHeight, State.SelectedTileset.TileWidth, State.SelectedTileset.TileHeight);
            
            uv0 = new Vector2(
                (float)boundsTile.X / State.TilesetImageSize.X,
                (float)boundsTile.Y / State.TilesetImageSize.Y
            );

            uv1 = new Vector2(
                (float)(boundsTile.X + boundsTile.Width) / State.TilesetImageSize.X,
                (float)(boundsTile.Y + boundsTile.Height) / State.TilesetImageSize.Y
            );
            defaultTile = start;
        }

        public void ConfirmTileIndexSelection()
        {
            if (defaultTile != TileIndex.Zero)
            {
                State.DefaultTileIndex = defaultTile;
                State.TileUv0 = uv0;
                State.TileUv1 = uv1;
            }
        }

        public bool CreateAutoTile()
        {
            if (State.SelectedTileset == null || State.DefaultTileIndex == TileIndex.Zero)
                return false;
            
            var autoTile = new AutoTileRulesetDefinition()
            {
                AssetId = Ulid.NewUlid(),
                Name = State.TilesetName,
                TargetTileset = State.SelectedTileset.AssetId,
                DefaultTile = State.DefaultTileIndex,
            };

            var materialId = materialRegistry.GetFirstAvailableMaterialId();

            if (materialId != MaterialId.Empty)
            {
                materialRegistry.RegisterRuleset(materialId, autoTile.AssetId, autoTile.Name);
                materialRegistry.Save();
            }
            else
            {
                ImNotify.Show("Warning!", "No available material id for the new auto tile.", NotifyType.Warning, 5F);
            }

            assetManager.SaveAsset(autoTile);
            
            return true;
        }
    }
}