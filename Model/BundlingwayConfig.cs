﻿namespace Bundlingway.Model
{
    public class BundlingwayConfig
    {

        public class CorePackageStatus
        {
            public EPackageStatus Status { get; set; }
            public string? LocalVersion { get; set; }
            public string? RemoteVersion { get; set; }
            public string? RemoteLink { get; set; }
            public string? Location { get; set; }
        }

        public class GameData
        {
            public string? InstallationFolder { get; set; }
            public string? ClientLocation { get; set; }
        }

        public CorePackageStatus ReShade { get; set; } = new CorePackageStatus();
        public CorePackageStatus GPosingway { get; set; } = new CorePackageStatus();
        public CorePackageStatus Bundlingway { get; set; } = new CorePackageStatus();
        public UIData UI { get; set; } = new UIData();

        public GameData Game { get; set; } = new GameData();

        public Dictionary<string, string> Shortcuts { get; set; } = new Dictionary<string, string>();

        public class UIData
        {
            public bool TopMost { get; set; }
        }
    }
}
