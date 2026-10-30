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
using System.Numerics;
using System.Reflection;
using Arch.Core;
using Hexa.NET.ImGui;
using Microsoft.Xna.Framework.Input;
using GameTime = Microsoft.Xna.Framework.GameTime;

namespace RPGCreator.Player;

public partial class GamePlayer
{
    private bool _showDebugOverlay = true;
    private bool _wasF12Pressed;
    private Entity _selectedDebugEntity = Entity.Null;
    
    private ImGuiRenderer? _debugImGuiRenderer;
    private SDL3.SDL.SDL_EventFilter? _sdlEventFilter;

    partial void InitializeDebugTools()
    {
        _debugImGuiRenderer = new ImGuiRenderer(this);
        var io = ImGui.GetIO();
        io.DisplaySize = new Vector2(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);

        unsafe
        {
            _sdlEventFilter = DebugImGui_SDLEventFilter;
            SDL3.SDL.SDL_AddEventWatch(_sdlEventFilter, IntPtr.Zero);
        }
    }
    
    private unsafe bool DebugImGui_SDLEventFilter(nint userdata, SDL3.SDL.SDL_Event* e)
    {
        var io = ImGui.GetIO();
        switch (e->type)
        {
            case (uint)SDL3.SDL.SDL_EventType.SDL_EVENT_MOUSE_MOTION:
                io.AddMousePosEvent(e->motion.x, e->motion.y);
                break;
            case (uint)SDL3.SDL.SDL_EventType.SDL_EVENT_MOUSE_BUTTON_DOWN:
            case (uint)SDL3.SDL.SDL_EventType.SDL_EVENT_MOUSE_BUTTON_UP:
                int btn = e->button.button == 1 ? 0 : e->button.button == 3 ? 1 : 2;
                io.AddMouseButtonEvent(btn, e->type == (uint)SDL3.SDL.SDL_EventType.SDL_EVENT_MOUSE_BUTTON_DOWN);
                break;
            case (uint)SDL3.SDL.SDL_EventType.SDL_EVENT_MOUSE_WHEEL:
                io.AddMouseWheelEvent(e->wheel.x, e->wheel.y);
                break;
            case (uint)SDL3.SDL.SDL_EventType.SDL_EVENT_WINDOW_RESIZED:
                io.DisplaySize = new Vector2(e->window.data1, e->window.data2);
                break;
        }
        return true;
    }

    partial void UpdateDebugTools(GameTime gameTime)
    {
        if (!Window.Title.Contains("[DEBUG"))
            Window.Title += " [DEBUG - F12: ECS Inspector]";

        var kb = Keyboard.GetState();
        bool f12Down = kb.IsKeyDown(Keys.F12);
        if (f12Down && !_wasF12Pressed)
        {
            _showDebugOverlay = !_showDebugOverlay;
        }
        _wasF12Pressed = f12Down;
    }

    partial void DrawDebugTools(GameTime gameTime)
    {
        if (!_showDebugOverlay || _debugImGuiRenderer == null)
            return;

        _debugImGuiRenderer.BeforeLayout(gameTime);
        DrawEcsInspectorWindow(gameTime);
        _debugImGuiRenderer.AfterLayout();
    }

    private void DrawEcsInspectorWindow(GameTime gameTime)
    {
        ImGui.SetNextWindowSize(new Vector2(460, 520), ImGuiCond.FirstUseEver);
        if (!ImGui.Begin("ECS World Inspector (F12)", ref _showDebugOverlay))
        {
            ImGui.End();
            return;
        }

        double fps = gameTime.ElapsedGameTime.TotalSeconds > 0
            ? 1.0 / gameTime.ElapsedGameTime.TotalSeconds
            : 0;

        ImGui.Text($"FPS: {fps:F1} ({gameTime.ElapsedGameTime.TotalMilliseconds:F2} ms)");

        var world = _sceneManager.Simulation.CurrentWorld;
        if (world == null)
        {
            ImGui.TextDisabled("No active ECS World loaded.");
            ImGui.End();
            return;
        }

        ImGui.Text($"Entities: {world.Size} | Archetypes: {world.Archetypes.Count}");
        ImGui.Separator();

        if (ImGui.BeginTable("##EcsDebugSplit", 2, ImGuiTableFlags.BordersInnerV | ImGuiTableFlags.Resizable))
        {
            ImGui.TableSetupColumn("Entities", ImGuiTableColumnFlags.WidthFixed, 150f);
            ImGui.TableSetupColumn("Components", ImGuiTableColumnFlags.WidthStretch);

            // --- Left Column: Entity List ---
            ImGui.TableNextColumn();
            ImGui.BeginChild("##EntityListChild");
            var allQuery = new QueryDescription();
            world.Query(in allQuery, (Entity entity) =>
            {
                bool isSelected = _selectedDebugEntity == entity;
                if (ImGui.Selectable($"Entity #{entity.Id} (v{entity.Version})", isSelected))
                {
                    _selectedDebugEntity = entity;
                }
            });
            ImGui.EndChild();

            // --- Right Column: Component Inspector for Selected Entity ---
            ImGui.TableNextColumn();
            ImGui.BeginChild("##ComponentInspectorChild");
            if (_selectedDebugEntity != Entity.Null && world.IsAlive(_selectedDebugEntity))
            {
                var components = world.GetAllComponents(_selectedDebugEntity);
                ImGui.Text($"Entity #{_selectedDebugEntity.Id} ({components.Length} components)");
                ImGui.Separator();

                foreach (var comp in components)
                {
                    if (comp == null) continue;
                    var compType = comp.GetType();

                    if (ImGui.CollapsingHeader(compType.Name, ImGuiTreeNodeFlags.DefaultOpen))
                    {
                        ImGui.Indent();
                        DrawComponentFieldsAndProperties(comp, compType);
                        ImGui.Unindent();
                    }
                }
            }
            else
            {
                ImGui.TextDisabled("Select an entity on the left to inspect its components.");
            }
            ImGui.EndChild();

            ImGui.EndTable();
        }

        ImGui.End();
    }

    private static void DrawComponentFieldsAndProperties(object comp, Type compType)
    {
        var fields = compType.GetFields(BindingFlags.Public | BindingFlags.Instance);
        foreach (var field in fields)
        {
            var val = field.GetValue(comp);
            ImGui.TextDisabled($"{field.Name}:");
            ImGui.SameLine();
            ImGui.Text(val?.ToString() ?? "null");
        }

        var props = compType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        foreach (var prop in props)
        {
            if (!prop.CanRead || prop.GetIndexParameters().Length > 0) continue;
            var val = prop.GetValue(comp);
            ImGui.TextDisabled($"{prop.Name}:");
            ImGui.SameLine();
            ImGui.Text(val?.ToString() ?? "null");
        }
    }
    
    partial void DisposeDebugTools()
    {
        if (_sdlEventFilter != null)
            SDL3.SDL.SDL_RemoveEventWatch(_sdlEventFilter, IntPtr.Zero);
        _debugImGuiRenderer?.Dispose();
    }
}