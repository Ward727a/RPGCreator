using RPGCreator.core.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;

namespace RPGCreator.core.config
{
    class ConfigProject
    {

        /// <summary>
        /// If the project is currently open in the editor or not.
        /// </summary>
        public bool IsLoaded = false;
        /// <summary>
        /// Define if the project is defined as favorite inside the launcher.
        /// </summary>
        public bool IsFavorite = false;
        /// <summary>
        /// Define if the project is defined as archived inside the launcher.<br/><br/>
        /// An archived project is compressed to take less space and will need to be un-compressed before editing it again.
        /// </summary>
        public bool IsArchived = false;
        /// <summary>
        /// Defined if the project is part of a source control like Github or Gitlab.
        /// </summary>
        public bool IsSourceControled = false;

        public struct ProjectData(string name)
        {
            /// <summary>
            /// Name of the project.<br/>User-made.
            /// </summary>
            public string Name = name;
            /// <summary>
            /// Authors of the project.<br/>User-made.
            /// </summary>
            public string[] authors = [];
            /// <summary>
            /// Short description of the project.<br/>User-made.
            /// </summary>
            public string ShortDescription = "";
            /// <summary>
            /// Description of the project, it will be used for the executable (and launcher?).<br/>User-made.
            /// </summary>
            public string Description = "";
            /// <summary>
            /// Icon of the project, it will be used inside the launcher, and inside the game/executable.<br/>User-made.
            /// </summary>
            public ResourcePath IconPath;
            /// <summary>
            /// Version of the project.<br/>User-made.
            /// </summary>
            public Version ProjectVersion = new(0, 0, 1, 0);
        }

        public struct ProjectSourceControl()
        {
            public enum SourceControlType
            {
                GITHUB,
                GITLAB,
                MAX
            }
            //TODO: Finish this. Need to check how this work.
        }

        public ProjectData Info = new("NoNamedDefined");
        public string BasePath = "";

        public Version EditorVersion = new(0, 0, 0, 0);

        public ProjectSourceControl SourceControl = new();
    }
}
