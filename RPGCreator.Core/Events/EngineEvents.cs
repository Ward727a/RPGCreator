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
using RPGCreator.Core.Events.EventArgs;
using RPGCreator.Core.Managers.AssetsManager.EventsArgs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Input;
using Microsoft.Xna.Framework.Graphics;
using RPGCreator.Core.Type.Assets.Animations;
using Serilog;

namespace RPGCreator.Core.Events
{

    // If you want to add another event, please add it inside one of the region (or create a new one if needed).
    // ===
    // Please, for each new event, add a comment to explain what it does and when it is called, plus add a prefix to the event name to know where it is / should be called.
    // Like this: (for Core) CoreEventName / OnCoreEventName | (for UI) UIEventName / OnUIEventName | (for RTP) RTPEventName / OnRTPEventName | (for Engine) EngineEventName / OnEngineEventName

    /// <summary>
    /// This class manage all global events of the engine.<br/>
    /// </summary>
    public class EngineEvents : BaseEventSource
    {

        internal EngineEvents()
        {
            
            Log.Information($"EngineEvents initialized.");
            
        }

        // This manage all the engine related events.
        // For example, once a project is unloaded, you can subscribe to this event to do something AFTER ALL parts of the engine are done unloading it.
        // In the same way, you can subscribe to the "EngineLoadProject" event to do something BEFORE ALL parts of the engine start loading it.
        // If you need to "ask" the engine to do something, then you should use the "AskEngine[EventName]" method. (Right now this is still limited to only some events, but this will be expanded in the future)
        //
        // Be careful to not confound some events with others, example: UILoadedProject, RTPLoadedProject, and EngineLoadProject are 3 different events.

        // Small schema:
        //
        // ╔═══════════════════╗             ╔═══════════════╗     ╔════════════════╗             ╔═════════════════╗     ╔══════════════════╗                     ╔═════════════════════╗
        // ║ EngineLoadProject ║ == Then ==> ║ UILoadProject ║ AND ║ RTPLoadProject ║ == THEN ==> ║ UILoadedProject ║ AND ║ RTPLoadedProject ║ == THEN FINALLY ==> ║ EngineLoadedProject ║
        // ╚═══════════════════╝             ╚═══════════════╝     ╚════════════════╝             ╚═════════════════╝     ╚══════════════════╝                     ╚═════════════════════╝

        #region ENGINE
        /// <summary>
        /// When the engine is starting. This is where you can start subscribe to other events.<br/>
        /// Be careful, this is called before all other parts are ready, so you should not use any other part of the engine here.<br/>
        /// </summary>
        public event EventHandler<EngineStartingArgs>? EngineStarting;

        /// <summary>
        /// Call <see cref="EngineStarting"/>. Only the core can call this.
        /// </summary>
        /// <param name="args"></param>
        internal virtual void OnEngineStarting(EngineStartingArgs args) => EngineStarting?.Invoke(this, args);

        /// <summary>
        /// When the engine is stopping. This is here that all events that are not still unsuscribed should be unsuscribed!
        /// </summary>
        public event EventHandler<EngineStoppingArgs>? EngineStopping;

        /// <summary>
        /// Call <see cref="EngineStopping"/>. Only the core can call this.
        /// </summary>
        /// <param name="args"></param>
        internal virtual void OnEngineStopping(EngineStoppingArgs args) => EngineStopping?.Invoke(this, args);

        /// <summary>
        /// When the engine is fully ready. This is called once the core and the UI is ready. For the RTP see <see cref="RTPReady"/>.
        /// </summary>
        public event EventHandler<EngineReadyArgs>? EngineReady;

        /// <summary>
        /// Call <see cref="EngineReady"/>. Only the core can call this.
        /// </summary>
        /// <param name="args"></param>
        internal virtual void OnEngineReady(EngineReadyArgs args) => EngineReady?.Invoke(this, args);

        //public event EventHandler<EngineLoadProjectArgs>? EngineLoadProject;
        //public virtual void OnEngineLoadProject(EngineLoadProjectArgs) => EngineLoadProject?.Invoke(this, args);
        //public event EventHandler<EngineLoadedProjectArgs>? EngineLoadedProject;
        //public virtual void OnEngineLoadedProject(EngineLoadedProjectArgs) => EngineLoadedProject?.Invoke(this, args);

        //public event EventHandler<EngineCreateProjectArgs>? EngineCreateProject;
        //public virtual void OnEngineCreateProject(EngineCreateProjectArgs) => EngineCreateProject?.Invoke(this, args);
        //public event EventHandler<EngineCreatedProjectArgs>? EngineCreatedProject;
        //public virtual void OnEngineCreatedProject(EngineCreatedProjectArgs) => EngineCreatedProject?.Invoke(this, args);

        //public event EventHandler<EngineDeleteProjectArgs>? EngineDeleteProject;
        //public virtual void OnEngineDeleteProject(EngineDeleteProjectArgs) => EngineDeleteProject?.Invoke(this, args);
        //public event EventHandler<EngineDeletedProjectArgs>? EngineDeletedProject;
        //public virtual void OnEngineDeletedProject(EngineDeletedProjectArgs) => EngineDeletedProject?.Invoke(this, args);

        //public event EventHandler<EngineUnloadProjectArgs>? EngineUnloadProject;
        //public virtual void OnEngineUnloadProject(EngineUnloadProjectArgs) => EngineUnloadProject?.Invoke(this, args);
        //public event EventHandler<EngineUnloadedProjectArgs>? EngineUnloadedProject;
        //public virtual void OnEngineUnloadedProject(EngineUnloadedProjectArgs) => EngineUnloadedProject?.Invoke(this, args);

        public event EventHandler<EngineErrorArgs>? EngineError;
        public virtual void OnEngineError(EngineErrorArgs args) => EngineError?.Invoke(this, args);

        #endregion

        #region CORE
        /// <summary>
        /// Once the core is ready. Only the core, for other part, see <see cref="UIReady"/> for UI, and <see cref="RTPReady"/> for the RealTime Preview.
        /// </summary>
        public event EventHandler<CoreReadyArgs>? CoreReady;
        /// <summary>
        /// Call <see cref="CoreReady"/>. Only the core can call this.
        /// </summary>
        /// <param name="e"></param>
        internal virtual void OnCoreReady(CoreReadyArgs e) => CoreReady?.Invoke(this, e);

        public event EventHandler<CoreDataReadyArgs>? CoreDataReady;
        internal virtual void OnCoreDataReady(CoreDataReadyArgs e) => CoreDataReady?.Invoke(this, e);

        public event EventHandler<CoreManagersReadyArgs>? CoreManagersReady;
        internal virtual void OnCoreManagersReady(CoreManagersReadyArgs e) => CoreManagersReady?.Invoke(this, e);

        public event EventHandler<CoreModulesReadyArgs>? CoreModulesReady;
        internal virtual void OnCoreModulesReady(CoreModulesReadyArgs e) => CoreModulesReady?.Invoke(this, e);


        /// <summary>
        /// If the core has an error, 99% of the time, this will be an fatal error because if the core has a problem, all other parts could then get an error.
        /// </summary>
        public event EventHandler<CoreErrorArgs>? CoreError; 
        /// <summary>
        /// Call <see cref="CoreError"/>. Only the core can call this.
        /// </summary>
        /// <param name="e"></param>
        internal virtual void OnCoreError(CoreErrorArgs e) => CoreError?.Invoke(this, e);

        public event EventHandler<AssetsManagerUpdatedAssetArgs>? AssetsManagerUpdatedAsset;
        internal void OnAssetsManagerUpdatedAsset(AssetsManagerUpdatedAssetArgs e) => AssetsManagerUpdatedAsset?.Invoke(this, e);

        #endregion

        #region UI
        /// <summary>
        /// Once the UI is ready. Only the UI, for other part, see <see cref="CoreReady"/> for Core, and <see cref="RTPReady"/> for the RealTime Preview.
        /// </summary>
        public event EventHandler<UIReadyArgs>? UIReady;
        /// <summary>
        /// Call <see cref="UIReady"/>
        /// </summary>
        /// <param name="e"></param>
        public virtual void OnUIReady(UIReadyArgs e) => UIReady?.Invoke(this, e);

        // ===
        // Window part
        // ===

        /// <summary>
        /// Once the UI open the launcher window.
        /// </summary>
        public event EventHandler<UIWindowOpenArgs>? UILauncherOpened;
        public virtual void OnUILauncherOpened(UIWindowOpenArgs e) => UILauncherOpened?.Invoke(this, e);

        /// <summary>
        /// Once the UI close the launcher window.
        /// </summary>
        public event EventHandler<UIWindowCloseArgs>? UILauncherClosed;
        public virtual void OnUILauncherClosed(UIWindowCloseArgs e) => UILauncherClosed?.Invoke(this, e);

        /// <summary>
        /// Once the UI open the editor window.
        /// </summary>
        public event EventHandler<UIWindowOpenArgs>? UIEditorOpened;
        public virtual void OnUIEditorOpened(UIWindowOpenArgs e) => UIEditorOpened?.Invoke(this, e);

        /// <summary>
        /// Once the UI close the editor window.
        /// </summary>
        public event EventHandler<UIWindowCloseArgs>? UIEditorClosed;
        public virtual void OnUIEditorClosed(UIWindowCloseArgs e) => UIEditorClosed?.Invoke(this, e);

        // ===
        // Project part
        // ===

        //public event EventHandler<UILoadedProjectArgs>? UILoadedProject;
        //public event EventHandler<UIUnloadedProjectArgs>? UIUnloadedProject;

        //// ===
        //// Map part // Global events are inside of the ProjectEvent class (from the core)
        //// ===

        //public event EventHandler<UILoadMapArgs>? UILoadMap;
        //public event EventHandler<UILoadedMapArgs>? UILoadedMap;

        //public event EventHandler<UIUnloadMapArgs>? UIUnloadMap;
        //public event EventHandler<UIUnloadedMapArgs>? UIUnloadedMap;

        //public event EventHandler<UICreateMapArgs>? UICreateMap;
        //public event EventHandler<UICreatedMapArgs>? UICreatedMap;

        //public event EventHandler<UIDeleteMapArgs>? UIDeleteMap;
        //public event EventHandler<UIDeletedMapArgs>? UIDeletedMap;

        //public event EventHandler<UISaveMapArgs>? UISaveMap;
        //public event EventHandler<UISavedMapArgs>? UISavedMap;

        #endregion

        // RTP => RealTime Preview
        #region RTP
        public event EventHandler<RTPCreatingArgs>? RTPCreating;
        public virtual void OnRTPCreating(RTPCreatingArgs e) => RTPCreating?.Invoke(this, e);

        public event EventHandler<RTPCreatedArgs>? RTPCreated;
        public virtual void OnRTPCreated(RTPCreatedArgs e) => RTPCreated?.Invoke(this, e);


        /// <summary>
        /// Once the RTP (RealTime Preview) is ready. Only the RTP, for other part, see <see cref="CoreReady"/> for Core, and <see cref="UIReady"/> for UI.
        /// </summary>
        public event EventHandler<RTPReadyArgs>? RTPReady; // RTP = RealTime Preview
        /// <summary>
        /// Call <see cref="RTPReady"/>.
        /// </summary>
        /// <param name="e"></param>
        public virtual void OnRTPReady(RTPReadyArgs e) => RTPReady?.Invoke(this, e);
        /// <summary>
        /// Once the RTP (RealTime Preview) is showed, this is called after the UI Editor is opened for example.
        /// </summary>
        public event EventHandler<RTPShowedArgs>? RTPShowed;
        public virtual void OnRTPShowed(RTPShowedArgs e) => RTPShowed?.Invoke(this, e);
        /// <summary>
        /// Once the RTP (RealTime Preview) is hidden, this is called after the UI Editor is closed for example.
        /// </summary>
        public event EventHandler<RTPHiddenArgs>? RTPHidden;
        public virtual void OnRTPHidden(RTPHiddenArgs e) => RTPHidden?.Invoke(this, e);
        
        public event EventHandler<KeyEventArgs>? RTPKeyPressed;
        public virtual void OnRTPKeyPressed(KeyEventArgs e) => RTPKeyPressed?.Invoke(this, e);

        
        public event EventHandler<(AnimationInstance, AnimationInstance)>? DEBUG_RTPAnimationAtlasGenerated;
        public virtual void OnDEBUG_RTPAnimationAtlasGenerated(AnimationInstance e, AnimationInstance e2) => DEBUG_RTPAnimationAtlasGenerated?.Invoke(this, (e, e2));
        
        
        //// ===
        //// Project part
        //// ===
        ///// <summary>
        ///// Called when the RTP is loading a project. This is called before the project is loaded, so you can do some stuff before the project is loaded.
        ///// </summary>
        //public event EventHandler<RTPLoadProjectArgs>? RTPLoadProject;
        ///// <summary>
        ///// Called when the RTP as loaded a project. This is called after the project is loaded, so you can do some stuff after the project is loaded.
        ///// </summary>
        //public event EventHandler<RTPLoadedProjectArgs>? RTPLoadedProject;

        ///// <summary>
        ///// Called when the RTP is unloading a project. This is called before the project is unloaded, so you can do some stuff before the project is unloaded.
        ///// </summary>
        //public event EventHandler<RTPUnloadProjectArgs>? RTPUnloadProject;
        ///// <summary>
        ///// Called when the RTP as unloaded a project. This is called after the project is unloaded, so you can do some stuff after the project is unloaded.
        ///// </summary>
        //public event EventHandler<RTPUnloadedProjectArgs>? RTPUnloadedProject;

        //// ===
        //// Map part // Global events are inside of the ProjectEvent class (from the core)
        //// ===

        ///// <summary>
        ///// Called when the RTP is loading a map. This is called before the map is loaded, so you can do some stuff before the map is loaded.
        ///// </summary>
        //public event EventHandler<RTPLoadMapArgs>? RTPLoadMap;

        ///// <summary>
        ///// Called when the RTP as loaded a map. This is called after the map is loaded, so you can do some stuff after the map is loaded.
        ///// </summary>
        //public event EventHandler<RTPLoadedMapArgs>? RTPLoadedMap;

        //public event EventHandler<RTPSaveMapArgs>? RTPSaveMap;
        //public event EventHandler<RTPSavedMapArgs>? RTPSavedMap;

        #endregion
    }
}
