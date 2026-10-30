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
using RPGCreator.EngineLib.Inputs.Keyboard;
using RPGCreator.EngineLib.Inputs.Mouse;
using RPGCreator.SDK.Inputs;
using RPGCreator.SDK.Services.EngineService;
using RPGCreator.Shared.Attributes;
using RPGCreator.Shared.Enums;
using RPGCreator.Shared.Types;

namespace RPGCreator.EngineLib;

[EngineService(typeof(IInputsService), ServiceLifetime.Singleton, EngineModeFlags.Editor)]
public class EngineInputs(IMouseState? mouseState = null, IKeyboardState? keyboardState = null) : IInputsService
{
    public event Action<IInputSource>? InputSourceChanged;
    private IInputSource? InputSource { get; set; }

    public IMouseState MouseState { get; } = mouseState ?? new EngineMouseState();
    public IKeyboardState KeyboardState { get; } = keyboardState ?? new EngineKeyboardState();

    public bool HasInputSource => InputSource != null;
    public StringName InputSourceName { get; private set; } = StringName.Empty;

    public void SetInputSource(IInputSource inputSource)
    {
        InputSource = inputSource;
        InputSourceName = inputSource.GetInputSourceName();
        InputSourceChanged?.Invoke(inputSource);
        Update();
    }
    
    public bool IsInputSource(IInputSource? inputSource)
    {
        return InputSource == inputSource;
    }

    public void Update()
    {
        MouseState.ResetDeltas();
        if (!HasInputSource) return;
        MouseState.Update(InputSource!.GetRawMouseData());
        KeyboardState.Update(InputSource!.GetRawKeyboardData());
    }
}