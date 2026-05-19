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

using Microsoft.Extensions.DependencyInjection;
using RPGCreator.EditorLib.Bootstrapper;
using RPGCreator.EngineLib;
using RPGCreator.EngineLib.Types.Interfaces;

namespace RPGCreator.EditorLib;

public static class EditorBootstrapper
{
    public static void AddEditorServices(this IServiceCollection services)
    {
        EngineBootstrapper.AddServices(services);
        EditorUiBootstrapper.AddServices(services);
        RuntimeBootstrapper.AddServices(services);
    }
}