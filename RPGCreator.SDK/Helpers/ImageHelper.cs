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

using MetadataExtractor;
using MetadataExtractor.Formats.Jpeg;
using MetadataExtractor.Formats.Png;

namespace RPGCreator.SDK.Helpers;

public static class ImageHelper
{
    public static (int Width, int Height) GetImageDimensions(string filePath)
    {
        var directories = ImageMetadataReader.ReadMetadata(filePath);

        foreach (var directory in directories)
        {
            if (directory.TryGetInt32(JpegDirectory.TagImageWidth, out var width) &&
                directory.TryGetInt32(JpegDirectory.TagImageHeight, out var height))
            {
                return (width, height);
            }
        
            if (directory.Name == "PNG-IHDR") 
            {
                var w = directory.GetInt32(PngDirectory.TagImageWidth);
                var h = directory.GetInt32(PngDirectory.TagImageHeight);
                return (w, h);
            }
        }

        throw new Exception("Unable to determine image dimensions. The file may not be a valid JPEG or PNG image.");
    }
}