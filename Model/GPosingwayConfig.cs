using Bundlingway.Utilities.ManagedResources;

namespace Bundlingway.Model
{
    public class GPosingwayConfig
    {

        public class CorePackageStatus
        {
            public EPackageStatus Status { get; set; }
            public string LocalVersion { get; set; }
            public string RemoteVersion { get; set; }
            public string RemoteLink { get; set; }
        }

        public class GameData
        {
            public string InstallationFolder { get; set; }
            public string ClientLocation { get; set; }
        }

        public CorePackageStatus ReShade { get; set; } = new CorePackageStatus();
        public CorePackageStatus GPosingway { get; set; } = new CorePackageStatus();
        public GameData Game { get; set; } = new GameData();

    }
}
