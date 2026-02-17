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

using RPGCreator.SDK.Serializer;

namespace RPGCreator.SDK.EngineService;

public interface IEngineConfig : IService, ISerializable, IDeserializable
{
    public string GetString(string key, string defaultValue = "");
    public int GetInt(string key, int defaultValue = 0);
    public bool GetBool(string key, bool defaultValue = false);
    public float GetFloat(string key, float defaultValue = 0f);
    public double GetDouble(string key, double defaultValue = 0.0);
    public T Get<T>(string key, T defaultValue);
    
    public void SetString(string key, string value);
    public void SetInt(string key, int value);
    public void SetBool(string key, bool value);
    public void SetFloat(string key, float value);
    public void SetDouble(string key, double value);
    public void Set<T>(string key, T value);
}