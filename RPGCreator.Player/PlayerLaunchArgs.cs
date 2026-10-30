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

using System;

namespace RPGCreator.Player;

public sealed class PlayerLaunchArgs
{
    public string ProjectConfigPath { get; private set; } = string.Empty;
    public Ulid MapId { get; private set; } = Ulid.Empty;
    public string SaveFilePath { get; private set; } = string.Empty;
    public bool IsDebug { get; private set; }

    public static PlayerLaunchArgs Parse(string[] args)
    {
        var result = new PlayerLaunchArgs();

        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "--project" when i + 1 < args.Length:
                    result.ProjectConfigPath = args[++i];
                    break;

                case "--map" when i + 1 < args.Length:
                    if (Ulid.TryParse(args[++i], out var mapId))
                        result.MapId = mapId;
                    break;

                case "--load" when i + 1 < args.Length:
                    result.SaveFilePath = args[++i];
                    break;

                case "--debug":
                    result.IsDebug = true;
                    break;
            }
        }

        return result;
    }
}