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
using RPGCreator.SDK.Common.Attributes;
using RPGCreator.SDK.GameUI.Enums;
using RPGCreator.SDK.GameUI.Visual;
using RPGCreator.SDK.Types;
using RPGCreator.Shared.Types;

namespace RPGCreator.SDK.GameUI.Controls;

[EngineClass("rpgc", "game_ui", "controls", "inputs", "button_control")]
public partial class ButtonControl : BaseControl
{
    private TextControl _textControl = new TextControl() { IsInternal = false };

    public override string DisplayControlName { get; set; } = "Button Control";
    public override string Description { get; set; } = "A control that displays a button.";
    public override URN Urn => _urnModule.ToUrnModule("rpgc").ToUrn("button_control");

    public EditableControlPropertyDescriptor<float> CornerRadiusProperty { get; private set; }
    public EditableControlPropertyDescriptor<float> BorderThicknessProperty { get; private set; }
    public EditableControlPropertyDescriptor<Color> BorderColorProperty { get; private set; }
    public EditableControlPropertyDescriptor<Color> BackgroundColorProperty { get; private set; }
    
    public override BaseControl Create()
    {
        return new ButtonControl();
    }

    public ButtonControl()
    {
        Visual = new ButtonVisual();
        Visual.Width = 150;
        Visual.Height = 50;
        _textControl.Text = "Button";
        _textControl.GetExposedProperties();
        _textControl.PositionXUnitProperty.Set(EPositionUnitType.Percentage);
        _textControl.PositionYUnitProperty.Set(EPositionUnitType.Percentage);
        _textControl.PositionProperty.Set(new Vector2(0, 0));
        _textControl.SizeXUnitProperty.Set(ESizeUnitType.Percentage);
        _textControl.SizeYUnitProperty.Set(ESizeUnitType.Percentage);
        _textControl.SizeProperty.Set(new Vector2(100, 100));
        _textControl.VerticalAlignmentProperty.Set(ETextVAlignment.Center);
        _textControl.HorizontalAlignmentProperty.Set(ETextHAlignment.Center);
        AddChild(_textControl);
    }

    protected override void MakeExposedProperties()
    {
        base.MakeExposedProperties();
        
        CornerRadiusProperty = RegisterPropertyDescriptor(new EditableControlPropertyDescriptor<float>("Corner radius", () => ((ButtonVisual)Visual).CornerRadius, "Appearance".ToPipedPath(),
            "The corner radius of the button.", f => ((ButtonVisual)Visual).CornerRadius = f));
        BorderThicknessProperty = RegisterPropertyDescriptor(new EditableControlPropertyDescriptor<float>("Border thickness", () => ((ButtonVisual)Visual).BorderThickness, "Appearance".ToPipedPath(),
            "The thickness of the button's border.", f => ((ButtonVisual)Visual).BorderThickness = f));
        BorderColorProperty = RegisterPropertyDescriptor(new EditableControlPropertyDescriptor<Color>("Border color", () => ((ButtonVisual)Visual).BorderColor, "Appearance".ToPipedPath(),
            "The color of the button's border.", c => ((ButtonVisual)Visual).BorderColor = c));
        BackgroundColorProperty = RegisterPropertyDescriptor(new EditableControlPropertyDescriptor<Color>("Background color", () => ((ButtonVisual)Visual).BackgroundColor, "Appearance".ToPipedPath(),
            "The color of the button's background.", c => ((ButtonVisual)Visual).BackgroundColor = c));
        RegisterPropertyDescriptor(_textControl.TextProperty);
        RegisterPropertyDescriptor(_textControl.TextColorProperty);
    }

    public override void OnUpdate()
    {
        
    }

    public override void SyncVisual()
    {
        
    }
}