namespace RPGCreator.Core.ModuleSDK.UIModule;

public readonly struct UIRegion : IEquatable<UIRegion>
{
    public string Id { get; init; }
    
    public UIRegion(string id)
    {
        Id = id;
    }
    
    #region Predefined Regions
    
    public static UIRegion EditorMenuBar => new("EditorMenuBar");
    public static UIRegion EditorToolbar => new("EditorToolbar");
    public static UIRegion EditorLeftPanel => new("EditorLeftPanel");
    public static UIRegion EditorLeftPanelBody => new("EditorLeftPanel.body");
    public static UIRegion EditorRightPanel => new("EditorRightPanel");
    public static UIRegion EditorCenterPanel => new("EditorCenterPanel");
    
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