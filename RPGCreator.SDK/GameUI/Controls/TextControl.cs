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
using RPGCreator.SDK.Common.Logging;
using RPGCreator.SDK.GameUI.Enums;
using RPGCreator.SDK.GameUI.Visual;
using RPGCreator.SDK.Services.RuntimeService;
using RPGCreator.SDK.Types;
using RPGCreator.Shared.Types;

namespace RPGCreator.SDK.GameUI.Controls;

[EngineClass("rpgc", "game_ui", "controls", "display", "text_control")]
public partial class TextControl : BaseControl
{
    public override string DisplayControlName { get; set; } = "Text Control";
    public override string Description { get; set; } = "A control that displays text.";
    public override BaseControl Create()
    {
        return new TextControl("Text Control")
        {
        };
    }

    public override URN Urn => _urnModule.ToUrnModule("rpgc").ToUrn("text_control");

    private bool _hasHeightBeenManuallyChanged;
    private bool _hasWidthBeenManuallyChanged;
    private bool _hasHUnitBeenManuallyChanged;
    private bool _hasWUnitBeenManuallyChanged;

    private bool _vAlignmentDirty = true;
    private bool _hAlignmentDirty = true;
    
    public string Text { get; set; } = string.Empty;
    public Color TextColor { get; set; } = Color.White;
    public IRpgFont? Font { get; set; }
    
    private ETextVAlignment _vAlignment = ETextVAlignment.Top;
    private ETextHAlignment _hAlignment = ETextHAlignment.Left;

    public TextControl()
    {
        Visual = new TextVisual()
        {
            Control = this
        };
        IRpgFont? font = null;
        RuntimeServices.OnceServiceReady<IFontService>(
            (fontService) =>
            {
                fontService.GetFont("Arial").OnSuccess((_font) => font = _font).OnFailure((_) =>
                {
                    RuntimeServices.FontService.LoadFont(@"C:/Windows/Fonts/arial.ttf", "Arial");
                    RuntimeServices.FontService.GetFont("Arial").OnSuccess((_font) => font = _font).OnFailure((err) =>
                    {
                        Logger.Error("Failed to load font: {0}", err);
                    });
                });

                Font = font ?? throw new Exception("Failed to load font");
            });
    }

    public TextControl(string text) : this()
    {
        Text = text;
    }
    
    public TextControl(string text, Color textColor) : this()
    {
        Text = text;
        TextColor = textColor;
    }
    
    public EditableControlPropertyDescriptor<string> TextProperty { get; protected set; }
    public EditableControlPropertyDescriptor<Color> TextColorProperty { get; protected set; }
    public EditableControlPropertyDescriptor<ETextVAlignment> VerticalAlignmentProperty { get; protected set; }
    public EditableControlPropertyDescriptor<ETextHAlignment> HorizontalAlignmentProperty { get; protected set; }

    protected override void MakeExposedProperties()
    {
        base.MakeExposedProperties();
        
        TextProperty = RegisterPropertyDescriptor(new EditableControlPropertyDescriptor<string>("Text", () => Text, "Content".ToPipedPath(),
            "The text to display.", s => Text = s ?? string.Empty, s => !string.IsNullOrEmpty(s)));
        TextColorProperty = RegisterPropertyDescriptor(new EditableControlPropertyDescriptor<Color>("Text color", () => TextColor, "Content".ToPipedPath(),
            "The color of the text.", c => TextColor = c));
        
        VerticalAlignmentProperty = RegisterPropertyDescriptor(new EditableControlPropertyDescriptor<ETextVAlignment>("Vertical alignment", () => _vAlignment, "Content".ToPipedPath(),
            "The vertical alignment of the text.", v => _vAlignment = v));
        HorizontalAlignmentProperty = RegisterPropertyDescriptor(new EditableControlPropertyDescriptor<ETextHAlignment>("Horizontal alignment", () => _hAlignment, "Content".ToPipedPath(),
            "The horizontal alignment of the text.", h => _hAlignment = h));
        
        SizeProperty.ValueChanged += (newValue, oldValue) =>
        {
            _vAlignmentDirty = true;
            _hAlignmentDirty = true;
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
            _hAlignmentDirty = true;
            if (newValue is not ESizeUnitType newUnit || oldValue is not ESizeUnitType oldUnit)
            {
                return;
            }

            _hasWUnitBeenManuallyChanged = newUnit != ESizeUnitType.Pixels;
        };
        
        SizeYUnitProperty.ValueChanged += (newValue, oldValue) =>
        {
            _vAlignmentDirty = true;
            if (newValue is not ESizeUnitType newUnit || oldValue is not ESizeUnitType oldUnit)
            {
                return;
            }
            
            _hasHUnitBeenManuallyChanged = newUnit != ESizeUnitType.Pixels;
        };

        VerticalAlignmentProperty.ValueChanged += (_, _) =>
        {
            _vAlignmentDirty = true;
        };
        
        HorizontalAlignmentProperty.ValueChanged += (_, _) => 
        {
            _hAlignmentDirty = true;
        };

    }

    public override void OnUpdate()
    {
    }

    public override void SyncVisual()
    {
        if(!IsPropertiesInitialized || Font == null)
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
        CalculateAlignment();
    }

    private void CalculateAlignment()
    {
        var sizeValue = Visual.GlobalBounds;
        
        if (_hAlignmentDirty)
        {
            float newXOffset = (_hAlignment) switch
            {
                ETextHAlignment.Center => (sizeValue.Width - Font.MeasureString(Text).X) / 2,
                ETextHAlignment.Right => sizeValue.Width - Font.MeasureString(Text).X,
                _ => 0
            };
            (Visual as TextVisual).TextOffset = new Vector2(newXOffset, (Visual as TextVisual).TextOffset.Y);
            _hAlignmentDirty = false;
        }

        if (_vAlignmentDirty)
        {
            float newYOffset = (_vAlignment) switch
            {
                ETextVAlignment.Center => (sizeValue.Height - Font.MeasureString(Text).Y) / 2,
                ETextVAlignment.Bottom => sizeValue.Height - Font.MeasureString(Text).Y,
                _ => 0
            };
            (Visual as TextVisual).TextOffset = new Vector2((Visual as TextVisual).TextOffset.X, newYOffset);
            _vAlignmentDirty = false;
        }
    }
}