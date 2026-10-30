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
using Fluxor;
using Microsoft.Extensions.DependencyInjection;
using RPGCreator.SDK.Common.Logging;
using RPGCreator.SDK.Scenes;
using RPGCreator.SDK.Services.EngineService;
using RPGCreator.SDK.States;
using RPGCreator.SDK.States.Actions;
using RPGCreator.Shared.Attributes;
using RPGCreator.Shared.Enums;

namespace RPGCreator.EngineLib.Services;

public readonly struct GameStartOptions(IState<ProjectState> projectState)
{
    public bool IsDebug { get; init; }
    public string SaveFilePath { get; init; }
    public Ulid MapId { get; init; }

    public override string ToString()
    {
        return $"IsDebug: {IsDebug}, SaveFilePath: {SaveFilePath}, MapId: {MapId}";
    }
    
    public string ToExecutableArguments()
    {

        if (!projectState.Value.HasProject)
        {
            Logger.Error("Error: No project is currently loaded. Cannot start the game without a project.");
            return string.Empty;
        }
        
        // For now, all project has a 'project.config.xml' created at the root of the project folder.
        // If in the future we want to allow users to choose where to put the config file, we can add a new property in the project class and use it here.
        string args = $"--project \"{Path.Combine(projectState.Value.Directory, "project.config.json")}\"";
        
        if(!string.IsNullOrEmpty(SaveFilePath) && MapId != Ulid.Empty)
        {
            Logger.Warning("Both SaveFilePath and MapId are set. SaveFilePath will take precedence.");
        }
        
        if (IsDebug)
            args += " --debug";
        if (!string.IsNullOrEmpty(SaveFilePath))
            args += $" --load \"{SaveFilePath}\"";
        if (MapId != Ulid.Empty && string.IsNullOrEmpty(SaveFilePath))
            args += $" --map {MapId}";
        return args;
    }
}

[EngineService(typeof(IGamePlayerService), ServiceLifetime.Singleton, EngineModeFlags.Editor)]
public class GamePlayerService : IGamePlayerService
{
    public IDispatcher Dispatcher { get; }
    private readonly IState<GamePlayerState> _gamePlayerState;
    private readonly IState<ProjectState> _projectState;
    private readonly IState<SceneState> _sceneState;
    private const string PathToDebugExecutable = "./Assets/GamePlayer/Debug/net10.0/RPGCreator.Player.exe";
    private const string PathToReleaseExecutable = "./Assets/GamePlayer/Release/net10.0/RPGCreator.Player.exe";

    public GamePlayerService(
        IState<GamePlayerState> gamePlayerState,
        IState<ProjectState> projectState,
        IState<SceneState> sceneState,
        IDispatcher dispatcher)
    {
        Dispatcher = dispatcher;
        _gamePlayerState = gamePlayerState;
        _projectState = projectState;
        _sceneState = sceneState;
    }
    
    public void CheckForExecutablePath(out bool hasDebug, out bool hasRelease)
    {
        hasDebug = File.Exists(PathToDebugExecutable);
        hasRelease = File.Exists(PathToReleaseExecutable);
    }

    public void StartDebugGame()
    {
        CheckForExecutablePath(out var hasDebug, out _);
        if (!hasDebug)
        {
            Logger.Error("Error: Couldn't find any valid Debug executable to start.");
            return;
        }

        var options = new GameStartOptions(_projectState)
        {
            IsDebug = true,
            MapId = _sceneState.Value.CurrentMapId
        }.ToExecutableArguments();

        LaunchExecutable(PathToDebugExecutable, options, true);
    }

    public void StartGame()
    {
        CheckForExecutablePath(out _, out var hasRelease);
        if (!hasRelease)
        {
            Logger.Error("Error: Couldn't find any valid Release executable to start.");
            return;
        }

        var options = new GameStartOptions(_projectState)
        {
            IsDebug = false,
            MapId = _sceneState.Value.CurrentMapId
        }.ToExecutableArguments();

        LaunchExecutable(PathToReleaseExecutable, options, false);
    }

    public void LoadGame(string saveFilePath)
    {
        CheckForExecutablePath(out var hasDebug, out var hasRelease);
        if(!hasRelease)
        {
            Logger.Error("Error: Couldn't find any valid executable to start.");
            return;
        }

        var exe = PathToReleaseExecutable;
    }

    public void StartGameAt(Ulid mapId)
    {
        CheckForExecutablePath(out var hasDebug, out var hasRelease);
        if(!hasRelease)
        {
            Logger.Error("Error: Couldn't find any valid executable to start.");
            return;
        }

        var exe = PathToReleaseExecutable;
    }

    public void StopGame()
    {
        if (_gamePlayerState.Value.GameProcess == null || _gamePlayerState.Value.GameProcess.HasExited)
        {
            Logger.Warning("No game process is currently running.");
            return;
        }

        try
        {
            Dispatcher.Dispatch(new GamePlayerActions.RequestStopGameAction());
        }
        catch (Exception ex)
        {
            Logger.Error("An error occurred while trying to stop the game process: {message}", args: ex.Message);
        }
    }


    private void LaunchExecutable(string exe, string args, bool debug = false)
    {
        try
        {
            var startInfo = new ProcessStartInfo()
            {
                FileName = exe,
                Arguments = args,
                WorkingDirectory = Path.GetDirectoryName(Path.GetFullPath(exe)),
                UseShellExecute = false,
                CreateNoWindow = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            
            var gameProcess = Process.Start(startInfo);
            
            if(gameProcess is null)
                throw new Exception("Failed to start the game process.");
            
            gameProcess.OutputDataReceived += (s, e) =>
            {
                if (e.Data != null) Logger.Info($"[Game] {e.Data}");
            };
            gameProcess.ErrorDataReceived += (s, e) =>
            {
                if (e.Data != null) Logger.Error($"[Game Error] {e.Data}");
            };

            gameProcess.BeginOutputReadLine();
            gameProcess.BeginErrorReadLine();
            
            Logger.Info($"Game started with PID: {gameProcess.Id}");
            gameProcess.EnableRaisingEvents = true;
            gameProcess.Exited += (sender, e) =>
            {
                Logger.Info(
                    $"Game process with PID {gameProcess?.Id} has exited with code {gameProcess?.ExitCode}.");
            };
            
            
            Dispatcher.Dispatch(new GamePlayerActions.StartGameAction(debug, gameProcess));
        }
        catch (Exception ex)
        {
            Logger.Error("An error occurred while trying to start the game process: {message}", args: ex.Message);
        }
    }
}
