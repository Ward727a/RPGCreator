using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Layout;
using RPGCreator.SDK.Graph;
using RPGCreator.SDK.Graph.Nodes;

namespace RPGCreator.UI.Common.Blueprint.LeftBar;

public class GraphLeftBarNodesList : UserControl
{

    private Action<Node>? AddNode = null;
    
    private TreeView menu;
    
    public GraphLeftBarNodesList(Action<Node> addNode)
    {
        AddNode = addNode;
        CreateComponents();
        RegisterEvents();
    }
    
    private void CreateComponents()
    {
        menu = new TreeView()
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch
        };
        
        // Get the paths to the nodes
        var nodesPaths = GraphNodeRegistry.GetNestedPaths();
        // Add nodes categories and nodes here as TreeViewItems
        LoadPathsRecursively(nodesPaths);
        
        Content = menu;
    }
    
    private void LoadPathsRecursively(
        Dictionary<string, object?> paths, 
        TreeViewItem parentItem = null!)
    {
        foreach (var kvp in paths)
        {
            if(kvp.Key == "@hide")
                continue; // Skip the @hide key
            var item = new TreeViewItem() { Header = kvp.Key };
            if(parentItem != null)
                parentItem.Items.Add(item);
            else 
                menu.Items.Add(item);
            if (kvp.Value is Dictionary<string, object?> subPaths)
            {
                LoadPathsRecursively(subPaths, item);
            } else if (kvp.Value is Node)
            {
                item.PointerPressed += (sender, args) =>
                {
                    if (!args.Properties.IsLeftButtonPressed)
                        return;
                    var node = kvp.Value as Node;
                    if (node != null)
                    {
                        AddNode?.Invoke(node.Clone());
                    }
                };
            }
        }
    }
    
    private void RegisterEvents()
    {
    }

}