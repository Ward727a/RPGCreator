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
using RPGCreator.SDK.Common.Attributes;
using RPGCreator.SDK.GameUI.Visual;
using RPGCreator.SDK.Types;

namespace RPGCreator.SDK.GameUI.Controls;

/// <summary>
/// This is a test control.<br/>
/// This should NEVER be used in production code and is only meant for testing purposes. It may be removed without warning at any time.
/// </summary>
[EngineClass("rpgc", "game_ui", "controls", "debug", "test_panel_control")]
public partial class TestControl() : BaseControl
{
    public override string DisplayControlName { get; set; } = "Test Control";
    public override string Description { get; set; } = "A control that is used for testing purposes only.";
    public override BaseControl Create()
    {
        return new TestControl();
    }

    protected TestVisual _Visual => (TestVisual)Visual;

    private Color _defaultColor = Color.Red;
    private Color _hoverColor = Color.Blue;

    private bool _filled = true;

    private Vector2 _grabOffset;
    
    public override URN Urn => _urnModule.ToUrnModule("rpgc").ToUrn("test_control");

    #region ExposedProperties
    
    public EditableControlPropertyDescriptor<bool> FilledProperty { get; protected set; }
    public EditableControlPropertyDescriptor<Color> DefaultColorProperty { get; protected set; }
    public EditableControlPropertyDescriptor<Color> HoverColorProperty { get; protected set; }
    public ReadOnlyControlPropertyDescriptor<string> TestProperty { get; protected set; }
    
    #endregion
    
    public TestControl(Color? color = null, Color? hoverColor = null) : this()
    {
        Visual = new TestVisual()
        {
            Control = this
        };
        
        if (color != null)
        {
            _Visual.RectColor = color.Value;
            _defaultColor = color.Value;
        }

        if (hoverColor != null)
            _hoverColor = hoverColor.Value;

        ClipHitTestToBounds = false;
    }

    protected override void MakeExposedProperties()
    {
        base.MakeExposedProperties();

        FilledProperty = RegisterPropertyDescriptor(new EditableControlPropertyDescriptor<bool>("Filled", () => _filled,
            "Appearance".ToPipedPath().Extend("Background"), "Define whether the control is filled with color.", (value) => _filled = value));
        DefaultColorProperty = RegisterPropertyDescriptor(new EditableControlPropertyDescriptor<Color>("Background color", () => _defaultColor,
            "Appearance".ToPipedPath().Extend("Background"), "Define the color of the background.", (value) => _defaultColor = value));
        HoverColorProperty = RegisterPropertyDescriptor(new EditableControlPropertyDescriptor<Color>("Hover color", () => _hoverColor,
            "Appearance".ToPipedPath().Extend("Background"), "Define the color of the background when hovered by the mouse.", (value) => _hoverColor = value));
        
        TestProperty = RegisterPropertyDescriptor(new ReadOnlyControlPropertyDescriptor<string>("Test Property", () => "Test Value",
            "Test".ToPipedPath(), "This is a test property that cannot be edited."));
    }

    public override void OnUpdate()
    {
    }

    public override void SyncVisual()
    {
        if (_isMouseOver)
        {
            _Visual.RectColor = _hoverColor;
        }
        else
        {
            _Visual.RectColor = _defaultColor;
        }
    }

    protected override void OnDragStart()
    {
        _grabOffset = GlobalStates.ViewportMouseState.Position - Visual.GlobalBounds.Position;
    }

    protected override void OnDragMove(Vector2 deltaPosition)
    {
        Vector2 targetScreenPos = GlobalStates.ViewportMouseState.Position - _grabOffset;

        if (Parent != null)
        {
            Vector2 parentPos = Parent.Visual.GlobalBounds.Position;
            Vector2 localPos = targetScreenPos - parentPos;

            _Visual.X = (int)localPos.X;
            _Visual.Y = (int)localPos.Y;
        }
        else
        {
            _Visual.X = (int)targetScreenPos.X;
            _Visual.Y = (int)targetScreenPos.Y;
        }

        _Visual.XUnit = EPositionUnitType.Pixels;
        _Visual.YUnit = EPositionUnitType.Pixels;
    }
}