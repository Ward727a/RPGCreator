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

using System;
using System.Collections.Generic;
using System.IO;
using Hexa.NET.ImGui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RPGCreator.Editor.ImGuiLib;
using RPGCreator.SDK.Common.Logging;
using RPGCreator.SDK.Modules.Editor.Ui;
using RPGCreator.SDK.Projects;
using RPGCreator.SDK.Services.EngineService;
using Vector2 = System.Numerics.Vector2;
using Vector4 = System.Numerics.Vector4;

namespace RPGCreator.Editor.UI.Views;

public class EngineLauncher : SimpleEditorUiWindow
{
    private readonly IProjectsManager _projectsManager;
    private readonly Game _game;
    public static ImGuiWindowFlags Flags => ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoDocking | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse;

    private readonly ImAnimPointer<Vector4> _errorNameAnimInt = ImAnim.Create(ImGui.GetStyle().Colors[(int)ImGuiCol.Text], ImGuiExt.Colors.Error,
        .3f);
    private readonly ImAnimPointer<Vector4> _errorProjectAnimInt = ImAnim.Create(ImGui.GetStyle().Colors[(int)ImGuiCol.Text], ImGuiExt.Colors.Error,
        .3f);
    
    
    private readonly Texture2D _rpgCreatorLogo;
    public ImTextureRef RpgCreatorLogoImGui { get; set; }

    public EngineLauncher(IProjectsManager projectsManager, ImGuiRenderer imGuiRenderer, GraphicsDevice graphicsDevice, Game game, IEditorViewContext context, EditorViewDefinition definition) : base(context, definition)
    {
        
        _rpgCreatorLogo = LoadImageFromDisk(graphicsDevice, AppDomain.CurrentDomain.BaseDirectory + "/Content/logo.png");
        RpgCreatorLogoImGui = imGuiRenderer.BindTexture(_rpgCreatorLogo);
        _projectsManager = projectsManager;
        _game = game;
        ImAnim.Stop(_errorNameAnimInt);
        ImAnim.Stop(_errorProjectAnimInt);
    }


    public Texture2D LoadImageFromDisk(GraphicsDevice graphicsDevice, string filePath)
    {
        using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
    
        var value = Texture2D.FromStream(graphicsDevice, fileStream);
        value.Name = "FromLoadImageFromDisk_InEditor";
        return value;
    }
    
    public override void OnOpened()
    {
        ImGui.SetNextWindowFocus();
    }

    public override void OnClosed()
    {
        ImAnim.Destroy(_errorNameAnimInt);
        ImAnim.Destroy(_errorProjectAnimInt);
    }
    
    private bool _jumpToNewProject;
    private bool _deleteFoldersToo;
    
    public override void Draw(EDrawAs drawAs = EDrawAs.Unknown)
    {
        ImGuiViewportPtr viewport = ImGui.GetMainViewport();
        ImGui.SetNextWindowPos(viewport.WorkPos);
        ImGui.SetNextWindowSize(viewport.WorkSize);
        ImGui.SetNextWindowViewport(viewport.ID);
        
        if(ImGui.Begin(Name, Flags))
        {
            if(ImGui.BeginTabBar("EngineLauncherTabs"))
            {
                if (ImGui.BeginTabItem("Projects list"))
                {
                    DrawProjectList();
                    ImGui.EndTabItem();
                }

                ImGuiTabItemFlags tabItemFlags = ImGuiTabItemFlags.None;

                if (_jumpToNewProject)
                {
                    tabItemFlags |= ImGuiTabItemFlags.SetSelected;
                    _jumpToNewProject = false;
                }

                if (ImGui.BeginTabItem("New project", tabItemFlags))
                {
                    DrawNewProject();
                    ImGui.EndTabItem();
                }
            }
            ImGui.EndTabBar();
            
            DrawDeletePopup();
            DrawEditProjectPopup();
            DrawOpenLoadingPopup();
        }
        ImGui.End();

    }
    
    #region ProjectList
    private void DrawProjectList()
    {
        if(ImGui.BeginChild("Projects list"))
        {
            var projects = _projectsManager.GetAllProjects();
            if (projects.Count == 0)
            {
                DrawNoProjects();
            }
            else
            {
                DrawProjects(projects);
            }
        }
        ImGui.EndChild();
    }

    private void DrawNoProjects()
    {
        ImGui.CenteredText("No Project Found", ImGuiExt.CenteringDirection.Both);
        if (ImGui.CenteredButton("Create New Project", ImGuiExt.CenteringDirection.Horizontal))
        {
            _jumpToNewProject = true;
        }
    }

    private readonly Dictionary<string, IBaseProject> _projectMetadataCache = new();

    private void DrawProjects(List<ProjectLink> projectLinks)
    {
        if (ImGui.BeginChild("ProjectListRegion"))
        {
            ImGuiListClipper clipper = new ImGuiListClipper();
        
            clipper.Begin(projectLinks.Count);

            while (clipper.Step())
            {
                for (int i = clipper.DisplayStart; i < clipper.DisplayEnd; i++)
                {
                    var projectLink = projectLinks[i];

                    if (!_projectMetadataCache.TryGetValue(projectLink.ProjectLastKnownPath, out var metadata))
                    {
                        if (_projectsManager.TryGetProject(projectLink.ProjectLastKnownPath, out metadata))
                        {
                            _projectMetadataCache[projectLink.ProjectLastKnownPath] = metadata;
                        }
                        else
                        {
                            clipper.End(); 
                            continue; 
                        }
                    }

                    DrawSingleProjectItem(metadata, i);
                }
            }
        
        }
        ImGui.EndChild();
    }

    public IBaseProject? _selectedProject = null;
    private bool _deleteProject = false;
    private bool _editProject = false;
    private bool _openProject = false;
    
    private void DrawSingleProjectItem(IBaseProject project, int id)
    {
        var metadata = project.MetaData;
        var drawList = ImGui.GetWindowDrawList();

        drawList.ChannelsSplit(2);
        
        drawList.ChannelsSetCurrent(1);
        
        ImGui.Text($"Engine V{metadata.EditorVersion.ToString()}");
        if (ImGui.BeginItemTooltip())
        {
            ImGui.Text("The engine version that this project was made with.");
            ImGui.EndTooltip();
        }
        ImGui.SameLine();
        ImGui.SetCursorPosY(ImGui.GetCursorPosY() + ImGui.CalcTextSize("V").Y * .5f);
        ImGui.Separator();
        
        ImGui.PushID(id);
        
        ImGui.SetCursorPosX(ImGui.GetCursorPosX() + 10.0f);
        ImGui.BeginGroup();
        {
            if (ImGui.BeginTable("##project_table", 2))
            {
                ImGui.TableSetupColumn("##Icon", ImGuiTableColumnFlags.WidthFixed, 64f);
                ImGui.TableSetupColumn("##Data", ImGuiTableColumnFlags.WidthStretch);
                
                ImGui.TableNextColumn();
                ImGui.Image(RpgCreatorLogoImGui, new Vector2(64));

                ImGui.TableNextColumn();

                if (ImGui.BeginTable("##project_data", 2))
                {
                    ImGui.TableSetupColumn("##FirstColumn", ImGuiTableColumnFlags.WidthFixed);
                    ImGui.TableSetupColumn("##SecondColumn", ImGuiTableColumnFlags.WidthStretch);
                    
                    ImGui.TableNextColumn();
                    ImGui.Text("Name:");
                    ImGui.TableNextColumn();
                    ImGui.TextWrapped(metadata.Name);
                    
                    ImGui.TableNextColumn();
                    ImGui.Text("Description:");
                    ImGui.TableNextColumn();
                    if(string.IsNullOrWhiteSpace(metadata.Description))
                    {
                        ImGui.TextColored(ImGui.GetStyle().Colors[(int)ImGuiCol.TextDisabled], "No description available.");
                    }
                    else
                    {
                        ImGui.TextWrapped(metadata.Description);
                    }

                    ImGui.TableNextColumn();
                    ImGui.Text("Author(s):");
                    ImGui.TableNextColumn();
                    if (metadata.Authors.Count <= 0)
                    {
                        ImGui.TextColored(ImGui.GetStyle().Colors[(int)ImGuiCol.TextDisabled], "No author available.");
                    }
                    else
                    {
                        ImGui.TextWrapped(string.Join(", ", metadata.Authors));
                    }
                    
                    ImGui.TableNextColumn();
                    ImGui.Text("Directory:");
                    ImGui.TableNextColumn();
                    ImGui.TextWrapped(metadata.Directory);

                    if (_selectedProject == project)
                    {
                        ImGui.TableNextColumn();
                        ImGui.TableNextColumn();
                        
                        var availSize = ImGui.GetContentRegionAvail();
                        var paddingBetweenButton = ImGui.GetStyle().ItemSpacing.X;
                        
                        ImGui.BeginGroup();
                        ImGui.SetCursorPosX(availSize.X + paddingBetweenButton * 3);
                        if (ImGui.Button("Delete", ImGuiExt.ButtonType.Error))
                        {
                            _deleteProject = true;
                        }
                        ImGui.SameLine();
                        if (ImGui.Button("Edit"))
                        {
                            _editProject = true;
                        }
                        ImGui.SameLine();
                        if (ImGui.Button("Open"))
                        {
                            Logger.Debug("Open button clicked.");
                            _openProject = true;
                        }
                        ImGui.EndGroup();
                    }
                    
                    ImGui.EndTable();
                }

                ImGui.EndTable();
            }
        }
        ImGui.EndGroup();

        var isPressed = ImGui.IsItemClicked(ImGuiMouseButton.Left);

        if (isPressed)
        {
            _selectedProject = project;
        }
        
        var isHovered = ImGui.IsItemHovered();
        var min = ImGui.GetItemRectMin();
        var max = ImGui.GetItemRectMax();

        drawList.ChannelsSetCurrent(0);

        uint rectColor;

        if (isHovered)
        { 
            rectColor = ImGui.ColorConvertFloat4ToU32(new Vector4(.23f, .53f, .88f, .2f));
            drawList.AddRectFilled(min, max, rectColor, 4.0f); // 4.0f = arrondi des bords (optionnel)
        }

        if (isPressed || _selectedProject?.MetaData.Unique == metadata.Unique)
        {
            rectColor = ImGui.ColorConvertFloat4ToU32(new Vector4(.23f, .53f, .88f, .2f));
            drawList.AddRectFilled(min, max, rectColor, 4.0f); // 4.0f = arrondi des bords (optionnel)
        }
        
        drawList.ChannelsMerge();
        
        ImGui.PopID();
    }
    
    
    private void DrawDeletePopup()
    {
        if (_deleteProject)
        {
            ImGui.OpenPopup("Are you sure?");
            _deleteProject = false;
        }

        if (ImGui.BeginPopupModal("Are you sure?", ImGuiWindowFlags.AlwaysAutoResize))
        {
            if (_selectedProject == null)
            {
                ImGui.CloseCurrentPopup();
            }
            else
            {
                ImGui.Text("Are you sure that you want to delete the project '");
                ImGui.SameLine(0f, 0f);
                ImGui.TextColored(new Vector4(0.8f, 0.2f, 0.2f, 1.0f), _selectedProject.MetaData.Name);
                ImGui.SameLine(0f, 0f);
                ImGui.Text("'?");
                ImGui.Text("This project will be lost forever! (A long time!)");
                ImGui.Separator();

                ImGui.Checkbox("Also delete the project folder", ref _deleteFoldersToo);
                if (_deleteFoldersToo)
                {
                    ImGui.Text("WARNING: This will delete EVERYTHING in the project folder!");
                }
                
                ImGui.Separator();

                if (ImGui.Button("Yes, delete it", new Vector2(120, 0)))
                {
                    Logger.Debug($"Project '{_selectedProject.MetaData.Name}' deleted.");
                    _projectsManager.DeleteProject(_selectedProject);

                    if (_deleteFoldersToo)
                    {
                        Directory.Delete(_selectedProject.MetaData.Directory, true);
                    }

                    _deleteFoldersToo = false;
                    ImNotify.Show("Project deleted", $"Project '{_selectedProject.MetaData.Name}' deleted.", NotifyType.Error);
                    _selectedProject = null;
                    ImGui.CloseCurrentPopup();
                }
                
                ImGui.SameLine();
                
                if (ImGui.Button("Cancel", new Vector2(120, 0)))
                {
                    ImGui.CloseCurrentPopup();
                }
            }
            ImGui.EndPopup();
        }
    }

    private string _editedProjectName = "";
    private string _editedProjectDescription = "";
    private string _editedNewProjectAuthor = "";
    private List<string> _editedProjectAuthors = [];

    private void DrawEditProjectPopup()
    {
        
        if (_editProject)
        {
            ImGui.OpenPopup("Edit Project");
            _editProject = false;
                
            _editedProjectName = ""+_selectedProject.MetaData.Name;
            _editedProjectDescription = ""+_selectedProject.MetaData.Description;
            _editedProjectAuthors = [.. _selectedProject.MetaData.Authors];
        }

        if (ImGui.BeginPopupModal("Edit Project", ImGuiWindowFlags.NoResize))
        {
            ImGui.SetWindowSize(new Vector2(400, 300));
            if (_selectedProject == null)
            {
                ImGui.CloseCurrentPopup();
            }
            else
            {
                
                ImGui.Text("Edit Project '");
                ImGui.SameLine(0f, 0f);
                ImGui.Text(_selectedProject.MetaData.Name);
                ImGui.SameLine(0f, 0f);
                ImGui.Text("':");
                ImGui.Separator();
                
                ImGui.BeginTable("EditProjectTable", 2, ImGuiTableFlags.None);
                {
                    ImGui.TableSetupColumn("##FirstColumn", ImGuiTableColumnFlags.WidthFixed);
                    ImGui.TableSetupColumn("##SecondColumn", ImGuiTableColumnFlags.WidthStretch);

                    ImGui.TableNextRow();

                    ImGui.TableSetColumnIndex(0);
                    ImGui.AlignTextToFramePadding();

                    ImGui.TextColored(ImAnim.Get(_errorNameAnimInt), "(*) Project Name:");

                    ImGui.TableSetColumnIndex(1);
                    ImGui.SetNextItemWidth(-1.0f);
                    ImGui.InputText("##ProjectName", ref _editedProjectName, 256);

                    ImGui.TableNextColumn();
                    ImGui.Text("Project Description:");

                    ImGui.TableNextColumn();
                    ImGui.SetNextItemWidth(-1.0f);
                    ImGui.InputTextMultiline("##ProjectDescription", ref _editedProjectDescription, 256);
                    if (string.IsNullOrEmpty(_editedProjectDescription) && !ImGui.IsItemActive())
                    {
                        var pos = ImGui.GetItemRectMin();
                        pos.X += ImGui.GetStyle().FramePadding.X;
                        pos.Y += ImGui.GetStyle().FramePadding.Y;

                        ImGui.GetWindowDrawList().AddText(
                            pos, 
                            ImGui.GetColorU32(ImGuiCol.TextDisabled), 
                            "(Optional)"
                        );
                    }
                    
                    ImGui.TableNextColumn();
                    ImGui.Text("Project Authors:");
                    
                    ImGui.TableNextColumn();
                    ImGui.InputTextWithHint("##ProjectAuthors", "Type here a new author", ref _editedNewProjectAuthor, 256);
                    ImGui.SameLine();
                    if (ImGui.Button("Add"))
                    {
                        _editedProjectAuthors.Add(_editedNewProjectAuthor);
                        _editedNewProjectAuthor = "";
                    }
                    ImGui.BeginGroup();

                    var copyArray = _editedProjectAuthors.ToArray();
                    for (int i = 0; i < copyArray.Length; i++)
                    {
                        var author = copyArray[i];
                        ImGui.Text("-");
                        ImGui.SameLine();
                        ImGui.Text(author);
                        ImGui.SameLine();
                        if (ImGui.Button($"Remove##{i}"))
                        {
                            _editedProjectAuthors.RemoveAt(i);
                        }
                    }
                    
                    ImGui.EndGroup();
                }
                ImGui.EndTable();

                if (ImGui.Button("Save change", new Vector2(120, 0)))
                {
                    _selectedProject.MetaData.Name = _editedProjectName;
                    _selectedProject.MetaData.Description = _editedProjectDescription;
                    _selectedProject.MetaData.Authors = _editedProjectAuthors;
                    Logger.Debug($"Project '{_selectedProject.MetaData.Name}' edited.");
                    ImGui.CloseCurrentPopup();
                }
                
                ImGui.SameLine();
                
                if (ImGui.Button("Cancel", new Vector2(120, 0)))
                {
                    ImGui.CloseCurrentPopup();
                }
            }
            ImGui.EndPopup();
        }
    }
    
    
    private void DrawOpenLoadingPopup()
    {
        if (_openProject)
        {
            ImGui.OpenPopup("Loading, please wait...");
            _openProject = false;
        }

        if (ImGui.BeginPopupModal("Loading, please wait...", ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.AlwaysAutoResize))
        {
            ImGui.SetWindowPos(new Vector2((ImGui.GetIO().DisplaySize.X - 400) * 0.5f, (ImGui.GetIO().DisplaySize.Y - 300) * 0.5f));
            ImGui.SetWindowSize(new Vector2(400, 300), ImGuiCond.Appearing);
            
            if (_selectedProject == null)
            {
                ImGui.CloseCurrentPopup();
            }
            else
            {
                ImGui.CenteredText("Loading project, please wait...", ImGuiExt.CenteringDirection.Horizontal);
                ImGui.Spacing();
                ImGui.IndeterminateProgressBar("##LoadingBar", new Vector2(300, 20), new Vector4(0.2f, 0.6f, 0.2f, 1.0f), new Vector4(0.3f, 0.3f, 0.3f, 1.0f));
                _projectsManager.OpenProject(_selectedProject);
                ImGui.CloseCurrentPopup();
                EditorWindowManager.Close(this);
            }
            ImGui.EndPopup();
        }
    }
    
    #endregion

    #region NewProject
    private string _newProjectName = "";
    private string _newProjectDescription = "";
    private string _newProjectFolder = "";

    private bool _missingField;
    private void DrawNewProject()
    {
        if (ImGui.BeginChild("New project"))
        {
            ImGui.BeginTable("NewProjectTable", 2, ImGuiTableFlags.None);
            {
                ImGui.TableSetupColumn("##FirstColumn", ImGuiTableColumnFlags.WidthFixed);
                ImGui.TableSetupColumn("##SecondColumn", ImGuiTableColumnFlags.WidthStretch);

                ImGui.TableNextRow();

                ImGui.TableSetColumnIndex(0);
                ImGui.AlignTextToFramePadding();

                ImGui.TextColored(ImAnim.Get(_errorNameAnimInt), "(*) Project Name:");

                ImGui.TableSetColumnIndex(1);
                ImGui.SetNextItemWidth(-1.0f);
                ImGui.InputText("##ProjectName", ref _newProjectName, 256);

                ImGui.TableNextRow();
                ImGui.TableSetColumnIndex(0);
                ImGui.AlignTextToFramePadding();
                ImGui.Text("Project Description:");
                
                ImGui.TableSetColumnIndex(1);
                ImGui.SetNextItemWidth(-1.0f);
                ImGui.InputTextMultiline("##ProjectDescription", ref _newProjectDescription, 256);
                if (string.IsNullOrEmpty(_newProjectDescription) && !ImGui.IsItemActive())
                {
                    var pos = ImGui.GetItemRectMin();
                    pos.X += ImGui.GetStyle().FramePadding.X;
                    pos.Y += ImGui.GetStyle().FramePadding.Y;

                    ImGui.GetWindowDrawList().AddText(
                        pos, 
                        ImGui.GetColorU32(ImGuiCol.TextDisabled), 
                        "(Optional)"
                    );
                }
                
                ImGui.TableNextRow();
                ImGui.TableSetColumnIndex(0);
                ImGui.AlignTextToFramePadding();
                
                ImGui.TextColored(ImAnim.Get(_errorProjectAnimInt), "(*) Project Folder:");
                
                ImGui.TableSetColumnIndex(1);
                var availWidth = ImGui.GetContentRegionAvail().X;
                var buttonSize = ImGui.CalcButtonSize("Choose...");
                var spacing = ImGui.GetStyle().ItemSpacing.X;

                // The minus one here is to align perfectly the button (openFolderDialogButton) with the elements before him (MultiLineTextInput, and InputText)
                ImGui.SetNextItemWidth(availWidth - buttonSize.X - spacing - 1);
                ImGui.InputText("##ProjectFolder", ref _newProjectFolder, 256);
                ImGui.SameLine();
                ImGui.OpenFolderDialogButton("Choose...", new ImGuiExt.FolderDialogParameters(_game.Window.Handle, folders =>
                {
                    if (folders.Length > 0)
                    {
                        _newProjectFolder = folders[0];
                    }
                })
                {
                    DefaultLocation = _newProjectFolder
                });
                
            }
            ImGui.EndTable();
            
            ImGui.Spacing();
            
            ImGui.Separator();

            ImGui.TextColored(ImGui.GetStyle().Colors[(int)ImGuiCol.TextDisabled], "All fields with a '(*)' are REQUIRED fields.");
            
            ImGui.Spacing();
            
            {
                var availSize = ImGui.GetContentRegionAvail().X;
                var buttonSize = ImGui.CalcButtonSize("Create Project");
                ImGui.SetCursorPosX((availSize - buttonSize.X) * 0.5f);
                
                if (ImGui.Button("Create project"))
                {
                    _missingField = false;
                    
                    if (string.IsNullOrEmpty(_newProjectName))
                    {
                        if(ImAnim.IsReverse(_errorNameAnimInt))
                        {
                            ImAnim.Reverse(_errorNameAnimInt);
                        }
                        ImAnim.Start(_errorNameAnimInt);
                        
                        _missingField = true;
                    }
                    else
                    {
                        if(ImAnim.GetTimeElapsed(_errorNameAnimInt) > 0)
                        {
                            if(!ImAnim.IsReverse(_errorNameAnimInt))
                            {
                                ImAnim.Reverse(_errorNameAnimInt);
                            }
                            ImAnim.Start(_errorNameAnimInt);
                        }
                    }

                    if (string.IsNullOrEmpty(_newProjectFolder))
                    {
                        if(ImAnim.IsReverse(_errorProjectAnimInt))
                        {
                            ImAnim.Reverse(_errorProjectAnimInt);
                        }
                        ImAnim.Start(_errorProjectAnimInt);
                        
                        _missingField = true;
                    }
                    else
                    {
                        if(ImAnim.GetTimeElapsed(_errorProjectAnimInt) > 0)
                        {
                            if(!ImAnim.IsReverse(_errorProjectAnimInt))
                            {
                                ImAnim.Reverse(_errorProjectAnimInt);
                            }
                            ImAnim.Start(_errorProjectAnimInt);
                        }
                    }

                    if (Directory.GetFiles(_newProjectFolder).Length > 0 ||
                        Directory.GetDirectories(_newProjectFolder).Length > 0)
                    {
                        ImNotify.Show("Directory not empty", "The selected project folder is not empty. Please select an empty folder.", NotifyType.Error);
                        _missingField = true;
                        if(ImAnim.IsReverse(_errorProjectAnimInt))
                        {
                            ImAnim.Reverse(_errorProjectAnimInt);
                        }
                        ImAnim.Start(_errorProjectAnimInt);
                        
                        _missingField = true;
                    }

                    if (!_missingField)
                    {
                        CreateProject();
                    }
                }
            }
        }
        ImGui.EndChild();        
    }

    private void CreateProject()
    {
        _projectsManager.CreateProject(_newProjectName, _newProjectFolder, _newProjectDescription);
        ImNotify.Show("Project Created", $"The project '{_newProjectName}' has been created successfully.", NotifyType.Success);
    }
    #endregion
}