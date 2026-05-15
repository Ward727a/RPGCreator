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

namespace RPGCreator.SDK.Common.Debug;

/// <summary>
/// Debug build settings, you can add your own const here, and edit them then rebuild the engine.<br/>
/// This allow to have a main "panel" to enable/disable debug features.<br/>
/// <br/>
/// This should ONLY be used for heavy debug features that can impact performance or memory usage if enabled, and/or if your class COULD be initialized before EngineConfig. 
/// <br/>
/// <br/>
/// WARNING: Do not enable these settings in production, they ARE NOT optimized at ALL, and WILL NOT BE optimized!!!
/// </summary>
public static class DebugBuildSettings
{
    internal const bool EngineTypesRegistry = false;
    internal const bool EngineTypesRegistry_Analyzer = true;
}