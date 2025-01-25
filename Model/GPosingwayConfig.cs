namespace GPosingway.Model
{
    public class GPosingwayConfig
    {
        public class ReShadeStatus
        {
            public string Status { get; set; }
            public bool IsMissing { get; set; }
        }

        public class GPosingwayStatus
        {
            public string Status { get; set; }
            public bool IsMissing { get; set; }
        }

        public string XIVPath { get; set; }
        public ReShadeStatus ReShade { get; set; } = new ReShadeStatus();
        public GPosingwayStatus GPosingway { get; set; } = new GPosingwayStatus();
    }
}
