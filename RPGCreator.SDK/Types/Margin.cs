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

namespace RPGCreator.SDK.Types;

public record struct Margin(int Left, int Top, int Right, int Bottom)
{
    public static readonly Margin Zero = new(0, 0, 0, 0);
    
    public int Horizontal => Left + Right;
    public int Vertical => Top + Bottom;
    
    public Margin WithLeft(int left) => this with { Left = left };
    public Margin WithTop(int top) => this with { Top = top };
    public Margin WithRight(int right) => this with { Right = right };
    public Margin WithBottom(int bottom) => this with { Bottom = bottom };
    
    public Margin WithHorizontal(int horizontal) => this with { Left = horizontal, Right = horizontal };
    public Margin WithVertical(int vertical) => this with { Top = vertical, Bottom = vertical };
    
    public Margin WithAll(int all) => this with { Left = all, Top = all, Right = all, Bottom = all };

    public Margin(int Horizontal, int Vertical) : this(Horizontal, Vertical, Horizontal, Vertical)
    {
    }

    public Margin(int all) : this(all, all)
    {
    }
    
    public Margin() : this(0)
    {
    }
}

