namespace RPGCreator.SDK.Types.Interfaces;

public interface IBaseProjectLink
{
    Ulid ProjectID { get; }
    string ProjectConfigPath { get; }
}