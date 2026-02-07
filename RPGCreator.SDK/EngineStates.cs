using System.ComponentModel;
using System.Numerics;
using RPGCreator.SDK.Assets.Definitions.Maps;
using RPGCreator.SDK.Assets.Definitions.Maps.Layers;
using RPGCreator.SDK.Assets.Definitions.Tilesets;
using RPGCreator.SDK.Editor.Brushes;
using RPGCreator.SDK.Inputs;
using RPGCreator.SDK.Projects;
using RPGCreator.SDK.Types.Interfaces;

namespace RPGCreator.SDK;

public enum BrushMode
{
    Tiling,
    Entities
}

public interface IBrushState : INotifyPropertyChanged
{
    IBrushInfo? CurrentBrush { get; set; }
    BrushMode CurrentMode { get; set; }
    object? CurrentObjectToPaint { get; set; }
    bool IsPlacing { get; set; }
    bool IsDrawing { get; set; }
    Vector2 LastDrawAt { get; set; }
}

public interface IEditorState : INotifyPropertyChanged
{
    bool InEditorMode { get; set; }
    bool InPlacingMode { get; set; }
    bool InDrawingMode { get; set; }
    bool ShowCollisionLayer { get; set; }
    bool ShowEntityLayer { get; set; }
    MapDefinition? CurrentMap { get; set; }
    BaseLayerDef? CurrentLayer { get; set; }
    ITileDef? CurrentTile { get; set; }
}

public interface IProjectState : INotifyPropertyChanged
{
    IBaseProject? CurrentProject { get; set; }
}

/// <summary>
/// The global state of the engine.
/// This contains information about the application and the current editor/project state.
/// </summary>
public static class EngineStates
{
    /// <summary>
    /// The name of the application.
    /// </summary>
    public static string ApplicationName => "RPG Creator";
    /// <summary>
    /// The current version of the engine.
    /// </summary>
    public static Version ApplicationVersion => new Version(0, 1, 0);
    public static IEditorState EditorState = null!;
    public static IProjectState ProjectState = null!;
    public static IBrushState BrushState = null!;
    public static IMouseState MouseState = null!;
    public static IKeyboardState KeyboardState = null!;
    public static IMouseState ViewportMouseState = null!;
    public static IKeyboardState ViewportKeyboardState = null!;
}