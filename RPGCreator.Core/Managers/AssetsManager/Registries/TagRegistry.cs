using RPGCreator.Core.ModuleSDK.Definition;

namespace RPGCreator.Core.Managers.AssetsManager.Registries;

public class TagRegistry
{

    public static TagRegistry Instance { get; } = new TagRegistry();

    private TagRegistry()
    {
    }

    private readonly Dictionary<string, TagDef> _registeredTagDefs = new();
    
    public void RegisterTag(TagDef tag)
    {
        _registeredTagDefs.TryAdd(tag.Id, tag);
    }
    
    public IEnumerable<TagDef> GetTags()
    {
        return _registeredTagDefs.Values;
    }
    
    public bool IsKnown(string tagId) => _registeredTagDefs.ContainsKey(tagId);
}