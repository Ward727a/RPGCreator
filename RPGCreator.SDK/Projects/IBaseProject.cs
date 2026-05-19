using RPGCreator.SDK.Assets.MetaData;

namespace RPGCreator.SDK.Projects;

public interface IBaseProject
{
    ProjectMetaData MetaData { get; }
}