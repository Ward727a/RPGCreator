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

using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Input;
using RPGCreator.SDK;
using RPGCreator.SDK.EngineService;
using RPGCreator.SDK.Logging;
using RPGCreator.UI.Content.AssetsManage;

namespace RPGCreator.UI.Content.Editor;

public class EditorMenuBar : UserControl
{
    private Menu _menu;
    
    public EditorMenuBar()
    {
        CreateComponents();
        RegisterEvents();
        this.Content = _menu;
    }

    private void CreateComponents()
    {
        _menu = new Menu()
        {
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top,
        };
        
        CreateAssetsManagerMenu();
        CreateUiEditorMenu();
        CreateFileMenu();
        CreateEditMenu();
        CreateHelpMenu();
        _menu.Items.Add(_assetsMgrMenu);
        _menu.Items.Add(_uiEditorMenu);
        _menu.Items.Add(_fileMenu);
        _menu.Items.Add(_editMenu);
        _menu.Items.Add(_helpMenu);
    }
    
    #region MenuItems

    private MenuItem _assetsMgrMenu;
    private void CreateAssetsManagerMenu()
    {
        _assetsMgrMenu = new MenuItem() { Header = "Assets Manager", HotKey = new KeyGesture(Key.A, KeyModifiers.Shift) };
    }
    private void RegisterAssetsManagerMenuEvents()
    {
        _assetsMgrMenu.Click += (_, _) =>
        {
            var assetsManageWindow = new AssetsManageWindow();
            
            var window = TopLevel.GetTopLevel(this) as Window;
            
            if(window == null)
            {
                Logger.Error("Unable to find parent window for Assets Management. Cannot open Assets Management window.");
                EditorUiServices.NotificationService.Error("Error", "Unable to open Assets Management window.");
                return;
            }
            
            assetsManageWindow.ShowDialog(window).ContinueWith(t =>
            {
                if (t.IsFaulted)
                {
                    // Handle any errors that occurred while showing the assets management window
                    Logger.Error("Error showing assets management window: " + t.Exception?.Message);
                    EditorUiServices.NotificationService.Error("Error", "An error occurred while opening the Assets Management window.");
                }
            });
        };

    }
    
    private MenuItem _uiEditorMenu;
    private void CreateUiEditorMenu()
    {
        _uiEditorMenu = new MenuItem() { Header = "UI Editor", HotKey = new KeyGesture(Key.U, KeyModifiers.Shift) };
    }
    private void RegisterUiEditorMenuEvents()
    {
    }

    private MenuItem _fileMenu;
    private MenuItem _fileNewMenuItem;
    private MenuItem _fileOpenMenuItem;
    private MenuItem _fileOpenProjectFolderMenuItem;
    private MenuItem _fileSaveMenuItem;
    private MenuItem _fileExportMenuItem;
    private MenuItem _fileImportMenuItem;
    private MenuItem _fileExitMenuItem;
    private MenuItem _fileCloseProjectMenuItem;
    private MenuItem _fileChangeProjectMenuItem;
    private void CreateFileMenu()
    {
        _fileMenu = new MenuItem() { Header = "File", HotKey = new KeyGesture(Key.F, KeyModifiers.Shift) };
        _fileNewMenuItem = new MenuItem() { Header = "New", HotKey = new KeyGesture(Key.N, KeyModifiers.Control) };
        _fileOpenMenuItem = new MenuItem() { Header = "Open", HotKey = new KeyGesture(Key.O, KeyModifiers.Control) };
        _fileOpenProjectFolderMenuItem = new MenuItem() { Header = "Open Project Folder", HotKey = new KeyGesture(Key.O, KeyModifiers.Control | KeyModifiers.Shift) };
        _fileSaveMenuItem = new MenuItem() { Header = "Save", HotKey = new KeyGesture(Key.S, KeyModifiers.Control) };
        _fileExportMenuItem = new MenuItem() { Header = "Export", HotKey = new KeyGesture(Key.E, KeyModifiers.Control) };
        _fileImportMenuItem = new MenuItem() { Header = "Import", HotKey = new KeyGesture(Key.I, KeyModifiers.Control) };
        _fileCloseProjectMenuItem = new MenuItem() { Header = "Close Project", HotKey = new KeyGesture(Key.W, KeyModifiers.Control) };
        _fileChangeProjectMenuItem = new MenuItem() { Header = "Change Project", HotKey = new KeyGesture(Key.O, KeyModifiers.Control | KeyModifiers.Alt) };
        _fileExitMenuItem = new MenuItem() { Header = "Exit", HotKey = new KeyGesture(Key.F4, KeyModifiers.Alt) };
        
        _fileMenu.Items.Add(_fileNewMenuItem);
        _fileMenu.Items.Add(_fileOpenMenuItem);
        _fileMenu.Items.Add(_fileOpenProjectFolderMenuItem);
        _fileMenu.Items.Add(_fileSaveMenuItem);
        _fileMenu.Items.Add(_fileExportMenuItem);
        _fileMenu.Items.Add(_fileImportMenuItem);
        _fileMenu.Items.Add(new Separator());
        _fileMenu.Items.Add(_fileCloseProjectMenuItem);
        _fileMenu.Items.Add(_fileChangeProjectMenuItem);
        _fileMenu.Items.Add(new Separator());
        _fileMenu.Items.Add(_fileExitMenuItem);
        
    }
    private void RegisterFileMenuEvents()
    {
        _fileOpenProjectFolderMenuItem.Click += (s, e) =>
        {
            var currentProject = EngineServices.ProjectsManager.GetCurrentProject();

            if (currentProject == null)
            {
                Logger.Error("No project is currently open. Cannot open project folder.");
                EditorUiServices.NotificationService.Error("Couldn't open project folder", "No project is currently open.");
                return;
            }
            
            if (string.IsNullOrEmpty(currentProject.Path))
            {
                Logger.Error("No project path found inside the actually opened project. Cannot open project folder.");
                EditorUiServices.NotificationService.Error("Couldn't open project folder", "No project path found inside the actually opened project.");
                return;
            }
            
            OpenFolder(currentProject.Path);
        };
        
        _fileSaveMenuItem.Click += (s, e) =>
        {
            var currentProject = EngineServices.ProjectsManager.GetCurrentProject();

            if (currentProject == null)
            {
                Logger.Error("No project is currently open. Cannot save project.");
                EditorUiServices.NotificationService.Error("Couldn't save project", "No project is currently open.");
                return;
            }
            
            RuntimeServices.MapService.SaveMap();
            
            foreach (var pack in EngineServices.AssetsManager.GetLoadedPacks())
            {
                pack.Save();
            }
            
            currentProject.Save();
        };
    }
    
    private MenuItem _editMenu;
    private MenuItem _editUndoMenuItem;
    private MenuItem _editRedoMenuItem;
    private MenuItem _editPreferencesMenuItem;
    private void CreateEditMenu()
    {
        _editMenu = new MenuItem() { Header = "Edit", HotKey = new KeyGesture(Key.E, KeyModifiers.Shift) };
        _editUndoMenuItem = new MenuItem() { Header = "Undo", HotKey = new KeyGesture(Key.Z, KeyModifiers.Control), IsEnabled = false };
        _editRedoMenuItem = new MenuItem() { Header = "Redo", HotKey = new KeyGesture(Key.Y, KeyModifiers.Control), IsEnabled = false };
        _editPreferencesMenuItem = new MenuItem() { Header = "Preferences", HotKey = new KeyGesture(Key.P, KeyModifiers.Control) };
        
        ToolTip.SetShowOnDisabled(_editUndoMenuItem, true);
        ToolTip.SetTip(_editUndoMenuItem, "Nothing to Undo");
        ToolTip.SetShowOnDisabled(_editRedoMenuItem, true);
        ToolTip.SetTip(_editRedoMenuItem, "Nothing to Redo");
        
        _editMenu.Items.Add(_editUndoMenuItem);
        _editMenu.Items.Add(_editRedoMenuItem);
        _editMenu.Items.Add(new Separator());
        _editMenu.Items.Add(_editPreferencesMenuItem);
        
    }
    private void RegisterEditMenuEvents()
    {
        _editUndoMenuItem.Click += (s, e) => EngineServices.UndoRedoService.UndoLastCommand();
        _editRedoMenuItem.Click += (s, e) => EngineServices.UndoRedoService.RedoLastCommand();
        
        EngineServices.OnceServiceReady((ICommandManager undoRedoService) =>
        {
            undoRedoService.StateChanged += () =>
            {
                _editUndoMenuItem.IsEnabled = undoRedoService.CanUndo;
                ToolTip.SetTip(_editUndoMenuItem, undoRedoService.CanUndo ? MakeUndoRedoTip(true, undoRedoService.GetUndoCommandName()) : "Nothing to Undo");
                _editRedoMenuItem.IsEnabled = undoRedoService.CanRedo;
                ToolTip.SetTip(_editRedoMenuItem, undoRedoService.CanRedo ? MakeUndoRedoTip(false, undoRedoService.GetRedoCommandName()) : "Nothing to Redo");
            };
        });
    }
    
    private TextBlock MakeUndoRedoTip(bool Undo, string commandName)
    {
        var text = new TextBlock() { Inlines = new InlineCollection()};
        text.Inlines.Add(new Run() { Text = Undo ? "Undo - " : "Redo - ", FontWeight = Avalonia.Media.FontWeight.Bold });
        text.Inlines.Add(new Run() { Text = commandName });
        return text;
    }
    
    private MenuItem _helpMenu;
    private MenuItem _helpDocumentationMenuItem;
    private MenuItem _helpProposeFeaturesMenuItem;
    private MenuItem _helpReportIssuesMenuItem;
    private MenuItem _helpCommunityMenuItem;
        private MenuItem _communityDiscordMenuItem;
        private MenuItem _communityWebsiteMenuItem;
    private MenuItem _helpAboutMenuItem;
    private void CreateHelpMenu()
    {
        _helpMenu = new MenuItem() { Header = "Help", HotKey = new KeyGesture(Key.H, KeyModifiers.Shift) };
        _helpDocumentationMenuItem = new MenuItem() { Header = "Documentation", HotKey = new KeyGesture(Key.F1) };
        _helpProposeFeaturesMenuItem = new MenuItem() { Header = "Propose Features", HotKey = new KeyGesture(Key.F2) };
        _helpReportIssuesMenuItem = new MenuItem() { Header = "Report Issues", HotKey = new KeyGesture(Key.F3) };
        
        _helpCommunityMenuItem = new MenuItem() { Header = "Community" };
        _communityDiscordMenuItem = new MenuItem() { Header = "Discord" };
        _communityWebsiteMenuItem = new MenuItem() { Header = "Website" };
        _helpCommunityMenuItem.Items.Add(_communityDiscordMenuItem);
        _helpCommunityMenuItem.Items.Add(_communityWebsiteMenuItem);
        
        _helpAboutMenuItem = new MenuItem() { Header = "About", HotKey = new KeyGesture(Key.F4) };
        
        _helpMenu.Items.Add(_helpDocumentationMenuItem);
        _helpMenu.Items.Add(_helpProposeFeaturesMenuItem);
        _helpMenu.Items.Add(_helpReportIssuesMenuItem);
        _helpMenu.Items.Add(new Separator());
        _helpMenu.Items.Add(_helpCommunityMenuItem);
        _helpMenu.Items.Add(_helpAboutMenuItem);
        
    }
    private void RegisterHelpMenuEvents()
    {
        _helpDocumentationMenuItem.Click += (s, e) => OpenUrl("https://doc.rpgcreator.dev/");
        _helpProposeFeaturesMenuItem.Click += (s, e) => OpenUrl("https://github.com/Ward727a/RPGCreator/issues");
        _helpReportIssuesMenuItem.Click += (s, e) => OpenUrl("https://github.com/Ward727a/RPGCreator/issues");
        _communityDiscordMenuItem.Click += (s, e) => OpenUrl("https://discord.gg/4yfq4NNzs4");
        _communityWebsiteMenuItem.Click += (s, e) => OpenUrl("https://rpgcreator.dev");
        _helpAboutMenuItem.Click += (s, e) => OpenUrl("https://github.com/Ward727a/RPGCreator/blob/Dev/README.md");
    }

    #endregion

    private void RegisterEvents()
    {
        RegisterAssetsManagerMenuEvents();
        RegisterUiEditorMenuEvents();
        RegisterFileMenuEvents();
        RegisterEditMenuEvents();
        RegisterHelpMenuEvents();
    }
    
    #region Helpers
    
    public void OpenUrl(string url)
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            });
        }
        catch
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                url = url.Replace("&", "^&");
                Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                Process.Start("xdg-open", url);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                Process.Start("open", url);
            }
            else
            {
                #if DEBUG
                throw;
                #else
                Logger.Error("Unsupported platform for opening URLs.");
                EditorUiServices.NotificationService.Error("Unsupported platform", "Cannot open URLs on this platform.");
                #endif
            }
        }
    }
    
    public void OpenFolder(string folderPath)
    {
        if (!Directory.Exists(folderPath)) return;

        Process.Start(new ProcessStartInfo
        {
            FileName = folderPath,
            UseShellExecute = true
        });
    }
    
    #endregion
}