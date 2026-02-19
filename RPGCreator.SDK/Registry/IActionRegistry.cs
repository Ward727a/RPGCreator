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

using RPGCreator.SDK.Types;

namespace RPGCreator.SDK.Registry;

public record struct ActionInfo
{
    public URN Urn { get; set; }
    public PipedPath ActionPath { get; set; }
    public string DisplayName { get; set; }
    public string Description { get; set; }
    public Action<object[]?> Action { get; set; }
}

public interface IActionRegistry : IService
{
    public void RegisterAction(ActionInfo actionInfo, bool overrideIfExists = false);
    public void UnregisterAction(URN urn);
    public ActionInfo? GetAction(URN urn);
}