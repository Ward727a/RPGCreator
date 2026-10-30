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

using System.Runtime.InteropServices;
using System.Text.Json;
using CommunityToolkit.HighPerformance;
using RPGCreator.SDK.Assets;
using RPGCreator.SDK.AssetsPre;
using RPGCreator.SDK.Common.Logging;
using RPGCreator.SDK.Modules.Editor.Ui;
using RPGCreator.SDK.Serializer;
using RPGCreator.Shared.Attributes;
using RPGCreator.Shared.Types;

namespace RPGCreator.EngineLib.Services.Modules.Features.UiFeatureProvider;

/// <summary>
/// Registry to store and manage editor UI elements.
/// </summary>
[EngineService]
public class EditorUiRegistry(ITypesRegistry typesRegistry, ISerializerService serializerService)
{
    private readonly ScopedLogger _logger = Logger.ForContext<EditorUiRegistry>();

    private readonly Dictionary<URN, Type> _views = new();
    private readonly Dictionary<URN, EditorViewDefinition> _viewDefinitions = new();
    private readonly Dictionary<URN, List<PendingBinding>> _uiBindings = new();

    private readonly Dictionary<URN, Dictionary<StringName, EditorUiDeclareBinding>> _dataTemplates =
        new();

    private readonly Dictionary<URN, Dictionary<StringName, List<EditorUiBinding>>> _childBindingsInWait = new();
    
    private readonly Dictionary<Ulid, EditorUiBinding> _bindingsMap = new();

    public void RegisterView(URN viewId, Type type, EditorViewDefinition definition)
    {
        if (!_views.TryAdd(viewId, type))
        {
            _logger.Warning("View with id {ViewId} already registered", args: viewId);
        }

        _viewDefinitions[viewId] = definition;
    }

    public Result<Type> TryGetView(URN viewId)
    {
        return _views.TryGetValue(viewId, out var type)
            ? Result<Type>.Success(type)
            : Result<Type>.Failure($"View with id {viewId} not found");
    }

    public Result<EditorViewDefinition> TryGetViewDefinition(URN viewId)
    {
        return _viewDefinitions.TryGetValue(viewId, out var definition)
            ? Result<EditorViewDefinition>.Success(definition)
            : Result<EditorViewDefinition>.Failure($"View definition with id {viewId} not found");
    }

    /// <summary>
    /// Registers a binding template for the specified parent view ID. This method associates
    /// the given binding template with a parent view and ensures that any child bindings
    /// waiting for this specific template are attached.
    /// </summary>
    /// <param name="parentViewId">The unique identifier of the parent view to which the binding template will be registered.</param>
    /// <param name="bindingTemplate">The binding template to register. This template defines the structure and identifier of the binding.</param>
    public void RegisterBinding(URN parentViewId, EditorUiDeclareBinding bindingTemplate)
    {
        _dataTemplates.TryGetValue(parentViewId, out var bindingTemplates);
        
        bindingTemplates ??= new Dictionary<StringName, EditorUiDeclareBinding>();
        
        bindingTemplates[bindingTemplate.BindId] = bindingTemplate;
        
        _dataTemplates[parentViewId] = bindingTemplates;

        if (!_childBindingsInWait.TryGetValue(parentViewId, out var childBindings)) return;

        if (!childBindings.TryGetValue(bindingTemplate.BindId, out var pendingChildren)) return;

        foreach (var pendingChild in pendingChildren)
        {
            AttachBinding(pendingChild);
        }
    }

    public void AttachBinding(EditorUiBinding binding)
    {
        ref readonly var childViewId = ref binding.ChildViewId;
        ref readonly var parentViewId = ref binding.TargetViewId;
        ref readonly var bindId = ref binding.TargetBindId;

        if (string.IsNullOrWhiteSpace(parentViewId.ToString()))
        {
            _logger.Warning("Parent view id for binding of {childViewId} is null or empty!", args: childViewId);
        }
        
        if (string.IsNullOrWhiteSpace(bindId.ToString()))
        {
            _logger.Warning("Parent Bind id for binding of {childViewId} is null or empty!", args: childViewId);
        }
     
        binding.BindingId = Ulid.NewUlid();

        if (_dataTemplates.TryGetValue(parentViewId, out var bindingTemplates))
        {
            if (bindingTemplates.TryGetValue(bindId, out var bindingTemplate))
            {
                var payloadResult = ResolvePayload(binding, bindingTemplate);

                var pendingBinding = new PendingBinding(binding.BindingId, childViewId, bindId, payloadResult.Value);
                // In case of success, then we link the binding to the parent
                // Can be retrieved later with the 'GetBindingsFor' method.
                LinkBindingToParent(binding.TargetViewId, pendingBinding);
            }

            return;
        }

        // Otherwise, we put it in wait, so it can be retrieved once the parent that
        // should be linked to this binding has been declared.
        if (_childBindingsInWait.TryGetValue(parentViewId, out var childBindings))
        {
            if (childBindings.TryGetValue(bindId, out var pendingBinding))
            {
                pendingBinding.Add(binding);
            }
            else
            {
                childBindings.Add(bindId, [binding]);
            }
        }
        else
        {
            _childBindingsInWait.Add(parentViewId, new Dictionary<StringName, List<EditorUiBinding>>
            {
                {
                    bindId,
                    [binding]
                }
            });
        }
    }

    public void UnattachBinding(URN viewId, PendingBinding binding)
    {
        var parentId = viewId;
        
        if(_uiBindings.TryGetValue(parentId, out var list))
        {
            list.Remove(binding);
        }
        
    }

    public ReadOnlySpan<PendingBinding> GetBindingsFor(URN parentViewId) =>
        _uiBindings.TryGetValue(parentViewId, out var list)
            ? list.AsSpan()
            : ReadOnlySpan<PendingBinding>.Empty;


    private void LinkBindingToParent(URN parentViewId, PendingBinding pendingBinding)
    {
        if (_uiBindings.TryGetValue(parentViewId, out var list))
        {
            list.Add(pendingBinding);
        }
        else
        {
            _uiBindings.Add(parentViewId, new List<PendingBinding> { pendingBinding });
        }

        if (_bindingsMap.TryGetValue(pendingBinding.BindingId, out var binding))
        {
            if (_childBindingsInWait.TryGetValue(parentViewId, out var childBindings))
            {
                if (childBindings.TryGetValue(pendingBinding.TargetBindId, out var bindings))
                {
                    bindings.Remove(binding);
                }
            }
        }
    }
    
    private Result<EditorUiPayloadData> ResolvePayload(EditorUiBinding childBinding,
        EditorUiDeclareBinding parentTemplate)
    {
        var payload = new EditorUiPayloadData();

        foreach (var template in parentTemplate.DataTemplates)
        {
            
            var isPresent = childBinding.Data.TryGetValue(template.Name, out JsonElement jsonElement);
            
            switch (isPresent)
            {
                case false when template.Optional:
                    continue;
                case false:
                    return Result<EditorUiPayloadData>.Fail(
                        $"Binding invalid : The field '{template.Name}' is required by the parent '{parentTemplate.BindId}'.");
            }

            var targetTypeResult = typesRegistry.GetType(template.Type);

            if (targetTypeResult.IsFailure)
            {
                return Result<EditorUiPayloadData>.Fail(
                    $"Error while deserializing '{template.Name}' : Type '{template.Type}' is not mapped in the engine.");
            }

            var targetType = targetTypeResult.Value;

            try
            {
                var materializedObject = jsonElement.Deserialize(targetType, serializerService.JsonOptions);

                if (materializedObject == null)
                {
                    return Result<EditorUiPayloadData>.Fail(
                        $"Error while deserializing '{template.Name}' : Unable to deserialize to {targetType.Name}.");
                }

                payload.SetData(template.Name, materializedObject);
            }
            catch (Exception ex)
            {
                return Result<EditorUiPayloadData>.Fail(
                    $"Error while deserializing '{template.Name}' : {ex.Message}");
            }
        }

        return Result<EditorUiPayloadData>.Success(payload);
    }
}