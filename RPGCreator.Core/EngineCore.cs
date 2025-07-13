#region LICENSE
//
// RPG Creator - Open-source RPG Engine.
// (c) 2025 Ward
// 
// This file is part of RPG Creator and is distributed under the MIT License.
// You are free to use, modify, and distribute this file under the terms of the MIT License.
// See LICENSE for details.
// 
// ---
// 
// Ce fichier fait partie de RPG Creator et est distribué sous licence MIT.
// Vous êtes libre de l'utiliser, de le modifier et de le distribuer sous les termes de la licence MIT.
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
using RPGCreator.Core.Events;
using RPGCreator.Core.Events.EventArgs;
using RPGCreator.Core.Scheduler;
using RPGCreator.Core.Type.Assets.BaseAssetsPack;

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
        // Suppressing this error, this should never happen. And if it happen, then it should cause a fatal crash!
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        static public EngineCore Instance { get; private set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

        static public bool HasInstance => Instance != null;

        static public bool IsCoreReady { get; private set; } = false;
        static public bool IsUIReady { get; private set; } = false;
        static public bool IsRTPReady { get; private set; } = false;

        public EngineScheduler Scheduler { get; private set; }
        public EngineConfigs Configs { get; private set; }
        public EngineData Data { get; private set; }
        public EngineEvents Events { get; private set; }
        public EngineManagers Managers { get; private set; }
        public EngineModules Modules { get; private set; }
        public EngineSerializer Serializer { get; private set; }

        // TODO: Remove?
        //public static BaseAssetsPack TESTPACK;

        public static bool ManagersReady = false;
        public static bool ModulesReady = false;

        private int _openedWindowsCount = 0; // Count of opened windows, used to know if the engine is ready to be closed or not.

        private EngineCore() 
        {
            if (Instance != null)
            {
                throw new Exception("Engine core has already been initialized, it should happen only once.");
            }
            Instance = this;

            Scheduler = new EngineScheduler();
            Configs = new EngineConfigs();
            Data = new EngineData();
            Events = new EngineEvents();
            Managers = new EngineManagers();
            Modules = new EngineModules();
            Serializer = new EngineSerializer();

            Managers.Init();

            Managers.Projects.CreateProject("test project new config", "C:\\Users\\Ward\\Desktop\\Test");

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

            Events.RTPCreated += (sender, args) =>
            {
                Data.RTPGame = args.RTP;
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


        static public EngineCore InitCore()
        {
            Instance = new();

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
