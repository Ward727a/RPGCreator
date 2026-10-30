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

using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using RPGCreator.SDK.Common.Logging;
using RPGCreator.SDK.Mediator.Messages;
using RPGCreator.Shared.Attributes;
using RPGCreator.Shared.Enums;
using RPGCreator.Shared.Types;

namespace RPGCreator.Editor.Handlers;

[EngineService(typeof(INotificationHandler<UiMessage.BindMessage>), ServiceLifetime.Singleton, EngineModeFlags.Editor)]
public class UIMessageHandler : INotificationHandler<UiMessage.BindMessage>
{
    private static readonly URN MessageId = new("rpgc://messages/menu_click/asset_manager"); 
    
    public Task Handle(UiMessage.BindMessage notification, CancellationToken cancellationToken)
    {
        if (notification.MessageId != MessageId)
            return Task.CompletedTask;
        
        Logger.Debug("Received message: {MessageId}", notification.MessageId);
        return Task.CompletedTask;
    }
}