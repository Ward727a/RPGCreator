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
using Avalonia;

namespace RPGCreator.UI.Styles
{
    internal class DefaultStyle : BaseStyle
    {
        public override string StyleName => "Default style";

        public override Thickness Margin => new Thickness(4);

        public override int TitleFontSize => 32;

        public override int SubtitleFontSize => 24;

        public override int TextFontSize => 18;
        public override int MediumTextFontSize => 14; // Default to TextFontSize, can be overridden if needed

        public override int SmallTextFontSize => 12;
    }
}
