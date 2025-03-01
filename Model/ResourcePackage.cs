﻿using Bundlingway.Utilities.ManagedResources;

namespace Bundlingway.Model
{
    public class ResourcePackage
    {
        public string Name { get; set; }
        public string Source { get; set; }
        public string Type { get; set; }
        public string Status { get; set; }
        public bool Default { get; set; }
        public bool Installed { get; set; }
        public string LocalPresetFolder { get; set; }
        public string LocalTextureFolder { get; set; }
        public string LocalShaderFolder { get; set; }
        public bool Hidden { get; set; }
        public bool Favorite { get; set; }
        public bool Locked { get; set; }
    }
}
