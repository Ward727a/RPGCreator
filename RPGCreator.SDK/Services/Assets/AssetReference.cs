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

using System.Text.Json.Serialization;
using RPGCreator.Shared.Attributes;

namespace RPGCreator.SDK.Services.Assets;

[TypeDiscriminator("rpgc", "sdk", "asset_reference")]
public class AssetReference<TAsset>(Ulid assetId)
    where TAsset : class, IAssetDto
{
    [JsonInclude]
    public Ulid AssetId { get; init; } = assetId;

    [JsonIgnore]
    private TAsset? _asset = null;

    public TAsset Resolve(IAssetManager manager)
    {
        if (_asset == null)
        {
            var result = manager.GetAsset<TAsset>(AssetId);
            
            if (result.IsFailure)
                throw new Exception($"Failed to resolve asset with id {AssetId}");
            
            _asset = result.Value;
        }
        
        return _asset;
    }
}