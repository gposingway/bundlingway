namespace Bundlingway.Model
{
    public class GPosingwayConfig
    {

        public class CorePackageStatus
        {
            public string Status { get; set; }
            public string LocalVersion { get; set; }
            public string RemoteVersion { get; set; }
            public string RemoteLink { get; set; }
        }

        public string XIVPath { get; set; }
        public CorePackageStatus ReShade { get; set; } = new CorePackageStatus();
        public CorePackageStatus GPosingway { get; set; } = new CorePackageStatus();
        public string? GameFolder { get; set; }
    }
}
