#region LICENSE
//
// RPG Creator - Open-source RPG Engine.
// (c) 2025 Ward
// 
// This file is part of RPG Creator and is distributed under the MIT License.
// You are free to use, modify, and distribute this file under the terms of the MIT License.
// See LICENSE for details.
// 
// ---
// 
// Ce fichier fait partie de RPG Creator et est distribué sous licence MIT.
// Vous êtes libre de l'utiliser, de le modifier et de le distribuer sous les termes de la licence MIT.
// Voir LICENSE pour plus de détails.
// 
// Contact:
// => Mail: Ward727a@gmail.com
//    Please use this object: "RPG Creator [YourObject]"
// => Discord: ward727
// 
// For urgent inquiries, sending both an email and a message on Discord is highly recommended for a quicker response.
// 
// 
#endregion
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace RPGCreator.Core.Storage
{
    public abstract class XmlFileManagerBase<T>
    {
        protected string FilePath { get; set; }

        public XmlFileManagerBase(string filePath)
        {
            FilePath = filePath;
        }

        public T Load()
        {
            if (!File.Exists(FilePath))
            {
                throw new FileNotFoundException("XML file couldn't be found.", FilePath);
            }

            XmlSerializer serializer = new(typeof(T));

            using StreamReader sr = new(FilePath);

            object? value = serializer.Deserialize(sr);

            if (value != null)
            {
                return (T)value;
            }

            throw new InvalidOperationException($"Data inside the XML file returned NULL: '{FilePath}'.");
        }

        public void Save(T obj)
        {
            XmlSerializer serializer = new(typeof(T));

            using StreamWriter sw = new(FilePath);

            serializer.Serialize(sw, obj);
        }
    }
}
