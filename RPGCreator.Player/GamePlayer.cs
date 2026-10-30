using System;
using System.IO;
using System.Linq;
using Fluxor;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RPGCreator.EngineLib;
using RPGCreator.SDK.Assets.Map;
using RPGCreator.SDK.Common.Logging;
using RPGCreator.SDK.Resources;
using RPGCreator.SDK.Scenes;
using RPGCreator.SDK.Services.Assets;
using RPGCreator.SDK.Services.EngineService;
using RPGCreator.SDK.Services.RuntimeService;
using RPGCreator.Shared.Enums;

namespace RPGCreator.Player;

public partial class GamePlayer : Game, IGameRunner
{
    private readonly PlayerLaunchArgs _launchArgs;
    private SpriteBatch _spriteBatch = null!;
    private EngineHost _engineHost = null!;
    private IServiceProvider _provider = null!;
    private ISceneManager _sceneManager = null!;

    // Implemented only in Debug builds (see DebugUI/GamePlayer.Debug.cs).
    partial void InitializeDebugTools();
    partial void UpdateDebugTools(GameTime gameTime);
    partial void DrawDebugTools(GameTime gameTime);
    partial void DisposeDebugTools();

    public GamePlayer(PlayerLaunchArgs launchArgs)
    {
        _launchArgs = launchArgs;

        var graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";

        graphics.PreferredBackBufferWidth = 1280;
        graphics.PreferredBackBufferHeight = 720;
        graphics.PreferMultiSampling = true;

        IsMouseVisible = true;
        Window.AllowUserResizing = true;
    }

    protected override void Initialize()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        _engineHost = EngineHost.Build(
            game: this,
            spriteBatch: _spriteBatch,
            engineMode: EngineModeFlags.Player,
            configureCustomServices: services =>
            {
                services.AddSingleton(this);
                services.AddSingleton<IGameRunner>(_ => this);
                services.AddSingleton(_launchArgs);
            });

        _provider = _engineHost.Services;
        _sceneManager = _engineHost.SceneManager;

        InitializeDebugTools();

        base.Initialize();
    }

    protected override void LoadContent()
    {
        LoadProjectAndStartScene();
        base.LoadContent();
    }

    private void LoadProjectAndStartScene()
    {
        if (string.IsNullOrWhiteSpace(_launchArgs.ProjectConfigPath) || !File.Exists(_launchArgs.ProjectConfigPath))
        {
            Logger.Error("No valid project path provided (--project). Path: '{0}'", _launchArgs.ProjectConfigPath);
            return;
        }

        var projectsManager = _provider.GetRequiredService<IProjectsManager>();
        var resourceManager = _provider.GetRequiredService<IResourceManager>();
        var assetManager = _provider.GetRequiredService<IAssetManager>();
        var dispatcher = _provider.GetRequiredService<IDispatcher>();

        if (!projectsManager.TryGetProject(_launchArgs.ProjectConfigPath, out var project) || project == null)
        {
            Logger.Error("Failed to load project from '{0}'.", _launchArgs.ProjectConfigPath);
            return;
        }

        projectsManager.OpenProject(project);
        resourceManager.LoadAllResourcesFromDisk();

        Window.Title = string.IsNullOrWhiteSpace(project.MetaData.Name) ? "RPG Creator Player" : project.MetaData.Name;

        // Resolve target map: either from --map argument, or the first map in the project
        Ulid targetMapId = _launchArgs.MapId;
        if (targetMapId == Ulid.Empty)
        {
            targetMapId = assetManager.ListAssets<MapDefinition>().FirstOrDefault();
        }

        if (targetMapId != Ulid.Empty)
        {
            Logger.Info("Loading initial scene MapId: {0}", targetMapId);
            _sceneManager.LoadSceneAsync(targetMapId);
        }
        else
        {
            Logger.Warning("No map found to load in project '{0}'.", project.MetaData.Name);
        }
    }

    public void UpdateKeyboard()
    {
    }

    protected override void Update(GameTime gameTime)
    {
        _sceneManager.Simulation.Update(gameTime.ElapsedGameTime.TotalSeconds);
        UpdateDebugTools(gameTime);
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.SetRenderTarget(null);
        GraphicsDevice.Clear(Color.Black);

        _sceneManager.DrawScene(gameTime.ElapsedGameTime.TotalSeconds);

        DrawDebugTools(gameTime);

        base.Draw(gameTime);
    }

    protected override void OnExiting(object sender, EventArgs args)
    {
        DisposeDebugTools();
        _sceneManager?.UnloadScene();
        (_provider?.GetService<IResourceManager>() as IDisposable)?.Dispose();
        base.OnExiting(sender, args);
    }
}