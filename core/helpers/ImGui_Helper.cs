using ImGuiNET;
using RPGCreator.core.interfaces;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace RPGCreator.core.helpers
{
    static class ImGui_Helper
    {
        private static Dictionary<int, ImFontPtr> AvailFonts = [];
        public enum ALIGNEMENT
        {
            LEFT,
            RIGHT,
            CENTER,
            CUSTOM
        }

        /// <summary>
        /// Align the next item in ratio to a string.
        /// </summary>
        /// <param name="text_to_align">The string to align.</param>
        /// <param name="align">The alignement type (If set to custom, you may specify a custom offset with <paramref name="customOffset"/>).</param>
        /// <param name="customOffset">The custom offset (0 = LEFT / 0.5 = CENTER / 1 = RIGHT). Only needed if <paramref name="align"/> is equal to CUSTOM</param>
        static public void AlignNextText(string text_to_align, ALIGNEMENT align, float customOffset = 0f)
        {
            ImGuiStylePtr style = ImGui.GetStyle();

            float size = ImGui.CalcTextSize(text_to_align).X + style.FramePadding.X * 2.0f;
            float available = ImGui.GetContentRegionAvail().X;

            float offset_to_add;
            switch(align)
            {
                case ALIGNEMENT.LEFT:
                    offset_to_add = 0f;
                    break;
                case ALIGNEMENT.CENTER:
                    offset_to_add = .5f;
                    break;
                case ALIGNEMENT.RIGHT:
                    offset_to_add = 1f;
                    break;
                default:
                    offset_to_add = customOffset;
                    break;
            }
            float offset = (available - size) * offset_to_add;
            ImGui.SetCursorPosX(ImGui.GetCursorPosX() + offset);
        }
    
        static public void AddFont(int size)
        {
            ImGuiIOPtr io = ImGui.GetIO();
            AvailFonts.Add(size, io.Fonts.AddFontFromFileTTF(BaseContent.GetImGuiFont(), size));
        }

        /// <summary>
        /// Get a loaded font.
        /// </summary>
        /// <param name="size">The font size wanted.</param>
        /// <returns></returns>
        static public ImFontPtr GetFont(int size)
        {
            if(AvailFonts.Count == 0)
            {
                Log.Logger.Fatal($"No loaded font size for ImGui!!");
                return null;
            }

            if(!AvailFonts.ContainsKey(size))
            {
                Log.Logger.Error($"Couldn't found font size {size}");
            }

            return AvailFonts.GetValueOrDefault(size, AvailFonts.First().Value);
        }


    }
}
