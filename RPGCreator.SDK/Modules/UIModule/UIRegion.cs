namespace RPGCreator.SDK.Modules.UIModule;

public readonly struct UIRegion : IEquatable<UIRegion>
{
    public string Id { get; init; }
    
    public UIRegion(string id)
    {
        Id = id;
    }
    
    #region Predefined Regions
    
    // EDITOR
    public static UIRegion EditorMenuBar => new("EditorMenuBar");
    public static UIRegion EditorToolbar => new("EditorToolbar");
    public static UIRegion EditorLeftPanel => new("EditorLeftPanel");
    public static UIRegion EditorLeftPanelComponents => new("EditorLeftPanel.Components");
    public static UIRegion EditorLeftPanelNonePanel => new("EditorLeftPanel.NonePanel");
    public static UIRegion EditorLeftPanelTilingPanel => new("EditorLeftPanel.TilingPanel");
    public static UIRegion EditorLeftPanelTilingPanelTilesetItem => new("EditorLeftPanel.TilingPanel.TilesetItem");
    public static UIRegion EditorLeftPanelEntitiesPanel => new("EditorLeftPanel.EntitiesPanel");
    public static UIRegion EditorRightPanel => new("EditorRightPanel");
    public static UIRegion EditorCenterPanel => new("EditorCenterPanel");
    
    // ASSETS MANAGER
    public static UIRegion AssetsManager => new("AssetsManager");
    public static UIRegion AssetsManagerMenu => new("AssetsManager.Menu");
    
    // AUTO LAYER EDITOR
    public static UIRegion AutoLayerEditor => new("AutoLayerEditor");
    public static UIRegion AutoLayerEditorIntRefList => new("AutoLayerEditor.IntRefList");
    public static UIRegion AutoLayerEditorIntRefListItem => new("AutoLayerEditor.IntRefList.Item");
    public static UIRegion AutoLayerEditorIntRefListMenu => new("AutoLayerEditor.IntRefList.Menu");
    public static UIRegion AutoLayerEditorIntRefListCreateModal => new("AutoLayerEditor.IntRefList.CreateModal");
    
    #endregion

    public override string ToString()
    {
        return Id;
    }

    public bool Equals(UIRegion other)
    {
        return Id == other.Id;
    }

    public override bool Equals(object? obj)
    {
        return obj is UIRegion other && Equals(other);
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }
    
    public static bool operator ==(UIRegion left, UIRegion right) => left.Equals(right);
    public static bool operator !=(UIRegion left, UIRegion right) => !left.Equals(right);
    
    public static implicit operator UIRegion(string id) => new(id);
}