namespace RPGCreator.Core.Types.Editor;

public class IconMeta
{
    public string Id { get; set; }
    public string BaseIconId { get; set; }
    public string Name { get; set; }
    public string IconValue => $"mdi-{Name}";
    public string Codepoint { get; set; }
    public List<string> Aliases { get; set; }
    public List<string> Styles { get; set; }
    public string Version { get; set; }
    public bool Deprecated { get; set; }
    public List<string> Tags { get; set; }
    public string Author { get; set; }
}