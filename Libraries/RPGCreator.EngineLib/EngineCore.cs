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

using System.Diagnostics;
using RPGCreator.SDK;
using RPGCreator.SDK.Common.Logging;
using RPGCreator.SDK.GameUI.Events.Actions;
using RPGCreator.SDK.Inputs;
using RPGCreator.SDK.Registry;
using RPGCreator.SDK.Services.EngineService;
using RPGCreator.Shared.Attributes;
using RPGCreator.Shared.Types;

namespace RPGCreator.EngineLib
{
    /// <summary>
    /// Engine core. <br/>
    /// This part manage all the other engine part, and it's the entrypoint for the UI and the realtime preview.<br/>
    /// For all data related, check "EngineData".<br/>
    /// For all events related, check "EngineEvents".<br/>
    /// </summary>
    [EngineService]
    public class EngineCore
    {
        private readonly IGuiActionRegistry _actionRegistry;
        private readonly IKeyedActionManager _keyActionManager;

        private readonly ScopedLogger _logger = Logger.ForContext<EngineCore>();
        
        static public bool IsCoreReady { get; private set; } = false;

        public IInputsService Inputs { get; private set; }

        public static void StartLogger()
        {
            Logger.Implementation = new EngineLogger();
        }
        
        public EngineCore(IInputsService inputs, IGuiActionRegistry actionRegistry, IKeyedActionManager keyActionManager)
        {
            if (IsCoreReady)
            {
                throw new Exception("Engine core has already been initialized, it should happen only once.");
            }

            _actionRegistry = actionRegistry;
            _keyActionManager = keyActionManager;
            Inputs = inputs;
        }
        
        public void StartCore()
        {

            if (IsCoreReady) return;
            
            var timer = new Stopwatch();
            timer.Start();
            _logger.Info("Start core at {0}", args: DateTime.Now);
            
            IsCoreReady = true;
            
            _actionRegistry.RegisterAction<PrintAction>();
            _actionRegistry.RegisterAction<BpAction>();
            
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
            
            Logger.Warning("---");
            Logger.Warning("NORMAL WARNING: The warning below can be ignored!");
            _keyActionManager.SetBinding(KeyboardKeys.Z, "forward");
            _keyActionManager.SetBinding(KeyboardKeys.S, "backward");
            _keyActionManager.SetBinding(KeyboardKeys.Q, "left");
            _keyActionManager.SetBinding(KeyboardKeys.D, "right");
            
            _keyActionManager.SetBinding(ScrollType.Up, "zoom_in");
            _keyActionManager.SetBinding(ScrollType.Down, "zoom_out");
            
            _keyActionManager.SetBinding(MouseButton.Left, "left_click");
            _keyActionManager.SetBinding(MouseButton.Middle, "middle_click");
            _keyActionManager.SetBinding(MouseButton.Right, "right_click");
            Logger.Warning("---");
            
            timer.Stop();
            
            _logger.Info("EngineCore started at {Time}.", args: DateTime.Now);
            _logger.Info("Core initialization took {Time} ms.", args: timer.ElapsedMilliseconds);
        }

    }
}
