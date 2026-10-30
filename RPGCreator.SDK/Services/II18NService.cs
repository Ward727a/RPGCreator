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
using RPGCreator.Shared.Types;

namespace RPGCreator.SDK.Services;

public interface II18NService
{
    public struct LanguageFileDto
    {
        [JsonInclude]
        [JsonPropertyName("Version")]
        public Version LanguageFileVersion;
        [JsonInclude]
        public string LanguageNameOriginal;
        [JsonInclude]
        public string LanguageNameInEnglish;
        [JsonInclude]
        [JsonPropertyName("Data")]
        public Dictionary<StringName, string> LanguageData;
    }
    
    /// <summary>
    /// Loads a language file from the specified file path into the system.
    /// </summary>
    /// <param name="filepath">The path of the language file to load.</param>
    /// <returns>A <see cref="Result"/> indicating the success or failure of the operation.</returns>
    public Result LoadLanguage(string filepath);

    /// <summary>
    /// Loads an additional language file from the specified file path into the system.
    /// </summary>
    /// <param name="filepath">The path of the additional language file to load.</param>
    /// <returns>A <see cref="Result"/> indicating the success or failure of the operation.</returns>
    /// <remarks>
    /// The additional language file will be merged with the existing language data, overwriting any existing keys with the same name.
    /// </remarks>
    public Result LoadAdditionalLanguageFile(string filepath);

    public string Get(StringName key);
    public string Get(StringName key, params object[] args);
}