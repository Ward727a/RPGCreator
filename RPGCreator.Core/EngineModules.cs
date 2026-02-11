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
using System.Reflection;
using System.Runtime.Loader;
using RPGCreator.Core.Common;
using RPGCreator.Core.Module;
using RPGCreator.SDK;
using RPGCreator.SDK.EditorUiService;
using RPGCreator.SDK.EngineService;
using RPGCreator.SDK.Logging;
using RPGCreator.SDK.Modules;
using RPGCreator.SDK.Types;

namespace RPGCreator.Core
{

    
    public class ModuleContext : AssemblyLoadContext
    {
        private readonly AssemblyDependencyResolver _resolver;

        public ModuleContext(string modulePath) : base(isCollectible: true)
        {
            _resolver = new AssemblyDependencyResolver(modulePath);
        }

        protected override Assembly? Load(AssemblyName assemblyName)
        {
            string? assemblyPath = _resolver.ResolveAssemblyToPath(assemblyName);
            return assemblyPath != null ? LoadFromAssemblyPath(assemblyPath) : null;
        }
    }
    
    public class EngineModules : IModuleManager
    {
        private readonly Dictionary<URN, ModuleContext> _contexts = new();
        private readonly Dictionary<URN, ModuleCandidate> _loadedModulesByUrn = new();
        private readonly Dictionary<URN, BaseModule> _startedModulesByUrn = new();
        private readonly List<Assembly> _moduleAssemblies = new();
        
        private readonly ScopedLogger _logger = Logger.ForContext<EngineModules>();
        
        // This is the SHA256 checksum of the module DLL file to ensure integrity.
        // Those should be updated with each new module version. (even for small changes!)
        private readonly List<string> CHECKSUM_INTERNAL_MODULES =
        [ 
            // "f3886692656072a8702c0d0faf32d956fefa6078f47087d4a5113b1927d5b6eb", // TestModule.dll - For now disabled so it doesn't load automatically
        ];

        private readonly string MODULES_PATH = $"{AppContext.BaseDirectory}Assets/Modules/";
        
        internal EngineModules()
        {                   
            
            
            TaskScheduler.UnobservedTaskException += (sender, e) => 
            {
                foreach (var inner in e.Exception.InnerExceptions)
                {
                    if (inner.TargetSite != null && inner.TargetSite.DeclaringType != null &&
                        inner.TargetSite.DeclaringType == typeof(EngineModules))
                    {
                        _logger.Critical("Unobserved task exception in a module: {Exception}", args: inner);

                        if (inner is UnauthorizedAccessException UAE)
                        {
                            _logger.Critical("UnauthorizedAccessException detected: {Message}", args: UAE.Message);
                            _logger.Critical("This may indicate a security violation within the module!!!");
                            EditorUiServices.NotificationService.Error("SECURITY_ALERT!", $"Security Alert: A module attempted an unauthorized operation. The engine remains stable, but please review module usage.", new NotificationOptions(60000));
                            
                        }
                    }
                }
            };
            _logger.Info($"EngineModules initialized.");

            if (EngineCore.DetectedMode == EngineCore.EEngineMode.PlayerMode)
            {
                _logger.Debug("Player mode detected, skipping module loading.");
                return;
            }

            if (!Directory.Exists(MODULES_PATH))
            {
                _logger.Error("Engine modules directory not found.");
                return;
            }

            ClearTempModulesShadowCopies("", new EngineSecurityToken());
            
            foreach (var directory in Directory.GetDirectories(MODULES_PATH))
            {
                var files = Directory.GetFiles(directory, "*.dll");
                foreach (var file in files)
                {
                    try
                    {
                        // Calculate the SHA256 checksum of the file.
                        var hashString = ShaUtil.ComputeSha256(file);
                        if (CHECKSUM_INTERNAL_MODULES.Contains(hashString)
                            #if DEBUG
                            || true
                            #endif
                            )
                        {
                            // DISABLED FOR NOW
                            // I was kinda annoyed during development, having to update the checksum each time I made a small change.
                            // So this is disabled for now, but should be re-enabled before release.
                            #if DEBUG
                            _logger.Error("[ModuleLoader] Warning: Module integrity check is currently disabled. This should only be used for development purposes.");
                            _logger.Error("[ModuleLoader] Warning: Module integrity check is currently disabled. This should only be used for development purposes.");
                            _logger.Error("[ModuleLoader] Warning: Module integrity check is currently disabled. This should only be used for development purposes.");
                            _logger.Error("[ModuleLoader] Warning: Module integrity check is currently disabled. This should only be used for development purposes.");
                            _logger.Error("[ModuleLoader] Warning: Module integrity check is currently disabled. This should only be used for development purposes.");
                            _logger.Error("[ModuleLoader] Warning: Module integrity check is currently disabled. This should only be used for development purposes.");
                            _logger.Error("[ModuleLoader] Warning: Module integrity check is currently disabled. This should only be used for development purposes.");
                            #endif
                            if (TryLoadModule(file, new EngineSecurityToken()))
                            {
                                _logger.Info($"Module file '{file}' loaded successfully.");
                            }
                        }
                        else
                        {
                            _logger.Warning($"Module file '{file}' failed integrity check and will not be loaded.");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.Error($"Failed to load module from file '{file}'. Exception: {ex}");
                    }
                }
            }

            _logger.Info("Gotten (i){int}, (f){float}, (s){string}, (b){bool}, (v2){vector2} states components size",
                args:
                [
                    EngineServices.ECS.StateRegistry.TotalInt,
                    EngineServices.ECS.StateRegistry.TotalFloat,
                    EngineServices.ECS.StateRegistry.TotalString,
                    EngineServices.ECS.StateRegistry.TotalBool,
                    EngineServices.ECS.StateRegistry.TotalVector2
                ]);
            
            AppDomain.CurrentDomain.UnhandledException += (sender, e) => 
            {
                var ex = (Exception)e.ExceptionObject;
                
                var assembly = ex.TargetSite?.DeclaringType?.Assembly;
    
                if (assembly != null && _moduleAssemblies.Contains(assembly)) 
                {
                    Logger.Critical("Unhandled exception in module assembly '{Assembly}': {Exception}", args: [assembly.FullName, ex]);
                }
            };

        }

        internal (string dllCopy, string? pdpCopy, string originalDll, string? originalPdp) GetShadowCopyPath(string modulePath)
        {
            
            var moduleFileName = Path.GetFileNameWithoutExtension(modulePath);
            var pdpFileName = $"{moduleFileName}.pdb";
            var moduleDirectory = Path.GetDirectoryName(modulePath);
            
            var shadowPdpCopyName = $"_runned_temp_{moduleFileName}.pdb";
            var shadowDllCopyName = $"_runned_temp_{moduleFileName}.dll";
            
            var originalDll = modulePath;
            string PdpPath = Path.Combine(moduleDirectory!, pdpFileName);

            var pdpCopyPath = (string?)null;
            var originalPdp = (string?)null;

            try
            {
                if (File.Exists(PdpPath))
                {
                    pdpCopyPath = Path.Combine(moduleDirectory!, shadowPdpCopyName);
                    originalPdp = PdpPath;
                }
                var shadowCopyPath = Path.Combine(moduleDirectory!, shadowDllCopyName);
                
                return (shadowCopyPath, pdpCopyPath, originalDll, originalPdp);
            }
            catch (Exception ex)
            {
                _logger.Error("Failed to get shadow copy paths for module '{ModulePath}'. Exception: {Exception}", args: [modulePath, ex]);
                return ("", null, originalDll, null);
            }
        }
        
        

        private bool IsSameCopy(string pathToModule)
        {
            var copyPath = GetShadowCopyPath(pathToModule);
            string? copyPdpPath = copyPath.pdpCopy;
            string copyDllPath = copyPath.dllCopy;
            string originalDllPath = copyPath.originalDll;
            string? originalPdpPath = copyPath.originalPdp;
            
            if(File.Exists(copyDllPath))
            {
                var copySha = ShaUtil.ComputeSha256(copyDllPath);
                var originalSha = ShaUtil.ComputeSha256(originalDllPath);
                if (copySha != originalSha)
                {
                    return false;
                }

                if (!string.IsNullOrEmpty(copyPdpPath) && File.Exists(copyPdpPath))
                {
                    if(string.IsNullOrEmpty(originalPdpPath) || !File.Exists(originalPdpPath))
                    {
                        return false;
                    }
                    var copyPdbSha = ShaUtil.ComputeSha256(copyPdpPath);
                    var originalPdbSha = ShaUtil.ComputeSha256(originalPdpPath);
                    
                    return copyPdbSha == originalPdbSha;
                }
                return true;
            }
            return false;
        }

        
        internal string CreateShadowCopyPath(string modulePath)
        {
            // If the shadow copy already exists and is the same, return it directly
            if(IsSameCopy(modulePath))
            {
                var existingCopyPath = GetShadowCopyPath(modulePath);
                return existingCopyPath.dllCopy;
            }
            var copyPath = GetShadowCopyPath(modulePath);
            string? copyPdpPath = copyPath.pdpCopy;
            string copyDllPath = copyPath.dllCopy;
            string originalDllPath = copyPath.originalDll;
            string? originalPdpPath = copyPath.originalPdp;

            if (string.IsNullOrEmpty(copyDllPath))
                return "";
            
            try
            {
                if (!string.IsNullOrEmpty(copyPdpPath) && !string.IsNullOrEmpty(originalPdpPath))
                {
                    File.Copy(originalPdpPath, copyPdpPath, true);
                }
                File.Copy(originalDllPath, copyDllPath, true);
                return copyDllPath;
            }
            catch (Exception ex)
            {
                _logger.Error("Failed to create shadow copy for module '{ModulePath}'. Exception: {Exception}", args: [modulePath, ex]);
                return "";
            }
        }

        public bool TryLoadModule(string modulePath, EngineSecurityToken token)
        {
            if (token == null)
            {
                StackTrace stackTrace = new StackTrace();
                var callingMethod = stackTrace.GetFrame(1)?.GetMethod();
                throw new UnauthorizedAccessException($"TryLoadModule method can only be called by the engine. Unauthorized call from method: {callingMethod?.DeclaringType?.FullName}.{callingMethod?.Name} in assembly {callingMethod?.DeclaringType?.Assembly.FullName} estimed path: {callingMethod?.DeclaringType?.Assembly.Location}");
            }

            var copy = CreateShadowCopyPath(modulePath);

            if (string.IsNullOrEmpty(copy))
            {
                _logger.Error("Failed to load module from path '{ModulePath}' due to shadow copy creation failure.", args: modulePath);
                return false;
            }
            
            var context = new ModuleContext(copy);
            var assembly = context.LoadFromAssemblyPath(copy);

            var moduleType = assembly.GetLoadableTypes().FirstOrDefault(t => 
                typeof(BaseModule).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);
            
            if (moduleType == null)
            {
                _logger.Debug("No valid module type found in assembly '{AssemblyPath}'.", args: copy);
                return false;
            }
            
            var attr = assembly.GetCustomAttribute<ModuleManifestAttribute>();

            if (attr != null)
            {
                _contexts[attr.Urn] = context;
                _loadedModulesByUrn[attr.Urn] = new ModuleCandidate(attr, moduleType);
                _logger.Info("Module '{ModuleName}' v{ModuleVersion} by {ModuleAuthor} loaded successfully from assembly '{AssemblyPath}'.",
                    args:[attr.Name, attr.Version, attr.Author, copy]);
                
                #if DEBUG
                StartModule(attr.Urn, new EngineSecurityToken());
                #endif
                
                return true;
            }
            else
            {
                _logger.Error("ModuleManifestAttribute not found on module type '{ModuleType}' in assembly '{AssemblyPath}'.", args:[moduleType.FullName, copy]);
                return false;
            }
        }

        public bool IsModuleLoaded(URN moduleUrn)
        {
            return _loadedModulesByUrn.ContainsKey(moduleUrn);
        }

        public bool IsModuleStarted(URN moduleUrn)
        {
            return _startedModulesByUrn.ContainsKey(moduleUrn);
        }

        public ModuleCandidate? GetLoadedModule(URN moduleUrn)
        {
            return _loadedModulesByUrn.GetValueOrDefault(moduleUrn);
        }
        
        internal BaseModule? GetModuleInternal(URN moduleUrn)
        {
            return _startedModulesByUrn.GetValueOrDefault(moduleUrn);
        }

        public IEnumerable<ModuleCandidate> GetAllLoadedModules(EngineSecurityToken token)
        {
            return _loadedModulesByUrn.Values;
        }

        public IEnumerable<IEngineModuleInfo> GetAllStartedModules()
        {
            return _startedModulesByUrn.Values;
        }
        
        public void ClearTempModulesShadowCopies(string path = "", EngineSecurityToken? token = null)
        {            
            if(token == null)
            {
                StackTrace stackTrace = new StackTrace();
                var callingMethod = stackTrace.GetFrame(1)?.GetMethod();
                throw new UnauthorizedAccessException($"ClearTempModulesShadowCopies method can only be called by the engine. Unauthorized call from method: {callingMethod?.DeclaringType?.FullName}.{callingMethod?.Name} in assembly {callingMethod?.DeclaringType?.Assembly.FullName} estimed path: {callingMethod?.DeclaringType?.Assembly.Location}");
            }
            
            if (!Directory.Exists(MODULES_PATH) && string.IsNullOrEmpty(path))
            {
                return;
            }
            
            var searchPath = string.IsNullOrEmpty(path) ? MODULES_PATH : path;
            
            string[] filesToDelete = Directory.GetFiles(searchPath, "_runned_temp_*", SearchOption.AllDirectories);
            
            foreach (var filePath in filesToDelete)
            {
                try
                {
                    File.Delete(filePath);
                    _logger.Info("Deleted temporary module shadow copy file: {FilePath}", args: filePath);
                }
                catch (Exception ex)
                {
                    _logger.Warning("Failed to delete temporary module shadow copy file: {FilePath}. Exception: {Exception}", args: [filePath, ex]);
                }
            }
        }
        
        public void PlanStarting(out HashSet<URN> startOrder, out HashSet<URN> incompatibilities, List<URN>? modulesToStart = null)
        {
            modulesToStart ??= _loadedModulesByUrn.Keys.ToList();
            
            startOrder = [];
            var visiting = new HashSet<URN>();
            incompatibilities = [];

            foreach (var urn in modulesToStart)
            {
                VisitAndCheck(urn, startOrder, visiting, incompatibilities);
            }
            
            if(startOrder.Count > modulesToStart.Count)
            {
                _logger.Warning("Some modules were started due to dependencies but were not explicitly requested:");
                foreach (var urn in startOrder)
                {
                    if (!modulesToStart.Contains(urn))
                    {
                        _logger.Warning(" - {ModuleUrn}", args: urn);
                    }
                }
                if(incompatibilities.Count > 0)
                {
                    _logger.Warning("Incompatibilities detected with the following modules:");
                    foreach (var urn in incompatibilities)
                    {
                        _logger.Warning(" - {ModuleUrn}", args: urn);
                    }
                }
                _logger.Warning("Need to wait for user confirmation to proceed...");
            }
        }

        private void VisitAndCheck(URN urn, HashSet<URN> startOrder, HashSet<URN> visiting, HashSet<URN> incompatibilities)
        {
            if (startOrder.Contains(urn)) return;
            if (visiting.Contains(urn)) throw new Exception($"Circle dependencies detected for {urn}");

            visiting.Add(urn);

            var candidate = GetLoadedModule(urn);
            if (candidate != null)
            {
                incompatibilities.UnionWith(candidate.Value.Incompatibilities);
                foreach (var depUrn in candidate.Value.Dependencies)
                {
                    if (!URN.TryParse(depUrn, out var resolvedUrn) || resolvedUrn == URN.Empty) continue;
                    VisitAndCheck(new URN(depUrn), startOrder, visiting, incompatibilities);
                }
            }

            visiting.Remove(urn);
            startOrder.Add(urn);
        }
        
        public bool StartModule(URN moduleUrn, EngineSecurityToken token)
        {
            if (token == null)
            {
                StackTrace stackTrace = new StackTrace();
                var callingMethod = stackTrace.GetFrame(1)?.GetMethod();
                throw new UnauthorizedAccessException($"TryLoadModule method can only be called by the engine. Unauthorized call from method: {callingMethod?.DeclaringType?.FullName}.{callingMethod?.Name} in assembly {callingMethod?.DeclaringType?.Assembly.FullName} estimed path: {callingMethod?.DeclaringType?.Assembly.Location}");
            }
            if (!IsModuleLoaded(moduleUrn))
                return false;
            
            if(IsModuleStarted(moduleUrn))
                return true; // Already started
            
            var moduleCandidate = GetLoadedModule(moduleUrn);
            if (moduleCandidate == null)
                return false;
            
            var moduleType = moduleCandidate.Value.ModuleType;
            if (moduleType == null)
                return false; // Should not happen, but just in case
            BaseModule? module;
            try{
                module = (BaseModule?)Activator.CreateInstance(moduleType);
            } catch (Exception ex)
            {
                _logger.Error("Failed to create instance of module type '{ModuleType}' for module '{ModuleUrn}'. Exception: {Exception}",
                    args: [moduleType.FullName, moduleUrn, ex]);
                return false;
            }
            
            if (module == null)
                return false; // Failed to create instance

            try
            {
                module.Initialize(token);
            } catch (Exception ex)
            {
                _logger.Error("Failed to initialize module '{ModuleUrn}'. Exception: {Exception}",
                    args: [moduleUrn, ex]);
                return false;
            }

            _startedModulesByUrn[moduleUrn] = module;
            _moduleAssemblies.Add(moduleType.Assembly);
            return true;
        }

        public bool StopModule(URN moduleUrn, EngineSecurityToken token)
        {
            if (token == null)
            {
                StackTrace stackTrace = new StackTrace();
                var callingMethod = stackTrace.GetFrame(1)?.GetMethod();
                throw new UnauthorizedAccessException($"TryLoadModule method can only be called by the engine. Unauthorized call from method: {callingMethod?.DeclaringType?.FullName}.{callingMethod?.Name} in assembly {callingMethod?.DeclaringType?.Assembly.FullName} estimed path: {callingMethod?.DeclaringType?.Assembly.Location}");
            }
            if (!IsModuleStarted(moduleUrn))
                return true; // Already stopped
            
            var module = GetModuleInternal(moduleUrn);
            
            if (module == null)
                return true; // Not even loaded, consider it stopped

            try
            {
                module.Shutdown(token);
            }
            catch (Exception ex)
            {
                _logger.Error("Failed to shutdown module '{ModuleUrn}'. Exception: {Exception}",
                    args: [moduleUrn, ex]);
                return false;
            }

            _startedModulesByUrn.Remove(moduleUrn);
            _moduleAssemblies.Remove(module.GetType().Assembly);
            return true;
        }

        public void UnloadModule(URN moduleUrn)
        {
            if (IsModuleStarted(moduleUrn))
                return;
            var module = GetLoadedModule(moduleUrn);
            if (module == null && !_contexts.ContainsKey(moduleUrn))
                return;
            _loadedModulesByUrn.Remove(moduleUrn);
            var context = _contexts.GetValueOrDefault(moduleUrn);
            _contexts.Remove(moduleUrn);
            context?.Unload();
        }

        public void ForceUnloadModule(URN moduleUrn, EngineSecurityToken token)
        {
            if (token == null)
            {
                StackTrace stackTrace = new StackTrace();
                var callingMethod = stackTrace.GetFrame(1)?.GetMethod();
                throw new UnauthorizedAccessException($"TryLoadModule method can only be called by the engine. Unauthorized call from method: {callingMethod?.DeclaringType?.FullName}.{callingMethod?.Name} in assembly {callingMethod?.DeclaringType?.Assembly.FullName} estimed path: {callingMethod?.DeclaringType?.Assembly.Location}");
            }
            if (IsModuleStarted(moduleUrn))
            {
                StopModule(moduleUrn, token);
            }
            var module = GetLoadedModule(moduleUrn);
            if (module == null)
                return;
            _loadedModulesByUrn.Remove(moduleUrn);
            _startedModulesByUrn.Remove(moduleUrn);
            var context = _contexts.GetValueOrDefault(moduleUrn);
            _contexts.Remove(moduleUrn);
            context?.Unload();
            
            _logger.Critical("Forcefully unloaded module {ModuleUrn}. This may lead to instability if the module was in use.", args: moduleUrn);
        }
    }
}
