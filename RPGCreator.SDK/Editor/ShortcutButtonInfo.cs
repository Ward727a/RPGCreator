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
using RPGCreator.SDK.Types;

namespace RPGCreator.SDK.Editor;

[SerializingType("ShortcutButtonInfo")]
public record struct ShortcutButtonInfo : ISerializable, IDeserializable
{
    public string Name { get; set; }
    public string Description { get; set; }
    public string Icon { get; set; }
    public URN ActionUrn { get; set; }
    private Action<object[]?>? Action { get; set; }
    public Action<object[]?> GetAction()
    {
        var info = this;

        Action ??= RegistryServices.ActionRegistry.GetAction(ActionUrn)?.Action ?? ((_) =>
        {
            Logger.Error("Action not found for shortcut: " + info.ActionUrn);
        });

        return Action;
    }

    public SerializationInfo GetObjectData()
    {
        return new SerializationInfo(typeof(ShortcutButtonInfo))
            .AddValue(nameof(Name), Name)
            .AddValue(nameof(Description), Description)
            .AddValue(nameof(Icon), Icon)
            .AddValue(nameof(ActionUrn), ActionUrn);
    }

    public List<Ulid> GetReferencedAssetIds()
    {
        return [];
    }

    public void SetObjectData(DeserializationInfo info)
    {
        info.TryGetValue(nameof(Name), out string? name);
        info.TryGetValue(nameof(Description), out string? description);
        info.TryGetValue(nameof(Icon), out string? icon);
        info.TryGetValue(nameof(ActionUrn), out URN? actionUrn);
        
        Name = name ?? "Unnamed Shortcut";
        Description = description ?? "No description.";
        Icon = icon ?? "mdi-alert-circle";
        ActionUrn = actionUrn ?? URN.Empty;
    }
}