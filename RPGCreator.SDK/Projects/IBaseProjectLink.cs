namespace RPGCreator.SDK.Projects;

public interface IBaseProjectLink
{
    Ulid ProjectID { get; }
    string ProjectConfigPath { get; }
}