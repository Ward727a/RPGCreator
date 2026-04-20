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

using System.Numerics;
using RPGCreator.RTP.GameUI.Enums;
using RPGCreator.SDK.GameUI.Visual;
using RPGCreator.SDK.RuntimeService;
using RPGCreator.SDK.Types;

namespace RPGCreator.SDK.GameUI.Controls;

public class TextControl : BaseControl
{
    public override URN Urn => _urnModule.ToUrnModule("rpgc").ToUrn("text_control");
    public override StringName ControlName => "Text Control";

    private bool _hasHeightBeenManuallyChanged;
    private bool _hasWidthBeenManuallyChanged;
    private bool _hasHUnitBeenManuallyChanged;
    private bool _hasWUnitBeenManuallyChanged;
    
    public string Text { get; set; }
    public Color TextColor { get; set; }
    public IRpgFont Font { get; set; }

    public TextControl() : this(string.Empty)
    {
    }

    public TextControl(string text) : this(text, Color.White)
    {
    }
    
    public TextControl(string text, Color textColor)
    {
        Text = text;
        TextColor = textColor;
        Visual = new TextVisual()
        {
            Control = this
        };

    }
    
    public EditableControlPropertyDescriptor<string> TextProperty { get; protected set; }
    public EditableControlPropertyDescriptor<Color> TextColorProperty { get; protected set; }

    protected override void MakeExposedProperties()
    {
        base.MakeExposedProperties();
        
        TextProperty = RegisterPropertyDescriptor(new EditableControlPropertyDescriptor<string>("Text", () => Text, "Content".ToPipedPath(),
            "The text to display.", s => Text = s ?? string.Empty, s => !string.IsNullOrEmpty(s)));
        TextColorProperty = RegisterPropertyDescriptor(new EditableControlPropertyDescriptor<Color>("Text color", () => TextColor, "Content".ToPipedPath(),
            "The color of the text.", c => TextColor = c));
        
        SizeProperty.ValueChanged += (newValue, oldValue) =>
        {
            if (newValue is not Vector2 newSizeValue || oldValue is not Vector2 oldSizeValue)
            {
                return;
            }

            _hasHeightBeenManuallyChanged = Math.Abs(newSizeValue.Y - oldSizeValue.Y) > 0.001;
            _hasWidthBeenManuallyChanged = Math.Abs(newSizeValue.X - oldSizeValue.X) > 0.001;
            _hasHUnitBeenManuallyChanged = Visual.HeightUnit != ESizeUnitType.Pixels;
            _hasWUnitBeenManuallyChanged = Visual.WidthUnit != ESizeUnitType.Pixels;
        };
        
        SizeXUnitProperty.ValueChanged += (newValue, oldValue) =>
        {
            if (newValue is not ESizeUnitType newUnit || oldValue is not ESizeUnitType oldUnit)
            {
                return;
            }

            _hasWUnitBeenManuallyChanged = newUnit != ESizeUnitType.Pixels;
        };
        
        SizeYUnitProperty.ValueChanged += (newValue, oldValue) =>
        {
            if (newValue is not ESizeUnitType newUnit || oldValue is not ESizeUnitType oldUnit)
            {
                return;
            }
            
            _hasHUnitBeenManuallyChanged = newUnit != ESizeUnitType.Pixels;
        };
    }

    public override void OnUpdate()
    {
    }

    public override void SyncVisual()
    {
        if(!IsPropertiesInitialized)
            return;
        
        var newSize = new Vector2(Visual.Width, Visual.Height);
        if (!_hasHUnitBeenManuallyChanged && !_hasHeightBeenManuallyChanged)
        {
            newSize.Y = (int)Math.Round(Font.MeasureString(Text).Y);
        }
        if (!_hasWUnitBeenManuallyChanged && !_hasWidthBeenManuallyChanged)
        {
            newSize.X = (int)Math.Round(Font.MeasureString(Text).X);
        }
        
        SizeProperty.Set(newSize);
    }
}