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
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using RPGCreator.Core.Services;
using RPGCreator.Core.Services.Configurations;
using System;
using System.Collections.ObjectModel;
using System.Linq;
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

namespace RPGCreator.Core.Internal
{
    public partial class Tag : ObservableObject
    {

        private XElement? XML;
        private bool HasXML => (XML != null && !IsDeleted);

        private bool _deleted = false;
        public bool IsDeleted => _deleted;

        [ObservableProperty]
        private string _name = "undefined";
        [ObservableProperty]
        private Color _color = Color.FromArgb(255, 0, 0, 0);

        public override string ToString()
        {
            return $"{this.Name};{this.Color}";
        }

        public Tag(string name, Color color)
        {
            this.Name = name;
            this.Color = color;

        }

        public void LinkXML(XElement xml)
        {
            if(!HasXML)
            {
                XML = xml;
                return;
            }
            throw new InvalidOperationException("The class is already linked to an XML. LinkXML called two time on the same variable?");
        }

        partial void OnColorChanged(Color value)
        {
            if (HasXML)
            {
                XML.Element("Color")?.SetValue(value.ToString());
            }
        }

        partial void OnNameChanged(string value)
        {
            if (HasXML)
            {
                XML.Element("Name")?.SetValue(value);
            }
        }
    }

    public partial class Project : ObservableObject
    {
        /// <summary>
        /// This event is emitted when the project is deleted from the engine by calling <see cref="DeleteProject"/>
        /// </summary>
        public event EventHandler OnProjectDeleted;

        /// <summary>
        /// This element keep the XML Element used by the project. It should never be needed by other class.
        /// </summary>
        private XElement? XML;
        private bool HasXML => XML != null && !IsDeleted;

        private bool _deleted = false;
        /// <summary>
        /// True if the project is deleted from the XML, false otherwise.
        /// </summary>
        public bool IsDeleted => _deleted;

        [ObservableProperty]
        private string _name = "undefined";
        [ObservableProperty]
        private string _description = "undefined";
        [ObservableProperty]
        private Version _editorVersion = new();
        [ObservableProperty]
        private Version _version = new();
        public ObservableCollection<string> Authors = [];

        public ObservableCollection<Tag> Tags = [];
        [ObservableProperty]
        private string _copyright = "undefined";

        [ObservableProperty]
        private string _path = "undefined";
        [ObservableProperty]
        private DateTime _lastEdited = DateTime.Now;

        [ObservableProperty]
        private bool _isFavorite = false;
        [ObservableProperty]
        private bool _isArchived = false;

        private AssetsConf? _AssetsConf;
        public AssetsConf? AssetsConf
        {
            get
            {
                //if (_AssetsConf == null)
                //    throw new Exception("Assets configuration is null!");
                return _AssetsConf;
            }
            set
            {
                _AssetsConf ??= value;
            }
        }
        
        public bool HasAssets => AssetsConf != null;

        public Project(string name)
        {
            Name = name;
        }

        public Project(XElement xml, string name)
        {
            XML = xml;
            register_event();
            Name = name;
        }

        public void LinkXML(XElement xml)
        {
            if (XML == null)
            {
                XML = xml;
                register_event();
                return;
            }
            throw new InvalidOperationException("The class is already linked to an XML. LinkXML called two time on the same variable?");
        }

        private void register_event()
        {
            Tags.CollectionChanged += (_, _) =>
            {
                if (HasXML)
                {
                    XElement? tagsElement = XML.Element("Tags");

                    if (tagsElement == null)
                    {
                        tagsElement = new XElement("Tags");
                        XML.Add(tagsElement);
                    }

                    foreach (Tag tag in Tags)
                    {
                        if (tagsElement.Elements().Where(e => e.Value == tag.Name).Any()) // Ignore tag that was already in.
                            continue;
                        tagsElement.Add(new XElement("Tag", tag.Name));
                    }

                    // Remove tag(s) present in XML but not in collection.
                    tagsElement.Elements("Tag").Where(e => Tags.Any(i => i.Name == e.Value))
                        .ToList()
                        .ForEach(e => e.Remove());
                }
            };

            Authors.CollectionChanged += (_, _) =>
            {
                if (HasXML)
                {
                    XElement? authorsElement = XML.Element("Authors");

                    if (authorsElement == null)
                    {
                        authorsElement = new XElement("Authors");
                        XML.Add(authorsElement);
                    }

                    foreach (string author in Authors)
                    {
                        if (authorsElement.Elements().Where(e => e.Value == author).Any()) // Ignore author that was already in.
                            continue;
                        authorsElement.Add(new XElement("Author", author));
                    }

                    // Remove author(s) present in XML but not in collection.
                    authorsElement.Elements("Tag").Where(e => Authors.Contains(e.Value))
                        .ToList()
                        .ForEach(e => e.Remove());
                }
            };
        }

        partial void OnNameChanged(string value)
        {
            if(HasXML)
            {
                SetXMLValue("Name", value);
            }
        }

        partial void OnLastEditedChanged(DateTime value)
        {
            if (HasXML)
            {
                SetXMLValue("LastEdited", value.ToString());
            }
        }

        partial void OnIsFavoriteChanged(bool value)
        {
            if (HasXML)
            {
                SetXMLValue("IsFavorite", value.ToString());
            }
        }

        partial void OnIsArchivedChanged(bool value)
        {
            if (HasXML)
            {
                SetXMLValue("IsArchived", value.ToString());
            }
        }

        partial void OnPathChanged(string value)
        {
            if (HasXML)
            {
                SetXMLValue("Path", value);
            }
        }

        partial void OnVersionChanged(Version value)
        {
            if(HasXML)
            {
                SetXMLValue("Version", value.ToString());
            }
        }

        partial void OnEditorVersionChanged(Version value)
        {
            if (HasXML)
            {
                SetXMLValue("EditorVersion", value.ToString());
            }
        }

        partial void OnDescriptionChanged(string value)
        {
            if (HasXML)
            {
                SetXMLValue("Description", value);
            }
        }

        partial void OnCopyrightChanged(string value)
        {
            if(HasXML)
            {
                SetXMLValue("Copyright", value);
            }
        }

        private void SetXMLValue(string element_name, string value)
        {
            if (XML != null)
            {
                if (XML.Element(element_name) != null)
                {
                    XML.Element(element_name)!.SetValue(value);
                }
            }
        }

        public XElement ToXElement()
        {
            XElement projectX = new XElement("Project",
                new XElement("Name", Name),
                new XElement("Description", Description),
                new XElement("Path", Path),
                new XElement("Copyright", Copyright),
                new XElement("EditorVersion", EditorVersion.ToString()),
                new XElement("Version", Version.ToString()),
                new XElement("LastEdited", LastEdited.ToString()),
                new XElement("IsFavorite", IsFavorite.ToString()),
                new XElement("IsArchived", IsArchived.ToString()),
                new XElement("Authors"),
                new XElement("Tags")
            );

            foreach(string author in Authors)
            {
                projectX.Element("Authors")?.Add("Author", author);
            }

            foreach(Tag tag in Tags)
            {
                projectX.Element("Tags")?.Add("Tag", tag.Name);
            }

            return projectX;
        }

        /// <summary>
        /// Delete this project. This is a one way process, once called, it can't be cancelled!<br/>
        /// This emit the event <see cref="OnProjectDeleted"/> once it's done (and it's a success).<br/>
        /// This also set the variable <see cref="IsDeleted"/> on true.
        /// </summary>
        /// <returns></returns>
        public bool DeleteProject()
        {
            if(XML != null)
            {
                XML.Remove();
                XML = null;

                _deleted = true;

                OnProjectDeleted?.Invoke(this, EventArgs.Empty);

                return true;
            }
            return false;
        }
    }
}
