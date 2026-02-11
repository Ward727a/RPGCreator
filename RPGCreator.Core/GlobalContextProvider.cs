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

using System.Collections.Concurrent;
using RPGCreator.SDK.EngineService;
using RPGCreator.SDK.Types;

namespace RPGCreator.Core;

public class GlobalContextProvider : IGlobalContextProvider
{
    private readonly ConcurrentDictionary<URN, Func<object>> _providers = new();

    public UrnSingleModule ModuleForContext => "global_context".ToUrnSingleModule();

    public void RegisterProvider(URN key, Func<object> resolver)
    {
        _providers[key] = resolver;
    }

    public T? Get<T>(URN key)
    {
        if (_providers.TryGetValue(key, out var resolver))
        {
            var result = resolver();
            if (result is T typedResult)
            {
                return typedResult;
            }
        }
        return default;
    }
}