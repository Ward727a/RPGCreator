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

using System.Diagnostics.CodeAnalysis;
using RPGCreator.SDK.Projects;
using RPGCreator.SDK.Types.Interfaces;

namespace RPGCreator.SDK.EngineService;

public interface IProjectsManager : IService
{
    public event Action<IBaseProject>? OnProjectOpened;
    
    List<BaseProjectLink> GetAllProjects();
    public IBaseProject? CreateProject(string projectName, string projectPath);
    public bool TryGetProject(string configPath, [NotNullWhen(true)] out IBaseProject? project);
    public void OpenProject(IBaseProject project);
    public void CloseCurrentProject();
    public IBaseProject? GetCurrentProject();
}