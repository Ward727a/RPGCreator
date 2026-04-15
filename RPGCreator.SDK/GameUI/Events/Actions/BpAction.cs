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

using RPGCreator.SDK.Assets.Definitions.Blueprints;
using RPGCreator.SDK.Attributes;
using RPGCreator.SDK.GameUI.Events.Contexts;
using RPGCreator.SDK.Graph;
using RPGCreator.SDK.Logging;

namespace RPGCreator.SDK.GameUI.Events.Actions;

public partial class BpAction : IGuiAction
{
    public string Name => "Blueprint Action";
    public string Description => "Allows to create a custom action from a blueprint.";
    public Type[] SupportedEventContexts { get; } = [typeof(GuiEventContext)];

    [GuiControlProperty(DisplayName = "Blueprint", Description = "The blueprint to use for this action.")]
    private BlueprintData? _blueprintData;
    
    private BaseBpCompiledLogic _logic;
    
    private readonly CompiledBpContext _compiledBpContext = new(1);
    
    public BpAction()
    {
        
    }

    public void Execute(GuiEventContext context)
    {
        _compiledBpContext.Arguments.SetArgument(0, BpValue.FromObject(context));
        
        if(_logic.Validate(_compiledBpContext))
            _logic.Execute(_compiledBpContext);
        else
            Logger.Error("Blueprint action failed to validate.");
    }

    public bool Match(GuiEventContext context)
    {
        if (_blueprintData == null)
            return false;
        
        var arguments = _blueprintData.Parameters.Where(p => p.IsArgument);
            
        if(arguments.FirstOrDefault() is not { } BpValue)
            return false;

        return BpValue.Type == context.GetType();
    }

    public IGuiAction Clone()
    {
        return new BpAction()
        {
            _logic = _logic
        };
    }
}