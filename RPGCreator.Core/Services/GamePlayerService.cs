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
using RPGCreator.SDK;
using RPGCreator.SDK.EngineService;
using RPGCreator.SDK.GlobalState;
using RPGCreator.SDK.Logging;

namespace RPGCreator.Core.Services;

public readonly struct GameStartOptions
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

        if (GlobalStates.ProjectState.CurrentProject == null)
        {
            Logger.Error("Error: No project is currently loaded. Cannot start the game without a project.");
            return string.Empty;
        }
        
        // For now, all project has a 'project.config.xml' created at the root of the project folder.
        // If in the future we want to allow users to choose where to put the config file, we can add a new property in the project class and use it here.
        string args = $"--project \"{Path.Combine(GlobalStates.ProjectState.CurrentProject.Path, "project.config.xml")}\"";
        
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

public class GamePlayerService : IGamePlayerService
{
    private const string PathToDebugExecutable = "./Assets/GamePlayer/Debug/net10.0/RPGCreator.Player.exe";
    private const string PathToReleaseExecutable = "./Assets/GamePlayer/Release/net10.0/RPGCreator.Player.exe";

    private readonly GamePlayerState _gamePlayerState;

    public GamePlayerService()
    {
        _gamePlayerState = new GamePlayerState();
        GlobalStates.GamePlayerState = _gamePlayerState;
    }
    
    public void CheckForExecutablePath(out bool hasDebug, out bool hasRelease)
    {
        hasDebug = File.Exists(PathToDebugExecutable);
        hasRelease = File.Exists(PathToReleaseExecutable);
    }

    public void StartGame()
    {
        CheckForExecutablePath(out var hasDebug, out var hasRelease);
        if(!hasRelease)
        {
            Logger.Error("Error: Couldn't find any valid executable to start.");
            return;
        }

        var exe = PathToReleaseExecutable;
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

    public void StartDebugGame()
    {
        CheckForExecutablePath(out var hasDebug, out var hasRelease);
        if(!hasDebug)
        {
            Logger.Error("Error: Couldn't find any valid executable to start.");
            return;
        }

        var options = new GameStartOptions()
        {
            IsDebug = true
        }.ToExecutableArguments();
        
        LaunchExecutable(PathToDebugExecutable, options);
        _gamePlayerState.IsDebugging = true;
    }

    public void StopGame()
    {
        if (_gamePlayerState.GameProcess == null || _gamePlayerState.GameProcess.HasExited)
        {
            Logger.Warning("No game process is currently running.");
            return;
        }

        try
        {
            _gamePlayerState.GameProcess.Kill(entireProcessTree: true);
            Logger.Info($"Game process with PID {_gamePlayerState.GameProcess.Id} has been terminated.");
        }
        catch (Exception ex)
        {
            Logger.Error("An error occurred while trying to stop the game process: {message}", args: ex.Message);
        }
    }


    private void LaunchExecutable(string exe, string args)
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
            
            _gamePlayerState.GameProcess = Process.Start(startInfo);
            _gamePlayerState.GameProcess.OutputDataReceived += (s, e) => { if(e.Data != null) Logger.Info($"[Game] {e.Data}"); };
            _gamePlayerState.GameProcess.ErrorDataReceived += (s, e) => { if(e.Data != null) Logger.Error($"[Game Error] {e.Data}"); };

            _gamePlayerState.GameProcess.BeginOutputReadLine();
            _gamePlayerState.GameProcess.BeginErrorReadLine();
            if (_gamePlayerState.GameProcess == null)
            {
                Logger.Error("Failed to start the game process. Process.Start returned null.");
            }
            else
            {
                Logger.Info($"Game started with PID: {_gamePlayerState.GameProcess.Id}");
                _gamePlayerState.GameProcess.EnableRaisingEvents = true;
                _gamePlayerState.GameProcess.Exited += (sender, e) =>
                {
                    Logger.Info($"Game process with PID {_gamePlayerState.GameProcess?.Id} has exited with code {_gamePlayerState.GameProcess?.ExitCode}.");
                    _gamePlayerState.Reset();
                };
                _gamePlayerState.IsPlaying = true;
            }
        } catch (Exception ex)
        {
            Logger.Error("An error occurred while trying to start the game process: {message}", args: ex.Message);
        }
    }
}

public class GamePlayerState : BaseState, IGamePlayerState
{
    public override void Reset()
    {
        IsPlaying = false;
        IsDebugging = false;
        GameProcess = null;
    }

    public bool IsPlaying
    {
        get;
        internal set => SetProperty(ref field, value);
    }

    public bool IsDebugging
    {
        get;
        internal set => SetProperty(ref field, value);
    }

    public Process? GameProcess
    {
        get;
        internal set => SetProperty(ref field, value);
    }
}