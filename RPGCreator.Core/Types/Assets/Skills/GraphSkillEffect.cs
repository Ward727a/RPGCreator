using RPGCreator.Core.Parser.Graph;
using RPGCreator.Core.Runtimes;
using RPGCreator.Core.Runtimes.ECS;
using RPGCreator.Core.Types.Blueprint;
using RPGCreator.Core.Types.Internal;
using Serilog;

namespace RPGCreator.Core.Types.Assets.Skills;

/// <summary>
/// Graph-based implementation of a skill effect.
/// Used to allow users to create custom skill effects using the graph visual scripting system.
/// </summary>
public class GraphSkillEffect : ISkillEffect, IHasSavePath, ISerializable, IDeserializable
{
    
    /// <summary>
    /// The pack identifier that this stat belongs to.
    /// </summary>
    public Ulid? PackId { get; set; }
    
    private GraphDocumentCompiled GraphEvent;
    public IReadOnlyList<SkillEffectPropertyDescriptor> PropertyDescriptors { get; private set; }

    public void SetEvent(GraphDocumentCompiled graphEvent)
    {
        GraphEvent = graphEvent;
    }

    public GraphDocumentCompiled GetEvent()
    {
        return GraphEvent;
    }

    public GraphSkillEffect()
    {
    }
    
    public GraphSkillEffect(string name)
    {
        Unique = Ulid.NewUlid();
        Urn = new URN("skill_effect", $"{name}@{Unique}");
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
    
    public Ulid Unique { get; private set; }
    public URN Urn { get; private set; }
    public object Clone()
    {
        var clone = new GraphSkillEffect(DisplayName)
        {
            PropertyDescriptors = PropertyDescriptors,
            Properties = new Dictionary<string, object>(Properties),
            GraphEvent = GraphEvent
        };
        return clone;
    }

    public string DisplayName { get; private set; }
    public Dictionary<string, object> Properties { get; set; }
    public void ApplyEffect(Entity caster, List<Entity> target)
    {
        var env = new GraphEvalEnvironment();
        env.SetVariable("skill_effect.caster", caster);
        env.SetVariable("skill_effect.targets", target);

        foreach (var property in Properties)
        {
            env.SetVariable($"skill_effect.props.{property.Key}", property.Value);
        }

        GetEvent().Run(env);
    }

    public string SavePath { get; set; }

    public void Save()
    {
        if (string.IsNullOrEmpty(SavePath))
        {
            throw new Exception("SavePath is not set for GraphSkillEffect.");
        }
        
        var directory = Path.GetDirectoryName(SavePath);
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory!);
        }

        var graphEventPath = Path.Combine(directory!,
            Path.GetFileNameWithoutExtension(SavePath) + "_graph.xml");
        
        EngineCore.Instance.Serializer.Serialize(this, out var data);
        File.WriteAllText(SavePath, data);
        Log.Information("GraphSkillEffect saved to {SavePath}", SavePath);
    }
    
    public SerializationInfo GetObjectData()
    {
        var graphEventPath = Path.Combine(Path.GetDirectoryName(SavePath),
            Path.GetFileNameWithoutExtension(SavePath) + "_graph.xml");

        // Check if the document path from graph event is still inside the temp folder
        if (GraphEvent.DocumentPath.StartsWith(Path.GetTempPath()))
        {
            // If it is, we need to move, and rename the file to the correct location
            var tempPath = GraphEvent.DocumentPath;
            if (File.Exists(tempPath))
            {
                File.Copy(tempPath, graphEventPath, true);
                File.Delete(tempPath);
            }
        }
        else if (GraphEvent.DocumentPath != graphEventPath)
        {
            // If the document path is different from the expected path, we need to copy it to the correct location
            var currentPath = GraphEvent.DocumentPath;
            if (File.Exists(currentPath))
            {
                File.Copy(currentPath, graphEventPath, true);
            }
        }
        
        return new SerializationInfo(typeof(GraphSkillEffect))
            .AddValue(nameof(Unique), Unique)
            .AddValue(nameof(DisplayName), DisplayName)
            .AddValue(nameof(PropertyDescriptors), PropertyDescriptors.ToList())
            .AddValue(nameof(GraphEvent), graphEventPath);
    }

    public void SetObjectData(Serializer.DeserializationInfo info)
    {
        info.TryGetValue(nameof(Unique), out Ulid unique, Ulid.NewUlid());
        info.TryGetValue(nameof(DisplayName), out string displayName, string.Empty);
        info.TryGetList(nameof(PropertyDescriptors), out List<SkillEffectPropertyDescriptor> propertyDescriptors);
        info.TryGetValue(nameof(GraphEvent), out string graphEventPath, string.Empty);

        // Unique and DisplayName are readonly, so we can't set them directly.
        // We can only set them in the constructor.
        // However, since we need to set them here, we will use reflection to set them.
        Unique = unique;
        Urn = new URN("skill_effect", $"{displayName}@{Unique}");
        DisplayName = displayName;
        SetPropertiesDescriptors(propertyDescriptors);

        // Load the graph event from the path.
        if (!string.IsNullOrEmpty(graphEventPath))
        {
            var graphDocument = GraphDocument.Load(graphEventPath);
            var graphDocumentCompiled = GraphDocumentCompiler.Compile(graphDocument);
            if (graphDocumentCompiled != null)
            {
                GraphEvent = graphDocumentCompiled;
            }
            else
            {
                throw new Exception($"Error while loading GraphSkillEffect.GraphEvent from path: {graphEventPath}");
            }
        }
    }
}