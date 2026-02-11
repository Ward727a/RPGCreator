using RPGCreator.SDK.Attributes;
using RPGCreator.SDK.ECS.Entities;
using RPGCreator.SDK.Graph;
using RPGCreator.SDK.Logging;
using RPGCreator.SDK.Serializer;
using RPGCreator.SDK.Types;
using RPGCreator.SDK.Types.Internals;

namespace RPGCreator.SDK.Assets.Definitions.Skills;

/// <summary>
/// Graph-based implementation of a skill effect.
/// Used to allow users to create custom skill effects using the graph visual scripting system.
/// </summary>
[SerializingType("GraphSkillEffect")]
public class GraphSkillEffect : BaseAssetDef, ISkillEffect, IHasSavePath, ISerializable, IDeserializable
{
    
    public override UrnSingleModule UrnModule => "graph_skill_effect".ToUrnSingleModule();
    /// <summary>
    /// The pack identifier that this stat belongs to.
    /// </summary>
    public Ulid? PackId { get; set; }
    
    private IGraphScript _graphEvent = null!;
    public IReadOnlyList<SkillEffectPropertyDescriptor> PropertyDescriptors { get; private set; } = null!;

    public void SetEvent(IGraphScript graphEvent)
    {
        _graphEvent = graphEvent;
    }

    public IGraphScript GetEvent()
    {
        return _graphEvent;
    }

    public GraphSkillEffect()
    {
    }
    
    public GraphSkillEffect(string name)
    {
        Unique = Ulid.NewUlid();
        DisplayName = name;
        Properties = new Dictionary<string, object>();
    }

    public void SetPropertiesDescriptors(List<SkillEffectPropertyDescriptor> descriptors)
    {
        PropertyDescriptors = descriptors;
        
        if(Properties == null)
            Properties = new Dictionary<string, object>();
        
        // From the descriptors, we can determine what properties we need to have in the Properties dictionary.
        // We will initialize the Properties dictionary with the default values from the descriptors.
        foreach (var descriptor in descriptors)
        {
            if (!Properties.ContainsKey(descriptor.Name))
            {
                Properties[descriptor.Name] = descriptor.DefaultValue.GetType() == typeof(object)? descriptor.Type.GetDefaultValue() : descriptor.DefaultValue;
            }
        }
        
    }
    
    public object Clone()
    {
        var clone = new GraphSkillEffect(DisplayName)
        {
            PropertyDescriptors = PropertyDescriptors,
            Properties = new Dictionary<string, object>(Properties),
            _graphEvent = _graphEvent
        };
        return clone;
    }

    public string DisplayName { get; private set; } = null!;
    public Dictionary<string, object> Properties { get; set; } = null!;

    public void ApplyEffect(Entity caster, List<Entity> target)
    {
        var env = EngineServices.GraphService.CreateEnvironment();
        env.SetVariable("skill_effect.caster", caster);
        env.SetVariable("skill_effect.targets", target);

        foreach (var property in Properties)
        {
            env.SetVariable($"skill_effect.props.{property.Key}", property.Value);
        }

        EngineServices.GraphService.Run(GetEvent(), env);
    }

    public string SavePath { get; set; } = null!;

    // public void Save()
    // {
    //     if (string.IsNullOrEmpty(SavePath))
    //     {
    //         throw new Exception("SavePath is not set for GraphSkillEffect.");
    //     }
    //     
    //     var directory = Path.GetDirectoryName(SavePath);
    //     if (!Directory.Exists(directory))
    //     {
    //         Directory.CreateDirectory(directory!);
    //     }
    //
    //     var graphEventPath = Path.Combine(directory!,
    //         Path.GetFileNameWithoutExtension(SavePath) + "_graph.xml");
    //     
    //     EngineCore.Instance.Serializer.Serialize(this, out var data);
    //     File.WriteAllText(SavePath, data);
    //     Log.Information("GraphSkillEffect saved to {SavePath}", SavePath);
    // }
    
    public SerializationInfo GetObjectData()
    {
        var graphEventPath = _graphEvent.DocumentPath;

        // // Check if the document path from graph event is still inside the temp folder
        // if (_graphEvent.DocumentPath.StartsWith(Path.GetTempPath()))
        // {
        //     // If it is, we need to move, and rename the file to the correct location
        //     var tempPath = _graphEvent.DocumentPath;
        //     if (File.Exists(tempPath))
        //     {
        //         File.Copy(tempPath, graphEventPath, true);
        //         File.Delete(tempPath);
        //     }
        // }
        // else if (_graphEvent.DocumentPath != graphEventPath)
        // {
        //     // If the document path is different from the expected path, we need to copy it to the correct location
        //     var currentPath = _graphEvent.DocumentPath;
        //     if (File.Exists(currentPath))
        //     {
        //         File.Copy(currentPath, graphEventPath, true);
        //     }
        // }
        
        return new SerializationInfo(typeof(GraphSkillEffect))
            .AddValue(nameof(Unique), Unique)
            .AddValue(nameof(DisplayName), DisplayName)
            .AddValue(nameof(PropertyDescriptors), PropertyDescriptors.ToList())
            .AddValue(nameof(_graphEvent), graphEventPath);
    }

    public List<Ulid> GetReferencedAssetIds()
    {
        return [];
    }

    public void SetObjectData(DeserializationInfo info)
    {
        info.TryGetValue(nameof(Unique), out Ulid unique, Ulid.NewUlid());
        info.TryGetValue(nameof(DisplayName), out string displayName, string.Empty);
        info.TryGetList(nameof(PropertyDescriptors), out List<SkillEffectPropertyDescriptor> propertyDescriptors);
        info.TryGetValue(nameof(_graphEvent), out string graphEventPath, string.Empty);

        // Unique and DisplayName are readonly, so we can't set them directly.
        // We can only set them in the constructor.
        // However, since we need to set them here, we will use reflection to set them.
        Unique = unique;
        DisplayName = displayName;
        SetPropertiesDescriptors(propertyDescriptors);

        // Load the graph event from the path.
        if (!string.IsNullOrEmpty(graphEventPath))
        {
            if (EngineServices.GraphService.TryLoadScript(graphEventPath, out var script))
            {
                _graphEvent = script;
            }
            else
            {
                Logger.Error("Failed to load graph script for GraphSkillEffect '{DisplayName}' from path '{GraphEventPath}'",
                    DisplayName, graphEventPath);
            }
        }
    }
}