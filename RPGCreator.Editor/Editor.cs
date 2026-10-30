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

using System;
using System.IO;
using CommunityToolkit.Diagnostics;
using Hexa.NET.ImGui;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RPGCreator.Editor.ImGuiLib;
using RPGCreator.Editor.Services;
using RPGCreator.Editor.UI.Controllers.Widgets;
using RPGCreator.EngineLib;
using RPGCreator.EngineLib.Services;
using RPGCreator.EngineLib.Services.Modules.Features;
using RPGCreator.EngineLib.Services.Modules.Features.UiFeatureProvider;
using RPGCreator.RuntimeLib.FNA;
using RPGCreator.SDK;
using RPGCreator.SDK.Common.Logging;
using RPGCreator.SDK.Modules.Editor.Ui;
using RPGCreator.SDK.Resources;
using RPGCreator.SDK.Scenes;
using RPGCreator.Shared.Enums;
using SDL3;
using Color = Microsoft.Xna.Framework.Color;
using Vector2 = System.Numerics.Vector2;

namespace RPGCreator.Editor;

public class Editor : Game
{
    private ImGuiRenderer _imGuiRenderer;
    private RenderTargetManager _renderTargetManager;
    private EngineHost _engineHost;

    private IServiceProvider? _provider;
    private ISceneManager _sceneManager;

    internal ImTextureRef RpgCreatorLogoImGui;
    
    private SDL.SDL_EventFilter _sdlEventFilter;
    private SpriteBatch _spriteBatch;

    public Editor()
    {
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
        _imGuiRenderer = new ImGuiRenderer(this);
        _renderTargetManager = new RenderTargetManager(GraphicsDevice);
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        _engineHost = EngineHost.Build(this, _spriteBatch, EngineModeFlags.Editor, (services) =>
        {
            services.AddSingleton<Game, Editor>(_ => this);
            services.AddSingleton(_imGuiRenderer);
            services.AddSingleton(_renderTargetManager);
        });
        
        _provider = _engineHost.Services;
        _sceneManager = _engineHost.SceneManager;
        
        EditorWindowManager.Init(_provider.GetRequiredService<IMediator>());
        
        ImConfig.Initialize(_provider, _imGuiRenderer, this);
        
        ImGui.CreateContext();
        ImGuiIOPtr io = ImGui.GetIO();
        
        ImAnim.Initialize(); // Initialize the base ImAnimPool<T>
        
        SDL.SDL_StartTextInput(Window.Handle);

        io.ConfigFlags |= ImGuiConfigFlags.DockingEnable; 
        io.ConfigFlags |= ImGuiConfigFlags.ViewportsEnable;
        
        unsafe
        {
            _sdlEventFilter = ImGui_SDLEventFilter;
        }
        
        _provider.GetRequiredService<EditorUiFeatureProvider>().ManualEnableFeature(typeof(Editor).Assembly, Path.Combine(RpgEnv.Path.ExecutableFolder, "Content", "Views"));
        
        _sceneManager = _provider.GetRequiredService<ISceneManager>();

        SDL.SDL_AddEventWatch(_sdlEventFilter, IntPtr.Zero);
        base.Initialize();
    }
    private unsafe bool ImGui_SDLEventFilter(nint userdata, SDL.SDL_Event* e)
    {
        ImGuiIOPtr io = ImGui.GetIO();

        switch (e->type)
        {
            case (uint)SDL.SDL_EventType.SDL_EVENT_MOUSE_MOTION:
                io.AddMousePosEvent(e->motion.x, e->motion.y);
                break;

            case (uint)SDL.SDL_EventType.SDL_EVENT_MOUSE_BUTTON_DOWN:
            case (uint)SDL.SDL_EventType.SDL_EVENT_MOUSE_BUTTON_UP:
            {
                int button = 0;
                if (e->button.button == (byte)SDL.SDL_MouseButtonFlags.SDL_BUTTON_LMASK) button = 0;
                else if (e->button.button == (byte)3) button = 1;
                else if (e->button.button == (byte)SDL.SDL_MouseButtonFlags.SDL_BUTTON_MMASK) button = 2;

                bool isDown = e->type == (uint)SDL.SDL_EventType.SDL_EVENT_MOUSE_BUTTON_DOWN;
                io.AddMouseButtonEvent(button, isDown);
                break;
            }
            
            case (uint)SDL.SDL_EventType.SDL_EVENT_KEY_DOWN:
            case (uint)SDL.SDL_EventType.SDL_EVENT_KEY_UP:
            {
                bool isDown = e->key.down;
                SDL.SDL_Keycode keycode = (SDL.SDL_Keycode)e->key.key;

                io.AddKeyEvent(ImGuiKey.ModCtrl, (e->key.mod & SDL.SDL_Keymod.SDL_KMOD_CTRL) != 0);
                io.AddKeyEvent(ImGuiKey.ModShift, (e->key.mod & SDL.SDL_Keymod.SDL_KMOD_SHIFT) != 0);
                io.AddKeyEvent(ImGuiKey.ModAlt, (e->key.mod & SDL.SDL_Keymod.SDL_KMOD_ALT) != 0);
                io.AddKeyEvent(ImGuiKey.ModSuper, (e->key.mod & SDL.SDL_Keymod.SDL_KMOD_GUI) != 0);
                io.AddKeyEvent(ImGui.SDLKeyToImGuiKey(keycode), isDown);
                break;
            }
            case (uint)SDL.SDL_EventType.SDL_EVENT_TEXT_INPUT:
            {
                io.AddInputCharactersUTF8(e->text.text);
                break;
            }

            case (uint)SDL.SDL_EventType.SDL_EVENT_MOUSE_WHEEL:
                io.AddMouseWheelEvent(e->wheel.x, e->wheel.y);
                break;
            case (uint)SDL.SDL_EventType.SDL_EVENT_WINDOW_RESIZED:
            {

                var x = e->window.data1;
                var y = e->window.data2;
                io.DisplaySize = new Vector2(x, y);
                
                break;
            }
            case (uint)SDL.SDL_EventType.SDL_EVENT_WINDOW_SHOWN:
            {
                var windowId = e->window.windowID;
                var windowPtr = SDL.SDL_GetWindowFromID(windowId);
                if (SDL.SDL_GetWindowSize(windowPtr, out int width, out int height))
                {
                    io.DisplaySize = new Vector2(width, height);
                }
                else
                {
                    throw new Exception("Failed to get window size");
                }

                break;
            }
        }

        return true;
    }

    protected override void OnExiting(object sender, EventArgs args)
    {
        SDL.SDL_RemoveEventWatch(_sdlEventFilter, IntPtr.Zero);
        ImGuiIOPtr io = ImGui.GetIO();
        io.ClearEventsQueue();

        _provider?.GetService<ISceneManager>()?.SaveSceneAsync();
        (_provider?.GetService<IEditorUiStore>() as EditorUiStore)?.SaveToDisk();
        (_provider?.GetService<IThumbnailCacheService>() as ThumbnailCacheService)?.Dispose();
        (_provider?.GetService<IResourceManager>() as ResourceManager)?.Dispose();
        
        _imGuiRenderer.Dispose();
        _renderTargetManager.Dispose();
        
        GC.Collect();
        GC.WaitForPendingFinalizers();
        
        base.OnExiting(sender, args);
        
        // So, normally, **this** should not be used like that.
        // But I tried to fix the deadlock for 3 days, without founding what is causing it.
        // So... Well, this work.
        // Is it dirty? Yes. But it works ¯\_(ツ)_/¯
        SDL.SDL_DestroyWindow(this.Window.Handle);
        SDL.SDL_Quit();
    }

    protected override void LoadContent()
    {
        Guard.IsNotNull(_provider);

        var router = _provider.GetRequiredService<IEditorUiRouter>();

        router.ActivateView("rpgc://views/engine_launcher").OnSuccess(view =>
        {
            if (view is IEditorUiWindow viewWindow)
            {
                EditorWindowManager.Open(viewWindow);
            }
        }).OnFailure(err =>
        {
            Logger.Error("Failed to activate the engine launcher window, due to: {err}", err);
        });
        
        base.LoadContent();
    }

    protected override void Draw(GameTime gameTime)
    {
        if (_imGuiRenderer == null)
            return;
        
        GraphicsDevice.SetRenderTarget(_renderTargetManager.GetOrCreateTarget2D(GamePreviewController.PreviewTargetName));
        GraphicsDevice.Clear(Color.CornflowerBlue with {A = 40});
        _sceneManager.DrawScene(gameTime.ElapsedGameTime.TotalSeconds);
        
        GraphicsDevice.SetRenderTarget(null);
        
        GraphicsDevice.Clear(new Color(30, 30, 30));
        
        _imGuiRenderer.BeforeLayout(gameTime);

        ImAnim.UpdateAll();

        EditorWindowManager.Draw();

        ImNotify.DrawNotification();
        ImModal.Render();
        
        _imGuiRenderer.AfterLayout();

        base.Draw(gameTime);
    }

    protected override void Update(GameTime gameTime)
    {
        _sceneManager.Simulation.Update(gameTime.ElapsedGameTime.TotalSeconds);
    }

}
