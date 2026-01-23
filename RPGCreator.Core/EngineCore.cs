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
using RPGCreator.Core.Configs;
using RPGCreator.Core.ECS;
using RPGCreator.Core.Events;
using RPGCreator.Core.Events.EventArgs;
using RPGCreator.Core.Inputs;
using RPGCreator.Core.Inputs.Keyboard;
using RPGCreator.Core.Inputs.Mouse;
using RPGCreator.Core.Parser.Graph;
using RPGCreator.Core.Parser.PRATT;
using RPGCreator.Core.Resources;
using RPGCreator.Core.Scheduler;
using RPGCreator.Core.Types.Assets.Items;
using RPGCreator.Core.Types.Assets.Tilesets;
using RPGCreator.Core.Types.Blueprint;
using RPGCreator.Core.Types.Editor.Context;
using RPGCreator.Core.Types.Map;
using RPGCreator.Core.Types.Map.Layers.AutoLayer;
using RPGCreator.SDK;
using RPGCreator.SDK.Assets;
using RPGCreator.SDK.Assets.Definitions.Characters;
using RPGCreator.SDK.Assets.Definitions.Maps;
using RPGCreator.SDK.Assets.Definitions.Stats;
using RPGCreator.SDK.Assets.Definitions.Tilesets;
using RPGCreator.SDK.Assets.Definitions.Tilesets.IntGrid;
using RPGCreator.SDK.EngineService;
using RPGCreator.SDK.Graph.Nodes;
using RPGCreator.SDK.Inputs;
using RPGCreator.SDK.Logging;
using Serilog;

using SDKAutoTileSolver = RPGCreator.SDK.Assets.Definitions.Maps.AutoLayer.AutoTileSolver;

namespace RPGCreator.Core
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
        
        public enum EEngineMode
        {
            EditorMode,
            PlayerMode
        }
        
        public readonly EEngineMode engineMode = EEngineMode.EditorMode;
        
        // Suppressing this error, this should never happen. And if it happen, then it should cause a fatal crash!
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        static internal EngineCore Instance { get; private set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

        static public bool HasInstance => Instance != null;

        static public bool IsCoreReady { get; private set; } = false;
        static public bool IsUIReady { get; private set; } = false;
        static public bool IsRTPReady { get; private set; } = false;

        internal EngineScheduler Scheduler { get; private set; }
        internal EngineConfigs Configs { get; private set; }
        internal EngineEvents Events { get; private set; }
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
            
            EngineStates.EditorState = new EditorState();
            EngineStates.EditorState.InEditorMode = mode == EEngineMode.EditorMode;
            EngineStates.ProjectState = new ProjectState();
            EngineStates.BrushState = new BrushState();
            
            EngineStates.MouseState = new EngineMouseState();
            EngineStates.KeyboardState = new EngineKeyboardState();

            if (EngineStates.EditorState.InEditorMode)
            {
                EngineStates.ViewportMouseState = new ViewportMouseState();
                EngineStates.ViewportKeyboardState = new ViewportKeyboardState();
            }
            
            var typeMapping = new AssetsTypeMapping();
            
            typeMapping.ScanAllEngineAssemblies();
            
            EngineServices.AssetTypeRegistry = typeMapping;
            
            EngineResourcesService resService = new EngineResourcesService();
            
            resService.RegisterLoader<Avalonia.Media.Imaging.Bitmap>(new AvaloniaBitmapLoader());
            
            EngineServices.ResourcesService = resService;
            
            Instance = this;

            Scheduler = new EngineScheduler();
            Serializer = new EngineSerializer();
            EngineServices.SerializerService = Serializer;
            Configs = new EngineConfigs();
            Events = new EngineEvents();
            Managers = new EngineManagers();
            Modules = new EngineModules();
            #if DEBUG
            // In debug mode, we load the engine icons for debug tools (like IconsExplorer).
            Icons = new EngineIcons();
            #endif
            
            EngineServices.GraphService = new GraphService();
            EngineServices.PrattFormulaService = new PrattFormulaService();
            EngineServices.GraphNodeScanner = new GraphNodeScanner();
            EngineServices.ECS = new ECSService();
            EngineServices.InputsService = new InputsService();

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
            
            SDKAutoTileSolver.Service = new AutoTileSolver();
            
            Managers.Init();
            
            _logger.Info("Starting scanning for blueprint opcodes handlers...");
            
            // Scan the assemblies for all blueprint opcodes handlers
            // This will register all the handlers in the graph table.
            GraphTable.ScanAssemblies();
            
            _logger.Info("Blueprint opcodes handlers scanning completed.");
            _logger.Info("Found {Count} handlers.", args: GraphTable.ValidOpcodes.Count);
            
            _logger.Info("Starting scanning for graph nodes...");
            
            // Scan the assemblies for all graph nodes
            // This will register all the nodes in the graph node registry.
            EngineServices.GraphNodeScanner.ScanCurrentAssembly();
            _logger.Info("Graph nodes scanning completed.");
            _logger.Info("Found {Count} nodes.", args: GraphNodeRegistry.GetAllNodes().Count);
            
            _logger.Info("Checking graph nodes and opcodes handlers consistency...");
            // Check if all registered handlers have a corresponding node in the graph node registry.

            GraphTable.CheckHandlersAndNodesConsistency();
            
            _logger.Info("Graph nodes and opcodes handlers consistency check completed.");
            _logger.Info("Check above for any errors or warnings.");
            
            _logger.Info("EngineCore initialized at {Time}.", args: DateTime.Now);

            // Managers.Projects.CreateProject("test project new config", "C:\\Users\\Ward\\Desktop\\Test");
            
            MapEditorContext.Initialize();

        }

        private void SubscribeBaseEvents()
        {
            // Suscribe to base events here
            Events.RTPReady += (sender, args) =>
            {
                IsRTPReady = true;
            };

            Events.UIReady += (sender, args) =>
            {
                IsUIReady = true;
            };

            Events.UIEditorOpened += (sender, args) =>
            {
                _openedWindowsCount++;
            };

            Events.UIEditorClosed += (sender, args) =>
            {
                _openedWindowsCount--;
                if (_openedWindowsCount <= 0)
                {
                    // If no windows are opened, then we can close the engine.
                    Events.OnEngineStopping(new());
                }
            };

            Events.UILauncherOpened += (sender, args) =>
            {
                _openedWindowsCount++;
            };

            Events.UILauncherClosed += (sender, args) =>
            {
                _openedWindowsCount--;
                if (_openedWindowsCount <= 0)
                {
                    // If no windows are opened, then we can close the engine.
                    Events.OnEngineStopping(new());
                }
            };

            Events.EngineStopping += (sender, args) =>
            {
                // This event is called when the engine is stopping, we can do some cleanup here.
            };
        }


        static public EngineCore InitCore(EEngineMode mode = EEngineMode.EditorMode)
        {
            Instance = new(mode);

            IsCoreReady = true;
            Instance.Events.OnCoreReady(new());

            return Instance;
        }

        static public EngineCore StartCore()
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
            // Start the engine
            Instance.Events.OnEngineStarting(new EngineStartingArgs());

            Instance.SubscribeBaseEvents();

            return Instance;
        }

        public void Update()
        {
            Scheduler.Update(0.016f);
        }

    }
}
