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
using System.Numerics;
using Hexa.NET.ImGui;
using RPGCreator.SDK.Common.Logging;

namespace RPGCreator.Editor.ImGuiLib;

public enum NotifyType { Info, Success, Warning, Error }

public static class ImNotify
{

    private static readonly ScopedLogger _scopedLogger = Logger.ForContext("ImNotify", "RPGCreator.RTP.ImGuiLib");
    
    private struct Toast
    {
        /// <summary>
        /// INTERNAL DO NOT TOUCH!
        /// </summary>
        public bool _created;
        public bool IsActive;
        public NotifyType Type;
        public string Title;
        public string Content;
        public float Timer;
        public float MaxDuration;
        
        public ImAnimPointer<float> FadeAnimId; 
    }

    private static readonly Toast[] _toasts = new Toast[10];
    
    private const ImGuiWindowFlags ToastFlags = ImGuiWindowFlags.NoDecoration |
                                                ImGuiWindowFlags.NoSavedSettings |
                                                ImGuiWindowFlags.NoFocusOnAppearing |
                                                ImGuiWindowFlags.NoNav |
                                                ImGuiWindowFlags.NoMove |
                                                ImGuiWindowFlags.Tooltip |
                                                ImGuiWindowFlags.NoInputs;
    
    public static void Show(string title, string content, NotifyType type, float duration = 3.0f)
    {
        for (int i = _toasts.Length-1; i > 0; i--)
        {
            if (!_toasts[i]._created)
            {
                ImAnimPointer<float> animId = ImAnim.Create(0f, 1f, 0.3f, AnimEndType.Clamp, EasingType.EaseOutQuad);

                _toasts[i] = new Toast
                {
                    _created = true,
                    IsActive = true,
                    Title = title,
                    Content = content,
                    Type = type,
                    Timer = 0f,
                    MaxDuration = duration,
                    FadeAnimId = animId
                };
                return;
            }
        }
        
        // In this case, there is too many notification!
        _scopedLogger.Warning("Tried to add a new notification, but reached the limit of notification!");
    }

    public static void DrawNotification()
    {
        var viewport = ImGui.GetMainViewport();
        
        float padding = 20.0f;
        Vector2 currentPos = new Vector2(
            viewport.WorkPos.X + viewport.WorkSize.X - padding,
            viewport.WorkPos.Y + viewport.WorkSize.Y - padding
        );
        
        for (int i = 0; i < _toasts.Length; i++)
        {
            ref Toast toast = ref _toasts[i];
            
            if (!toast._created)
                continue;
            
            if (!toast.IsActive)
            {
                if (ImAnim.IsReverse<float>(toast.FadeAnimId) && !ImAnim.IsActive<float>(toast.FadeAnimId))
                {
                    ImAnim.Destroy<float>(toast.FadeAnimId);
                    _toasts[i] = default;
                    continue;
                }
                
                if(!ImAnim.IsActive<float>(toast.FadeAnimId))
                {
                    ImAnim.Reverse<float>(toast.FadeAnimId);
                }
            }

            toast.Timer += ImGui.GetIO().DeltaTime;

            var maxSize = ImGui.CalcTextSize(toast.Content);
            if (ImGui.CalcTextSize(toast.Title).X > maxSize.X)
            {
                maxSize = ImGui.CalcTextSize(toast.Title);
            }
            maxSize.X = Math.Min(300, maxSize.X); // We limit the width of the notification to '300px'
            
            ImGui.SetNextWindowSize(maxSize with { Y = 0f, X = maxSize.X + ImGui.GetStyle().WindowPadding.X * 2}, ImGuiCond.Always);

            float alpha = ImAnim.Get<float>(toast.FadeAnimId);
            ImGui.SetNextWindowPos(currentPos, ImGuiCond.Always, new Vector2(1.0f, 1.0f));

            if (toast.Timer >= toast.MaxDuration)
            {
                toast.IsActive = false; 
            }
            
            ImGui.PushStyleVar(ImGuiStyleVar.Alpha, alpha);

            Vector4 bgColor = GetBackgroundColorForType(toast.Type);
            ImGui.PushStyleColor(ImGuiCol.Border, bgColor);

            if (ImGui.Begin($"##Toast_{i}", ToastFlags))
            {
                ImGui.TextWrapped(toast.Title);
                ImGui.Separator();
                ImGui.TextWrapped(toast.Content);
            }
            
            float toastHeight = ImGui.GetWindowHeight();
            currentPos.Y -= (toastHeight + 10.0f);

            ImGui.End();
            ImGui.PopStyleColor(1);
            ImGui.PopStyleVar();
        }
    }

    private static Vector4 GetBackgroundColorForType(NotifyType type)
    {
        return type switch
        {
            NotifyType.Error => new Vector4(0.6f, 0.2f, 0.2f, 0.9f),
            NotifyType.Success => new Vector4(0.2f, 0.4f, 0.2f, 0.9f),
            NotifyType.Warning => new Vector4(0.5f, 0.3f, 0.1f, 0.9f),
            _ => new Vector4(0.2f, 0.2f, 0.2f, 0.9f)
        };
    }
}