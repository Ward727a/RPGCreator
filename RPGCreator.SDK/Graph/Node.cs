using RPGCreator.Core.Types.Blueprint;
using RPGCreator.SDK.Logging;
using RPGCreator.SDK.Serializer;

namespace RPGCreator.SDK.Graph;

public abstract class Node : ISerializable, IDeserializable
{
    public Node Clone()
    {
        // Get the type of the current node
        var type = this.GetType();
        // Create a new instance of the node using the Activator
        var clone = (Node)Activator.CreateInstance(type)!;
        return clone;
    }

    public void SetData(GraphDocument.NodeData data)
    {
        Id = data.Id;
        X = data.X;
        Y = data.Y;
        
        List<string> AddedInputs = new();
        
        foreach (var input in Inputs)
        {
            GraphDocument.PortData? inputData = data.Inputs.FirstOrDefault(i => i.Name == input.Name);
            if (inputData.HasValue)
            {
                input.SetData(inputData.Value);
                AddedInputs.Add(input.Name);
            }
            else
            {
                Logger.Warning("Input port {PortName} not found in node data for node {NodeId}.", input.Name, Id);
            }
        }
        // Check if there are any inputs that were not found in the data
        foreach (var input in data.Inputs)
        {
            if (!AddedInputs.Contains(input.Name))
            {
                Logger.Warning("Input port {PortName} not found in node inputs for node {NodeId}.", input.Name, Id);
            }
        }
        
        List<string> AddedOutputs = new();
        foreach (var output in Outputs)
        {
            GraphDocument.PortData? outputData = data.Outputs.FirstOrDefault(o => o.Name == output.Name);
            if (outputData.HasValue)
            {
                output.SetData(outputData.Value);
                AddedOutputs.Add(output.Name);
            }
            else
            {
                Logger.Warning("Output port {PortName} not found in node data for node {NodeId}.", output.Name, Id);
            }
        }
    }
    
    public string Id { get; protected set; } = Ulid.NewUlid().ToString();
    public abstract EGraphOpCode OpCode { get; protected set; }
    public abstract string DisplayName { get; protected set; }
    /// <summary>
    /// A description of the node, used to provide more information about what the node does.<br/>
    /// This description will be displayed in the UI when the user hovers over the node.<br/>
    /// It can be used to provide more information about the node, such as its purpose, how to use it, or any other relevant information.<br/>
    /// If no description is provided, a default description will be used.<br/>
    /// The default description is "No description provided for this node.".<br/>
    /// It is recommended to provide a description for each node to help users understand what the node does and how to use it.<br/>
    /// The description should be concise and to the point, ideally no more than a few sentences.
    /// </summary>
    public virtual string Description { get; protected set; } = "No description provided for this node.";
    /// <summary>
    /// A path to the node, used to identify the node in the graph.<br/>
    /// It will also be used for the menu that allow to add the node to the graph.<br/>
    /// Each node can have the same path, each "section" should be separated by a '|' character.
    /// </summary>
    public abstract string Path { get; protected set; }
    public double X, Y;
    public readonly List<Port> Inputs = new();
    public readonly List<Port> Outputs = new();

    protected IReadOnlyCollection<Port> GetOutputs(string name)
    {
        return Outputs.Where(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase)).ToList().AsReadOnly();
    }
    
    protected Port TryGetOuput(string name)
    {
        return Outputs.FirstOrDefault(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase)) ?? throw new KeyNotFoundException($"Output port with name '{name}' not found.");
    }
    
    protected IReadOnlyCollection<Port> GetInputs(string name)
    {
        return Inputs.Where(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase)).ToList().AsReadOnly();
    }
    
    protected Port TryGetInput(string name)
    {
        return Inputs.FirstOrDefault(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase)) ?? throw new KeyNotFoundException($"Input port with name '{name}' not found.");
    }
    
    /// <summary>
    /// A properties dictionary that will be used to store the properties of the node.<br/>
    /// The properties here are used to store the inputs/outputs of the node to be used in the compiled code.<br/>
    /// These properties can be edited by the user in the UI, or set by other nodes.<br/>
    /// Please, note that even a small change in how the properties are made can lead to a complete crash of the engine.
    /// </summary>
    public Dictionary<string, object?> Properties { get; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// This method is called when the node is compiled to generate the properties that will be used in the compiled code.<br/>
    /// It should be overridden in derived classes to add specific properties.<br/>
    /// The properties are the inputs, for exemple if we have a node "Add" type: "op_math_add" that have 2 inputs "First input" and "Second input", the properties will be:
    /// <code>
    /// {
    ///     "A": 0, // First input, named "A" because this is what the type "op.math.add" expects
    ///     "B": 0 // Second input, named "B" because this is what the type "op.math.add" expects
    /// }
    /// </code>
    /// To be certains of what properties are expected, you can check the ENodeOpCode enum, it contains the expected properties for each node type.
    /// </summary>
    public abstract IEnumerable<GraphInstr> Emit(GraphDocument graph, GraphCompileContext context);

    public SerializationInfo GetObjectData()
    {
        
        var info = new SerializationInfo(GetType())
            .AddValue(nameof(Id), Id)
            .AddValue(nameof(OpCode), OpCode)
            .AddValue(nameof(DisplayName), DisplayName)
            .AddValue(nameof(Description), Description)
            .AddValue(nameof(Path), Path)
            .AddValue(nameof(X), X)
            .AddValue(nameof(Y), Y)
            .AddValue(nameof(Inputs), Inputs)
            .AddValue(nameof(Outputs), Outputs)
            .AddValue(nameof(Properties), Properties);
        
        return info;
    }

    public List<Ulid> GetReferencedAssetIds()
    {
        List<Ulid> referencedIds = new List<Ulid>();
        foreach (var property in Properties.Values)
        {
            if (property is Ulid ulidValue)
            {
                referencedIds.Add(ulidValue);
            }
            else if (property is IEnumerable<Ulid> ulidEnumerable)
            {
                referencedIds.AddRange(ulidEnumerable);
            }
        }
        return referencedIds;
    }

    public void SetObjectData(DeserializationInfo info)
    {
        info.TryGetValue(nameof(Id), out var id, string.Empty);
        Id = id;
        info.TryGetValue(nameof(OpCode), out EGraphOpCode opCode, EGraphOpCode.none);
        OpCode = opCode;
        info.TryGetValue(nameof(DisplayName), out var displayName, string.Empty);
        DisplayName = displayName;
        info.TryGetValue(nameof(Description), out var description, string.Empty);
        Description = description;
        info.TryGetValue(nameof(Path), out var path, string.Empty);
        Path = path;
        info.TryGetValue(nameof(X), out double x, 0D);
        X = x;
        info.TryGetValue(nameof(Y), out double y, 0D);
        Y = y;
        
        Inputs.Clear();
        Outputs.Clear();
        
        info.TryGetList(nameof(Inputs), out List<Port> inputs);
        Inputs.AddRange(inputs);
        
        info.TryGetList(nameof(Outputs), out List<Port> outputs);
        Outputs.AddRange(outputs);
        
        info.TryGetDictionary(nameof(Properties), out Dictionary<string, object?>? properties);
        Properties.Clear();
        foreach (var kvp in properties)
            Properties[kvp.Key] = kvp.Value;
    }
}