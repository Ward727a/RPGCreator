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
using Avalonia.Controls;
using Microsoft.Xna.Framework;
using RPGCreator.Core.Type.Project;
using RPGCreator.Core.Type.RTP;

namespace RPGCreator.Core
{
    public class EngineData
    {

        internal EngineData()
        { }

        public static string AppName => "RPG Creator";
        public static Version AppVersion => new(0, 1, 0);

        public BaseProject? EditedProject { get; internal set; }
        public RTP_Game? RTPGame { get; internal set; }
    }
}
