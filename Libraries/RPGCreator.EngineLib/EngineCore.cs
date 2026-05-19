#region LICENSE
//
// RPG Creator - Open-source RPG Engine.
// (c) 2025 Ward
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
// 
// 
#endregion

using RPGCreator.EngineLib.Common;
using RPGCreator.EngineLib.ECS;
using RPGCreator.EngineLib.Inputs;
using RPGCreator.EngineLib.Inputs.Keyboard;
using RPGCreator.EngineLib.Inputs.Mouse;
using RPGCreator.EngineLib.Module;
using RPGCreator.EngineLib.Parser.PRATT;
using RPGCreator.EngineLib.Registry;
using RPGCreator.EngineLib.Scheduler;
using RPGCreator.EngineLib.Services;
using RPGCreator.EngineLib.Types.Map.Layers.AutoLayer;
using RPGCreator.SDK;
using RPGCreator.SDK.Assets;
using RPGCreator.SDK.Common.Logging;
using RPGCreator.SDK.GameRunner;
using RPGCreator.SDK.GameUI.Controls;
using RPGCreator.SDK.GameUI.Events.Actions;
using RPGCreator.SDK.GlobalState;
using RPGCreator.SDK.Inputs;
using RPGCreator.SDK.Modules;
using RPGCreator.SDK.Services.EngineService;
using RPGCreator.SDK.Types;
using RPGCreator.Shared.Types;
using SDKAutoTileSolver = RPGCreator.SDK.Assets.Definitions.Maps.Layers.AutoLayer.AutoTileSolver;

namespace RPGCreator.EngineLib
{
    /// <summary>
    /// Engine core. <br/>
    /// This part manage all the other engine part, and it's the entrypoint for the UI and the realtime preview.<br/>
    /// For all data related, check "EngineData".<br/>
    /// For all events related, check "EngineEvents".<br/>
    /// </summary>
    public class EngineCore
    {

        private readonly ScopedLogger _logger = Logger.ForContext<EngineCore>();
        
        
        public readonly EEngineMode engineMode = EEngineMode.Editor;
        
        // Suppressing this error, this should never happen. And if it happen, then it should cause a fatal crash!
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        static internal EngineCore Instance { get; private set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

        static public bool HasInstance => Instance != null;

        static public bool IsCoreReady { get; private set; } = false;
        static public bool IsUIReady { get; private set; } = false;
        static public bool IsRTPReady { get; private set; } = false;

        internal EngineScheduler Scheduler { get; private set; }
        internal EngineManagers Managers { get; private set; }
        internal EngineModules Modules { get; private set; }
        internal EngineSerializer Serializer { get; private set; }
        internal EngineIcons Icons { get; private set; }

        public static bool ManagersReady = false;
        public static bool ModulesReady = false;

        private int _openedWindowsCount = 0; // Count of opened windows, used to know if the engine is ready to be closed or not.

        private EngineCore(EEngineMode mode) 
        {
            engineMode = mode;
            
            if (Instance != null)
            {
                throw new Exception("Engine core has already been initialized, it should happen only once.");
            }

            Logger.Implementation = new EngineLogger();
            
            #region GlobalStates Initialization
            GlobalStates.EngineMode = mode;
            GlobalStates.EditorState = new EditorState();
            GlobalStates.ProjectState = new ProjectState();
            GlobalStates.MapState = new MapState();
            GlobalStates.ToolState = new BaseToolState();
            
            GlobalStates.MouseState = new EngineMouseState();
            GlobalStates.KeyboardState = new EngineKeyboardState();

            if (GlobalStates.EngineMode == EEngineMode.Editor)
            {
                GlobalStates.ViewportMouseState = new ViewportMouseState();
                GlobalStates.ViewportKeyboardState = new ViewportKeyboardState();
            }
            else
            {
                GlobalStates.ViewportMouseState = GlobalStates.MouseState;
                GlobalStates.ViewportKeyboardState = GlobalStates.KeyboardState;
            }
            #endregion
            
            // Doing this as "ScanAllEngineAssemblies" is not implemented inside the IAssetsTypeMapping interface.
            // And we don't want that (or else all modules could do that, and it COULD be a problem...
            // In fact, I didn't check if it could be a problem, but I prefer to not take the risk for now, and we can always change that later if we need to).
            
            RegistryServices.GuiControl.RegisterControl(new TestControl());
            RegistryServices.GuiControl.RegisterControl(new TextControl());
            RegistryServices.GuiControl.RegisterControl(new ButtonControl());
            RegistryServices.GuiAction.RegisterAction(new PrintAction());
            RegistryServices.GuiAction.RegisterAction(new BpAction());
            
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                ClassesRegistry.AnalyzeAsm(assembly);
            }

            var gameUiControl = ClassesRegistry.GetEngineClassesWithUrnPrefix("game_ui/controls/de", URN.SearchComparisionType.ModuleOnly);

            Logger.Debug("Found {number} UI Controls with prefix {prefixSearchedFor}:", gameUiControl.Count, "game_ui/controls/de");
            foreach (var engineClass in gameUiControl)
            {
                Logger.Debug(engineClass.ToString());
            }
            
            Instance = this;

            var config = new EngineConfig();
            config.CreateOrLoadConfig();
            EngineServices.Config = config;
            // Configs = new EngineConfigs(); // Should be removed!!
            Managers = new EngineManagers();
            
            #if DEBUG
            // In debug mode, we load the engine icons for debug tools (like IconsExplorer).
            Icons = new EngineIcons();
            #endif

            Logger.Warning("---");
            Logger.Warning("NORMAL WARNING: The warning below can be ignored!");
            EngineServices.InputsService.SetBinding(KeyboardKeys.Z, "forward");
            EngineServices.InputsService.SetBinding(KeyboardKeys.S, "backward");
            EngineServices.InputsService.SetBinding(KeyboardKeys.Q, "left");
            EngineServices.InputsService.SetBinding(KeyboardKeys.D, "right");
            
            EngineServices.InputsService.SetBinding(ScrollType.Up, "zoom_in");
            EngineServices.InputsService.SetBinding(ScrollType.Down, "zoom_out");
            
            EngineServices.InputsService.SetBinding(MouseButton.Left, "left_click");
            EngineServices.InputsService.SetBinding(MouseButton.Middle, "middle_click");
            EngineServices.InputsService.SetBinding(MouseButton.Right, "right_click");
            Logger.Warning("---");
            
            SDKAutoTileSolver.Service = new AutoTileSolver(); // Kinda dirty, need to clean this
            
            Managers.Init();
            
            Modules = new EngineModules();
            EngineServices.ModuleManager = Modules;
            
            _logger.Info("EngineCore initialized at {Time}.", args: DateTime.Now);
        }

        public static (bool success, Ulid mapId) LoadGameData(IGameData data)
        {
            var directoryPath = AppContext.BaseDirectory;
            var acceptedHashes = data.AcceptedModuleHashes;
            var moduleDirectory = Path.Combine(directoryPath, data.ModulesPath);
            var projectPath = data.ProjectPath;
            var mainMapId = data.MainMapId;
            
            if (!File.Exists(projectPath))
            {
                Logger.Critical("Project file not found: {ProjectPath}", args: projectPath);
                return (false, Ulid.Empty);
            }
            
            if (!EngineServices.ProjectsManager.TryGetProject(data.ProjectPath, out var project))
            {
                Logger.Critical("Failed to load project at path: {ProjectPath}", args: data.ProjectPath);
                return (false, Ulid.Empty);
            }
            
            if (!Directory.Exists(moduleDirectory))
            {
                Logger.Critical("Module directory not found: {ModuleDirectory}", args: moduleDirectory);
                return (false, Ulid.Empty);
            }
            
            var dllList = Directory.GetFiles(moduleDirectory, "*.dll", SearchOption.AllDirectories);

            foreach (var dll in dllList)
            {
                if (dll.StartsWith("_runned_temp_"))
                    continue;
                var sha256 = ShaUtil.ComputeSha256(dll);
                
                if(!acceptedHashes.Contains(sha256, StringComparer.InvariantCultureIgnoreCase))
                {
                    Logger.Critical("Module hash mismatch for file: {FilePath}", args: dll);
                    Logger.Critical("File hash: {wrongSha}", args: sha256);
                    Logger.Critical("Available Hashes: {listHash}", args: acceptedHashes);
                    Logger.Critical("Please contact the game developer!");
                    continue;
                }

                EngineServices.ModuleManager.ClearTempModulesShadowCopies(Path.GetDirectoryName(dll) ?? string.Empty, new ());
                EngineServices.ModuleManager.TryLoadModule(dll, new());
            }

            var loadedModules = EngineServices.ModuleManager.GetAllLoadedModules(new EngineSecurityToken());
            
            List<URN> moduleToStart = new();
            
            foreach (var module in loadedModules)
            {
                moduleToStart.Add(module.ModuleUrn);
            }
            
            EngineServices.ModuleManager.PlanStarting(out HashSet<URN> startOrders, out HashSet<URN> incompatibleModules, moduleToStart);

            if (incompatibleModules.Count > 0)
            {
                Logger.Critical("Some modules are incompatible and cannot be started:");
                foreach (var urn in incompatibleModules)
                {
                    Logger.Critical(" - {ModuleUrn}", args: urn.ToString());
                }
                return (false, Ulid.Empty);
            }

            foreach (var startOrder in startOrders.Where(startOrder => !EngineServices.ModuleManager.StartModule(startOrder, new EngineSecurityToken())))
            {
                Logger.Critical("Failed to start module: {ModuleUrn}", args: startOrder.ToString());
                return (false, Ulid.Empty);
            }
            
            EngineServices.ProjectsManager.OpenProject(project);

            if (EngineServices.AssetsManager.Has(mainMapId))
                return (true, mainMapId);
            
            Logger.Critical("Failed to resolve main map with ID: {MapId}", args: mainMapId);
            return (false, Ulid.Empty);

        }



        public static EngineCore InitCore(EEngineMode mode = EEngineMode.Editor)
        {
            Instance = new(mode);

            IsCoreReady = true;
            
            GlobalStates.EngineMode = mode;

            return Instance;
        }

        public static EngineCore StartCore()
        {

            // Please don't remove the line below,
            // it's needed to not get an Avalonia designer error / crash.
            if (!IsCoreReady)
            {
#if DEBUG
                InitCore();
#else
                throw new Exception("Engine core has not been initialized, it should happen before starting the engine.");
#endif
            }

            return Instance;
        }

        public void Update()
        {
            
        }

    }
}
