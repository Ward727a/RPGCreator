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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;
using RPGCreator.SDK;
using RPGCreator.SDK.Common.Logging;
using RPGCreator.SDK.Graph.LOGIC;
using RPGCreator.UI.Blueprints;

namespace RPGCreator.UI.Content.Blueprint;

public class TreeViewNodeItemModel
{
    public string Name { get; set; }
    public List<TreeViewNodeItemModel> Children { get; set; } = new List<TreeViewNodeItemModel>();
    public INodeLogic? Node { get; set; }
    public Dictionary<string, int> ChildrenIndex { get; set; } = new Dictionary<string, int>();
}

public class BlueprintListVm : INotifyPropertyChanged
{
    private NodeRegistry _nodeRegistry;

    public bool IsLoading
    {
        get;
        private set
        {
            if (field == value) return;
            field = value;
            OnPropertyChanged();
        }
    }

    public bool StillLoadingChunk
    {
        get;
        private set
        {
            if (field == value) return;
            field = value;
            OnPropertyChanged();
        }
    }

    public TreeViewNodeItemModel? SelectedItem
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            OnPropertyChanged();
        }
    } = null;

    public GenericNodeViewModel? PreviewedNode
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            OnPropertyChanged();
        }
    }

    public bool HasPreviewedNode
    {
        get;
        private set
        {
            if (field == value) return;
            field = value;
            OnPropertyChanged();
        }
    }

    public string SearchQuery
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            OnPropertyChanged();
        }
    } = string.Empty;

    public RelayCommand SearchCommand { get; private set; }

    public ObservableCollection<TreeViewNodeItemModel> NodeItems { get; private set; } = new ObservableCollection<TreeViewNodeItemModel>();
    public BlueprintListVm(NodeRegistry? nodeRegistry)
    {
        if (nodeRegistry == null && RegistryServices.BpNodes is NodeRegistry _nodsRegistry)
            nodeRegistry = _nodsRegistry;
        _nodeRegistry = nodeRegistry ?? throw new Exception("Node registry cannot be null");
     
        SearchCommand = new RelayCommand(Search);
        
        IsLoading = true;
        _ = ExplodeNodesListToTreeView(_nodeRegistry.Nodes);
    }

    private void Search()
    {
        _ = ExplodeNodesListToTreeView(_nodeRegistry.Nodes);
    }
    
    private async Task ExplodeNodesListToTreeView(IEnumerable<INodeLogic> nodes)
    {
        NodeItems.Clear();
        
        List<TreeViewNodeItemModel> internalList = new List<TreeViewNodeItemModel>();
        Dictionary<string, TreeViewNodeItemModel> rootDict = new Dictionary<string, TreeViewNodeItemModel>();

        bool IsSearching = !string.IsNullOrWhiteSpace(SearchQuery);
        
        await Task.Run(() =>
        {
            foreach (var nodeLogic in nodes)
            {
                TreeViewNodeItemModel? nodeItem;

                var segments = nodeLogic.Category.Segments;

                if (segments.IsEmpty)
                    continue;

                if (nodeLogic.Category.StartsWith("@Hidden"))
                    continue;

                TreeViewNodeItemModel? currentParent = null;

                for (int i = 0; i < segments.Length; i++)
                {
                    string segmentName = segments[i];

                    if (i == 0)
                    {
                        if (!rootDict.TryGetValue(segmentName, out currentParent))
                        {
                            currentParent = new TreeViewNodeItemModel { Name = segmentName };
                            rootDict.Add(segmentName, currentParent);
                        }
                    }
                    else
                    {
                        if (currentParent == null)
                        {
                            Logger.Error("Error while exploding node: No parent found for node {node}",
                                nodeLogic.Title);
                            continue;
                        }

                        if (!currentParent.ChildrenIndex.TryGetValue(segmentName, out var index))
                        {
                            var newNode = new TreeViewNodeItemModel { Name = segmentName };

                            currentParent.Children.Add(newNode);
                            index = currentParent.Children.Count - 1;
                            currentParent.ChildrenIndex.Add(segmentName, index);
                        }

                        currentParent = currentParent.Children[index];
                    }
                }

                if (currentParent == null)
                {
                    Logger.Error("Error while trying to set parent: No parent found for node {node}", nodeLogic.Title);
                    continue;
                }

                if(IsSearching && !nodeLogic.Title.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase))
                    continue;

                currentParent.Children.Add(new TreeViewNodeItemModel
                {
                    Name = nodeLogic.Title,
                    Node = nodeLogic
                });
            }

            internalList = rootDict.Values.OrderBy(x => x.Name).ToList();
        });

        foreach (var chunk in internalList.Chunk(20))
        {
            Dispatcher.UIThread.Post(() =>
            {
                foreach (var item in chunk)
                {
                    NodeItems.Add(item);
                }
            });
            await Task.Delay(50);
            IsLoading = false;
            StillLoadingChunk = true;
        }
        StillLoadingChunk = false;
    }

    private void SelectedItemChanged()
    {
        if(SelectedItem != null && SelectedItem.Node != null)
            PreviewedNode = _nodeRegistry.GetNodeViewModel(SelectedItem.Node.Urn);
        else
            PreviewedNode = null;
        
        HasPreviewedNode = PreviewedNode != null;
    }
    
    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        if (propertyName == nameof(SelectedItem))
        {
            SelectedItemChanged();
        }
    }

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}