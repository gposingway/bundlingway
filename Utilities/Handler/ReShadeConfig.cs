using Bundlingway.Model;
using IniParser;
using IniParser.Model;
using System.Windows.Forms;

namespace Bundlingway.Utilities.Handler
{
    public static class ReShadeConfig
    {
        private static IniData _iniHandler ;
        private static Dictionary<string, string> _shortcuts = null;
        private static string fileLocation;

        public static  Dictionary<string, string> Shortcuts
        {
            get
            {
                if (_shortcuts != null) return _shortcuts;

                return Load();
            }
        }

        public static Dictionary<string, string> Load()
        {
            if (Instances.LocalConfigProvider.Configuration?.Game?.InstallationFolder == null) return null;

            fileLocation = Path.Combine(Instances.LocalConfigProvider.Configuration.Game.InstallationFolder, Constants.Files.LocalReshadeConfig);
            if (!File.Exists(fileLocation)) return null;

            _iniHandler = new FileIniDataParser().ReadFile(fileLocation);

            _shortcuts = [];

            foreach (var item in Constants.DefaultShortcuts.Where(i => i.Key.IndexOf("@") == -1))
            {
                if (_iniHandler.Global.ContainsKey(item.Key))
                {
                    _shortcuts.Add(item.Key, _iniHandler.Global[item.Key]);
                }
                else
                {
                    _shortcuts.Add(item.Key, item.Value);
                }
            }

            return _shortcuts;
        }

        internal static void SaveShortcuts(Dictionary<string, string> temporaryShortcuts)
        {
            if (fileLocation == null) return;

            foreach (var item in temporaryShortcuts.Where(i => i.Key.IndexOf("@") == -1))
            {
                _iniHandler.Global[item.Key] = item.Value;
            }

            var parser = new FileIniDataParser();

            parser.WriteFile(fileLocation, _iniHandler);

            _shortcuts = null;
            Load();
        }
    }
}
