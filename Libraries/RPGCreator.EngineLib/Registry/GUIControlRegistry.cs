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

using RPGCreator.SDK.GameUI.Controls;
using RPGCreator.SDK.Registry;
using RPGCreator.SDK.Types;
using RPGCreator.Shared.Types;

namespace RPGCreator.EngineLib.Registry;

/// <summary>
/// Gui control registry.<br/>
/// This class is used to register and unregister GUI controls.<br/>
/// GUI Controls are used to create the UI of the <b>Game</b> (not the editor!).<br/>
/// This allows the user to be able to use those controls in the game UI editor.
/// </summary>
public class GUIControlRegistry : IGUIControlRegistry
{
    private readonly Dictionary<URN, BaseControl> _controls = new();

    public event Action<BaseControl>? ControlRegistered;
    public event Action<BaseControl>? ControlUnregistered;

    public int ControlCount => _controls.Count;
    public IEnumerable<BaseControl> Controls => _controls.Values;

    public void RegisterControl(BaseControl control, bool overwriteIfExists = false)
    {
        if (_controls.ContainsKey(control.Urn) && !overwriteIfExists)
        {
            return;
        }

        _controls[control.Urn] = control;
        ControlRegistered?.Invoke(control);
    }

    public void UnregisterControl(URN controlUrn)
    {
        if (_controls.Remove(controlUrn, out var control))
        {
            ControlUnregistered?.Invoke(control);
        }
    }

    public BaseControl? GetControl(URN controlUrn)
    {
        return _controls.GetValueOrDefault(controlUrn);
    }

    public bool TryGetControl(URN controlUrn, out BaseControl? control)
    {
        return _controls.TryGetValue(controlUrn, out control);
    }

    public bool HasControl(URN controlUrn)
    {
        return _controls.ContainsKey(controlUrn);
    }

    public void ClearControls()
    {
        _controls.Clear();
    }
}