using RPGCreator.Core.Configs;
using RPGCreator.Core.Configs.Helpers;
using RPGCreator.Core.Types.Project;
using RPGCreator.SDK;
using RPGCreator.SDK.Serializer;
using RPGCreator.SDK.Types.Interfaces;
using Serilog;

namespace RPGCreator.Core.Types.Internal;

public class BaseProjectLink : IBaseProjectLink, ISerializable, IDeserializable
{
    public Ulid ProjectID { get; set; } = Ulid.NewUlid();
    public string ProjectConfigPath { get; set; } = string.Empty;

    public BaseProjectLink()
    {
    }
    

    public static BaseProjectLink CreateLinkFromProject(BaseProject project)
    {
        BaseProjectLink link = new BaseProjectLink();
        link.ProjectID = project.Id;
        link.ProjectConfigPath = Path.Combine(project.Path ?? "", $"project.config.xml");
        return link;
    }
    
    public SerializationInfo GetObjectData()
    {
        SerializationInfo info = new SerializationInfo(typeof(BaseProjectLink));
        info.AddValue("id", ProjectID);
        info.AddValue("project_config_path", ProjectConfigPath);
        return info;
    }

    public void SetObjectData(DeserializationInfo info)
    {
        info.TryGetValue("id", out Ulid _ProjectID);
        info.TryGetValue("project_config_path", out string _ProjectConfigPath);
        
        ProjectID = _ProjectID;
        ProjectConfigPath = _ProjectConfigPath;
        
        Log.Debug("[ProjectLink] SetObjectData ({0}, {1})", ProjectID, ProjectConfigPath);
    }
}