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
using RPGCreator.Core.Configs.EventsArgs;

namespace RPGCreator.Core.Configs
{
    public class EngineConfigEvents
    {

        public event EventHandler<LoadingConfigArgs>? LoadingConfig;
        public virtual void OnLoadingConfig(LoadingConfigArgs args)
        {
            LoadingConfig?.Invoke(this, args);
        }
        
        public event EventHandler<LoadedConfigArgs>? LoadedConfig;
        public virtual void OnLoadedConfig(LoadedConfigArgs args)
        {
            LoadedConfig?.Invoke(this, args);
        }
        
        public event EventHandler<CreatingConfigArgs>? CreatingConfig;
        public virtual void OnCreatingConfig(CreatingConfigArgs args)
        {
            CreatingConfig?.Invoke(this, args);
        }
        
        public event EventHandler<CreatedConfigArgs>? CreatedConfig;
        public virtual void OnCreatedConfig(CreatedConfigArgs args)
        {
            CreatedConfig?.Invoke(this, args);
        }
        
        public event EventHandler<SavingConfigArgs>? SavingConfig;
        public virtual void OnSavingConfig(SavingConfigArgs args)
        {
            SavingConfig?.Invoke(this, args);
        }
        
        public event EventHandler<SavedConfigArgs>? SavedConfig;
        public virtual void OnSavedConfig(SavedConfigArgs args)
        {
            SavedConfig?.Invoke(this, args);
        }

        public event EventHandler<UnloadingConfigArgs>? UnloadingConfig;
        public virtual void OnUnloadingConfig(UnloadingConfigArgs args)
        {
            UnloadingConfig?.Invoke(this, args);
        }
        
        public event EventHandler<UnloadedConfigArgs>? UnloadedConfig;
        public virtual void OnUnloadedConfig(UnloadedConfigArgs args)
        {
            UnloadedConfig?.Invoke(this, args);
        }
        
        public event EventHandler<ExportingConfigArgs>? ExportingConfig;
        public virtual void OnExportingConfig(ExportingConfigArgs args)
        {
            ExportingConfig?.Invoke(this, args);
        }
        
        public event EventHandler<ExportedConfigArgs>? ExportedConfig;
        public virtual void OnExportedConfig(ExportedConfigArgs args)
        {
            ExportedConfig?.Invoke(this, args);
        }

    }
}
