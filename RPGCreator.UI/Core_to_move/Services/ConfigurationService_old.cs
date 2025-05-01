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
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

/*
 * RPG Creator - Open-source RPG Engine.
 * (c) 2025 Ward
 * 
 * This file is part of RPG Creator and is distributed under the MIT License.
 * You are free to use, modify, and distribute this file under the terms of the MIT License.
 * See LICENSE for details.
 * 
 * ---
 * 
 * Ce fichier fait partie de RPG Creator et est distribué sous licence MIT.
 * Vous êtes libre de l'utiliser, de le modifier et de le distribuer sous les termes de la licence MIT.
 * Voir LICENSE pour plus de détails.
 */

namespace RPGCreator.Core.Services
{
    public class ConfigurationElementBase
    {
        public ConfigurationDocument Doc;
        public bool IsArray = false;
        public bool IsArrayItem = false;
        public string Fullname;
    }
    public class ConfigurationElement : ConfigurationElementBase
    {
        public ConfigurationElementBase? Parent;

        public XName Name;

        public string? Value;

        public List<ConfigurationElementBase> Elements = [];
        public bool HasElements => Elements.Count > 0;

        ~ConfigurationElement()
        {
            Console.WriteLine("Collected.");
        }

        protected ConfigurationElement() { }

        public ConfigurationElement(XElement xElement, ref Dictionary<string, ConfigurationElementBase> ElementsMap, ConfigurationDocument doc)
        {
            Doc = doc;
            Name = xElement.Name;
            Fullname = xElement.Name.ToString();
            ElementsMap.TryAdd(Fullname, this);
            if (!xElement.HasElements)
                Value = xElement.Value;
            ProcessChildrenElements(xElement, ref ElementsMap);
        }

        public ConfigurationElement(ConfigurationElementBase parent, XElement xElement, ref Dictionary<string, ConfigurationElementBase> ElementsMap)
        {
            Parent = parent;
            Doc = parent.Doc;
            Name = xElement.Name;
            var x = parent.Fullname;
            Fullname = $"{parent.Fullname}:{Name}";
            if (!xElement.HasElements)
                Value = xElement.Value;
            ProcessChildrenElements(xElement, ref ElementsMap);
        }

        protected virtual void ProcessChildrenElements(XElement xElement, ref Dictionary<string, ConfigurationElementBase> ElementsMap)
        {
            if (!xElement.HasElements)
                return;

            foreach(XElement element in xElement.Elements())
            {
                if(element.HasElements)
                {
                    var total_array_count = element.Elements().GroupBy(e => e.Name).Where(g => g.Count() > 1).Select(g => g.Key.LocalName).Count();
                    if (total_array_count == 1)
                    {
                        string item_name = element.Elements().GroupBy(e => e.Name).Where(g => g.Count() > 1).Select(g => g.Key.LocalName).ToList()[0];
                        ConfigurationArrayElement childArrElement = new(this, element, ref ElementsMap, item_name);
                        ElementsMap.TryAdd($"{childArrElement.Fullname}", childArrElement);
                        Elements.Add(childArrElement);
                        continue;
                    }
                    if (total_array_count > 1)
                        throw new InvalidOperationException("Can't add different named container inside one array, each array need to contains one and only one name.");
                }
                ConfigurationElement childElement = new(this, element, ref ElementsMap);
                ElementsMap.TryAdd($"{childElement.Fullname}", childElement);
                Elements.Add(childElement);
            }
        }

        public virtual bool TryAddChildren(XElement xElement, out ConfigurationElement? addedElement, ref Dictionary<string, ConfigurationElementBase> ElementsMap)
        {
            addedElement = null;
            if(!HasElements)
            {
                if (Value != null)
                {
                    Value = null;
                }
            }
            addedElement = new(this, xElement, ref ElementsMap);
            ElementsMap.TryAdd($"{addedElement.Fullname}", addedElement);
            Elements.Add(addedElement);
            return true;
        }

        public virtual bool TryAddArray(XElement xElement, out ConfigurationArrayElement? addedElement, ref Dictionary<string, ConfigurationElementBase> ElementsMap, string item_name)
        {
            addedElement = null;
            if (!HasElements)
            {
                if (Value != null)
                {
                    Value = null;
                }
            }
            addedElement = new(this, xElement, ref ElementsMap, item_name);
            ElementsMap.TryAdd($"{addedElement.Fullname}", addedElement);
            Elements.Add(addedElement);
            return true;
        }

        public virtual bool SetChildren(string? at, XElement xElement, out ConfigurationElement? settedElement, ref Dictionary<string, ConfigurationElementBase> ElementsMap)
        {
            settedElement = null;

            if (!HasElements)
                return false;

            string fullname_new = at ?? $"{Fullname}:{xElement.Name}";
            if(ElementsMap.TryGetValue(fullname_new, out ConfigurationElementBase? element))
            {
                if (element == null)
                    return false;

                // Remove the element first from the Elements list.
                Elements.Remove(element);

                // Then we remove his keys, and all other elements with it.
                var keysToDelete = ElementsMap.Keys.Where(k => k == fullname_new || k.StartsWith($"{fullname_new}:")).ToList();

                foreach (var key in keysToDelete)
                {
                    ElementsMap.Remove(key);
                }

                settedElement = new(this, xElement, ref ElementsMap);
                ElementsMap.TryAdd(fullname_new, settedElement);
                Elements.Add(settedElement);
                return true;
            }
            return false;
        }

        public virtual void ClearChildren(ref Dictionary<string, ConfigurationElementBase> ElementsMap)
        {
            foreach(ConfigurationElement child in Elements)
            {
                ElementsMap.Remove(child.Fullname);
                child.ClearChildren(ref ElementsMap);
            }
            Elements.Clear();
        }

        public virtual XElement ToXElement()
        {
            XElement element = new(Name);

            if(HasElements)
            {
                foreach (ConfigurationElementBase b_children in Elements)
                {
                    if(b_children is ConfigurationElement e_children)
                        element.Add(e_children.ToXElement());
                    else if(b_children is ConfigurationArrayElement a_children)
                    {
                        element.Add(a_children.ToXElement());
                    }
                }

                return element;
            }
            element.Value = Value ?? "";
            return element;
        }
    }

    public class ConfigurationArrayElement : ConfigurationElement
    {
        public Dictionary<int, ConfigurationArrayItem> ArrayMap = new Dictionary<int, ConfigurationArrayItem>();
        public string? ItemName;
        public int Count = 0;

        public ConfigurationArrayElement(ConfigurationElementBase parent, XElement xElement, ref Dictionary<string, ConfigurationElementBase> ElementsMap, string itemName)
        {
            Doc = parent.Doc;
            IsArray = true;
            ItemName = itemName;
            Parent = parent;
            Name = xElement.Name;
            Fullname = $"{parent.Fullname}:{Name}";
            if (!xElement.HasElements)
                Value = xElement.Value;
            ProcessChildrenElements(xElement, ref ElementsMap);
        }

        /// <summary>
        /// Initialize the array elements.
        /// </summary>
        /// <param name="itemName">Item name is the name of each new item that will be set.</param>
        /// <param name="mainKey">Main key is the key that will be used to retrieve each element.</param>
        /// <returns></returns>
        public ConfigurationArrayElement InitArray(string itemName, string mainKey)
        {
            ItemName = itemName;
            return this;
        }

        protected override void ProcessChildrenElements(XElement xElement, ref Dictionary<string, ConfigurationElementBase> ElementsMap)
        {
            this.Count = 0;
            foreach (XElement child_element in xElement.Elements())
            {
                if (child_element.Name.ToString() != ItemName)
                    continue;

                ConfigurationArrayItem item = new(this, child_element, ref ElementsMap, ItemName);

                ArrayMap.Add(this.Count, item);
                ElementsMap.Add(item.Fullname, item);
                this.Count+= 1;
            }
        }

        public override bool TryAddChildren(XElement xElement, out ConfigurationElement? addedElement, ref Dictionary<string, ConfigurationElementBase> ElementsMap)
        {
            if (ItemName == null)
                throw new InvalidOperationException("An element array need to be initialized by the InitArray function.");

            addedElement = null;

            if(xElement.Name != ItemName)
            {
                throw new InvalidOperationException($"This element array await an XElement with the name {ItemName}.");
            }

            if (xElement.Name.ToString() != ItemName)
            {
                return false;
            }

            ConfigurationArrayItem item = new(this, xElement, ref ElementsMap, ItemName);

            ArrayMap.Add(Count, item);
            ElementsMap.Add(item.Fullname, item);
            Count++;
            return true;
        }

        public override bool TryAddArray(XElement xElement, out ConfigurationArrayElement? addedElement, ref Dictionary<string, ConfigurationElementBase> ElementsMap, string item_name)
        {
            throw new NotImplementedException("You can add an array to another array, you need to add an item, then add an array to this item.");
        }

        public override XElement ToXElement()
        {
            XElement element = new(Name);

            foreach(ConfigurationArrayItem item in ArrayMap.Values)
            {
                element.Add(item.ToXElement());
            }
            return element;
        }
    }

    public class ConfigurationArrayItem : ConfigurationElementBase
    {
        public XName Name;
        public string? Key;
        public string? Value;
        public int Index = -1;

        public List<ConfigurationElementBase> Elements = [];
        public Dictionary<string, ConfigurationElementBase> Map = [];

        public bool HasElements => Elements.Count > 0;

        public ConfigurationArrayItem(ConfigurationArrayElement parent, XElement xElement, ref Dictionary<string, ConfigurationElementBase> ElementsMap, string itemName)
        {

            if (parent.ItemName == null)
            {
                throw new InvalidOperationException("The parent element array hasn't been initialized!");
            }

            Doc = parent.Doc;
            Name = xElement.Name;
            Index = parent.Count;

            Fullname = $"{parent.Fullname}:{Index}";
            if (xElement.HasElements)
            {
                LoadValues(xElement, ref ElementsMap);
                return;
            }
            Value = xElement.Value;
        }

        protected void LoadValues(XElement xElement, ref Dictionary<string, ConfigurationElementBase> ElementsMap)
        {
            foreach (XElement element in xElement.Elements())
            {
                if (element.HasElements)
                {
                    var total_array_count = element.Elements().GroupBy(e => e.Name).Where(g => g.Count() > 1).Select(g => g.Key.LocalName).Count();
                    if (total_array_count == 1)
                    {
                        string item_name = element.Elements().GroupBy(e => e.Name).Where(g => g.Count() > 1).Select(g => g.Key.LocalName).ToList()[0];
                        ConfigurationArrayElement childArrElement = new(this, element, ref ElementsMap, item_name);
                        ElementsMap.TryAdd($"{childArrElement.Fullname}", childArrElement);
                        Elements.Add(childArrElement);
                        continue;
                    }
                    if (total_array_count > 1)
                        throw new InvalidOperationException("Can't add different named container inside one array, each array need to contains one and only one name.");
                }
                ConfigurationElement childElement = new(this, element, ref ElementsMap);
                ElementsMap.TryAdd($"{childElement.Fullname}", childElement);
                Elements.Add(childElement);
            }
        }

        public bool TryAddArray(XElement xElement, out ConfigurationArrayElement? addedElement, ref Dictionary<string, ConfigurationElementBase> ElementsMap, string item_name)
        {
            addedElement = null;

            if(!HasElements)
            {
                if(Value != null)
                {
                    Value = null;
                }
            }

            addedElement = new(this, xElement, ref ElementsMap, item_name);
            ElementsMap.TryAdd($"{addedElement.Fullname}", addedElement);
            Elements.Add(addedElement);
            return true;
        }

        public XElement ToXElement()
        {
            XElement element = new(Name);
            if (HasElements)
            {
                foreach (ConfigurationElementBase b_children in Elements)
                {
                    if (b_children is ConfigurationElement e_child)
                        element.Add(e_child.ToXElement());
                    else if (b_children is ConfigurationArrayElement a_child)
                        element.Add(a_child.ToXElement());
                }

                return element;
            }
            element.Value = Value ?? "";
            return element;
        }

        public object? this[string Key]
        {
            get
            {
                return Doc[$"{Fullname}:{Key}"];
            }
            set
            {
                Doc[$"{Fullname}:{Key}"] = value;
            }
        }

    }

    public class ConfigurationDocument
    {

        event EventHandler? OnSave;

        public Dictionary<string, ConfigurationElementBase> ElementsMap = new Dictionary<string, ConfigurationElementBase>();
        ConfigurationElement Root;
        string? Filepath;

        protected ConfigurationDocument() { }

        public ConfigurationDocument(params object?[] content)
        {
            if(content.Length >= 1)
            {
                if (content[0] is ConfigurationElement element)
                    Root = element;
            }
            throw new InvalidOperationException("ConfigurationDocument need at least 1 XElement (root) to be able to be initialized.");
        }

        static public ConfigurationDocument Load(string filepath)
        {
            ConfigurationDocument doc = new ConfigurationDocument();

            doc.Filepath = filepath;
            doc.ParseXML();

            return doc;
        }

        protected void ParseXML()
        {
            if (string.IsNullOrEmpty(Filepath))
                return;

            if (File.Exists(Filepath))
            {
                XDocument xDoc = XDocument.Load(Filepath);

                if (xDoc.Root != null)
                {
                    Root = new(xDoc.Root, ref ElementsMap, this);
                }
            }
            else
            {
                XDocument xDoc = new XDocument(new XElement("Config"));
                xDoc.Save(Filepath);
                Root = new(xDoc.Root, ref ElementsMap, this);
            }
        }

        public bool AddElement(string path, XElement element, out ConfigurationElement? addedElement)
        {
            addedElement = null;
            if (ElementsMap.ContainsKey($"{path}:{element.Name}"))
                return false;

            //if(ElementsMap.TryGetValue(path, out ConfigurationElementBase? value))
            //{
            //    return value.TryAddChildren(element, out addedElement, ref ElementsMap);
            //}
            return false;
        }

        public bool AddArray(string path, string array_name, string item_name, XElement[] elements, out ConfigurationArrayElement? addedElement)
        {
            addedElement = null;
            if(ElementsMap.ContainsKey($"{path}:{array_name}"))
            {
                if(ElementsMap.TryGetValue($"{path}:{array_name}", out ConfigurationElementBase? base_value))
                {

                    if (base_value is ConfigurationArrayElement array)
                    {
                        addedElement = array;

                        foreach (XElement element in elements)
                        {
                            addedElement.TryAddChildren(element, out _, ref ElementsMap);
                        }
                        return true;
                    }
                    return false;
                }
                return false;
            }

            if(ElementsMap.TryGetValue(path, out ConfigurationElementBase? value))
            {
                if (value is ConfigurationElement element)
                {
                    if(element.TryAddArray(new XElement(array_name), out addedElement, ref ElementsMap, item_name))
                    {
                        if (addedElement == null)
                            return false;

                        foreach(XElement array_item in elements)
                        {
                            addedElement.TryAddChildren(array_item, out _, ref ElementsMap);
                        }
                        return true;
                    }

                }
                else if(value is ConfigurationArrayItem item)
                {
                    if(item.TryAddArray(new XElement(array_name), out addedElement, ref ElementsMap, item_name))
                    {
                        if (addedElement == null)
                            return false;

                        foreach (XElement array_item in elements)
                        {
                            addedElement.TryAddChildren(array_item, out _, ref ElementsMap);
                        }
                        return true;
                    }
                }
            }

            return false;
        }

        public object? this[string key]
        {
            get
            {
                if(ElementsMap.TryGetValue(key, out ConfigurationElementBase? value))
                {
                    return value;
                }
                return null;
            }
            set
            {
                if (ElementsMap.TryGetValue(key, out ConfigurationElementBase? element))
                {
                    if (element is ConfigurationElement c_element)
                    {
                        if (value is string value_str)
                        {
                            c_element.Value = value_str;
                            if (c_element.HasElements)
                            {
                                c_element.ClearChildren(ref ElementsMap);
                            }
                        }
                        if (value is XElement element_x)
                        {
                            if (element_x.Name != key.Split(':').Last())
                            {
                                element_x.Name = key.Split(':').Last();
                            }
                            if (c_element.Parent is ConfigurationElement parent)
                            {
                                parent.SetChildren(key, element_x, out _, ref ElementsMap);
                            }
                        }
                    }
                }
                else
                {
                    ConfigurationElement current = Root;
                    string fullkey = "";
                    foreach (string part in key.Split(":"))
                    {
                        if (part == key.Split(":").Last())
                        {
                            if (value is string value_str)
                            {
                                current?.TryAddChildren(new XElement(part, value_str), out _, ref ElementsMap);
                                return;
                            }

                            if (value is XElement element_x)
                            {
                                if (element_x.Name != part)
                                {
                                    element_x.Name = part;
                                }
                                current?.TryAddChildren(element_x, out _, ref ElementsMap);
                                return;
                            }
                        }

                        fullkey += part;
                        ConfigurationElementBase? New;
                        if (ElementsMap.TryGetValue(fullkey, out New))
                        {
                            if (New is ConfigurationElement _new)
                            {
                                current = _new;
                                fullkey += ":";
                                continue;
                            }
                        }

                        if (current.TryAddChildren(new XElement(part), out ConfigurationElement _New, ref ElementsMap))
                        {
                            current = _New;
                            fullkey += ":";
                            continue;
                        }
                    }

                }
            }
        }

        public bool Save()
        {
            if (Filepath == null)
                return false;

            Save(Filepath);
            return true;
        }

        public void Save(string filepath)
        {
            XDocument document = new(Root.ToXElement());

            document.Save(filepath);
            OnSave?.Invoke(this, EventArgs.Empty);
        }
    }
    public class ConfigurationService_old
    {
        public readonly ConfigurationDocument AppSettings;
        public readonly ConfigurationDocument Projects;
        protected static string CheckOrCreateFile(string path, string root = "Config")
        {
            string fullpath = Path.Combine(Directory.GetCurrentDirectory(), path);

            if (!File.Exists(fullpath))
            {
                XDocument doc = new XDocument(
                    new XElement(root)

                    );
                doc.Save(fullpath);
            }
            return fullpath;
        }

        public ConfigurationService_old()
        {
            AppSettings = ConfigurationDocument.Load(CheckOrCreateFile("appsettings.xml"));
            Projects = ConfigurationDocument.Load(CheckOrCreateFile("projects.xml", "Projects"));

            AppSettings.AddArray("Config", "test2", "test", [new XElement("test", "Hello from code!")], out _);

            if(AppSettings["Config:test2:0"] is ConfigurationArrayItem item)
            {
                item["testa"] = "Hello!";
            }

            AppSettings.Save();
        }
    }
}
