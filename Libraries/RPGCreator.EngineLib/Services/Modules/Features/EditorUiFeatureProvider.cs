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

using System.Diagnostics;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using RPGCreator.EngineLib.Services.Modules.Features.UiFeatureProvider;
using RPGCreator.SDK.Common.Logging;
using RPGCreator.SDK.Modules.Editor.Ui;
using RPGCreator.SDK.Serializer;
using RPGCreator.SDK.Services.ModuleServices;
using RPGCreator.Shared.Attributes;
using RPGCreator.Shared.Types;

namespace RPGCreator.EngineLib.Services.Modules.Features;

[EngineService]
public class EditorUiFeatureProvider(ISerializerService serializerService, EditorUiRegistry uiRegistry) : IModuleFeatureProcessor
{
    private readonly ScopedLogger _logger = Logger.ForContext<EditorUiFeatureProvider>();
    
    public StringName FeatureKey => new("editor_ui");

    public bool CanBeEnabled(SDK.Modules.Module module) =>
        Directory.Exists(Path.Combine(module.ModulePath, "Views"));

    public void EnableFeatureForModule(SDK.Modules.Module module)
    {
        List<string> errors = new();
        foreach (var jsonFile in GetJsonFiles(module))
        {
            if (serializerService.DeserializeFrom<EditorUiFeatureDto>(jsonFile, out var uiFeatureDto))
            {
                GetTargetClassType(module, uiFeatureDto, jsonFile).OnSuccess(targetType =>
                {

                    uiRegistry.RegisterView(uiFeatureDto.ViewId, targetType, new EditorViewDefinition(uiFeatureDto.ViewId, uiFeatureDto.Name));

                    foreach (var binding in uiFeatureDto.Binding)
                    {
                        uiRegistry.AttachBinding(binding);
                    }

                    foreach (var editorUiDeclareBinding in uiFeatureDto.DeclareBindings)
                    {
                        uiRegistry.RegisterBinding(uiFeatureDto.ViewId, editorUiDeclareBinding);
                    }
                });
            }
        }
    }

    internal void ManualEnableFeature(Assembly asm, string directory)
    {
        int found = 0;
        int count = 0;

        (int totalBinding, int registeredBinding) bindings = (0, 0);
        (int totalDeclareBinding, int registeredDeclareBinding) declareBindings = (0, 0);
        foreach (var jsonFile in Directory.GetFiles(directory, "*.json"))
        {
            found++;
            if (serializerService.DeserializeFrom<EditorUiFeatureDto>(jsonFile, out var uiFeatureDto))
            {
                GetTargetClassType(asm, uiFeatureDto, jsonFile)
                    .OnSuccess(targetType =>
                    {
                        count++;
                        uiRegistry.RegisterView(uiFeatureDto.ViewId, targetType, new EditorViewDefinition(uiFeatureDto.ViewId, uiFeatureDto.Name));

                        bindings.totalBinding += uiFeatureDto.Binding.Length;
                        foreach (var binding in uiFeatureDto.Binding)
                        {
                            binding.ChildViewId = uiFeatureDto.ViewId;
                            uiRegistry.AttachBinding(binding);
                            bindings.registeredBinding++;
                        }

                        declareBindings.totalDeclareBinding += uiFeatureDto.DeclareBindings.Length;
                        foreach (var editorUiDeclareBinding in uiFeatureDto.DeclareBindings)
                        {
                            uiRegistry.RegisterBinding(uiFeatureDto.ViewId, editorUiDeclareBinding);
                            declareBindings.registeredDeclareBinding++;
                        }
                    });
            }
        }
        
        _logger.Info("Found {Found} feature files and successfully registered {Count} views.", args: [found, count]);
        _logger.Info("Found {totalBinding} bindings and {registeredBinding} bindings were registered.", args: [bindings.totalBinding, bindings.registeredBinding]);
        _logger.Info("Found {totalDeclareBinding} declare bindings and {registeredDeclareBinding} declare bindings were registered.", args: [declareBindings.totalDeclareBinding, declareBindings.registeredDeclareBinding]);
    }

    public void DisableFeatureForModule(SDK.Modules.Module module)
    {
        throw new NotImplementedException();
    }
    
    #region Helpers
    
    private static string[] GetJsonFiles(SDK.Modules.Module module) =>
        Directory.GetFiles(Path.Combine(module.ModulePath, "editor_ui"), "*.json");
    
    
    private Result<Type> GetTargetClassType(SDK.Modules.Module module, EditorUiFeatureDto dto, string jsonFile)
    {
        if (string.IsNullOrWhiteSpace(dto.TargetClass))
        {
            _logger.Error("TargetClass is empty or missing in feature file '{File}'.", args: jsonFile);
            return Result.Fail();
        }

        var moduleAssembly = module.ModuleType?.Assembly;
        if (moduleAssembly == null)
        {
            _logger.Error("Module '{ModuleUrn}' has no loaded assembly.", args: module.Manifest.ModuleUrn);
            return Result.Fail();
        }
        
        return GetTargetClassType(moduleAssembly, dto, jsonFile);
    }

    private Result<Type> GetTargetClassType(Assembly asm, EditorUiFeatureDto dto, string jsonFile)
    {
        var targetType = asm.GetType(dto.TargetClass, false, false);
        
        if (targetType == null)
        {
            Debug.Assert(asm.FullName != null);
            
            _logger.Error("Target class '{TargetClass}' declared in '{File}' was not found in assembly '{Assembly}'.",
                args: [dto.TargetClass, jsonFile, asm.FullName]);
            return Result.Fail();
        }

        if (!typeof(IEditorUi).IsAssignableFrom(targetType) || targetType.IsAbstract || targetType.IsInterface)
        {
            _logger.Error("Target class '{TargetClass}' declared in '{File}' must be a non-abstract class implementing '{Interface}'.",
                args: [dto.TargetClass, jsonFile, nameof(IEditorUi)]);
            return Result.Fail();
        }

        return targetType;
    }
    
    #endregion
}

public sealed class EditorUiFeatureDto
{
    /// <summary>
    /// Unique identifier for the view.
    /// </summary>
    [JsonInclude]
    public URN ViewId = URN.Empty;

    /// <summary>
    /// The target class for the view.
    /// </summary>
    [JsonInclude]
    public string TargetClass = "";
    
    /// <summary>
    /// Default name for the view.
    /// </summary>
    [JsonInclude]
    public string Name = "";
    
    /// <summary>
    /// Bind the view to another parent view, allowing to transfer data between views.
    /// </summary>
    [JsonPropertyName("Binders")]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    [JsonInclude]
    public EditorUiBinding[] Binding { get; init; } = [];
    
    /// <summary>
    /// Declare bindings for the view. Allowing other views to bind to this view as children.
    /// </summary>
    [JsonPropertyName("DeclareBinders")]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    [JsonInclude]
    public EditorUiDeclareBinding[] DeclareBindings { get; init; } = [];
}

public sealed class EditorUiBinding
{
    /// <summary>
    /// The child view id for the binding.
    /// </summary>
    public URN ChildViewId = URN.Empty;
    
    /// <summary>
    /// The target view for the binding.
    /// </summary>
    [JsonInclude]
    public URN TargetViewId = URN.Empty;
    
    /// <summary>
    /// The target bind for the binding.
    /// </summary>
    [JsonInclude]
    public StringName TargetBindId = "";
    
    /// <summary>
    /// The data payload for the binding.
    /// </summary>
    [JsonInclude]
    public Dictionary<string, JsonElement> Data { get; init; } = new();
    
    [JsonIgnore]
    public Ulid BindingId = Ulid.Empty;
}

public sealed class EditorUiDeclareBinding
{
    public record struct DataTemplateElement(string Name, URN Type, bool Optional);
    
    /// <summary>
    /// The bind id for the binding.
    /// </summary>
    [JsonInclude]
    public StringName BindId = "";
    
    /// <summary>
    /// The data templates for the binding.
    /// </summary>
    [JsonInclude]
    public DataTemplateElement[] DataTemplates = [];
}