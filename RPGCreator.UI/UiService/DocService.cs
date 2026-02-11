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

using System.Collections.Generic;
using System.IO;
using RPGCreator.SDK.EditorUiService;
using RPGCreator.SDK.Logging;
using RPGCreator.SDK.Types;

namespace RPGCreator.UI.UiService;

public class DocService : IDocService
{
    
    /// <summary>
    /// URN => Documentation topic.
    /// String => Documentation path or content.
    /// </summary>
    private readonly Dictionary<URN, string> _documentation = new Dictionary<URN, string>();
    
    /// <summary>
    /// URN => IsPath. True if the documentation is a path, false if it's content.
    /// </summary>
    private readonly Dictionary<URN, bool> _isPath = new Dictionary<URN, bool>();
    
    public string GetDocumentation(URN topicUrn)
    {
        if(!_documentation.TryGetValue(topicUrn, out var documentation))
            return string.Empty;

        if (!_isPath[topicUrn]) return documentation;
        
        return File.Exists(documentation) ? File.ReadAllText(documentation) : string.Empty;
    }

    public bool AddDocumentation(URN topicUrn, string content)
    {
        if(!_documentation.TryAdd(topicUrn, content))
            return false;

        _isPath.Add(topicUrn, false);
        Logger.Debug("Added documentation for topic {0}", topicUrn);
        return true;
    }

    public bool AddDocumentationFromPath(URN topicUrn, string path)
    {
        if(_documentation.ContainsKey(topicUrn))
            return false;
        
        if(!File.Exists(path))
            return false;
        
        _documentation.Add(topicUrn, path);
        _isPath.Add(topicUrn, true);
        Logger.Debug("Added documentation for topic {0} with path {1}", topicUrn, path);
        return true;
    }
}