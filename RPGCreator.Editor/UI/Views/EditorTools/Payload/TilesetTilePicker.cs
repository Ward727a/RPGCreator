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
using System.Collections.Generic;
using System.Linq;
using Fluxor;
using Hexa.NET.ImGui;
using Microsoft.Xna.Framework;
using RPGCreator.Editor.Tools;
using RPGCreator.SDK.Modules.Editor.Ui;
using RPGCreator.SDK.States;
using RPGCreator.SDK.Tools;
using RPGCreator.Shared.Attributes;
using RPGCreator.Shared.Types;
using Vector2 = System.Numerics.Vector2;

namespace RPGCreator.Editor.UI.Views.EditorTools.Payload;

public class TilesetTilePicker(
    IEditorViewContext context,
    EditorViewDefinition definition,
    TilesetTilePickerController controller)
    : ComplexEditorUi<TilesetTilePickerController>(context, definition, controller)
{
    public override void OnActivate(object? args)
    {
        if (args is TilesetTilePickerArgs typedArgs)
        {
            Controller.Initiate(typedArgs);
        }
    }

    public override void Dispose()
    {
        Controller.Dispose();
        GC.SuppressFinalize(this);
    }

    public override void Draw(EDrawAs drawAs = EDrawAs.Unknown)
    {
        var childFlags = ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse;
        if (ImGui.BeginChild("TilesetViewport", Vector2.Zero, childFlags))
        {
            if (ImGui.IsWindowHovered() &&
                (ImGui.IsMouseDown(ImGuiMouseButton.Right) || ImGui.IsMouseDown(ImGuiMouseButton.Middle)))
            {
                var delta = ImGui.GetIO().MouseDelta;
                Controller.SetMovement(delta.X, delta.Y);
            }

            if (ImGui.IsWindowHovered() && ImGui.GetIO().MouseWheel != 0)
            {
                Controller.SetZoom(ImGui.GetIO().MouseWheel * 0.1f);
            }

            var imageScreenPos = ImGui.GetCursorScreenPos();

            ImGui.Image(Controller.State.Image, Controller.State.ScaledImageSize);

            if (ImGui.IsItemHovered())
            {
                var mousePos = ImGui.GetMousePos();

                var relativePos = mousePos - imageScreenPos;

                var scaledTileSize = Controller.State.ScaledTileSize;

                var tileX = (int)(relativePos.X / scaledTileSize.X);
                var tileY = (int)(relativePos.Y / scaledTileSize.Y);
                
                if (ImGui.IsMouseClicked(ImGuiMouseButton.Left))
                {
                    Controller.StartSelectTile(tileX, tileY);
                }
                
                if (Controller.State.IsSelecting)
                {
                    Controller.UpdateSelectionTile(tileX, tileY);
                }

                if (ImGui.IsMouseReleased(ImGuiMouseButton.Left))
                {
                    Controller.EndSelectTile(tileX, tileY);
                }

                var drawList = ImGui.GetWindowDrawList();
                var hoverTopLeft = imageScreenPos + new Vector2(tileX * scaledTileSize.X, tileY * scaledTileSize.Y);
                var hoverBottomRight = hoverTopLeft + scaledTileSize;

                drawList.AddRectFilled(hoverTopLeft, hoverBottomRight, 0x80FFFF00);
            }

            if (Controller.State.StartSelectedTile is { X: >= 0, Y: >= 0 })
            {
                ImDrawListPtr drawList = ImGui.GetWindowDrawList();

                var scaledTileH = Controller.State.ScaledTileSize.Y;
                var scaledTileW = Controller.State.ScaledTileSize.X;
    
                int minX = Math.Min(Controller.State.StartSelectedTile.X, Controller.State.EndSelectedTile.X);
                int minY = Math.Min(Controller.State.StartSelectedTile.Y, Controller.State.EndSelectedTile.Y);
                int maxX = Math.Max(Controller.State.StartSelectedTile.X, Controller.State.EndSelectedTile.X);
                int maxY = Math.Max(Controller.State.StartSelectedTile.Y, Controller.State.EndSelectedTile.Y);
    
                Vector2 selectTopLeft = imageScreenPos + new Vector2(minX * scaledTileW, minY * scaledTileH);
                Vector2 selectBottomRight = imageScreenPos + new Vector2((maxX + 1) * scaledTileW, (maxY + 1) * scaledTileH);
    
                drawList.AddRect(selectTopLeft, selectBottomRight, 0xFF0000FF, 0.0f, ImDrawFlags.None, 3.0f);

                for (int x = minX + 1; x <= maxX; x++)
                {
                    float lineX = imageScreenPos.X + x * scaledTileW;
                    drawList.AddLine(selectTopLeft with { X = lineX }, selectBottomRight with { X = lineX }, 0x800000FF, 1.0f);
                }
                for (int y = minY + 1; y <= maxY; y++)
                {
                    float lineY = imageScreenPos.Y + y * scaledTileH;
                    drawList.AddLine(selectTopLeft with { Y = lineY }, selectBottomRight with { Y = lineY }, 0x800000FF, 1.0f);
                }
            }
        }

        ImGui.EndChild();
    }
}

public record struct TilesetTilePickerArgs(
    ImTextureRef Image,
    Point ImageSize,
    Point TileSize,
    Action<TileIndex, TileIndex> OnTileSelected,
    bool SupportMultiTile = false,
    Point? StartIndex = null,
    Point? EndIndex = null,
    bool AllowPick = true);

[EditorUiController]
public class TilesetTilePickerController : IDisposable
{
    public readonly TilesetTilePickerModel State;

    public TilesetTilePickerController(TilesetTilePickerModel state)
    {
        State = state;
    }

    private bool _supportMultiTile;

    public void Initiate(TilesetTilePickerArgs args)
    {
        State.Image = args.Image;
        State.ImageSize = args.ImageSize;
        State.TileSize = args.TileSize;
        State.OnTileSelected = args.OnTileSelected;
        State.StartSelectedTile = args.StartIndex ?? new Point(-1, -1);
        State.EndSelectedTile = args.EndIndex ?? new Point(-1, -1);
        State.TilesByRows = (State.ImageSize.X / State.TileSize.X);
        
        _supportMultiTile = args.SupportMultiTile;
        if (!_supportMultiTile && State.StartSelectedTile != State.EndSelectedTile)
        {
            State.EndSelectedTile = State.StartSelectedTile;
            State.OnTileSelected.Invoke(MakeIndex(State.StartSelectedTile), MakeIndex(State.EndSelectedTile));
        }

        State.AllowPick = args.AllowPick;
        State.Zoom = 1f;
        UpdateScaledImageSize();
        UpdateScaledTileSize();
    }
    
    public void StartSelectTile(int tileX, int tileY)
    {
        if (!State.AllowPick)
            return;
        
        State.IsSelecting = true;
        State.StartSelectedTile = new Point(tileX, tileY);
        State.EndSelectedTile = new Point(tileX, tileY);
    }
    
    public void UpdateSelectionTile(int tileX, int tileY)
    {
        if (State.IsSelecting && _supportMultiTile)
        {
            State.EndSelectedTile = new Point(tileX, tileY);
        }
    }
    
    public void EndSelectTile(int tileX, int tileY)
    {
        if (State.IsSelecting)
        {
            State.IsSelecting = false;
            if(_supportMultiTile)
                State.EndSelectedTile = new Point(tileX, tileY);
            State.OnTileSelected.Invoke(MakeIndex(State.StartSelectedTile), MakeIndex(State.EndSelectedTile));
        }
    }

    public void SetZoom(float wheelValue)
    {
        State.Zoom += wheelValue * 0.2f;
        State.Zoom = Math.Clamp(State.Zoom, 0.2f, 2.0f);
        UpdateScaledImageSize();
        UpdateScaledTileSize();
    }
    
    public void SetMovement(float deltaX, float deltaY)
    {
        ImGui.SetScrollX(ImGui.GetScrollX() - deltaX);
        ImGui.SetScrollY(ImGui.GetScrollY() - deltaY);
    }

    private void UpdateScaledImageSize()
    {
        State.ScaledImageSize = new Vector2(State.ImageSize.X * State.Zoom, State.ImageSize.Y * State.Zoom);
    }

    private void UpdateScaledTileSize()
    {
        State.ScaledTileSize = new Vector2(State.TileSize.X * State.Zoom, State.TileSize.Y * State.Zoom);
    }
    
    private TileIndex MakeIndex(Point tile)
    {
        return TileIndex.Make(tile.X, tile.Y, State.TilesByRows);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}

[EditorUiModel]
public class TilesetTilePickerModel
{
    public Point StartSelectedTile = new(-1, -1);
    public Point EndSelectedTile = new(-1, -1);
    public bool IsSelecting;
    public bool AllowPick = true;
    public float Zoom = 1.0f;
    
    public ImTextureRef Image;
    public Point TileSize = Point.Zero;
    public Point ImageSize = Point.Zero;
    public Action<TileIndex, TileIndex> OnTileSelected = null!;

    public Vector2 ScaledImageSize;
    public Vector2 ScaledTileSize;

    public int TilesByRows;
}