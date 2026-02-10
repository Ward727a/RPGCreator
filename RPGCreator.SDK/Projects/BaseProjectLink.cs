using RPGCreator.SDK.Attributes;
using RPGCreator.SDK.Logging;
using RPGCreator.SDK.Serializer;
using RPGCreator.SDK.Types.Interfaces;

namespace RPGCreator.SDK.Projects;

[SerializingType("BaseProjectLink")]
public class BaseProjectLink : IBaseProjectLink, ISerializable, IDeserializable
{
    public Ulid ProjectID { get; set; } = Ulid.NewUlid();
    public string ProjectConfigPath { get; set; } = string.Empty;

    public BaseProjectLink()
    {
    }
    

    public static BaseProjectLink CreateLinkFromProject(IBaseProject project)
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

    public List<Ulid> GetReferencedAssetIds()
    {
        return [ProjectID];
    }

    public void SetObjectData(DeserializationInfo info)
    {
        info.TryGetValue("id", out Ulid _ProjectID);
        info.TryGetValue("project_config_path", out string _ProjectConfigPath);
        
        ProjectID = _ProjectID;
        ProjectConfigPath = _ProjectConfigPath;
        
        Logger.Debug("[ProjectLink] SetObjectData ({0}, {1})", ProjectID, ProjectConfigPath);
    }
}