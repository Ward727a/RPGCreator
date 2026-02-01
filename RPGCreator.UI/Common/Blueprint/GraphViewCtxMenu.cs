using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using RPGCreator.Core.Types;
using RPGCreator.SDK.Graph;
using RPGCreator.SDK.Graph.Nodes;

namespace RPGCreator.UI.Common.Blueprint;

public sealed class GraphViewCtxMenu : UserControl
{

    public Flyout menu;
    private TreeView TreeView;
    private GraphDocument Doc;
    private Point SpawnNodePos;
    private TreeViewItem? SearchedNode;
    
    private Dictionary<string, object?> paths = new();
    
    public GraphViewCtxMenu(GraphDocument doc) : base()
    {
        Doc = doc;
        menu = new Flyout();
        
        CreateComponents();
        
        // Get the paths to the nodes
        var nodesPaths = GraphNodeRegistry.GetNestedPaths();
        
        LoadPathsRecursively(nodesPaths);

        RegisterEvents();
    }

    private void RegisterEvents()
    {
        
        
    }

    private void CreateComponents()
    {
        menu.Placement = PlacementMode.Pointer;
        menu.ShowMode = FlyoutShowMode.Standard;
        var scrollViewerPanel = new StackPanel
        {
            Orientation = Orientation.Vertical
        };
        
        menu.Content = new Border
        {
            Child = new ScrollViewer
            {
                Content = scrollViewerPanel,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                MaxHeight = 400,
            },
            MinWidth = 250,
        };
        
        Content = menu;
        var TreeSearchBox = new TextBox
        {
            Watermark = "Search...",
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Margin = new Thickness(5),
        };

        void CollapseAll(TreeViewItem treeViewItem)
        {
            treeViewItem.IsVisible = true;
            treeViewItem.IsExpanded = false;
            treeViewItem.IsSelected = false;
            foreach (var item in treeViewItem.Items)
            {
                if (item is TreeViewItem tvi)
                {
                    CollapseAll(tvi);
                }
            }
        }
        
        void IsSearchedFor(TreeViewItem treeViewItem, string searchText)
        {
            if (treeViewItem.Items.Count != 0)
            {
                foreach (var item in treeViewItem.Items)
                {
                    if (item is TreeViewItem tvi)
                    {
                        IsSearchedFor(tvi, searchText);
                    }
                }
                // If any child is visible, make this item visible
                treeViewItem.IsVisible = false;
                foreach (var item in treeViewItem.Items)
                {
                    if (item is TreeViewItem tvi && tvi.IsVisible)
                    {
                        treeViewItem.IsVisible = true;
                        treeViewItem.IsExpanded = true;
                        break;
                    }
                }
            }
            else
            {
                // Leaf node
                treeViewItem.IsVisible = treeViewItem.Header.ToString()!.ToLower().Contains(searchText.ToLower());
                if (treeViewItem.IsVisible)
                {
                    treeViewItem.IsSelected = true;
                    SearchedNode = treeViewItem;
                }
            }
        }
        
        TreeSearchBox.TextChanged += ((_, _) =>
        {
            foreach (var item in TreeView.Items)
            {
                if (item is TreeViewItem tvi)
                {
                    if(string.IsNullOrEmpty(TreeSearchBox.Text))
                    {
                        SearchedNode = null;
                        CollapseAll(tvi);
                        continue;
                    }
                    IsSearchedFor(tvi, TreeSearchBox.Text);
                }
            }
        });
        
        TreeSearchBox.KeyDown += (sender, args) =>
        {
            if (args.Key == Avalonia.Input.Key.Enter)
            {
                // If a node is searched, spawn it
                if (SearchedNode != null)
                {
                    SearchedNode.IsSelected = true;
                    
                    var key = SearchedNode.Header.ToString()!;
                    if (paths.ContainsKey($"{key}_node") && paths[$"{key}_node"] is Node node)
                    {
                        var clone = node.Clone();
                        clone.X = (int)SpawnNodePos.X;
                        clone.Y = (int)SpawnNodePos.Y;
                        Doc.AddNode(clone);
                    }
                }
                
                GlobalStaticUIData.CloseContext();
            }
        };
        
        scrollViewerPanel.Children.Add(TreeSearchBox);
        // Add a TreeView inside the scrollViewerPanel
        TreeView = new TreeView
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch
        };
        scrollViewerPanel.Children.Add(TreeView);
    }
    
    private void LoadPathsRecursively(
        Dictionary<string, object?> paths, 
        TreeViewItem parentItem = null!)
    {
        foreach (var kvp in paths)
        {
            if(kvp.Key == "@hide")
                continue; // Skip the @hide key
            var item = new TreeViewItem { Header = kvp.Key };
            if(parentItem != null)
                parentItem.Items.Add(item);
            else 
                TreeView.Items.Add(item);
            if (kvp.Value is Dictionary<string, object?> subPaths)
            {
                LoadPathsRecursively(subPaths, item);
            } else if (kvp.Value is Node)
            {
                if(kvp.Value is not Node node)
                    return;
                ToolTip.SetTip(item, node.Description);
                item.PointerPressed += (sender, args) =>
                {
                    if (!args.Properties.IsLeftButtonPressed)
                        return;
                    GlobalStaticUIData.CloseContext();
                    // Set the position of the node to the mouse position in the GraphView
                    var clone = node.Clone();
                    // var pos = args.GetPosition(this.GetVisualParent());
                    clone.X = (int)SpawnNodePos.X;
                    clone.Y = (int)SpawnNodePos.Y;
                    Doc.AddNode(clone);
                };
                
                this.paths[$"{kvp.Key}_node"] = kvp.Value;
            }
        }
    }

    public void Open(Control? control, Point spawnNodePos)
    {
        SpawnNodePos = spawnNodePos;
        if(GlobalStaticUIData.CurrentContext != null)
            GlobalStaticUIData.CloseContext();
        GlobalStaticUIData.CurrentContext = menu;
        GlobalStaticUIData.OpenContext(control);
    }
    
}