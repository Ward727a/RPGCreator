#region LICENSE
//
// RPG Creator - Open-source RPG Engine.
// (c) 2025 Ward
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
// 
// 
#endregion

using System.Numerics;
using CommunityToolkit.Diagnostics;
using RPGCreator.SDK;
using RPGCreator.SDK.EngineService;
using RPGCreator.SDK.GlobalState;
using RPGCreator.SDK.Inputs;
using RPGCreator.SDK.RuntimeService;
using Serilog;

namespace RPGCreator.Core.Services
{
    public class ToolService : IToolService
    {
        
        public ToolService()
        {
            GlobalStates.ViewportMouseState.ButtonDown += (button) =>
            {
                var insideOf = GlobalStates.ViewportMouseState.InObject;
                
                // the "MonoGameImage" can be found in the "EditorWindowControl.cs" file, for the "mgImage" object.
                if (insideOf is not ("MonoGameImage"))
                {
                    return;
                }
                
                UseAt(GlobalStates.ViewportMouseState.Position, button);
            };
            
            GlobalStates.ViewportMouseState.Moved += (deltaPosition) =>
            {
                var insideOf = GlobalStates.ViewportMouseState.InObject;
                
                // the "MonoGameImage" can be found in the "EditorWindowControl.cs" file, for the "mgImage" object.
                if (insideOf is not ("MonoGameImage"))
                {
                    return;
                }
                
                MoveAt(GlobalStates.ViewportMouseState.Position, deltaPosition);
                
                if (GlobalStates.ViewportMouseState.LeftButtonPressed)
                {
                    UseAt(GlobalStates.ViewportMouseState.Position, MouseButton.Left);
                }

                if (GlobalStates.ViewportMouseState.RightButtonPressed)
                {
                    UseAt(GlobalStates.ViewportMouseState.Position, MouseButton.Right);
                }
                
                if (GlobalStates.ViewportMouseState.MiddleButtonPressed)
                {
                    UseAt(GlobalStates.ViewportMouseState.Position, MouseButton.Middle);
                }
            };
        }
        
        public ToolLogic? GetSelectedTool()
        {
            return GlobalStates.ToolState.ActiveTool;
        }

        public void UseAt(Vector2 at, MouseButton button)
        {
            Log.Debug($"Tool clicked at: {at}");
            if(GlobalStates.ToolState.ActiveTool == null)
            {
                return;
            }
            GlobalStates.ToolState.ActiveTool.UseAt(at, button);
        }

        public void MoveAt(Vector2 at, Vector2 deltaPosition)
        {
            if (GlobalStates.ToolState.Payload == null)
            {
                return;
            }
            
            if (GlobalStates.ToolState.ActiveTool == null)
            {
                return;
            }

            if (deltaPosition.Length() < 1f)
                return; // Skip preview update if the mouse hasn't moved significantly
            
            GlobalStates.ToolState.ActiveTool.MoveInsideViewport(at, deltaPosition);
        }

        public void ClearPreview()
        {
            //Console.WriteLine("Clearing brush preview.");
        }
    }
}
