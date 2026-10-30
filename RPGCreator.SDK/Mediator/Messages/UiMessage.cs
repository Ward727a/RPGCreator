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

using MediatR;
using RPGCreator.Shared.Types;

namespace RPGCreator.SDK.Mediator.Messages;

public static class UiMessage
{
    public readonly record struct ActivatedView(URN ViewId, object? Args = null) : INotification;
    public readonly record struct OpenedView(URN ViewId) : INotification;
    public readonly record struct ClosedView(URN ViewId) : INotification;
    public readonly record struct Dropped(URN ViewId) : INotification;
    public readonly record struct BindMessage(URN MessageId, Dictionary<string, object?> Arguments) : INotification;
}