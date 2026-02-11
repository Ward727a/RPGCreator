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

using System.Globalization;
using _BaseModule.Enums;
using _BaseModule.Features.Entity;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using RPGCreator.SDK;
using RPGCreator.SDK.Assets.Definitions.Stats;
using RPGCreator.SDK.Modules.Definition;
using RPGCreator.SDK.Modules.SimpleEvents;
using RPGCreator.SDK.Types;
using Ursa.Controls;

namespace _BaseModule.SimpleEvents.Conditions;

public class StatValueSimpleCondition : BaseSimpleEventCondition
{
    public override URN Urn => Module.ToUrnModule("rpgc").ToUrn("stat_value_condition");
    public override string Name => "Stat Value Condition";
    public override string Description => "Checks if a stat value meets a certain condition.";
    public override bool ResultExpected => true;
    public override bool EvaluateCondition(CustomData context, CustomData parameters)
    {
        var world = RuntimeServices.GameSession.ActiveEcsWorld;
        if(world == null)
            return false;

        var componentManager = world.ComponentManager;
        
        context.GetOrDefault("entityId", -1, out int entityId);
        context.GetOrDefault("statIdx", -1, out int ctxStatId);
        
        // If [statUnique].[valueType] [comparison] [value] then ...
        // If health.actualValue < 50 then ...
        // If health actualValue < 50 then ...
        
        parameters.GetOrDefault("statUnique", Ulid.Empty, out Ulid statUnique);
        parameters.GetOrDefault("valueType", StatValueType.ActualValue, out StatValueType valueType);
        parameters.GetOrDefault("comparison", ComparisonType.Equal, out ComparisonType comparison);
        parameters.GetOrDefault("value", 0.0, out double value);
        
        if (entityId == -1 || statUnique == Ulid.Empty || ctxStatId == -1)
            return false;

        if (!componentManager.HasComponent<StatComponent>(entityId))
            return false;
        
        ref var statComponent = ref componentManager.GetComponent<StatComponent>(entityId);
        ReadOnlySpan<StatData> statsSpan = statComponent.Stats.AsSpan();
        var statData = statsSpan[ctxStatId];
        
        if(statData.StatDefId != statUnique)
            return false;
        
        double valueToCheck = valueType switch
        {
            StatValueType.ActualValue => statData.ActualValue,
            StatValueType.FinalValue => statData.FinalValue,
            StatValueType.BaseValue => statData.BaseValue,
            StatValueType.MinValue => statData.MinValue,
            _ => statData.ActualValue
        };
        
        return comparison switch
        {
            ComparisonType.Equal => Math.Abs(valueToCheck - value) < 0.0001,
            ComparisonType.NotEqual => Math.Abs(valueToCheck - value) > 0.0001,
            ComparisonType.Greater => valueToCheck > value,
            ComparisonType.Less => valueToCheck < value,
            ComparisonType.GreaterOrEqual => valueToCheck >= value,
            ComparisonType.LessOrEqual => valueToCheck <= value,
            _ => false
        };
        
    }

    public override string GetConditionText(CustomData parameters)
    {
        var statName = "[CONDITION ERROR: Specified stat not found]";
        if (EngineServices.AssetsManager.TryResolveAsset(parameters.GetAs<Ulid>("statUnique"),
                out BaseStatDefinition? statDef))
        {
            statName = statDef.DisplayName;
        }
        return $"the {parameters.GetAs<StatValueType>("valueType").ToDisplayString()} {statName} {parameters.GetAs<ComparisonType>("comparison").ToReadableString()} {parameters.GetAs<double>("value")}";
    }

    public override Dictionary<string, string> GetConditionsTextAsPart(CustomData parameters)
    {
        if (!EngineServices.AssetsManager.TryResolveAsset(parameters.GetAs<Ulid>("statUnique"),
                out BaseStatDefinition? statDef))
        {
            return new Dictionary<string, string>
            {
                {"@b_statUnique", "the "}, // Here we use a special key with @ for a static text, and b for before (just for a better readability). The only important part is the '@' !!
                {"valueType", $"{parameters.GetAs<StatValueType>("valueType").ToDisplayString()}"},
                {"statUnique", "[CONDITION ERROR: Specified stat not found]"},
                {"comparison", parameters.GetAs<ComparisonType>("comparison").ToReadableString()},
                {"value", parameters.GetAs<double>("value").ToString(CultureInfo.InvariantCulture)}
            };
        }
        return new Dictionary<string, string>
        {
            {"@b_statUnique", "the "}, // Here we use a special key with @b_ prefix, meaning: @b_ => [@]AT [b]Before, we can also use @a_ for after. NO SPACE IS ADDED AUTOMATICALLY!
            {"valueType", $"{parameters.GetAs<StatValueType>("valueType").ToDisplayString()}"},
            {"statUnique", statDef.DisplayName}, // Here the space is added automatically after the stat name (due to the fact that this is not a special prefix).
            {"comparison", parameters.GetAs<ComparisonType>("comparison").ToReadableString()},
            {"value", parameters.GetAs<double>("value").ToString(CultureInfo.InvariantCulture)} // Here no space is added automatically before the value, as it's the last part of the condition text.
        };
    }

    public override List<SimpleEventPropertyDescriptor> GetConditionProperties()
    {
        return new List<SimpleEventPropertyDescriptor>
        {
            new SimpleEventPropertyDescriptor
            (
                0,
                "statUnique",
                typeof(BaseStatDefinition),
                "Stat to check",
                "The stat to check. This determines which stat of the entity will be checked against the specified value.", 
                async (data) =>
                {
                    var assets = EngineServices.AssetsManager.GetAssetsOfType<BaseStatDefinition>();
                    
                    var result = await EditorUiServices.DialogService.ShowSelectAsync("Select a stat",
                        "Select the stat to check in the condition.",
                        assets,
                        asset => asset.DisplayName,
                        confirmButtonText: "Select",
                        cancelButtonText: "Cancel");

                    return result; // If return true, then the engine will do NOTHING and we are responsible for setting the value in the parameters.
                }
            ),
            new SimpleEventPropertyDescriptor
            (
                1,
                "valueType",
                typeof(StatValueType),
                "Type of value",
                "The type of value to compare. This determines which value of the stat will be used for the comparison.",
                async (data) =>
                {
                    var options = Enum.GetValues(typeof(StatValueType));

                    var comboBox = new ComboBox()
                    {
                        ItemsSource = options,
                        ItemTemplate = new FuncDataTemplate<StatValueType> ((value, _) =>
                        {
                            var textBlock = new TextBlock()
                            {
                                Text = value.ToDisplayString()
                            };
                            return textBlock;
                        }),
                        SelectedItem = data.GetAs<StatValueType>("valueType")
                    };
                    comboBox.SelectionChanged += (sender, args) =>
                    {
                        if (comboBox.SelectedItem != null)
                        {
                            data.Set("valueType", comboBox.SelectedItem);
                        }
                    };
                    return comboBox;
                },
                DefaultValue: StatValueType.ActualValue
            ),
            new SimpleEventPropertyDescriptor
            (
                2,
                "comparison",
                typeof(ComparisonType),
                "Comparison",
                "The type of comparison to perform. This determines how the stat value will be compared to the specified value.",
                async (data) =>
                {
                    var options = Enum.GetValues(typeof(ComparisonType));

                    var comboBox = new ComboBox()
                    {
                        ItemsSource = options,
                        ItemTemplate = new FuncDataTemplate<ComparisonType> ((value, _) =>
                        {
                            var textBlock = new TextBlock()
                            {
                                Text = value.ToReadableString()
                            };
                            return textBlock;
                        }),
                        SelectedItem = data.GetAs<ComparisonType>("comparison")
                    };
                    comboBox.SelectionChanged += (sender, args) =>
                    {
                        if (comboBox.SelectedItem != null)
                        {
                            data.Set("comparison", comboBox.SelectedItem);
                        }
                    };
                    return comboBox;
                },
                DefaultValue: ComparisonType.Equal
            ),
            new SimpleEventPropertyDescriptor
            (
                3,
                "value",
                typeof(double),
                "The value to compare the stat against.",
                "The value to compare the stat against. This is the value that the stat will be compared to using the specified comparison type.",
                async (data) =>
                {
                    var comboBox = new NumericDoubleUpDown()
                    {
                        Value = data.GetAs<double>("value"),
                        Minimum = double.MinValue,
                        Maximum = double.MaxValue,
                    };
                    comboBox.ValueChanged += (sender, args) =>
                    {
                        if (comboBox.Value != null)
                        {
                            data.Set("value", comboBox.Value.Value);
                        }
                    };
                    return comboBox;
                },
                DefaultValue: 0
            )
        };
    }
}