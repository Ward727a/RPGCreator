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
using RPGCreator.Editor.UI.Views;
using RPGCreator.SDK.Mediator.Messages;
using RPGCreator.SDK.Modules.Editor.Ui;
using RPGCreator.Shared.Attributes;
using RPGCreator.Shared.Enums;
using RPGCreator.Shared.Types;

namespace RPGCreator.Editor.Handlers;


[EngineService(typeof(INotificationHandler<UiMessage.ClosedView>), ServiceLifetime.Singleton, EngineModeFlags.Editor)]
public class UiWindowHandler(IEditorUiRouter router) : INotificationHandler<UiMessage.ClosedView>
{
    public Task Handle(UiMessage.ClosedView notification, CancellationToken cancellationToken)
    {
        router.GetIfActive(new URN("rpgc://views/editor_window"))
            .Cast<MainEditorUiWindow>()
            .OnSuccess(view => view.RemoveOpenedTool(notification.ViewId));
        return Task.CompletedTask;
    }
}