namespace RPGCreator.SDK.Types.Interfaces;

public interface IBaseProject
{
    Ulid Id { get; }
    string? Name { get; set; }
    string? Path { get; set; }
    Version? Version { get; set; }
    List<string> Authors { get; }
    List<string> AssetsPackPath { get; }
    void Save();
}