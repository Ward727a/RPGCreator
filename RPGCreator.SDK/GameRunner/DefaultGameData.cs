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

using RPGCreator.SDK.Attributes;
using RPGCreator.SDK.Logging;
using RPGCreator.SDK.Serializer;

namespace RPGCreator.SDK.GameRunner;

[SerializingType("DefaultGameData")]
public class DefaultGameData : IGameData
{
    public string ProjectPath { get; private set; } = "";
    public string ModulesPath { get; private set; } = "";
    
    private List<string> _modulesHashes = new();
    public IReadOnlyList<string> AcceptedModuleHashes => _modulesHashes;
    public Ulid MainMapId { get; private set; } = Ulid.Empty;

    public SerializationInfo GetObjectData()
    {
        var info = new SerializationInfo(typeof(DefaultGameData))
            .AddValue(nameof(ProjectPath), ProjectPath)
            .AddValue(nameof(ModulesPath), ModulesPath)
            .AddValue(nameof(MainMapId), MainMapId)
            .AddValue(nameof(_modulesHashes), _modulesHashes);

        return info;
    }

    public List<Ulid> GetReferencedAssetIds()
    {
        return new List<Ulid> { MainMapId };
    }

    public void SetObjectData(DeserializationInfo info)
    {
        info.TryGetValue(nameof(ProjectPath), out string? projectPath);
        info.TryGetValue(nameof(ModulesPath), out string? modulesPath);
        info.TryGetValue(nameof(MainMapId), out Ulid mainMapId);
        info.TryGetValue(nameof(_modulesHashes), out List<string>? modulesHashes);

        if (projectPath == null)
        {
            Logger.Critical("DefaultGameData: ProjectPath is null during deserialization.");
            throw new InvalidOperationException("Error deserializing DefaultGameData: ProjectPath is null.");
        }
        ProjectPath = projectPath;
        if (modulesPath == null)
        {
            Logger.Critical("DefaultGameData: ModulesPath is null during deserialization.");
            throw new InvalidOperationException("Error deserializing DefaultGameData: ModulesPath is null.");
        }
        if (mainMapId == Ulid.Empty)
        {
            Logger.Critical("DefaultGameData: MainMapId is empty during deserialization.");
            throw new InvalidOperationException("Error deserializing DefaultGameData: MainMapId is empty.");
        }
        MainMapId = mainMapId;
        
        ModulesPath = modulesPath;
        if (modulesHashes == null)
        {
            modulesHashes = new List<string>();
        }
        _modulesHashes = modulesHashes;
    }
}