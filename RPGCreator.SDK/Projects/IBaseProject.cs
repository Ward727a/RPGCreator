using RPGCreator.SDK.EngineService;

namespace RPGCreator.SDK.Projects;

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
    IGlobalPathData GlobalPathData { get; }
    void Save();
}