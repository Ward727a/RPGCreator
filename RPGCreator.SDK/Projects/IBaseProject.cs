using RPGCreator.SDK.Assets.MetaData;
using RPGCreator.SDK.EngineService;

namespace RPGCreator.SDK.Projects;

public interface IBaseProject
{
    ProjectMetaData MetaData { get; }
}