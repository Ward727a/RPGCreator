#region LICENSE
//
// RPG Creator - Open-source RPG Engine.
// (c) 2025 Ward
// 
// This file is part of RPG Creator and is distributed under the MIT License.
// You are free to use, modify, and distribute this file under the terms of the MIT License.
// See LICENSE for details.
// 
// ---
// 
// Ce fichier fait partie de RPG Creator et est distribué sous licence MIT.
// Vous êtes libre de l'utiliser, de le modifier et de le distribuer sous les termes de la licence MIT.
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
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Xna.Framework.Graphics;
using RPGCreator.Core.Services;
using RPGCreator.MonoGame;
using RPGCreator.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace RPGCreator.Core.Types
{
    public partial class Tileset : ObservableObject
    {
        [ObservableProperty]
        private bool _is_hardcopy;

        [ObservableProperty]
        private string name;

        [ObservableProperty]
        private string path;

        [ObservableProperty]
        private int dimension;

        public Guid? Unique { get; private set; }

        // Reserved for EditorGame
        public Texture2D texture;

        private XElement XML { get; set; }
        private bool HasXML => XML != null;

        public Tileset()
        {
        }

        public void SetOrMakeGUID(Guid? unique = null)
        {
        }

        public void CreateTexture()
        {
            EditorGame t = App.Services.GetRequiredService<EditorService>().GameEditor;
            texture = Texture2D.FromFile(App.Services.GetRequiredService<EditorService>().GameEditor!.GraphicsDevice, Path);
        }


        public Tileset(XElement xml) : this()
        {
            XML = xml;
            if (Unique != null)
                return;

            if (XML.Element("Unique") == null)
            {
                Unique = Guid.NewGuid();
                XML.AddFirst(new XElement("Unique", Unique));
            }
            else
            {
                Unique = Guid.Parse(XML.Element("Unique")!.Value);
            }
        }

        partial void OnDimensionChanged(int value)
        {
            if (!HasXML) return;
            XML.Element("Dimension").SetValue(value);
        }

        partial void OnIs_hardcopyChanged(bool value)
        {
            if (!HasXML) return;
            XML.Element("IsHardcopy").SetValue(value ? "true" : "false");
        }

        partial void OnNameChanged(string value)
        {
            if (!HasXML) return;
            XML.Element("Name").SetValue(value);
        }

        partial void OnPathChanged(string value)
        {
            if (!string.IsNullOrEmpty(Path) && File.Exists(Path) && App.Services.GetRequiredService<EditorService>().HasGame && App.Services.GetRequiredService<EditorService>().GameEditor!.GraphicsDevice != null)
            {
                texture = Texture2D.FromFile(App.Services.GetRequiredService<EditorService>().GameEditor!.GraphicsDevice, Path);
            }
            if (!HasXML) return;
            XML.Element("Path").SetValue(value);
        }


    }
}
