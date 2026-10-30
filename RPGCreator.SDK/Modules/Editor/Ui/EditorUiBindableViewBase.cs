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
using RPGCreator.Shared.Types;

namespace RPGCreator.SDK.Modules.Editor.Ui;

public abstract class EditorUiBindableViewBase : IEditorUiBindableView
{
    public URN ViewId { get; }
    protected readonly IEditorUiRouter GlobalRouter;
    protected readonly IEditorUiRouterLocal LocalRouter;
    protected readonly IEditorUiStoreContext Store;
    private readonly Dictionary<StringName, List<BoundChild>> _boundChildren = new();
    private readonly Dictionary<URN, BoundChild> _boundChildIds = new();

    protected EditorUiBindableViewBase(IEditorViewContext viewContext, EditorViewDefinition definition)
    {
        GlobalRouter = viewContext.GlobalRouter;
        LocalRouter = viewContext.LocalRouter;
        ViewId = definition.ViewId;
        ImGuiId = $"###{definition.ViewId}";
        Name = $"{definition.Name}";
        
        Store = viewContext.Store.ForContext(ViewId);
    }

    protected readonly record struct BoundChild
    {
        private readonly Ulid _id;
        
        public readonly URN ViewId;
        public readonly EditorUiPayloadData Payload;

        public BoundChild(URN viewId, EditorUiPayloadData payload)
        {
            _id = Ulid.NewUlid();
            
            ViewId = viewId;
            Payload = payload;
        }

        public override int GetHashCode()
        {
            return _id.GetHashCode();
        }
    }

    public void OnBind(URN childViewId, StringName bindId, EditorUiPayloadData data)
    {
        if (!_boundChildren.TryGetValue(bindId, out var list))
        {
            list = new List<BoundChild>();
            _boundChildren[bindId] = list;
        }
        
        var boundChild = new BoundChild(childViewId, data);
        
        list.Add(boundChild);
        _boundChildIds[boundChild.ViewId] = boundChild;
        OnChildBound(boundChild, bindId);
    }

    public void OnUnbind(URN childViewId, StringName bindId, EditorUiPayloadData data)
    {
        if (_boundChildren.TryGetValue(bindId, out var list))
        {
            if(_boundChildIds.TryGetValue(childViewId, out var boundChild))
            {
                list.Remove(boundChild);
                _boundChildIds.Remove(childViewId);
                OnChildUnbound(boundChild, bindId);
            }
        }
    }

    public virtual void OnActivate(object? args) { }

    public virtual void OnDeactivate() { }
    public bool Disposed { get; private set; }

    protected virtual void OnChildBound(BoundChild child, StringName bindId)
    {
    }

    protected virtual void OnChildUnbound(BoundChild child, StringName bindId)
    {
    }

    protected ReadOnlySpan<BoundChild> GetChildrenFor(StringName bindId)
    {
        return _boundChildren.TryGetValue(bindId, out var list) 
            ? CollectionsMarshal.AsSpan(list) 
            : ReadOnlySpan<BoundChild>.Empty;
    }
    
    public string Name
    {
        get;
        protected set
        {
            if(value.Contains("###"))
                value = value.Split("###")[0];
            
            field = $"{value}{ImGuiId}";
        }
    }

    public string ImGuiId { get; private set; }

    public abstract void Draw(EDrawAs drawAs = EDrawAs.Unknown);
    
    public virtual void Dispose()
    {
        GC.SuppressFinalize(this);
        
        _boundChildren.Clear();
        Disposed = true;
    }
}