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

using CommunityToolkit.Diagnostics;
using RPGCreator.SDK.Assets.Compiler;
using RPGCreator.SDK.ECS;
using RPGCreator.SDK.Registry;

namespace RPGCreator.EngineLib.Registry;

public class RuntimeCompilerRegistry : IRuntimeCompilerRegistry
{
    private Dictionary<Ulid, IAssetRuntimeCompiler> _registeredCompilers = new();
    private Dictionary<Type, Ulid> _registeredCompilerIds = new();

    public RuntimeCompilerRegistry()
    {
        RegisterRuntimeCompiler(new MapRuntimeCompiler());
        RegisterRuntimeCompiler(new TileLayerRuntimeCompiler());
        RegisterRuntimeCompiler(new TileChunkRuntimeCompiler());
    }
    
    public Ulid RegisterRuntimeCompiler<TSource>(IAssetRuntimeCompiler<TSource> compiler, bool overrideIfExist = false) where TSource : class
    {
        if (HasRegisteredCompiler(typeof(TSource)) && !overrideIfExist)
        {
            return _registeredCompilerIds[typeof(TSource)];
        }
        
        var uniqueId = Ulid.NewUlid();
        _registeredCompilers[uniqueId] = compiler;
        _registeredCompilerIds[typeof(TSource)] = uniqueId;
        return uniqueId;
    }

    public void Compile(Type sourceType, object source, IEcsWorld world, ICompilerContext? context = null)
    {
        if(!HasRegisteredCompiler(sourceType))
        {
            throw new Exception($"No compiler registered for type {sourceType.FullName}");
        }
        var id = _registeredCompilerIds[sourceType];
        _registeredCompilers[id].Compile(source, world, context);
    }

    public TCompiler GetRegisteredCompiler<TCompiler, TSource>() where TSource : class
    {
        if(!HasRegisteredCompiler(typeof(TSource)))
        {
            throw new Exception($"No compiler registered for type {typeof(TSource).FullName}");
        }
        
        var compiler = _registeredCompilers[_registeredCompilerIds[typeof(TSource)]];
        Guard.IsOfType<TCompiler>(compiler);
        
        return (TCompiler) compiler;
    }

    public bool UnregisterRuntimeCompiler(Ulid uniqueId)
    {
        if(!_registeredCompilers.ContainsKey(uniqueId))
        {
            return false;
        }
        
        _registeredCompilers.Remove(uniqueId);
        return true;
    }

    public void Compile<TSource>(TSource source, IEcsWorld world, ICompilerContext? context = null) where TSource : class
    {
        if(!HasRegisteredCompiler(typeof(TSource)))
        {
            throw new Exception($"No compiler registered for type {typeof(TSource).FullName}");
        }
        var id = _registeredCompilerIds[typeof(TSource)];
        _registeredCompilers[id].Compile(source, world, context);
    }

    public void Compile(Ulid compilerId, object source, IEcsWorld world, ICompilerContext? context = null)
    {
        if(!_registeredCompilers.ContainsKey(compilerId))
        {
            throw new Exception($"No compiler registered with id {compilerId}");
        }
        _registeredCompilers[compilerId].Compile(source, world, context);
    }

    public bool HasRegisteredCompiler(Ulid uniqueId)
    {
        return _registeredCompilers.ContainsKey(uniqueId);
    }

    public bool HasRegisteredCompiler(Type sourceType)
    {
        return _registeredCompilerIds.ContainsKey(sourceType) && _registeredCompilers.ContainsKey(_registeredCompilerIds[sourceType]);
    }
}