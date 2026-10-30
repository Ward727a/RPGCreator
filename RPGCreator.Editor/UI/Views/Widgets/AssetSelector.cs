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
using RPGCreator.SDK.Assets;
using RPGCreator.SDK.Modules.Editor.Ui;
using RPGCreator.SDK.Services.Assets;

namespace RPGCreator.Editor.UI.Views.Widgets;

public class AssetSelector : ComplexEditorUi<AssetSelectorController>
{
    private readonly IAssetDrawerRegistry _assetDrawerRegistry;

    public readonly struct AssetSelectorArgs(List<Type> assetTypes, Action<Ulid> onAssetSelected)
    {
        public readonly List<Type> AssetTypes = assetTypes;
        public readonly Action<Ulid> OnAssetSelected = onAssetSelected;
    }
    
    public AssetSelector(IEditorViewContext context, EditorViewDefinition definition, AssetSelectorController controller, IAssetDrawerRegistry assetDrawerRegistry) : base(context, definition, controller)
    {
        _assetDrawerRegistry = assetDrawerRegistry;
    }

    public override void OnActivate(object? args)
    {
        if (args is AssetSelectorArgs arg)
        {
            Controller.InitState(arg);
            Controller.Refresh();
        }
    }

    public override void Draw(EDrawAs drawAs = EDrawAs.Unknown)
    {
        var flatAssets = Controller.State.FlatAssets;
        int totalItems = flatAssets.Count;
    
        if (ImGui.BeginTable("##assetSelector", 3))
        {
            ImGuiListClipper clipper = new ImGuiListClipper();

            clipper.Begin(totalItems);

            while (clipper.Step())
            {
                for (var i = clipper.DisplayStart; i < clipper.DisplayEnd; i++)
                {
                    var assetData = flatAssets[i];
                    var asset = Controller.GetAsset(assetData.AssetType, assetData.Id);

                    if (asset == null)
                    {
                        ImGui.TableNextRow();
                        ImGui.TableSetColumnIndex(2);
                        ImGui.TextDisabled($"[ERROR: Couldn't get asset of type {assetData.AssetType} with id {assetData.Id}.");
                    }
                    else
                    {
                        _assetDrawerRegistry.Draw(asset);
                    }
                }
            }
        
            clipper.End();
            ImGui.EndTable();
        }
    }
}

public class AssetSelectorController(AssetSelectorModel state, IAssetManager assetManager)
{
    public readonly AssetSelectorModel State = state;

    public void InitState(AssetSelector.AssetSelectorArgs args)
    {
        State.AssetTypes = args.AssetTypes;
        State.OnAssetSelected = args.OnAssetSelected;
    }

    public void Refresh()
    {
        State.FlatAssets.Clear();

        foreach (var assetType in State.AssetTypes)
        {
            var assetsForType = assetManager.ListAssets(assetType);
        
            foreach (var assetId in assetsForType)
            {
                State.FlatAssets.Add((assetType, assetId));
            }
        }
    }
    
    private Dictionary<Type, IEnumerable<Ulid>> GetAssets()
    {
        var list = new Dictionary<Type, IEnumerable<Ulid>>();
        foreach (var assetType in State.AssetTypes)
        {
            list[assetType] = assetManager.ListAssets(assetType);
        }
        return list;
    }

    public IAssetDto? GetAsset(Type assetType, Ulid assetId)
    {
        return assetManager.GetAsset(assetType, assetId).Match(asset=>asset, _=>null);
    }
}

public class AssetSelectorModel
{
    public List<Type> AssetTypes;
    public Action<Ulid> OnAssetSelected;
    public List<(Type AssetType, Ulid Id)> FlatAssets = new();
}