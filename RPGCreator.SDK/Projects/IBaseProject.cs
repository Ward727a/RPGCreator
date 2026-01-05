using RPGCreator.SDK.Projects;

namespace RPGCreator.SDK.Types.Interfaces;

public interface IBaseProject
{
    Ulid Id { get; }
    string? Name { get; set; }
    string Path { get; set; }
    string Description { get; set; }
    Version? Version { get; set; }
    List<string> Authors { get; }
    List<string> AssetsPackPath { get; }
    ProjectGameData GameData { get; }
    void Save();
}