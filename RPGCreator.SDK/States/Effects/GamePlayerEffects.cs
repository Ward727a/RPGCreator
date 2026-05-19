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

using Fluxor;
using RPGCreator.SDK.States.Actions;

namespace RPGCreator.SDK.States.Effects;

public class GamePlayerEffects
{
    private readonly IState<GamePlayerState> _gameState;

    public GamePlayerEffects(IState<GamePlayerState> gameState)
    {
        _gameState = gameState;
    }
    
    [EffectMethod]
    public Task HandleStartGame(GamePlayerActions.StartGameAction action, IDispatcher dispatcher)
    {
        action.GameProcess.Exited += OnProcessExited;

        try
        {
            action.GameProcess.EnableRaisingEvents = true;

            if (action.GameProcess.HasExited)
            {
                action.GameProcess.Exited -= OnProcessExited;
                action.GameProcess.Dispose();
                dispatcher.Dispatch(new GamePlayerActions.GameExitedAction());
            }
        }
        catch (Exception)
        {
            action.GameProcess.Exited -= OnProcessExited;
            action.GameProcess.Dispose();
            dispatcher.Dispatch(new GamePlayerActions.GameExitedAction());
        }

        return Task.CompletedTask;

        void OnProcessExited(object? sender, EventArgs e)
        {
            action.GameProcess.Exited -= OnProcessExited;
            action.GameProcess.Dispose();
            dispatcher.Dispatch(new GamePlayerActions.GameExitedAction());
        }
    }

    [EffectMethod]
    public Task HandleRequestStopGame(GamePlayerActions.RequestStopGameAction action, IDispatcher dispatcher)
    {
        var process = _gameState.Value.GameProcess;

        if (process is not { HasExited: false }) return Task.CompletedTask;
        
        try
        {
            if (!process.CloseMainWindow())
            {
                process.Kill(entireProcessTree: true);
            }
        }
        catch (Exception)
        {
            // Ignore exceptions here, as the process might have already exited or be in a state where it cannot be closed gracefully.
        }

        return Task.CompletedTask;
    }
}