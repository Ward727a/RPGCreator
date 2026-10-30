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
using RPGCreator.SDK.Common.Logging;
using RPGCreator.SDK.Mediator.Messages;
using RPGCreator.SDK.Modules.Editor.Ui;

namespace RPGCreator.Editor.Handlers;

public class ProjectOpenedHandler(IEditorUiStore editorUiStore,IEditorUiRouter router) : INotificationHandler<ProjectMessage.ProjectOpened>
{
    public Task Handle(ProjectMessage.ProjectOpened notification, CancellationToken cancellationToken)
    {
        editorUiStore.LoadFromDisk();
        
        router.DropView("rpgc://views/editor_window");
        router.ActivateView("rpgc://views/editor_window").OnSuccess(view =>
        {
            if (view is IEditorUiWindow viewWindow)
            {
                EditorWindowManager.Open(viewWindow);
            }
        }).OnFailure(err =>
        {
            Logger.Error("Failed to activate the base editor window, due to: {err}", err);
        });
        return Task.CompletedTask;
    }
}