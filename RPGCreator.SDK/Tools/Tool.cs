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

using System.Collections.Immutable;
using RPGCreator.SDK.Common.Logging;
using RPGCreator.SDK.Inputs;
using RPGCreator.SDK.Tools.ParameterTypes;
using RPGCreator.Shared.Types;
using Point = Microsoft.Xna.Framework.Point;

namespace RPGCreator.SDK.Tools;

public interface IToolParameter
{
    string DisplayName { get; }
    string Description { get; }
    Type ValueType { get; }
}

public interface IToolParameter<T> : IToolParameter
{
    ref T ValueRef { get; } 
}

public interface IToolParameterRanged<T> : IToolParameter<T> where T : struct
{
    T Min { get; }
    T Max { get; }
}

public abstract class Tool
{
    public URN ToolUrn;

    public abstract string DisplayName { get; }
    public virtual HashSet<ToolCapability> Capabilities { get; } = [];
    public virtual string Description { get; } = string.Empty;
    public virtual string Icon { get; } = string.Empty;
    
    public abstract IImmutableList<IToolParameter> GetDefaultParameters();

    /// <summary>
    /// Use the tool at the given absolute position.
    /// </summary>
    /// <param name="logic"></param>
    /// <param name="gridPosition"></param>
    /// <param name="button">The mouse button used to activate the tool. Defaults to MouseButton.Left.</param>
    public abstract void ClickedAt(IToolLogic logic, Point gridPosition, MouseButton button = MouseButton.Left);

    /// <summary>
    /// </summary>
    /// <param name="logic"></param>
    /// <param name="gridPosition"></param>
    /// <param name="button"></param>
    public virtual void HoveredAt(IToolLogic logic, Point gridPosition, MouseButton button = MouseButton.Left)
    {
    }

    public virtual void ReleasedAt(IToolLogic logic, Point gridPosition, MouseButton button = MouseButton.Left)
    {
    }
    
    public virtual void OnActivate() { }
    public virtual void OnDeactivate() { }
}

#if DEBUG
public class MockupTool : Tool
{
    public override string DisplayName => "Mockup Tool";
    public override string Description => "This is a mockup tool used for testing and demonstration purposes.";
    public override string Icon => "mdi-test-tube";
    private IntParameter SizeParameter { get; } = new IntParameter(
        "int parameter",
        "int with 10 default, 1 min, 100 max, step 1.",
        10,
        1,
        100,
        1
    );

    private BoolParameter ShowPreviewParameter { get; } = new BoolParameter(
        "bool parameter",
        "default true",
        true
    );
    
    
    public override IImmutableList<IToolParameter> GetDefaultParameters()
    {
        return [
            SizeParameter,
            ShowPreviewParameter,
        ];
    }

    public override void ClickedAt(IToolLogic logic, Point gridPosition, MouseButton button = MouseButton.Left)
    {
        Logger.Debug("Mockup tool used at position: " + gridPosition);
    }
}
#endif