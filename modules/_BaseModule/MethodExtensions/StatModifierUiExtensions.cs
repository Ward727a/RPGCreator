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

using _BaseModule.AssetDefinitions.BaseStats;
using _BaseModule.UI.StatsModifier;
using Avalonia.Controls;
using RPGCreator.SDK;
using RPGCreator.SDK.EditorUiService;
using RPGCreator.UI.Common;
using Ursa.Controls;

namespace RPGCreator.UI.Extensions;

using AssetManagerScope = UiExtensionExtensions_Generated.AssetsManagerScope;

#region Context

public class StatModifierEditorContext
{
    private readonly Config _config;
    
    public Grid EditorGrid => _config.GetEditorGrid();
    public ScrollViewer EditorScrollViewer => _config.GetEditorScrollViewer();
    public StackPanel EditorPanel => _config.GetEditorPanel();
    public TextBox NameTextBox => _config.GetNameTextBox();
    public TextBox DescriptionTextBox => _config.GetDescriptionTextBox();
    public Grid SelectStatPanel => _config.GetSelectStatPanel();
    public Button SelectStatForModifier => _config.GetSelectStatForModifier();
    public TextBlock SelectedStatTextBlock => _config.GetSelectedStatTextBlock();
    public Grid ModifierTypePanel => _config.GetModifierTypePanel();
    public Label ModifierTypeLabel => _config.GetModifierTypeLabel();
    public ComboBox ModifierTypeComboBox => _config.GetModifierTypeComboBox();
    public Grid StackingTypePanel => _config.GetStackingTypePanel();
    public Label StackingTypeLabel => _config.GetStackingTypeLabel();
    public ComboBox StackingTypeComboBox => _config.GetStackingTypeComboBox();
    public NumericFloatUpDown ValueUpDown => _config.GetValueUpDown();
    public Grid DurationPanel => _config.GetDurationPanel();
    public Label DurationHoursLabel => _config.GetDurationHoursLabel();
    public NumericDoubleUpDown DurationHoursUpDown => _config.GetDurationHoursUpDown();
    public Label DurationMinutesLabel => _config.GetDurationMinutesLabel();
    public NumericDoubleUpDown DurationMinutesUpDown => _config.GetDurationMinutesUpDown();
    public Label DurationSecondsLabel => _config.GetDurationSecondsLabel();
    public NumericDoubleUpDown DurationSecondsUpDown => _config.GetDurationSecondsUpDown();
    public Label DurationMillisecondsLabel => _config.GetDurationMillisecondsLabel();
    public NumericDoubleUpDown DurationMillisecondsUpDown => _config.GetDurationMillisecondsUpDown();
    public StackPanel ButtonsPanel => _config.GetButtonsPanel();
    public Button SaveButton => _config.GetSaveButton();
    public Button CancelButton => _config.GetCancelButton();
    public HelpButton HelpButton => _config.GetHelpButton();
    public StatModifierDefinition StatModifier => _config.GetStatModifier();
    
    /// <summary>Configuration object to initialize the context.</summary>
    public class Config
    {
        public Func<Grid> GetEditorGrid { get; init; } = null!;
        public Func<ScrollViewer> GetEditorScrollViewer { get; init; } = null!;
        public Func<StackPanel> GetEditorPanel { get; init; } = null!;
        public Func<TextBox> GetNameTextBox { get; init; } = null!;
        public Func<TextBox> GetDescriptionTextBox { get; init; } = null!;
        public Func<Grid> GetSelectStatPanel { get; init; } = null!;
        public Func<Button> GetSelectStatForModifier { get; init; } = null!;
        public Func<TextBlock> GetSelectedStatTextBlock { get; init; } = null!;
        public Func<Grid> GetModifierTypePanel { get; init; } = null!;
        public Func<Label> GetModifierTypeLabel { get; init; } = null!;
        public Func<ComboBox> GetModifierTypeComboBox { get; init; } = null!;
        public Func<Grid> GetStackingTypePanel { get; init; } = null!;
        public Func<Label> GetStackingTypeLabel { get; init; } = null!;
        public Func<ComboBox> GetStackingTypeComboBox { get; init; } = null!;
        public Func<NumericFloatUpDown> GetValueUpDown { get; init; } = null!;
        public Func<Grid> GetDurationPanel { get; init; } = null!;
        public Func<Label> GetDurationHoursLabel { get; init; } = null!;
        public Func<NumericDoubleUpDown> GetDurationHoursUpDown { get; init; } = null!;
        public Func<Label> GetDurationMinutesLabel { get; init; } = null!;
        public Func<NumericDoubleUpDown> GetDurationMinutesUpDown { get; init; } = null!;
        public Func<Label> GetDurationSecondsLabel { get; init; } = null!;
        public Func<NumericDoubleUpDown> GetDurationSecondsUpDown { get; init; } = null!;
        public Func<Label> GetDurationMillisecondsLabel { get; init; } = null!;
        public Func<NumericDoubleUpDown> GetDurationMillisecondsUpDown { get; init; } = null!;
        public Func<StackPanel> GetButtonsPanel { get; init; } = null!;
        public Func<Button> GetSaveButton { get; init; } = null!;
        public Func<Button> GetCancelButton { get; init; } = null!;
        public Func<HelpButton> GetHelpButton { get; init; } = null!;
        public Func<StatModifierDefinition> GetStatModifier { get; init; } = null!;
    }
    
    public StatModifierEditorContext(Config config)
    {
        _config = config;
    }
}

#endregion

public static class StatModifierUiExtensions
{
    private static IUiExtensionManager Manager => EditorUiServices.ExtensionManager;

    public static AssetManagerScope StatModifierEditor(
        this AssetManagerScope context, Action<StatModifierEditor, StatModifierEditorContext> callBack)
    {
        Manager.RegisterExtension(new ("BaseModule.StatModifierEditor"), (target, ctx) =>
        {
            if(ctx is StatModifierEditorContext typedCtx && target is StatModifierEditor editor)
                callBack(editor, typedCtx);
        });
        return context;
    }
}