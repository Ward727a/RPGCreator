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
using RPGCreator.SDK;
using RPGCreator.SDK.Common.Logging;
using RPGCreator.SDK.Modules;
using RPGCreator.SDK.Services.ModuleServices;
using RPGCreator.Shared.Attributes;
using RPGCreator.Shared.Types;

namespace RPGCreator.EngineLib.Services.Modules;

[EngineService]
public class ModuleShadowCopyManager
{
    private readonly ScopedLogger _logger = Logger.ForContext<ModuleShadowCopyManager>();
    
    private (string shadowDll, string? shadowPdp, string originalDll, string? originalPdp) GetShadowPath(
        string modulePath)
    {
        var moduleFileName = Path.GetFileNameWithoutExtension(modulePath);
        var pdpFileName = $"{moduleFileName}.pdb";

        var moduleDirectory = Path.GetDirectoryName(modulePath);

        var cacheDirResult = RpgEnv.PathFormat.GetModuleCachePath();

        if (cacheDirResult.IsFailure)
        {
            _logger.Error("Failed to get module cache path: {Error}", args: cacheDirResult.Error);
            return ("", null, modulePath, null);
        }

        var cacheDir = cacheDirResult.Value;

        var shadowPdpCopyName = $"{moduleFileName}.shadow.pdb";
        var shadowDllCopyName = $"{moduleFileName}.shadow.dll";

        var originalDll = modulePath;
        string pdpPath = Path.Combine(moduleDirectory!, pdpFileName);

        var shadowPdpPath = (string?)null;
        var originalPdp = (string?)null;

        try
        {
            if (File.Exists(pdpPath))
            {
                shadowPdpPath = Path.Combine(cacheDir, shadowPdpCopyName);
                originalPdp = pdpPath;
            }

            var shadowDllPath = Path.Combine(cacheDir, shadowDllCopyName);

            return (shadowDllPath, shadowPdpPath, originalDll, originalPdp);
        }
        catch (Exception ex)
        {
            _logger.Error("Failed to get shadow copy paths for module '{ModulePath}'. Exception: {Exception}",
                args: [modulePath, ex]);
            return ("", null, originalDll, null);
        }
    }

    internal Result<string> CreateShadow(string modulePath)
    {
        var copyPath = GetShadowPath(modulePath);
        var shadowDll = copyPath.shadowDll;
        var shadowPdp = copyPath.shadowPdp;

        var originalDll = copyPath.originalDll;
        var originalPdp = copyPath.originalPdp;

        if (string.IsNullOrEmpty(shadowDll))
            return Result<string>.Fail(
                $"Failed to create shadow copy for module '{modulePath}': shadow dll path is empty.");

        if (File.Exists(shadowDll))
        {
            if (TryDeleteShadow(shadowDll).IsFailure)
            {
                return Result<string>.Fail($"Failed to delete existing shadow copy for module '{modulePath}'.");
            }
        }

        try
        {
            if (!string.IsNullOrEmpty(shadowPdp) && !string.IsNullOrEmpty(originalPdp))
            {
                File.Copy(originalPdp, shadowPdp, true);
            }

            File.Copy(originalDll, shadowDll, true);
            return Result<string>.Success(shadowDll);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Result<string>.Fail("Unauthorized access: " + ex.Message +
                                       " (check if the module is already loaded in the engine)");
        }
        catch (PathTooLongException ex)
        {
            return Result<string>.Fail("Path too long: " + ex.Message);
        }
        catch (DirectoryNotFoundException ex)
        {
            return Result<string>.Fail("Directory not found: " + ex.Message);
        }
        catch (IOException ex)
        {
            return Result<string>.Fail("File locked by another process? Error: " + ex.Message);
        }
        catch (Exception ex)
        {
            return Result<string>.Fail($"Failed to create shadow copy for module '{modulePath}': {ex.Message}");
        }

        Result TryDeleteShadow(string path)
        {
            if (!File.Exists(path))
            {
                return Result.Success();
            }

            try
            {
                var attributes = File.GetAttributes(path);
                if ((attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                {
                    File.SetAttributes(path, attributes & ~FileAttributes.ReadOnly);
                }

                File.Delete(path);
                return Result.Success();
            }
            catch (IOException ex)
            {
                return Result.Fail($"File locked by another process: {ex.Message}");
            }
            catch (UnauthorizedAccessException ex)
            {
                return Result.Fail($"Unauthorized access: {ex.Message}");
            }
            catch (Exception ex)
            {
                return Result.Fail($"Failed to delete shadow copy: {ex.Message}");
            }
        }
    }

    public void ClearShadows(EngineSecurityToken? token = null)
    {
        if (token == null)
        {
            StackTrace stackTrace = new StackTrace();
            var callingMethod = stackTrace.GetFrame(1)?.GetMethod();
            throw new UnauthorizedAccessException(
                $"ClearShadows method can only be called by the engine. Unauthorized call from method: {callingMethod?.DeclaringType?.FullName}.{callingMethod?.Name} in assembly {callingMethod?.DeclaringType?.Assembly.FullName} estimed path: {callingMethod?.DeclaringType?.Assembly.Location}");
        }

        var cachePathResult = RpgEnv.PathFormat.GetModuleCachePath();

        if (cachePathResult.IsFailure)
        {
            _logger.Error("Failed to get module cache path: {Error}", args: cachePathResult.Error);
            return;
        }

        var cachePath = cachePathResult.Value;

        if (!Directory.Exists(cachePath))
        {
            _logger.Debug("Module cache directory does not exist: {ModuleCachePath}", args: cachePath);
            return;
        }

        try
        {
            Directory.Delete(cachePath, true);
        }
        catch (Exception ex)
        {
            _logger.Error("Failed to clear module shadows. Exception: {Exception}", args: ex);
        }
    }
}