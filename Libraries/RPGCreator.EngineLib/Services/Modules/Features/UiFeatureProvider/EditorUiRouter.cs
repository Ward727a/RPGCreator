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

using Microsoft.Extensions.DependencyInjection;
using RPGCreator.SDK.Common.Logging;
using RPGCreator.SDK.Modules.Editor.Ui;
using RPGCreator.Shared.Attributes;
using RPGCreator.Shared.Types;

namespace RPGCreator.EngineLib.Services.Modules.Features.UiFeatureProvider;

[EngineService(typeof(IEditorUiRouter))]
public class EditorUiRouter(EditorUiRegistry editorUiRegistry, IServiceProvider serviceProvider) : IEditorUiRouter
{
    private static readonly ScopedLogger Logger = SDK.Common.Logging.Logger.ForContext<EditorUiRouter>();
    private readonly Dictionary<URN, IEditorUiBindableView> _activeInstances = new();

    /// <summary>
    /// Activates a view identified by the specified <paramref name="viewId"/>.
    /// If the view is already active, the existing instance is returned. Otherwise, a new instance is created,
    /// initialized, and registered as active.
    /// </summary>
    /// <param name="viewId">The unique identifier of the view to be activated.</param>
    /// <param name="args">An optional object containing activation parameters specific to the view.</param>
    /// <returns>
    /// A <see cref="Result{IEditorUiBindableView}"/> that contains the activated view instance if successful,
    /// or an error message if the view could not be found or instantiated.
    /// </returns>
    public Result<IEditorUiBindableView> ActivateView(URN viewId, object? args = null)
    {
        if (_activeInstances.TryGetValue(viewId, out var existingView))
        {
            existingView.OnActivate(args);
            return Result<IEditorUiBindableView>.Success(existingView);
        }
        
        string error = "No Error";
        IEditorUiBindableView? viewInstance = null;
        
        editorUiRegistry.TryGetView(viewId).OnSuccess(viewType =>
        {
            var viewDefinitionResult = editorUiRegistry.TryGetViewDefinition(viewId);

            if (viewDefinitionResult.IsFailure)
            {
                error = $"Failed to find view definition for {viewId}";
                return;
            }
            
            var viewDefinition = viewDefinitionResult.Value;
            
            viewInstance = ActivatorUtilities.CreateInstance(serviceProvider, viewType, viewDefinition) as IEditorUiBindableView;

            if (viewInstance == null)
            {
                error = $"Failed to create instance of {viewType}";
                return;
            }
            
            _activeInstances[viewId] = viewInstance;
            
            viewInstance.OnActivate(args);
            
            var bindings = editorUiRegistry.GetBindingsFor(viewId);

            foreach (var binding in bindings)
            {
                viewInstance.OnBind(binding.ChildViewId, binding.TargetBindId, binding.PayloadData);
            }
        }).OnFailure(err=>error = err);
        
        if(viewInstance == null)
        {
            Logger.Error(error);
            return Result<IEditorUiBindableView>.Failure(error);
        }

        return Result<IEditorUiBindableView>.Success(viewInstance);
    }

    public void RefreshBind(URN viewId, StringName bindId)
    {
        GetIfActive(viewId).OnSuccess(view =>
        {
            var bindings = editorUiRegistry.GetBindingsFor(viewId);
            List<PendingBinding> removedBindings = new();
            foreach (var pendingBinding in bindings)
            {
                if(pendingBinding.TargetBindId == bindId)
                {
                    view.OnUnbind(pendingBinding.ChildViewId, bindId, pendingBinding.PayloadData);
                    removedBindings.Add(pendingBinding);
                }
            }
            
            foreach (var removedBinding in removedBindings)
            {
                view.OnBind(removedBinding.ChildViewId, bindId, removedBinding.PayloadData);
            }
        });
    }


    public void ManualBind(URN viewId, StringName bindId, PendingBinding binding)
    {
        GetIfActive(viewId).OnSuccess(view =>
        {
            view.OnBind(binding.ChildViewId, bindId, binding.PayloadData);
        });
    }
    
    public void ManualUnbind(URN viewId, StringName bindId, PendingBinding binding)
    {
        GetIfActive(viewId).OnSuccess(view =>
        {
            view.OnUnbind(binding.ChildViewId, bindId, binding.PayloadData);
        });
    }

    /// <summary>
    /// Removes a view identified by the specified <paramref name="viewId"/> from active instances, disposing of it.
    /// If <paramref name="shouldCascade"/> is true, recursively removes any child views bound to the specified view.
    /// </summary>
    /// <param name="viewId">The unique identifier of the view to be removed.</param>
    /// <param name="shouldCascade">Indicates whether child views bound to the specified view should also be removed.</param>
    /// <returns>
    /// A <see cref="Result"/> indicating whether the operation was successful.
    /// Returns a successful <see cref="Result"/> if the view was removed,
    /// or a failure <see cref="Result"/> if the view could not be found.
    /// </returns>
    public Result DropView(URN viewId, bool shouldCascade = false)
    {
        if (_activeInstances.Remove(viewId, out var existingView))
        {
            existingView.OnDeactivate();
            existingView.Dispose();

            if (shouldCascade)
            {
                var bindings = editorUiRegistry.GetBindingsFor(viewId);
                
                foreach (var binding in bindings)
                {
                    DropView(binding.ChildViewId, shouldCascade);
                }
            }
            
            return Result.Success();
        }
        
        return Result.Failure("No existing view found");
    }

    /// <summary>
    /// Retrieves an active view instance corresponding to the specified <paramref name="viewId"/>.
    /// If the view is currently active, the existing instance is returned. Otherwise, a failure result is returned.
    /// </summary>
    /// <param name="viewId">The unique identifier of the view to retrieve.</param>
    /// <returns>
    /// A <see cref="Result{IEditorUiBindableView}"/> containing the existing active view instance if found,
    /// or a failure message indicating the view is not active.
    /// </returns>
    public Result<IEditorUiBindableView> GetIfActive(URN viewId)
    {
        if (_activeInstances.TryGetValue(viewId, out var existingView))
            return Result<IEditorUiBindableView>.Success(existingView);
        
        return Result<IEditorUiBindableView>.Failure("No existing view found");
    }
}