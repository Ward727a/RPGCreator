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
using Hexa.NET.ImGui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RPGCreator.Editor.ImGuiLib;
using RPGCreator.Editor.UI.Models.Widgets.AssetsEditor;
using RPGCreator.Editor.UI.Views;
using RPGCreator.RuntimeLib.FNA;
using RPGCreator.SDK.Assets.Stats;
using RPGCreator.SDK.Common.Logging;
using RPGCreator.SDK.Modules.Editor.Ui;
using RPGCreator.SDK.Resources;
using RPGCreator.SDK.Services.Assets;
using RPGCreator.Shared.Attributes;
using RPGCreator.Shared.Types;

namespace RPGCreator.Editor.UI.Controllers.Widgets.AssetsEditor;

[EditorUiController]
public class StatAssetEditorController(
    StatAssetEditorModel model,
    IResourceManager resourceManager,
    GraphicsDevice graphicsDevice,
    ImGuiRenderer imGuiRenderer,
    IAssetManager assetManager
    )
{
    
    private IEditorUiBindableView? _iconSelectorView = null;
    public ResourceSelector.ResourceSelectorArguments ResourceSelectorArguments => new()
    {
        AllowImportFromOs = true,
        ResourceTypes = [IResourceManager.ResourceType.Image],
        OnResourceSelected = (resource) =>
        {
            if (Model.IconId == resource.ResourceId)
            {
                _iconSelectorView = null;
                return;
            }
            
            if (Model.IconId != Ulid.Empty)
            {
                if (Model.IconTexture.HasValue)
                {
                    var backup = Model.IconTexture.Value;
                    Model.IconTexture = null;
                    var textureInfo = imGuiRenderer.UnbindTexture(backup);
                    if (!textureInfo?.IsManaged ?? false)
                    {
                        textureInfo.Texture.Dispose();
                    }
                }
            }

            Model.IconSourcePath = resource.ResourceLocalPath;
            Model.IconId = resource.ResourceId;
            Model.IsDirty = true;

            resourceManager.GetResource(resource.ResourceId).OnSuccess(data =>
            {
                var texture2D = Texture2D.FromMemory(graphicsDevice, data);
                Model.IconTexture = imGuiRenderer.BindTexture(texture2D);

                resourceManager.GetResourceMeta<ResourceImageMeta>(resource).OnSuccess(meta =>
                {
                    var x = Math.Min(meta.ImageSize.X, 256);
                    var y = Math.Min(meta.ImageSize.Y, 256);
                    Model.IconSize = new Point(x, y);
                }).OnFailure(err =>
                {
                    Model.IconSize = new Point(32, 32);
                    Logger.Error("Couldn't load icon meta due to error: {err}", err);
                });
            }).OnFailure(err => { Logger.Error("Couldn't load icon texture due to error: {err}", err); });

            _iconSelectorView = null;
        }
    };
    
    public void OpenIconSelector(IEditorUiRouterLocal router)
    {
        router.ActivateView("rpgc://views/resource_selector", ResourceSelectorArguments).OnSuccess((selector) =>
        {
            _iconSelectorView = selector;
            ImGui.OpenPopup((_iconSelectorView as ResourceSelector)?.ModalId);
        });
    }
    
    public void TryDrawResourceSelector()
    {
        _iconSelectorView?.Draw(EDrawAs.Modal);
    }
    
    public StatAssetEditorModel Model => model;

    public void LoadStat(StatDefinition stat)
    {
        Model.Stat = stat;
        
        Model.Name = stat.Name;
        Model.Code = stat.Code;
        Model.Description = stat.Description;
        Model.IconId = stat.IconResource;
        Model.DefaultValue = stat.DefaultValue;
        Model.MinValue = stat.MinValue;
        Model.MaxValue = stat.MaxValue;
        Model.MaxStatId = stat.MaxStatId;
        Model.DerivedStatFormula = stat.DerivedFormula;
        Model.ThresholdRules = stat.ThresholdRules;

        if (Model.IconTexture.HasValue)
        {
            var texture2D = imGuiRenderer.UnbindTexture(Model.IconTexture.Value);
            if (!texture2D?.IsManaged ?? false)
            {
                texture2D.Texture.Dispose();
            }
        }

        Model.IconTexture = null;
        GetTexture();
    }

    public void Clear()
    {
        Model.Code = "";
        Model.Name = "";
        Model.Description = "";
        Model.IconId = Ulid.Empty;
        Model.DefaultValue = 0;
        Model.MinValue = 0;
        Model.MaxValue = 0;
        Model.MaxStatId = Ulid.Empty;
        Model.DerivedStatFormula = "";
        Model.ThresholdRules = [];
        if (Model.IconTexture.HasValue)
        {
            var texture2D = imGuiRenderer.UnbindTexture(Model.IconTexture.Value);
            if (!texture2D?.IsManaged ?? false)
            {
                texture2D.Texture.Dispose();
            }
        }
        Model.IconTexture = null;
    }
    
    public void GetTexture()
    {
        if (Model.IconId == Ulid.Empty) return;

        if(Model.IconTexture != null) return;

        resourceManager.GetResource(Model.IconId).OnSuccess(data =>
        {
            var texture2D = Texture2D.FromMemory(graphicsDevice, data);
            Model.IconTexture = imGuiRenderer.BindTexture(texture2D);
        }).OnFailure(err =>
        {
            Logger.Error("Error while trying to load icon texture for stat. Error: {err}", err);
        });
    }

    public void Save()
    {
        var stat = model.Stat;

        stat.Code = Model.Code;
        stat.Name = Model.Name;
        stat.Description = Model.Description;
        stat.IconResource = Model.IconId;
        stat.DefaultValue = Model.DefaultValue;
        stat.MinValue = Model.MinValue;
        stat.MaxValue = Model.MaxValue;
        stat.MaxStatId = Model.MaxStatId;
        stat.DerivedFormula = Model.DerivedStatFormula;
        stat.ThresholdRules = Model.ThresholdRules;

        assetManager.SaveAsset(stat);
    }

    public void AddThresholdRule()
    {
        Model.ThresholdRules.Add(new StatThresholdRule()
        {
            Trigger = StatThresholdTrigger.OnAbovePercent,
            Reaction = StatReactionKind.DestroyEntity,
            ThresholdValue = 20
        });
    }

    public IReadOnlyList<Ulid> GetStats()
    {
        return [.. assetManager.ListAssets<StatDefinition>()];
    }

    public Result<StatDefinition> GetStat(Ulid id) => assetManager.GetAsset<StatDefinition>(id);
    
    public bool WouldCreateMaxStatCycle(Ulid currentStatId, Ulid candidateMaxStatId)
    {
        if (candidateMaxStatId == Ulid.Empty)
            return false;

        if (currentStatId == candidateMaxStatId)
            return true;

        var visited = new HashSet<Ulid> { currentStatId };
        var nextId = candidateMaxStatId;

        while (nextId != Ulid.Empty)
        {
            if (!visited.Add(nextId))
                return true;

            var res = assetManager.GetAsset<StatDefinition>(nextId);
            if (res.IsFailure || res.Value == null)
                break;

            nextId = res.Value.MaxStatId;
        }

        return false;
    }
}