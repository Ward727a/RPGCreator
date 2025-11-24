using RPGCreator.Core.Configs;
using RPGCreator.Core.Configs.Helpers;
using RPGCreator.Core.Types.Project;

namespace RPGCreator.Core.Types.Internal;

public class BaseProjectLink : ISerializable, IDeserializable
{
    public Ulid ProjectID = Ulid.NewUlid();
    public string ProjectConfigPath = string.Empty;

    public BaseProjectLink()
    {
    }
    
    public bool TryGetProject(out BaseProject? project)
    {
        project = null;
        if (File.Exists(ProjectConfigPath))
        {
            EngineSerializer.Instance.Deserialize<BaseProject>(File.ReadAllText(ProjectConfigPath), out var _projectObject, out System.Type? objectType);

            if (objectType == null)
                return false;
            
            if(objectType == typeof(BaseProject))
                project = (BaseProject)_projectObject;
            else if (objectType.IsSubclassOf(typeof(BaseProject)))
                project = (BaseProject)_projectObject;
            else
                return false;
            
            return true;
        }
        return false;
    }

    public static BaseProjectLink CreateLinkFromProject(BaseProject project)
    {
        BaseProjectLink link = new BaseProjectLink();
        link.ProjectID = project.Id;
        link.ProjectConfigPath = Path.Combine(project.Path ?? "", $"project.config.xml");
        return link;
    }
    //
    // public bool TrySave()
    // {
    //     var projectsConf = EngineCore.Instance.Configs.GetConfig<ProjectsConf>("projects");
    // }
    public SerializationInfo GetObjectData()
    {
        SerializationInfo info = new SerializationInfo(typeof(BaseProjectLink));
        info.AddValue("id", ProjectID);
        info.AddValue("project_config_path", ProjectConfigPath);
        return info;
    }

    public void SetObjectData(Serializer.DeserializationInfo info)
    {
        info.TryGetValue("id", out ProjectID);
        info.TryGetValue("ProjectConfigPath", out ProjectConfigPath);
    }
}