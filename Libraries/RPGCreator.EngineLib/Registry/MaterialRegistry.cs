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

using Fluxor;
using RPGCreator.SDK;
using RPGCreator.SDK.Assets.AutoTiles;
using RPGCreator.SDK.Common.Logging;
using RPGCreator.SDK.Serializer;
using RPGCreator.SDK.Services.Assets;
using RPGCreator.SDK.Services.Map;
using RPGCreator.SDK.States;
using RPGCreator.Shared.Attributes;

namespace RPGCreator.EngineLib.Registry;

[EngineService(typeof(IMaterialRegistry))]
public class MaterialRegistry(
    ISerializerService serializerService,
    IAssetManager assetManager,
    IState<ProjectState> projectState) : IMaterialRegistry
{
    private readonly ScopedLogger Logger = SDK.Common.Logging.Logger.ForContext<MaterialRegistry>();
    private readonly Ulid[] _rulesets = new Ulid[128];
    private readonly string[] _materialNames = new string[128];
    public uint Version { get; private set; } = 0;

    public void RegisterRuleset(MaterialId id, Ulid ruleset, string name)
    {
        _rulesets[id] = ruleset;
        _materialNames[id] = name;
        Version++;
    }

    public MaterialId GetMaterialId(Ulid rulesetId)
    {
        var index = _rulesets.IndexOf(rulesetId);
        
        if(index == -1)
            return MaterialId.Empty;

        return (byte)index;
    }

    public Ulid GetRuleset(MaterialId id) => _rulesets[id];
    public string GetMaterialName(MaterialId id) => _materialNames[id];
    
    public IEnumerable<(string, MaterialId)> GetMaterials()
    {
        for (int i = 0; i < 128; i++)
        {
            if (_rulesets[i] != Ulid.Empty)
                yield return (_materialNames[i], (byte)i);
        }
    }
    
    public MaterialId GetFirstAvailableMaterialId()
    {
        for (byte i = 1; i < 128; i++)
        {
            if (_rulesets[i] == Ulid.Empty) return i;
        }

        return MaterialId.Empty;
    }

    public bool HasRuleset(MaterialId id) => _rulesets[id] != Ulid.Empty;

    public void Save()
    {
        var filePath = GetFilePath();

        if (string.IsNullOrWhiteSpace(filePath))
        {
            Logger.Critical("Failed to save material registry, file path is null or empty");
            return;
        }

        var saveData = new MaterialRegistrySaveData();

        for (byte i = 0; i < 128; i++)
        {
            if (_rulesets[i] == Ulid.Empty) continue;
            
            var autoTileId = _rulesets[i];
            var autoTileName = _materialNames[i];
            
            saveData.Materials.Add(new MaterialRegistrySaveData.MaterialEntry()
            {
                Name = autoTileName,
                Id = i,
                RulesetId = autoTileId
            });
        }

        serializerService.SerializeTo(saveData, filePath);
    }

    public void Load()
    {
        var filePath = GetFilePath();
        if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
        {
            Logger.Warning($"Material registry file not found at {filePath}. Starting with empty registry.");
            return;
        }

        try
        {
            if (!serializerService.DeserializeFrom(filePath, out MaterialRegistrySaveData? saveData))
            {
                Logger.Warning(
                    $"Failed to deserialize material registry from {filePath}. Starting with empty registry.");
                return;
            }

            foreach (var material in saveData.Materials)
            {
                if (material.Id >= 128)
                {
                    Logger.Error("Invalid material ID {id} for material {name} in {filepath}. Must be < 128.",
                        args: [material.Id, material.Name, filePath]);
                    continue;
                }

                if (!assetManager.HasAsset(material.RulesetId))
                {
                    Logger.Error(
                        "Material ruleset {rulesetId} for material ID {materialId} with name {materialName} in {filepath} does not exist.",
                        args: [material.RulesetId, material.Id, material.Name, filePath]);
                    continue;
                }

                var materialId = new MaterialId(material.Id);
                RegisterRuleset(materialId, material.RulesetId, material.Name);
            }
        }
        catch (Exception ex)
        {
            Logger.Critical($"Failed to load material registry: {ex.Message}");
        }
    }


    #region helpers

    private string? GetFilePath()
    {
        if (!projectState.Value.HasProject)
            return null;

        var assetFolder = RpgEnv.PathFormat.GetAssetsPath().Match(path => path, err => null);

        if (assetFolder == null)
            return null;

        var autoTilesFolder = Path.Combine(assetFolder, "autoTiles");

        if (!Directory.Exists(autoTilesFolder))
            Directory.CreateDirectory(autoTilesFolder);

        return Path.Combine(autoTilesFolder, "materials.json");
    }

    #endregion
}