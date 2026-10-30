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

using Hexa.NET.ImGui;
using RPGCreator.Editor.ImGuiLib;
using RPGCreator.SDK.Assets.Tileset;
using RPGCreator.Shared.Attributes;

namespace RPGCreator.Editor.Handlers.AssetDrawers;

public static class NativeAssetsDrawer
{
    private const int COLUMNS_COUNT_TEMPLATE = 3;
    private const string COLUMNS_COUNT_ERROR = "[ERROR: AssetDrawer in a TABLE need to have columns set to 3!]";
    
    [AssetTypeDrawer(typeof(TilesetDefinition))]
    public static void Draw(TilesetDefinition tileset)
    {
        var inTable = ImGui.InTable();
        var columnsCount = ImGui.GetColumnsCount();

        if (inTable)
        {
            if (columnsCount != COLUMNS_COUNT_TEMPLATE)
            {
                ImGui.TableNextRow();
                if (!ImGui.TableSetColumnIndex(columnsCount - 1)) return;
                
                ImGui.TextDisabled(COLUMNS_COUNT_ERROR);
                ImGui.SetItemTooltip(COLUMNS_COUNT_ERROR);
            }
            else
            {
                DrawTilesetInTable(tileset);
            }
        }
        else
        {
            DrawTilesetOutTable(tileset);
        }
    }

    // [NAME] | [DETAILS] | [AssetId]
    private static void DrawTilesetInTable(TilesetDefinition tileset)
    {
        ImGui.TableNextRow();
        ImGui.TableSetColumnIndex(0);
        ImGui.TextWrapped(tileset.Name);
        
        ImGui.TableSetColumnIndex(1);
        ImGui.TextWrapped("Details");
        if (ImGui.BeginItemTooltip())
        {
            ImGui.TextDisabled("ResPath: ");
            ImGui.SameLine();
            ImGui.TextWrapped(tileset.TextureResourcePath);
            
            ImGui.Separator();
            
            ImGui.TextDisabled("TileW: ");
            ImGui.SameLine();
            ImGui.TextWrapped(tileset.TileWidth.ToString());
            
            ImGui.Separator();
            
            ImGui.TextDisabled("TileH: ");
            ImGui.SameLine();
            ImGui.TextWrapped(tileset.TileHeight.ToString());
            
            ImGui.EndTooltip();
        }

        ImGui.TableSetColumnIndex(2);
        ImGui.Text(tileset.AssetId.ToString());
        
    }

    private static void DrawTilesetOutTable(TilesetDefinition tileset)
    {
        ImGui.Text(tileset.Name);
        ImGui.SameLine();
        ImGui.Text("Details");
        if (ImGui.BeginItemTooltip())
        {
            ImGui.TextDisabled("ResPath: ");
            ImGui.SameLine();
            ImGui.TextWrapped(tileset.TextureResourcePath);
            
            ImGui.Separator();
            
            ImGui.TextDisabled("TileW: ");
            ImGui.SameLine();
            ImGui.TextWrapped(tileset.TileWidth.ToString());
            
            ImGui.Separator();
            
            ImGui.TextDisabled("TileH: ");
            ImGui.SameLine();
            ImGui.TextWrapped(tileset.TileHeight.ToString());
            
            ImGui.EndTooltip();
        }
        ImGui.SameLine();
        ImGui.Text(tileset.AssetId.ToString());
    }

}