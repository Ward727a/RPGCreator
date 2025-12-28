using System.Drawing;

namespace RPGCreator.Core.ModuleSDK.Definition;

public record TagCategory(string Id)
{
    public string Id { get; } = Id;

    public static TagCategory All { get; } = new TagCategory("all");
    public static TagCategory Entity { get; } = new TagCategory("entity");
    public static TagCategory Character { get; } = new TagCategory("character");
}

public class TagDef
{
    public string Id { get; }
    public string DisplayName { get; }
    public string Description { get; }
    public Color Color { get; }
    public List<TagCategory> Category { get; }

    public TagDef(string id, string displayName, string description, Color color, List<TagCategory>? categories = null)
    {
        Id = id;
        DisplayName = displayName;
        Description = description;
        Color = color;
        if (categories is not null && categories.Count > 0)
        {
            Category = categories;
        }
        else
        {
            Category = new List<TagCategory> { TagCategory.All };
        }
    }
}