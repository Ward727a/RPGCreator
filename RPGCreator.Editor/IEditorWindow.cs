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

using Hexa.NET.ImGui;
using RPGCreator.SDK.Modules.Editor.Ui;
using RPGCreator.Shared.Types;

namespace RPGCreator.Editor;

public interface IEditorUiWindow
{
    public string Name { get; }
    public URN ViewId { get; }
    bool IsOpen { get; }
    void Open();
    void Close();
    void Draw(EDrawAs drawAs = EDrawAs.Unknown);
}

public abstract class ComplexEditorUiWindowBase<TController>(
    IEditorViewContext viewContext,
    EditorViewDefinition definition,
    TController controller)
    : ComplexEditorUi<TController>(viewContext, definition, controller), IEditorUiWindow where TController : class
{
    public bool IsOpen { get => IsOpenInternal; private set => IsOpenInternal = value; }
    protected bool IsOpenInternal = false;

    public void Open()
    {
        IsOpen = true;
        OnOpened();
    }

    public void Close()
    {
        IsOpen = false;
        OnClosed();
    }

    protected virtual void OnOpened(){}
    protected virtual void OnClosed(){}
}

public abstract class SimpleEditorUiWindow(IEditorViewContext viewContext, EditorViewDefinition definition)
    : SimpleEditorUi(viewContext, definition), IEditorUiWindow
{
    public bool IsOpen { get => IsOpenInternal; private set => IsOpenInternal = value; }
    protected bool IsOpenInternal = false;

    public void Open()
    {
        IsOpen = true;
        OnOpened();
    }

    public void Close()
    {
        IsOpen = false;
        OnClosed();
    }
    
    public virtual void OnOpened(){}
    public virtual void OnClosed(){}
}