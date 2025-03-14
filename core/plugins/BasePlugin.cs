using RPGCreator.core.io.datas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPGCreator.core.plugins
{
    //TODO This class still need to be worked on. For now it's just a template to see what it could be like but nothing is set in time.

    /// <summary>
    /// The base class for all plugins.
    /// </summary>
    class BasePlugin
    {

        public BasePlugin(string unique)
        {
            _unique_name = unique;
            Name = ConfigFile.plugins.GetPluginName(unique);
            Description = ConfigFile.plugins.GetPluginDescription(unique);
            Authors = [.. ConfigFile.plugins.GetPluginAuthors(unique)];
            ParentPlugins = [.. ConfigFile.plugins.GetPluginDependencies(unique)];
            if(Version.TryParse(ConfigFile.plugins.GetPluginVersion(unique), out Version _result))
            {
                PluginVersion = _result;
            } else
            {
                PluginVersion = new(0, 0, 0);
            }

            foreach (string EditorVersion in ConfigFile.plugins.GetEditorVersion(unique))
            {
                if(Version.TryParse(EditorVersion, out _result))
                {
                    ForVersions.Add(_result);
                }
            }

            if (ForVersions.Count == 0)
            {
                _state = PLUGIN_STATE.ERROR;
            }

            _main_folder = ConfigFile.plugins.GetPluginRoot(unique);
            _translations = ConfigFile.plugins.GetSupportedLanguages(unique);

            if (_state != PLUGIN_STATE.ERROR)
            {
                if (ConfigFile.plugins.IsOutdated(unique))
                {
                    _state = PLUGIN_STATE.OUTDATED;
                }
                else if (ConfigFile.plugins.IsEnabled(unique))
                {
                    _state = PLUGIN_STATE.ENABLED;
                }
                else
                {
                    _state = PLUGIN_STATE.DISABLED;
                }
            }
        }

        public enum PLUGIN_BUILD_TYPE
        {
            RELEASE,
            BETA,
            ALPHA,
            DEV,
            MAX
        }

        public enum PLUGIN_STATE
        {
            ENABLED,
            DISABLED,
            OUTDATED,
            NEED_UPDATE,
            ERROR,
            MAX
        }

        public enum PLUGIN_TYPE
        {
            GAME_SYSTEM,
            UI_UX,
            GRAPHICS,
            AUDIO,
            TOOLS,
            AI_NPC,
            NETWORKING,
            API,
            OTHER,
            MAX
        }

        public enum PLUGIN_LANGUAGE
        {
            LUA,
            CSHARP,
            MAX
        }

        protected string _unique_name; // A unique name defined by the developpers, it can be used by other plugin to specify requirements.

        public string Name;
        public string Description = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nulla vitae odio at orci aliquam scelerisque. Proin commodo nibh diam, non laoreet dolor varius a. Aliquam erat volutpat. Cras arcu lacus, lacinia non turpis tincidunt, porta hendrerit ipsum. Nulla quis dui dapibus magna imperdiet venenatis. Aliquam a tincidunt dolor, eu tristique elit. Mauris ac nulla eu massa vestibulum porttitor non ac dui. Donec vel odio sed leo sodales posuere in eu purus. Mauris pharetra turpis id sollicitudin vestibulum. Ut orci nulla, dignissim in hendrerit in, posuere in neque. Maecenas tincidunt, leo sit amet tempus sagittis, arcu metus mattis dui, eget fermentum tellus odio lobortis leo. Vestibulum eu pharetra arcu. Nulla suscipit neque sodales magna viverra, pretium tempus lacus blandit. Vivamus ligula felis, eleifend quis aliquam dictum, rhoncus eget augue.";
        public string SourceURL;

        public List<string> Authors;
        public List<string> ParentPlugins;

        public Version PluginVersion = new(1, 0, 0);
        public List<Version> ForVersions = []; // Specify what version of the engine the plugin support.


        protected PLUGIN_BUILD_TYPE _build_type = PLUGIN_BUILD_TYPE.DEV;
        protected PLUGIN_STATE _state = PLUGIN_STATE.DISABLED;
        protected PLUGIN_LANGUAGE _language = PLUGIN_LANGUAGE.LUA;
        protected PLUGIN_TYPE _type = PLUGIN_TYPE.OTHER;

        protected string _main_folder;
        protected string _contents_folder;
        protected string _translations_folder;

        protected string[] _translations;

        public bool IsOutdated()
        {
            return _state == PLUGIN_STATE.OUTDATED;
        }
    }
}
