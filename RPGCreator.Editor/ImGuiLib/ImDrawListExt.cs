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
using Hexa.NET.ImGui;

namespace RPGCreator.Editor.ImGuiLib;

public static class ImDrawListExt
{
    /// <summary>
    /// Adds a filled rectangle with optional stripes to a draw list.
    /// The background color of the rectangle is filled first, and then diagonal stripes
    /// are drawn over it based on the specified orientation, thickness, color, and spacing.
    /// </summary>
    /// <param name="drawList">
    /// The draw list where the striped rectangle will be rendered.
    /// </param>
    /// <param name="pMin">
    /// The minimum corner of the rectangle in screen coordinates.
    /// </param>
    /// <param name="pMax">
    /// The maximum corner of the rectangle in screen coordinates.
    /// </param>
    /// <param name="bgCol">
    /// The background color of the rectangle, specified as a 32-bit unsigned integer in ARGB format.
    /// </param>
    /// <param name="stripOrientation">
    /// The orientation of the stripes in <b>radians</b>. This specifies the angle of the diagonal lines.
    /// </param>
    /// <param name="stripThickness">
    /// The thickness of the individual stripes.
    /// </param>
    /// <param name="stripColor">
    /// The color of the stripes, specified as a Vector4 where each component is in the range [0, 1].
    /// </param>
    /// <param name="stripSpacing">
    /// The spacing between consecutive stripes. A larger value results in more space between each stripe.
    /// </param>
    public static void AddStripedRect(
        this ImDrawListPtr drawList,
        Vector2 pMin,
        Vector2 pMax,
        uint bgCol,
        float stripOrientation,
        float stripThickness,
        Vector4 stripColor,
        float stripSpacing)
    {
        if ((bgCol & 0xFF000000) != 0)
        {
            drawList.AddRectFilled(pMin, pMax, bgCol);
        }

        var stripColU32 = ImGui.ColorConvertFloat4ToU32(stripColor);

        if ((stripColU32 & 0xFF000000) != 0 && stripSpacing > 0)
        {
            drawList.PushClipRect(pMin, pMax, true);

            var center = (pMin + pMax) * 0.5f;
            var size = pMax - pMin;
            var radius = MathF.Sqrt(size.X * size.X + size.Y * size.Y) * 0.5f;

            var cos = MathF.Cos(stripOrientation);
            var sin = MathF.Sin(stripOrientation);
            var normal = new Vector2(cos, sin);
            var dir = new Vector2(-sin, cos);

            for (var d = -radius; d <= radius; d += stripSpacing)
            {
                var p0 = center + normal * d;
                drawList.AddLine(p0 - dir * radius, p0 + dir * radius, stripColU32, stripThickness);
            }

            drawList.PopClipRect();
        }
    }
}