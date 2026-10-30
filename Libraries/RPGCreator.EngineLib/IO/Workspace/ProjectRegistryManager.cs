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

using System.Text.Json;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using RPGCreator.SDK.Common.Logging;
using RPGCreator.SDK.IO.Workspace;
using RPGCreator.SDK.Mediator.Messages;
using RPGCreator.SDK.Services.EngineService;
using RPGCreator.Shared.Attributes;
using RPGCreator.Shared.Enums;

namespace RPGCreator.EngineLib.IO.Workspace;

[EngineService(typeof(IProjectRegistryManager), ServiceLifetime.Singleton, EngineModeFlags.All)]
public class ProjectRegistryManager(IEngineConfig engineConfig, IMediator mediator) : IProjectRegistryManager
{
    private readonly string _workspaceConfigPath = Path.Combine(engineConfig.Global.Paths.ConfigFolder, "projects.json");

    public WorkspaceList Projects { get; private set; } = new();

    public async Task LoadAsync()
    {
        if (!File.Exists(_workspaceConfigPath))
        {
            Projects = new WorkspaceList();
            await SaveAsync();
            return;
        }

        try
        {
            var json = await File.ReadAllTextAsync(_workspaceConfigPath);
            Projects = JsonSerializer.Deserialize<WorkspaceList>(json) ?? new();
            await mediator.Publish(new ProjectMessage.ProjectListLoaded(Projects));
        }
        catch (Exception e)
        {
            Projects = new WorkspaceList();
            Logger.Error(e, "Failed to load project registry.");
        }
    }

    public async Task SaveAsync()
    {
        var json = JsonSerializer.Serialize(Projects, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(_workspaceConfigPath, json);
        await mediator.Publish(new ProjectMessage.ProjectListSaved());
    }
}