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

using RPGCreator.SDK;
using RPGCreator.SDK.Attributes;
using RPGCreator.SDK.EngineService;
using RPGCreator.SDK.Logging;
using RPGCreator.SDK.Projects;

namespace RPGCreator.Core.Managers.ProjectsManager;

[EClass(DisplayName = "Project Config", Description = "Configuration for a project.", SerializeInProjectFolder = false)]
public partial class ProjectConfig : BaseConfig
{
    public List<BaseProjectLink> ProjectLinks = new List<BaseProjectLink>();

    private void AddProject(IBaseProject project)
    {

        if (project == null)
        {
            throw new ArgumentNullException(nameof(project), "Project cannot be null.");
        }

        var link = BaseProjectLink.CreateLinkFromProject(project);
    
        ProjectLinks.Add(link);
    }

    public bool AddOrUpdateProject(IBaseProject project)
    {
        MarkDirty();
        var link = ProjectLinks.Find(link => link.ProjectID == project.Id);
        
        if(link == null)
        {
            AddProject(project);
            link = ProjectLinks.Last();
        }
        
        string projectFilePath = link.ProjectConfigPath;
        
        if (string.IsNullOrEmpty(projectFilePath))
        {
            Logger.Error("Project config path is not set. Cannot save project.");
            return false;
        }
        
        var projectDir = Path.GetDirectoryName(projectFilePath);

        if (projectDir == null)
        {
            Logger.Error("Failed to get directory name for project config path: {ProjectConfigPath}", projectFilePath);
            return false;
        }
        
        if (!Directory.Exists(projectDir))
        {
            Directory.CreateDirectory(projectDir);
        }
        
        EngineServices.Serializer.SerializeTo(project, projectFilePath);
        return true;
    }
    
    protected override void _OnLoadedConfig()
    {
        ProjectLinks = Get("links", new List<BaseProjectLink>());
    }

    protected override void _OnSavedConfig()
    {
        Set("links", ProjectLinks);
    }
}