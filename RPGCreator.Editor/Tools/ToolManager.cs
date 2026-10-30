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
using RPGCreator.SDK.Inputs;
using RPGCreator.SDK.Registry;
using RPGCreator.SDK.States;
using RPGCreator.SDK.States.Actions;
using RPGCreator.SDK.Tools;
using RPGCreator.Shared.Attributes;
using RPGCreator.Shared.Enums;
using RPGCreator.Shared.Types;
using Point = Microsoft.Xna.Framework.Point;

namespace RPGCreator.Editor.Tools;

[EngineService(mode: EngineModeFlags.Editor)]
public class ToolManager(IToolRegistry registry, IState<ToolState> state, IDispatcher dispatcher)
{
    private bool _pressed;
    
    public void OnTileClicked(int gridX, int gridY, MouseButton button)
    {
        if (!HasTool() || !HasTooLogic())
            return;

        _pressed = true;
        GetCurrentTool().ClickedAt(GetCurrentToolLogic(), new Point(gridX, gridY), button);
    }

    public void OnTileHovered(int gridX, int gridY, MouseButton button)
    {
        if (!HasTool() || !HasTooLogic())
            return;
        
        GetCurrentTool().HoveredAt(GetCurrentToolLogic(), new Point(gridX, gridY), button);
    }
    
    public void OnTileReleased(int gridX, int gridY, MouseButton button)
    {
        if (!HasTool() || !HasTooLogic() || !_pressed)
            return;

        _pressed = false;
        GetCurrentTool().ReleasedAt(GetCurrentToolLogic(), new Point(gridX, gridY), button);
    }

    public void SelectTool(URN toolUrn)
    {
        var tool = registry.GetTool(toolUrn);
        if (tool == null)
            return;

        state.Value.ActiveTool?.OnDeactivate();
        dispatcher.Dispatch(new ToolActions.ChangeToolAction(tool));
        tool.OnActivate();
    }

    public void SelectToolLogic(URN layerType)
    {
        var toolLogic = registry.GetToolLogic(layerType);
        if (toolLogic == null)
            return;
        
        dispatcher.Dispatch(new ToolActions.ChangeToolLogicAction(toolLogic));
    }

    private bool HasTool() => state.Value.ActiveTool != null;
    private bool HasTooLogic() => state.Value.ActiveToolLogic != null;

    private Tool GetCurrentTool()
    {
        return state.Value.ActiveTool!;
    }
    
    private IToolLogic GetCurrentToolLogic()
    {
        return state.Value.ActiveToolLogic!;
    }
}