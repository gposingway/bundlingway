using Bundlingway.Utilities.ManagedResources;
using Newtonsoft.Json.Converters;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace Bundlingway.Model
{
    public class ResourcePackage
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public enum EStatus
        {
            Undefined,
            Prepared,
            Installed,
            [Description("Not Installed")]
            NotInstalled,
            [Description("Not Downloaded")]
            NotDownloaded,
            Unpacking,
            Installing,
            Error
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public enum EType
        {
            Undefined,
            [Description("Single Preset")]
            SinglePreset,
            [Description("Preset Collection")]
            PresetCollection,
            [Description("Shader Collection")]
            ShaderCollection,
            [Description("Mixed Collection")]
            MixedCollection,
            [Description("Core Package")]
            CorePackage,
        }

        public string Name { get; set; }
        public string Label { get; set; }
        public string Version { get; set; }
        public string Source { get; set; }
        public EType Type { get; set; }
        public EStatus Status { get; set; }
        public bool Default { get; set; }
        public string LocalPresetFolder { get; set; }
        public string LocalTextureFolder { get; set; }
        public string LocalShaderFolder { get; set; }
        public bool Bundle { get; set; }
        public bool Hidden { get; set; }
        public bool Favorite { get; set; }
        public bool Locked { get; set; }
        public string LocalFolder { get; set; }

        internal async Task Install()
        {
            // await Utilities.Handler.Package.Install(Name); // Handler removed, use IPackageService instead
        }
    }
}
