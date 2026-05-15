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

using RPGCreator.SDK.Assets;
using RPGCreator.SDK.Assets.Definitions;

namespace RPGCreator.SDK.EngineService;

public interface IGameFactory : IService
{
    public void Register<TInst, TDef>(IAssetFactory<TInst, TDef> factory)
        where TInst : class
        where TDef : IBaseAssetDef;

    public TInst CreateInstance<TInst>(IBaseAssetDef def) where TInst : class;

    public ValueTask<TInst> CreateInstanceAsync<TInst>(IBaseAssetDef def, CancellationToken ct = default)
        where TInst : class;

    public void ReleaseInstance<TInst>(TInst instance) where TInst : class;

    public void Release(IBaseAssetDef def);
    
    public void Refresh(IBaseAssetDef def);

    public void ClearAll();
}