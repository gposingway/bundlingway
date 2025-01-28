using Bundlingway.Model;

namespace Bundlingway.PostProcess.RawFile
{
    public class OptimizeTextures
    {
        public class ReplaceWellKnownPresetTextures : IRawFileProcess
        {
            private Dictionary<string, string> WellKnownFileReplacements2 = new Dictionary<string, string>
            {
                ["Wifi_Renaissance5.png"] = "Wifi_Renaissance5.jpeg",
                ["Wifi_Simplicity0.png"] = "Wifi_Simplicity0.jpeg",
            };

            private Dictionary<string, string> WellKnownFileReplacements = new Dictionary<string, string>
            {
                ["Wifi_Cinematic5.png"] = "Wifi_Cinematic5.jpeg",
                ["Wifi_Cinematic7.png"] = "Wifi_Cinematic7.jpeg",
                ["Wifi_Mystique1.png"] = "Wifi_Mystique1.jpeg",
                ["Wifi_Simplicity0.png"] = "Wifi_Simplicity0.jpeg",
                ["Wifi_Doodle4.png"] = "Wifi_Doodle4.jpeg",
                ["Wifi_Doodle3.png"] = "Wifi_Doodle3.jpeg",
                ["Wifi_Simplicity7.png"] = "Wifi_Simplicity7.jpeg",
                ["Wifi_Simplicity8.png"] = "Wifi_Simplicity8.jpeg",
                ["Wifi_Simplicity5.png"] = "Wifi_Simplicity5.jpeg",
                ["Wifi_Simplicity6.png"] = "Wifi_Simplicity6.jpeg",
                ["Wifi_Renaissance5.jpg"] = "Wifi_Renaissance5.jpeg"
            };

            public int Order { get; set; } = 0;
            public bool ApplyToPresets { get; set; } = true;
            public bool ApplyToShaders { get; set; } = false;

            public bool PostProcess(Preset preset)
            {
                throw new NotImplementedException();
            }

            public Dictionary<string, string> ReplacementMap(List<string> presetFileList, string baselinePath)
            {
                return WellKnownFileReplacements;
            }

        }
    }
}