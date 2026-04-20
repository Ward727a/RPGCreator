// RPG Creator - Open-source RPG Engine.
// (c) 2026 Ward
// 
// This file is part of RPG Creator and is distributed under the Apache 2.0 License.
// You are free to use, modify, and distribute this file under the terms of the Apache 2.0 License.
// See LICENSE for details.
// 
// ---
// 
// Ce fichier fait partie de RPG Creator et est distribué sous licence Apache 2.0.
// Vous êtes libre de l'utiliser, de le modifier et de le distribuer sous les termes de la licence Apache 2.0.
// Voir LICENSE pour plus de détails.
// 
// Contact:
// => Mail: Ward727a@gmail.com
//    Please use this object: "RPG Creator [YourObject]"
// => Discord: ward727
// 
// For urgent inquiries, sending both an email and a message on Discord is highly recommended for a quicker response.

using System.CodeDom.Compiler;
using RPGCreator.SDK.Modules.Definition;
using RPGCreator.SDK.Types;

namespace RPGCreator.SDK.Graph.LOGIC;

public abstract class BaseNodeLogic : INodeLogic
{
    public static UrnSingleModule DefaultUrnModule = "bp_nodes".ToUrnSingleModule();
    
    public static PipedPath DefaultCategory = "Default".ToPipedPath();
    public static PipedPath HiddenCategory = "@Hidden".ToPipedPath();
    public static PipedPath SpecialCategory = "@Special".ToPipedPath();

    public abstract FlowType FlowType { get; }
    public Ulid RuntimeId { get; set; }
    public virtual IReadOnlyList<IConnectorLogic> Inputs { get; set; } = [];
    public virtual IReadOnlyList<IConnectorLogic> Outputs { get; set; } = [];
    public abstract string Title { get; }
    public abstract PipedPath Category { get; }
    public abstract URN Urn { get; }
    public virtual URN HelpUrn => URN.Empty;
    public CustomData CustomData { get; set; } = new CustomData();
    public abstract void GenerateCode(CodeContext context, IndentedTextWriter writer);

    public virtual void PlacedInGraph(object? nodeEditorViewModel)
    {
        
    }
    
    public INodeLogic Clone()
    {
        var clone = (INodeLogic)MemberwiseClone();
        clone.Outputs = Outputs.Select(o => o.Clone()).ToList().AsReadOnly();
        clone.Inputs = Inputs.Select(i => i.Clone()).ToList().AsReadOnly();
        return clone;
    }

    public string GetConnectorValue(CodeContext context, IConnectorLogic connector)
    {
        if (connector.RuntimeHasParent)
        {
            context.GetVariableName(connector.RuntimeParentConnector, out var value);
            return value;
        }
        return connector.GetStringValue();
    }
}