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

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Numerics;
using RPGCreator.SDK.Inputs;
using RPGCreator.SDK.Logging;
using RPGCreator.SDK.Types;

namespace RPGCreator.SDK.GlobalState;

public interface IToolParameter : INotifyPropertyChanged, INotifyPropertyChanging
{
    Type ValueType { get; }
    
    string DisplayName { get; }
    string Description { get; }

    object? Value { get; }
    object? DefaultValue { get; }

    void Reset();
}

public abstract class BaseToolParameter(
    Type valueType,
    string displayName,
    string description,
    object? value,
    object? defaultValue)
    : BaseState, IToolParameter
{
    public Type ValueType { get; protected set; } = valueType;
    public string DisplayName { get; protected set; } = displayName;
    public string Description { get; protected set; } = description;

    public object? Value
    {
        get;
        set
        {
            if (Equals(field, value)) return;
            SetProperty(ref field, value);
        }
    } = value;

    public object? DefaultValue { get; protected set; } = defaultValue;
    
    public T? GetValueAs<T>(out bool success)
    {
        success = Value is T;
        if (Value is T typedValue)
        {
            return typedValue;
        }
        return default;
    }
}

public class RangeParameter(
    string displayName,
    string description,
    double defaultValue,
    double min,
    double max,
    double step)
    : BaseToolParameter(typeof(double), displayName, description, defaultValue, defaultValue)
{
    public double Min { get; protected set; } = min;
    public double Max { get; protected set; } = max;
    public double Step { get; protected set; } = step;

    public override void Reset()
    {
        Value = defaultValue;
    }
}

public class IntParameter(
    string displayName,
    string description,
    int defaultValue,
    int min,
    int max,
    int step)
    : BaseToolParameter(typeof(int), displayName, description, defaultValue, defaultValue)
{
    public int Min { get; protected set; } = min;
    public int Max { get; protected set; } = max;
    public int Step { get; protected set; } = step;

    public override void Reset()
    {
        Value = defaultValue;
    }
}

public class FloatParameter(
    string displayName,
    string description,
    float defaultValue,
    float min,
    float max,
    float step)
    : BaseToolParameter(typeof(float), displayName, description, defaultValue, defaultValue)
{
    public float Min { get; protected set; } = min;
    public float Max { get; protected set; } = max;
    public float Step { get; protected set; } = step;

    public override void Reset()
    {
        Value = defaultValue;
    }
}

public class DoubleParameter(
    string displayName,
    string description,
    double defaultValue,
    double min,
    double max,
    double step)
    : BaseToolParameter(typeof(double), displayName, description, defaultValue, defaultValue)
{
    public double Min { get; protected set; } = min;
    public double Max { get; protected set; } = max;
    public double Step { get; protected set; } = step;

    public override void Reset()
    {
        Value = defaultValue;
    }
}

public class BoolParameter(
    string displayName,
    string description,
    bool defaultValue)
    : BaseToolParameter(typeof(bool), displayName, description, defaultValue, defaultValue)
{
    public override void Reset()
    {
        Value = defaultValue;
    }
}

public class StringParameter(
    string displayName,
    string description,
    string defaultValue)
    : BaseToolParameter(typeof(string), displayName, description, defaultValue, defaultValue)
{
    public override void Reset()
    {
        Value = defaultValue;
    }
}

public interface ICustomObjectParameter
{
    string DisplayName { get; }
    string Description { get; }
    object GetValidUiControl();
}

public abstract class CustomObjectParameter<T>(
    string displayName,
    string description,
    T defaultValue)
    : BaseToolParameter(typeof(T), displayName, description, defaultValue, defaultValue), ICustomObjectParameter
{
    public override void Reset()
    {
        Value = defaultValue;
    }
    
    public abstract object GetValidUiControl();
}

[Flags]
public enum EPayloadType
{
    /// <summary>
    /// Allow the user to select a tile from a tileset.<br/>
    /// If you want to access the intgrid autotiles, use the <see cref="AutoTile"/>!
    /// </summary>
    SimpleTile = 1 << 0,
    /// <summary>
    /// Allow the user to select a tiles group from an intgrid autoTile set.<br/>
    /// If you want to access a specific tile from a tileset, use the <see cref="SimpleTile"/>!
    /// </summary>
    AutoTile = 1 << 1,
    /// <summary>
    /// Allow the user to select either a simple tile or an autotile<br/>
    /// This is just a combination of the <see cref="SimpleTile"/> and <see cref="AutoTile"/> flags, but it's provided for convenience.
    /// </summary>
    AllTiles = SimpleTile | AutoTile,
    /// <summary>
    /// Allow the user to select an object from the object palette.
    /// </summary>
    Object = 1 << 2,
    /// <summary>
    /// Allow the user to select a character from the character DB.
    /// </summary>
    Character = 1 << 3,
    /// <summary>
    /// Allow you to define a custom payload type, which can be used to create a custom UI for the payload selection.<br/>
    /// If you select this option, you must provide a custom UI control for the payload selection by implementing the 
    /// </summary>
    Custom = 1 << 4
}

public abstract class ToolLogic : BaseState
{
    public static URN NoHelp = "rpgc".ToUrnNamespace().ToUrnModule("help").ToUrn("no_help_defined");
    protected static UrnSingleModule ToolUrnModule => "tools".ToUrnSingleModule();
    protected static PipedPath GeneralCategory => "General".ToPipedPath();
    protected static PipedPath MapCategory => "Map".ToPipedPath();
    protected static PipedPath EntityCategory => "Entity".ToPipedPath();
    protected static PipedPath EventCategory => "Event".ToPipedPath();
    
    public abstract URN ToolUrn { get; protected set; }
    public abstract string DisplayName { get; }
    public abstract string Description { get; }
    public abstract string Icon { get; }
    public abstract PipedPath Category { get; }
    public abstract EPayloadType PayloadType { get; }
    public virtual URN HelpKey { get; } = NoHelp;
    
    public object? Payload { get; set => SetProperty(ref field, value); }
    
    public abstract ObservableCollection<IToolParameter> GetParameters();

    /// <summary>
    /// Return a custom payload UI control for this tool.<br/>
    /// This is only useful if the PayloadType is set to <see cref="EPayloadType.Custom"/>, otherwise it will be ignored.<br/>
    /// If you put the PayloadType to Custom, but didn't provide a valid UI control, the editor UI will give an error to the user.
    /// </summary>
    /// <returns>
    /// A valid <b>AVALONIA</b> control!<br/>
    /// If you return null, or any other type than a valid Avalonia control, the editor UI will give an error to the user when they try to use the tool.
    /// </returns>
    // Due to the fact that the SDK should not have a direct reference to Avalonia,
    // we can't use Avalonia.Controls.Control as the return type of this method, otherwise it would create a hard dependency on Avalonia.
    public virtual object? GetCustomPayloadUiControl()
    {
        return null;
    }
    
    /// <summary>
    /// Use the tool at the given absolute position.
    /// </summary>
    /// <param name="absolutePosition">The position in the world coordinates (pixels), not the map coordinates (tiles).</param>
    /// <param name="button">The mouse button used to activate the tool. Defaults to MouseButton.Left.</param>
    public abstract void UseAt(Vector2? absolutePosition = null, MouseButton button = MouseButton.Left);

    /// <summary>
    /// When the tool is active and the user moves the mouse inside the viewport, this method will be called with the current absolute position and the delta position since the last call.
    /// </summary>
    /// <param name="absolutePosition">The current position of the mouse in the world coordinates (pixels), not the map coordinates (tiles).</param>
    /// <param name="deltaPosition">The change in position since the last call, in world coordinates (pixels).</param>
    public virtual void MoveInsideViewport(Vector2 absolutePosition, Vector2 deltaPosition)
    {
    }

    public virtual void OnActivate() { }
    public virtual void OnDeactivate() { }
    
    public Vector2 AbsolutePositionToMapPosition(Vector2 absolutePosition)
    {
        return RuntimeServices.MapService.WorldToMapCoordinates(RuntimeServices.CameraService.ScreenToWorld(absolutePosition));
    }
    
    public override void Reset()
    {
        foreach (var parameter in GetParameters())
        {
            parameter.Reset();
        }
        
        // for safety, we reset the payload after resetting parameters, in case some parameters are used to generate the payload
        Payload = null;
    }
}

#if DEBUG
public class MockupTool : ToolLogic
{
    public override URN ToolUrn { get; protected set; } = ToolUrnModule.ToUrnModule("rpgc").ToUrn("mockup");
    public override string DisplayName => "Mockup Tool";
    public override string Description => "This is a mockup tool used for testing and demonstration purposes.";
    public override string Icon => "mdi-test-tube";
    public override PipedPath Category => GeneralCategory;
    public override EPayloadType PayloadType => EPayloadType.SimpleTile;

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
    
    private StringParameter StringParameter { get; } = new StringParameter(
        "string parameter",
        "default 'Hello World'",
        "Hello World"
    );
    
    private RangeParameter RangeParameter { get; } = new RangeParameter(
        "range parameter",
        "double with 0 default, 0 min, 1 max, step 0.1.",
        0,
        0,
        1,
        0.1
    );
    
    private FloatParameter FloatParameter { get; } = new FloatParameter(
        "float parameter",
        "float with 0 default, 0 min, 1 max, step 0.1.",
        0,
        0,
        1,
        0.1f
    );
    
    private DoubleParameter DoubleParameter { get; } = new DoubleParameter(
        "double parameter",
        "double with 0 default, 0 min, 1 max, step 0.1.",
        0,
        0,
        1,
        0.1
    );
    
    public override ObservableCollection<IToolParameter> GetParameters()
    {
        return [
            SizeParameter,
            ShowPreviewParameter,
            StringParameter,
            RangeParameter,
            FloatParameter,
            DoubleParameter
        ];
    }

    public override void UseAt(Vector2? absolutePosition = null, MouseButton button = MouseButton.Left)
    {
        Logger.Debug("Mockup tool used at position: " + absolutePosition);
    }
}
#endif

public interface IToolState : IState
{
    ToolLogic? ActiveTool { get; set; }
    object? Payload { get; set; }
    ObservableCollection<IToolParameter> ActiveToolParameters { get; }
}

public class BaseToolState : BaseState, IToolState
{
    public ToolLogic? ActiveTool
    {
        get;
        set
        {
            if(field == value) return;
            
            CallPropertyChanging(nameof(ActiveTool));
            CallPropertyChanging(nameof(Payload));
            CallPropertyChanging(nameof(ActiveToolParameters));
            
            if(field != null)
            {
                field.PropertyChanging -= OnActiveToolParametersChanging;
                field.PropertyChanged -= OnActiveToolParametersChanged;
                field.OnDeactivate();
            }
            
            field = value;
            
            if(field != null)
            {
                field.OnActivate();
                field.PropertyChanging += OnActiveToolParametersChanging;
                field.PropertyChanged += OnActiveToolParametersChanged;
            }
            CallPropertyChanged(nameof(ActiveTool));
            CallPropertyChanged(nameof(Payload));
            CallPropertyChanged(nameof(ActiveToolParameters));
            
        }
    }
    
    public object? Payload
    {
        get => ActiveTool?.Payload;
        set
        {
            if (ActiveTool?.Payload == value) return;
            CallPropertyChanging(nameof(Payload));
            ActiveTool?.Payload = value;
            CallPropertyChanged(nameof(Payload));
        }
    }

    public ObservableCollection<IToolParameter> ActiveToolParameters => ActiveTool?.GetParameters() ?? [];
    
    public override void Reset()
    {
        ActiveTool?.Reset();
        ActiveTool = null;
    }
    
    private void OnActiveToolParametersChanging(object? sender, PropertyChangingEventArgs e)
    {
        if (e.PropertyName == nameof(IToolParameter.Value))
        {
            CallPropertyChanging(nameof(ActiveToolParameters));
        }
    }
    
    private void OnActiveToolParametersChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(IToolParameter.Value))
        {
            CallPropertyChanged(nameof(ActiveToolParameters));
        }
    }
}