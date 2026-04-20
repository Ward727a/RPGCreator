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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using AvaloniaEdit.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MonoGame.Extended.Collections;
using RPGCreator.SDK.Assets.Definitions.Blueprints;
using RPGCreator.SDK.Registry;
using RPGCreator.SDK.Types;

namespace RPGCreator.UI.Common.Modal.Browser;

public partial class BlueprintBrowserVm : ObservableObject
{
    private readonly Func<BlueprintData, bool>? _matchingFunc;
    private readonly IBlueprintRegistry _registry;
    
    [ObservableProperty] private string _searchContent;
    
    [ObservableProperty] private BlueprintData _selectedBlueprint;
    [ObservableProperty] private int _selectedIndex;
    
    public ObservableCollection<BlueprintData> ItemsList { get; private set; } = [];
    public StringName ForTag { get; private set; } = StringName.Empty;
    
    public BlueprintBrowserVm(IBlueprintRegistry registry, Func<BlueprintData, bool>? MatchingFunc = null)
    {
        _matchingFunc = MatchingFunc;
        _registry = registry;
        RefreshItems();
    }

    public void SetForTag(StringName tag)
    {
        ForTag = tag;
        RefreshItems();
    }
    
    private void RefreshItems()
    {
        var blueprintDatas = _registry.GetAllBlueprints().Where(bp => (bp.HasTag(ForTag).IsSuccess || ForTag == StringName.Empty) && _matchingFunc?.Invoke(bp) == true);
        
        ItemsList.Clear();
        ItemsList.AddRange(blueprintDatas);
        OnPropertyChanged(nameof(ItemsList));
    }

    [RelayCommand]
    private void SearchFor(string searchContent)
    {
        
    }
}