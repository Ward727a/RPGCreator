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

using System.Runtime.CompilerServices;
using RPGCreator.SDK.Types;
using RPGCreator.Shared.Types;

namespace RPGCreator.SDK.Assets.Runtime;

public class RuntimeMaker : IRuntimeMaker
{
    private Dictionary<Type, Delegate> Makers { get; } = new();
    private ConditionalWeakTable<IDefinition, IRuntime> Runtimes { get; } = new();

    internal RuntimeMaker()
    {
    }

    public void RegisterMaker<TDefinition, TRuntime>(
        Func<TDefinition, Result<TRuntime>> maker,
        bool overrideExisting = false) 
        where TDefinition : class, IDefinition<TRuntime> 
        where TRuntime : class, IRuntime<TDefinition>
    {
        var tDef = typeof(TDefinition);
        if (HasMaker(tDef))
        {
            if (!overrideExisting)
                return;
            RemoveMaker(tDef);
        }
        
        AddMaker(tDef, maker);
    }

    public Result<TRuntime> CreateRuntime<TRuntime, TDefinition>(
        TDefinition definition, 
        bool createNew = false)
        where TRuntime : class, IRuntime<TDefinition> 
        where TDefinition : class, IDefinition<TRuntime>
    {
        var tDef = typeof(TDefinition);
        
        if(TryGetRuntime(definition, out var runtime) && !createNew)
        {
            if(runtime is TRuntime typedRuntime)
                return Result<TRuntime>.Success(typedRuntime);
        }
        
        if (HasMaker(tDef))
        {
            var maker = GetMaker<TDefinition, TRuntime>(tDef);
            if (maker != null)
                return maker(definition).OnSuccess(runtime =>
                {
                    RemoveRuntime(definition);
                    AddRuntime(definition, runtime);
                });
        }
        
        return Result<TRuntime>.Failure($"No maker registered for {tDef.Name}");
    }
    
    #region HelpersMaker
    private bool HasMaker(Type tDef) => Makers.TryGetValue(tDef, out _);
    private bool AddMaker(Type tDef, Delegate maker) => Makers.TryAdd(tDef, maker);
    private bool RemoveMaker(Type tDef) => Makers.Remove(tDef);
    private Func<TDefinition, Result<TRuntime>>? GetMaker<TDefinition, TRuntime>(Type tDef)    
    {
        if (Makers.TryGetValue(tDef, out var maker))
        {
            if (maker is Func<TDefinition, Result<TRuntime>> typedMaker)
                return typedMaker;
        }
        return null;
    }
    #endregion
    
    #region HelpersRuntime
    private bool HasRuntime(IDefinition definitionPoco) => Runtimes.TryGetValue(definitionPoco, out _);
    private bool AddRuntime(IDefinition definitionPoco, IRuntime runtime) => Runtimes.TryAdd(definitionPoco, runtime);
    private bool RemoveRuntime(IDefinition definitionPoco) => Runtimes.Remove(definitionPoco);
    private IRuntime? GetRuntime(IDefinition definitionPoco) => Runtimes.TryGetValue(definitionPoco, out var runtime) ? runtime : null;
    private bool TryGetRuntime(IDefinition definitionPoco, out IRuntime? runtime) => Runtimes.TryGetValue(definitionPoco, out runtime);
    #endregion
}