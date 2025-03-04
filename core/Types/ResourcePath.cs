using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPGCreator.core.Types
{
    class ResourcePath
    {
        private string _Path = "";
        /// <summary>
        /// Path to the resource.
        /// </summary>
        public string Path => _Path;
        private bool _IsValid = false;
        /// <summary>
        /// True if the resource exist, False otherwise
        /// </summary>
        public bool IsValid => _IsValid;

        public ResourcePath(string path)
        {
            _Path = path;
            _IsValid = File.Exists(path);
        }

        public ResourcePath(ResourcePath resourcePath)
        {
            _Path = resourcePath._Path;
            Recheck();
        }

        /// <summary>
        /// Check again if the file where Path point to exist or not.
        /// </summary>
        /// <returns>True if valid, False otherwise</returns>
        public bool Recheck()
        {
            _IsValid = File.Exists(_Path);
            return IsValid;
        }
        
    }
}
