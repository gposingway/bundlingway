namespace Bundlingway.Model
{
    public class GposingwayDefinitions
    {
        public required string version { get; set; }
        public required string gposingwayUrl { get; set; }
        public List<string> Deprecated { get; set; } = new();
        public List<OptionalItem> Optional { get; set; } = new();

        public class OptionalItem
        {
            public required string Name { get; set; }
            public required string Url { get; set; }
            public required string Mappings { get; set; }
        }
    }

}
